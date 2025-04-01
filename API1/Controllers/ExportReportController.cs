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
using System.Data.SqlClient;
using System.Data.Common;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;


[ApiController]
[Route("[controller]")]
public class ReportsController : ControllerBase
{    
    DateTime dati = new DateTime();
    Stopwatch sw = new Stopwatch();
    private readonly IReports _reportsRepository;
    private readonly ILogger<ReportsController> _logger;
    private readonly IDapperDbConnection _dapperDbConnection;
    private readonly string _connectionString;
    private readonly string _reportPath;


    public ReportsController(ILogger<ReportsController> logger, IReports reportsRepository,IDapperDbConnection dapperDbConnection, IConfiguration configuration)
    {
        _logger = logger;
        _reportsRepository = reportsRepository;
        _dapperDbConnection = dapperDbConnection;
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        _reportPath = configuration["ReportPath:Path"] ?? string.Empty;
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
            var generatedFiles = await _reportsRepository.GenerateReports(ReportId, ReportName, SpName);

            return Ok(new { message = "Files generated successfully.", files = generatedFiles });
        }
        catch (Exception ex)
        {
            await _reportsRepository.UpdateReportGeneratingStatus(ReportId, false);
            return StatusCode(500, new { message = "Error saving files.", error = ex.Message });
        }
    }

    [HttpGet("DownloadReportFile")]
    public IActionResult DownloadReportFile(string fileName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest(new { message = "File name is required." });
            }

            
            var filePath = Path.Combine(_reportPath, fileName);

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
    
    [HttpGet("DownloadFile")]
    public IActionResult DownloadFile()
    {
        if (!System.IO.File.Exists(_reportPath))
        {
            return BadRequest(new { message = "File path is not set or file does not exist." });
        }
        byte[] fileBytes = System.IO.File.ReadAllBytes(_reportPath);
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DownloadedFile.xlsx");
    }
}