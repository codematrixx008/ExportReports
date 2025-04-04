using System.Data;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using Formatting = Newtonsoft.Json.Formatting;

[Route("api/[controller]")]
[ApiController]
public class CsvController : ControllerBase
{
    [HttpPost("upload")]
    public IActionResult UploadCsv(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is empty");

        try
        {
            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true  // First row is the header
            });

            var records = csv.GetRecords<dynamic>().ToList();
            if (records.Count == 0)
                return BadRequest("CSV file contains no data.");

            DataTable dataTable = ConvertToDataTable(records.Skip(0)); // Skip header row

            // ✅ Serialize DataTable to JSON (fix serialization issue)
            string jsonResult = JsonConvert.SerializeObject(dataTable, Formatting.Indented);

            return Ok(jsonResult);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error processing CSV: {ex.Message}");
        }
    }

    private DataTable ConvertToDataTable(IEnumerable<dynamic> records)
    {
        DataTable dt = new DataTable();
        bool columnsAdded = false;

        foreach (var record in records)
        {
            var dict = (IDictionary<string, object>)record;
            if (!columnsAdded)
            {
                foreach (var key in dict.Keys)
                    dt.Columns.Add(key);
                columnsAdded = true;
            }

            var row = dt.NewRow();
            foreach (var key in dict.Keys)
                row[key] = dict[key] ?? DBNull.Value;  // Handle null values safely

            dt.Rows.Add(row);
        }

        return dt;
    }
}
