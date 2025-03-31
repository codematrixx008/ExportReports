using System.Data;
using System.Data.SqlClient;
using System.Text;
using Dapper;
using API1.Interface;
using API1.Model;
using OfficeOpenXml;
using Util;

namespace API1.Repository
{
    public class ReportsRepository : IReports
    {
        private readonly string _connectionString;
        private readonly IDapperDbConnection _dapperDbConnection;
        private readonly string _reportPath;

        public ReportsRepository(IDapperDbConnection dapperDbConnection, IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
            _dapperDbConnection = dapperDbConnection;
            _reportPath = configuration["ReportPath:Path"] ?? string.Empty;
        }

        public async Task<IEnumerable<ReportsModel>> GetAllReportsAsync()
        {
            try
            {
                using IDbConnection db = _dapperDbConnection.CreateConnection();
                return await db.QueryAsync<ReportsModel>("SELECT * FROM Reports");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdateReportGeneratingStatus(int reportId, bool isGenerating)
        {
            try
            {
                using IDbConnection db = _dapperDbConnection.CreateConnection();
                string query = "UPDATE Reports SET IsGenerating = @IsGenerating WHERE ReportId = @ReportId";
                int rowsAffected = await db.ExecuteAsync(query, new { IsGenerating = isGenerating, ReportId = reportId });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating report generating status", ex);
            }
        } 
        
        public async Task<bool> UpdateReportGeneratedStatus(int reportId, bool isGenerated)
        {
            try
            {
                using IDbConnection db = _dapperDbConnection.CreateConnection();
                string query = "UPDATE Reports SET IsGenerated = @IsGenerated WHERE ReportId = @ReportId";
                int rowsAffected = await db.ExecuteAsync(query, new { IsGenerated = isGenerated, ReportId = reportId });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating report generating status", ex);
            }
        }

        public async Task<bool> UpdateLastGeneratedOnAndBy(int reportId, DateTime lastGeneratedOn, int lastGeneratedBy)
        {
            try
            {
                using IDbConnection db = _dapperDbConnection.CreateConnection();
                string query = "UPDATE Reports SET LastGeneratedOn = @LastGeneratedOn, LastGeneratedBy = @LastGeneratedBy WHERE ReportId = @ReportId";
                int rowsAffected = await db.ExecuteAsync(query, new
                {
                    LastGeneratedOn = lastGeneratedOn,
                    LastGeneratedBy = lastGeneratedBy,
                    ReportId = reportId
                });

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating last generated details", ex);
            }
        }

        public async Task<MemoryStream> GetWorkbookStreamForReport(ExportReportsModel reportModal)
        {
            var stream = new MemoryStream();
            using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using SqlCommand command = new SqlCommand($"EXEC {reportModal.SpName}", connection);
            using SqlDataReader reader = await command.ExecuteReaderAsync();
            using var package = new ExcelPackage();

            var worksheet = package.Workbook.Worksheets.Add(reportModal.ReportName);
            worksheet.Cells["A1"].LoadFromDataReader(reader, true);

            package.SaveAs(stream);
            stream.Position = 0;

            return stream;
        }

        public async Task<MemoryStream> GetCsvStreamForReport(ExportReportsModel reportModal)
        {
            var stream = new MemoryStream();
            using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using SqlCommand command = new SqlCommand($"EXEC {reportModal.SpName}", connection);
            using SqlDataReader reader = await command.ExecuteReaderAsync();

            var csvBuilder = new StringBuilder();
            var columns = Enumerable.Range(0, reader.FieldCount)
                                    .Select(reader.GetName)
                                    .ToArray();
            csvBuilder.AppendLine(string.Join(",", columns));

            while (await reader.ReadAsync())
            {
                var values = columns.Select(column => reader[column].ToString().Replace(",", " "));
                csvBuilder.AppendLine(string.Join(",", values));
            }

            using var writer = new StreamWriter(stream, Encoding.UTF8);
            await writer.WriteAsync(csvBuilder.ToString());
            await writer.FlushAsync();
            stream.Position = 0;

            return stream;
        }

        public async Task<DataTable> GetWorkbookStreamForReportDataTable(ExportReportsModel reportModal)
        {
            DataTable dataTable = new DataTable();
            using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using SqlCommand command = new SqlCommand($"EXEC {reportModal.SpName}", connection);
            using SqlDataReader reader = await command.ExecuteReaderAsync();
            dataTable.Load(reader);

            return dataTable;
        }

        public async Task<bool> GenerateReports(int ReportId, string ReportName, string SpName)
        {
            try
            {
                using SqlConnection connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using SqlCommand command = new SqlCommand($"EXEC {SpName}", connection);
                using SqlDataReader reader = await command.ExecuteReaderAsync();

                var data = await Util.Util.ReadDataAsync(reader);

                await Util.Util.GenerateAndSaveReportAsync(_reportPath, ReportName, data);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating report generating status", ex);
            }
        }

    }
}