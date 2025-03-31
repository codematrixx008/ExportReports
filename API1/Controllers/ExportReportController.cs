using Microsoft.AspNetCore.Mvc;
using API1.Interface;
using API1.Model;
using OfficeOpenXml;
using System.Data;
using System.IO;
using System.Diagnostics;
using System.Text;
using API1.Repository;
using System.IO.Compression;
using System.IO.Pipes;
using Microsoft.Extensions.Logging;
using Dapper;


[ApiController]
[Route("[controller]")]
public class ReportsController : ControllerBase
{    
    DateTime dati = new DateTime();
    Stopwatch sw = new Stopwatch();
    private readonly IReports _reportsRepository;
    private readonly ILogger<ReportsController> _logger;
    private readonly IDapperDbConnection _dapperDbConnection;

public ReportsController(ILogger<ReportsController> logger, IReports reportsRepository,IDapperDbConnection dapperDbConnection)
    {
        _logger = logger;
        _reportsRepository = reportsRepository;
        _dapperDbConnection = dapperDbConnection;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReportsModel>>> GetAllReports()
    {
        var _reports = await _reportsRepository.GetAllReportsAsync();
        return Ok(_reports);
    }

    [HttpGet("ExportToExcelandCsv")]
    public async Task<IActionResult> ExportToFile([FromQuery] ExportReportsModel model)
    {
        TraceExecutionTime("ExportToFile", start: true);

        MemoryStream stream;
        string fileName;
        string contentType;

        if (model.ExportType.Equals("xlsx", StringComparison.OrdinalIgnoreCase))
        {
            stream = await _reportsRepository.GetWorkbookStreamForReport(model);
            fileName = $"{model.ReportName}.xlsx";
            contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        }
        else if (model.ExportType.Equals("csv", StringComparison.OrdinalIgnoreCase))
        {
            stream = await _reportsRepository.GetCsvStreamForReport(model);
            fileName = $"{model.ReportName}.csv";
            contentType = "text/csv";
        }
        else if (model.ExportType.Equals("zip", StringComparison.OrdinalIgnoreCase))
        {
            stream = new MemoryStream();
            contentType = "application/zip";
            fileName = $"{model.ReportName}_{DateTime.Now:yyyyMMdd_hhmmss}.zip";

            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                // XLSX file
                using (var xlsxStream = await _reportsRepository.GetWorkbookStreamForReport(model))
                {
                    var xlsxEntry = archive.CreateEntry($"{model.ReportName}.xlsx", System.IO.Compression.CompressionLevel.Fastest);
                    using (var entryStream = xlsxEntry.Open())
                    {
                        xlsxStream.Seek(0, SeekOrigin.Begin);
                        await xlsxStream.CopyToAsync(entryStream);
                    }
                }

                // CSV file
                using (var csvStream = await _reportsRepository.GetCsvStreamForReport(model))
                {
                    var csvEntry = archive.CreateEntry($"{model.ReportName}.csv", System.IO.Compression.CompressionLevel.Fastest);
                    using (var entryStream = csvEntry.Open())
                    {
                        csvStream.Seek(0, SeekOrigin.Begin);
                        await csvStream.CopyToAsync(entryStream);
                    }
                }
            }

            // Reset stream position to the beginning before returning the file
            stream.Seek(0, SeekOrigin.Begin);
        }
        else
        {
            return BadRequest("Unsupported file type. Please choose either 'xlsx', 'csv', or 'zip'.");
        }

        TraceExecutionTime("ExportToFile", stop: true);
        return File(stream.ToArray(), contentType, fileName);
    }

    private void TraceExecutionTime(string traceFunction, bool start = false, bool stop = false, bool restart = false)
    {
        if (start)
        {
            sw.Start();
        }
        else if (stop || restart)
        {
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            Debug.WriteLine($"Execution Time for {traceFunction} {ts.TotalMilliseconds} ms started at {dati.ToString()}");
            if (restart)
            {
                sw.Restart();
            }
        }
    }

