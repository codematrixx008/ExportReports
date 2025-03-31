using System.Data.Common;
using System.Data;
using System.Text;
using OfficeOpenXml;
using System.IO.Compression;
using Microsoft.VisualBasic.FileIO;
using System.Xml.Linq;

namespace Util
{
    public static class Util
    {
        public static async Task<List<Dictionary<string, object>>> ReadDataAsync(DbDataReader reader)
        {
            var dataList = new List<Dictionary<string, object>>();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader[i] ?? DBNull.Value;
                }
                dataList.Add(row);
            }

            return dataList;
        }

        public static async Task<byte[]> GetExcelBytesAsync(List<Dictionary<string, object>> objReaderData, string workSheetName)
        {
            return await Task.Run(() =>
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add(workSheetName);

                    // Write headers
                    var headers = objReaderData.First().Keys.ToList();
                    for (int i = 0; i < headers.Count; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = headers[i];
                        worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    }

                    // Write data
                    for (int row = 0; row < objReaderData.Count; row++)
                    {
                        for (int col = 0; col < headers.Count; col++)
                        {
                            worksheet.Cells[row + 2, col + 1].Value = objReaderData[row][headers[col]];
                        }
                    }

                    return package.GetAsByteArray();
                }
            });
        }

        public static async Task<byte[]> GetCsvBytesAsync(List<Dictionary<string, object>> objReaderData)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8);

            if (objReaderData.Count == 0) return memoryStream.ToArray();

            // Write headers
            var headers = objReaderData.First().Keys.ToArray();
            await writer.WriteLineAsync(string.Join(",", headers));

            // Write rows
            foreach (var row in objReaderData)
            {
                var values = headers.Select(h => $"\"{row[h]?.ToString().Replace("\"", "\"\"")}\"");
                await writer.WriteLineAsync(string.Join(",", values));
            }

            await writer.FlushAsync();
            return memoryStream.ToArray(); // Convert stream to byte array
        }

        public static async Task<byte[]> GetFileContent(string reportName, List<Dictionary<string, object>> objReaderData, string exportType)
        {
            try
            {
                byte[] excelData = [];
                byte[] csvData = [];


                if (exportType.Equals("xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    excelData = await GetExcelBytesAsync(objReaderData, reportName);
                    return excelData;
                }
                else if (exportType.Equals("csv", StringComparison.OrdinalIgnoreCase))
                {
                    csvData = await GetCsvBytesAsync(objReaderData);
                    return csvData;
                }
                else if (exportType.Equals("zip", StringComparison.OrdinalIgnoreCase))
                {
                    excelData = await GetExcelBytesAsync(objReaderData, reportName);
                    csvData = await GetCsvBytesAsync(objReaderData);
                    return await GetZipBytesAsync(excelData, reportName, csvData, reportName);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating file content: {ex.Message}");
            }
        }

        public static async Task<bool> GenerateAndSaveReportAsync(string fileDirectory, string reportName, List<Dictionary<string, object>> objReaderData)
        {
            var exportTypes = new List<string> { "xlsx", "csv", "zip" };
            var allFileByteContents = new Dictionary<string, byte[]>();
            try
            {
                if (!Directory.Exists(fileDirectory))
                {
                    Directory.CreateDirectory(fileDirectory);
                }

                foreach (var exportType in exportTypes)
                {
                    var fileName = $"{reportName}.{exportType}";
                    var filePath = Path.Combine(fileDirectory, fileName);

                    byte[] fileContent = [];
                    if (!exportType.Equals("zip", StringComparison.OrdinalIgnoreCase))
                    {
                        fileContent = await GetFileContent(reportName, objReaderData, exportType);
                    }
                    else
                    {
                        fileContent = await GetZipBytesAsync(allFileByteContents); //take all created data from "allFileByteContents" and crete zip for all combined 
                    }

                    if (fileContent == null || fileContent.Length == 0)
                    {
                        return false;
                    }
                    allFileByteContents[fileName] = fileContent; //fileContents["financeReport.csv"] = byte[]
                    await System.IO.File.WriteAllBytesAsync(filePath, allFileByteContents[fileName]);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //-------------------------------

        public static async Task<byte[]> GetExcelBytesAsync(IDataReader reader, string workSheetName)
        {
            return await Task.Run(() =>
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add(workSheetName);

                    // Get column headers from the IDataReader
                    var headers = new List<string>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        headers.Add(reader.GetName(i));
                        worksheet.Cells[1, i + 1].Value = headers[i];
                        worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    }

                    // Add data from IDataReader
                    int row = 2;
                    while (reader.Read()) // Read each row from IDataReader
                    {
                        for (int col = 0; col < headers.Count; col++)
                        {
                            worksheet.Cells[row, col + 1].Value = reader[col]; // Fetch column value
                        }
                        row++;
                    }

                    return package.GetAsByteArray();
                }
            });
        }

        public static async Task<byte[]> GetCsvBytesAsync(DbDataReader reader)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
            var columns = Enumerable.Range(0, reader.FieldCount)
                                    .Select(reader.GetName)
                                    .ToArray();
            await writer.WriteLineAsync(string.Join(",", columns));

            while (await reader.ReadAsync())
            {
                var values = columns.Select(column =>
                    $"\"{reader[column]?.ToString().Replace("\"", "\"\"")}\""); // Escape double quotes
                await writer.WriteLineAsync(string.Join(",", values));
            }

            await writer.FlushAsync();
            return memoryStream.ToArray(); // Convert stream to byte array
        }

        public static async Task<byte[]> GetFileContent(string reportName, DbDataReader reader, string exportType)
        {
            try
            {
                byte[] excelData = [];
                byte[] csvData = [];


                if (exportType.Equals("xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    excelData = await GetExcelBytesAsync(reader, reportName);
                    return excelData;
                }
                else if (exportType.Equals("csv", StringComparison.OrdinalIgnoreCase))
                {
                    csvData = await GetCsvBytesAsync(reader);
                    return csvData;
                }
                else if (exportType.Equals("zip", StringComparison.OrdinalIgnoreCase))
                {
                    excelData = await GetExcelBytesAsync(reader, reportName);
                    csvData = await GetCsvBytesAsync(reader);
                    return await GetZipBytesAsync(excelData, reportName, csvData, reportName);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating file content: {ex.Message}");
            }
        }

        public static async Task<bool> GenerateAndSaveReportAsync(string fileDirectory, string reportName, DbDataReader reader)
        {
            var exportTypes = new List<string> { "xlsx", "csv", "zip" };
            var allFileByteContents = new Dictionary<string, byte[]>();
            try
            {
                if (!Directory.Exists(fileDirectory))
                {
                    Directory.CreateDirectory(fileDirectory);
                }

                foreach (var exportType in exportTypes)
                {
                    var fileName = $"{reportName}.{exportType}";
                    var filePath = Path.Combine(fileDirectory, fileName);

                    byte[] fileContent = [];
                    if (!exportType.Equals("zip", StringComparison.OrdinalIgnoreCase))
                    {
                        fileContent = await GetFileContent(reportName, reader, exportType);
                    }
                    else
                    {
                        fileContent = await GetZipBytesAsync(allFileByteContents); //take all created data from "allFileByteContents" and crete zip for all combined 
                    }

                    if (fileContent == null || fileContent.Length == 0)
                    {
                        return false;
                    }
                    allFileByteContents[fileName] = fileContent; //fileContents["financeReport.csv"] = byte[]
                    await System.IO.File.WriteAllBytesAsync(filePath, allFileByteContents[fileName]);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //-------------------------------
        public static async Task<byte[]> GetZipBytesAsync(byte[] excelData, string excelFileName, byte[] csvData, string csvFileName)
        {
            if ((excelData == null || excelData.Length == 0) && (csvData == null || csvData.Length == 0))
            {
                throw new ArgumentException("Both Excel and CSV data are empty. Cannot create a ZIP file.");
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    if (excelData != null && excelData.Length > 0)
                    {
                        var excelEntry = archive.CreateEntry($"{excelFileName}.xlsx");
                        using (var entryStream = excelEntry.Open())
                        {
                            await entryStream.WriteAsync(excelData, 0, excelData.Length);
                        }
                    }

                    if (csvData != null && csvData.Length > 0)
                    {
                        var csvEntry = archive.CreateEntry($"{csvFileName}.csv");
                        using (var entryStream = csvEntry.Open())
                        {
                            await entryStream.WriteAsync(csvData, 0, csvData.Length);
                        }
                    }
                }
                return memoryStream.ToArray();
            }
        }

        public static async Task<byte[]> GetZipBytesAsync(Dictionary<string, byte[]> fileContents)
        {
            if (fileContents == null || !fileContents.Any(kv => kv.Value != null && kv.Value.Length > 0))
            {
                throw new ArgumentException("No valid files to include in the ZIP archive.");
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in fileContents)
                    {
                        if (file.Value != null && file.Value.Length > 0)
                        {
                            var entry = archive.CreateEntry(file.Key);
                            using (var entryStream = entry.Open())
                            {
                                await entryStream.WriteAsync(file.Value, 0, file.Value.Length);
                            }
                        }
                    }
                }
                return memoryStream.ToArray();
            }
        }

    }
}