    [HttpGet("SaveToServerForStaticReport")]
    public async Task<IActionResult> SaveToServerForStaticReport(int ReportId, string ReportName, string SpName, string ExportType)
    {
        try
        {
            // Mark report as generating
            await _reportsRepository.UpdateReportGeneratingStatus(ReportId, true);
            

            if (string.IsNullOrWhiteSpace(ReportName) || string.IsNullOrWhiteSpace(SpName) || string.IsNullOrWhiteSpace(ExportType))
            {
                return BadRequest(new { message = "Invalid input parameters." });
            }

            var now = DateTime.Now;
            var formattedDate = now.ToString("dd-MM-yyyy");
            var formattedTime = now.ToString("HH-mm-ss");

            var directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets/downloadsfiles");
            Directory.CreateDirectory(directoryPath); // Ensure directory exists

            List<string> generatedFiles = new List<string>();

            // If ExportType is "refresh", generate all formats
            var exportTypes = ExportType.ToLower() == "refresh"
                ? new List<string> { "xlsx", "csv", "zip" }
                : new List<string> { ExportType.ToLower() };

            foreach (var fileType in exportTypes)
            {
                var fileName = $"{ReportName}.{fileType}";
                var filePath = Path.Combine(directoryPath, fileName);

                byte[] fileContent = GenerateFileContent(ReportName, SpName, fileType);
                if (fileContent == null || fileContent.Length == 0)
                {
                    return StatusCode(500, new { message = $"File generation failed for {fileType}." });
                }

                await System.IO.File.WriteAllBytesAsync(filePath, fileContent);
                generatedFiles.Add(fileName);
            }

            // Mark report as finished generating
            await _reportsRepository.UpdateReportGeneratingStatus(ReportId, false);
            await _reportsRepository.UpdateReportGeneratedStatus(ReportId, true);
           
            await _reportsRepository.UpdateLastGeneratedOnAndBy(ReportId, DateTime.Now, 123);

            return Ok(new { message = "Files generated successfully.", files = generatedFiles });
        }
        catch (Exception ex)
        {
            await _reportsRepository.UpdateReportGeneratingStatus(ReportId, false);
            return StatusCode(500, new { message = "Error saving files.", error = ex.Message });
        }
    }


    //[HttpGet("SaveToServerForStaticReport")]
    //public async Task<IActionResult> SaveToServerForStaticReport(int ReportId, string ReportName, string SpName, string ExportType)
    //{
    //    try
    //    {
    //        // Mark report as generating
    //        await _reportsRepository.UpdateReportGeneratingStatus(ReportId, true);

    //        if (string.IsNullOrWhiteSpace(ReportName) || string.IsNullOrWhiteSpace(SpName) || string.IsNullOrWhiteSpace(ExportType))
    //        {
    //            return BadRequest(new { message = "Invalid input parameters." });
    //        }

    //        var now = DateTime.Now;
    //        var formattedDate = now.ToString("dd-MM-yyyy");
    //        var formattedTime = now.ToString("HH-mm-ss");

    //        // Validate ExportType
    //        var validExtensions = new HashSet<string> { "xlsx", "csv", "zip" };
    //        if (!validExtensions.Contains(ExportType.ToLower()))
    //        {
    //            return BadRequest(new { message = "Invalid export type." });
    //        }

    //        var fileExtension = ExportType.ToLower();
    //        var fileName = $"{ReportName}_{formattedDate}_{formattedTime}.{fileExtension}"; // Ensure unique file name

    //        var directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets/downloadsfiles");
    //        var filePath = Path.Combine(directoryPath, fileName);

    //        // Ensure directory exists
    //        Directory.CreateDirectory(directoryPath);

    //        // Generate file content
    //        byte[] fileContent = GenerateFileContent(ReportName, SpName, ExportType);
    //        if (fileContent == null || fileContent.Length == 0)
    //        {
    //            return StatusCode(500, new { message = "File generation failed." });
    //        }

    //        await System.IO.File.WriteAllBytesAsync(filePath, fileContent);

    //        // Mark report as finished generating
    //        await _reportsRepository.UpdateReportGeneratingStatus(ReportId, false);
    //        await _reportsRepository.UpdateLastGeneratedOnAndBy(ReportId, DateTime.Now, 123);


    //        return Ok(new { message = "File generated successfully.", fileName });
    //    }
    //    catch (Exception ex)
    //    {
    //        await _reportsRepository.UpdateReportGeneratingStatus(ReportId, false);
    //        return StatusCode(500, new { message = "Error saving file.", error = ex.Message });
    //    }
    //}



    [HttpGet("DownloadReportFile")]
    public IActionResult DownloadReportFile(string fileName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest(new { message = "File name is required." });
            }

            var directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets/downloadsfiles");
            var filePath = Path.Combine(directoryPath, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { message = "File not found." });
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var contentType = GetContentType(filePath);

            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error downloading file.", error = ex.Message });
        }
    }

    private string GetContentType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".csv" => "text/csv",
            ".zip" => "application/zip",
            _ => "application/octet-stream",
        };
    }
    private byte[] GenerateFileContent(string reportName, string spName, string exportType)
    {
        try
        {
            // Fetch data from database using Dapper
            using (var connection = _dapperDbConnection.CreateConnection())
            {
                var data = connection.Query(spName, commandType: CommandType.StoredProcedure).ToList();

                if (data == null || !data.Any())
                {
                    throw new Exception("No data found for the given report.");
                }

                if (exportType.Equals("xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    return GenerateExcel(data, reportName);
                }
                else if (exportType.Equals("csv", StringComparison.OrdinalIgnoreCase))
                {
                    return GenerateCsv(data);
                }
                else if (exportType.Equals("zip", StringComparison.OrdinalIgnoreCase))
                {
                    byte[] excelData = GenerateExcel(data, reportName);
                    byte[] csvData = GenerateCsv(data);
                    return GenerateZip(excelData, csvData, reportName);
                }
                else
                {
                    throw new Exception("Invalid export type.");
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error generating file content: {ex.Message}");
        }
    }
    private byte[] GenerateExcel(List<dynamic> data, string reportName)
    {
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add(reportName);

            // Add Headers
            var headers = ((IDictionary<string, object>)data.First()).Keys.ToList();
            for (int i = 0; i < headers.Count; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            // Add Data
            for (int row = 0; row < data.Count; row++)
            {
                var rowData = (IDictionary<string, object>)data[row];
                for (int col = 0; col < headers.Count; col++)
                {
                    worksheet.Cells[row + 2, col + 1].Value = rowData[headers[col]];
                }
            }

            return package.GetAsByteArray();
        }
    }
    private byte[] GenerateCsv(List<dynamic> data)
    {
        var csvBuilder = new StringBuilder();
        var headers = ((IDictionary<string, object>)data.First()).Keys.ToList();

        // Add Headers
        csvBuilder.AppendLine(string.Join(",", headers));

        // Add Data
        foreach (var row in data)
        {
            var rowData = (IDictionary<string, object>)row;
            csvBuilder.AppendLine(string.Join(",", headers.Select(h => rowData[h]?.ToString() ?? "")));
        }

        return Encoding.UTF8.GetBytes(csvBuilder.ToString());
    }
    private byte[] GenerateZip(byte[] excelData, byte[] csvData, string reportName)
    {
        using (var memoryStream = new MemoryStream())
        {
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var excelEntry = archive.CreateEntry($"{reportName}.xlsx");
                using (var entryStream = excelEntry.Open())
                {
                    entryStream.Write(excelData, 0, excelData.Length);
                }

                var csvEntry = archive.CreateEntry($"{reportName}.csv");
                using (var entryStream = csvEntry.Open())
                {
                    entryStream.Write(csvData, 0, csvData.Length);
                }
            }
            return memoryStream.ToArray();
        }
    }
}