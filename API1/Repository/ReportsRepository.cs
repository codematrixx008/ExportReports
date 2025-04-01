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

        //public async Task<IEnumerable<ReportsModel>> GetAllReportsAsync()
        //{
        //    try
        //    {
        //        using IDbConnection db = _dapperDbConnection.CreateConnection();
        //        return await db.QueryAsync<ReportsModel>("SELECT * FROM Reports");
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public async Task<IEnumerable<ReportsModel>> GetAllReportsAsync()
        {
            return await Task.FromResult(new List<ReportsModel>
             {
                 new ReportsModel { ReportID = 1,  ReportFileName = "Demo", ReportName = "1 Lakh", SpName = "sp_Get1LakhRecord", LastGeneratedOn = DateTime.Parse("2025-03-31 16:09:32.220"), LastGeneratedBy = 123, HasStaticFile = true, IsGenerating = false, IsGenerated = true },
                 new ReportsModel { ReportID = 2,  ReportFileName = "Demo", ReportName = "2 Lakh", SpName = "sp_Get2LakhRecord", LastGeneratedOn = DateTime.Parse("2025-03-28 16:19:07.733"), LastGeneratedBy = 0, HasStaticFile = false, IsGenerating = false, IsGenerated = false },
                 new ReportsModel { ReportID = 3,  ReportFileName = "Demo", ReportName = "5 Lakh", SpName = "sp_Get5LakhRecord", LastGeneratedOn = DateTime.Parse("2025-03-31 11:22:42.737"), LastGeneratedBy = 123, HasStaticFile = true, IsGenerating = false, IsGenerated = true },
                 new ReportsModel { ReportID = 4,  ReportFileName = "Demo", ReportName = "10 Lakh", SpName = "sp_Get10LakhRecord", LastGeneratedOn = DateTime.Parse("2025-03-28 16:19:07.733"), LastGeneratedBy = 0, HasStaticFile = true, IsGenerating = false, IsGenerated = false },
                 new ReportsModel { ReportID = 5,  ReportFileName = "Demo", ReportName = "Sales Report", SpName = "sp_GetSalesReport", LastGeneratedOn = DateTime.Parse("2025-03-31 11:14:14.673"), LastGeneratedBy = 123, HasStaticFile = true, IsGenerating = false, IsGenerated = true },
                 new ReportsModel { ReportID = 6,  ReportFileName = "Demo", ReportName = "Inventory Report", SpName = "sp_GetInventoryReport", LastGeneratedOn = DateTime.Parse("2025-03-28 16:19:07.733"), LastGeneratedBy = 0, HasStaticFile = true, IsGenerating = false, IsGenerated = false },
                 new ReportsModel { ReportID = 7,  ReportFileName = "Demo", ReportName = "Customer Report", SpName = "sp_GetCustomerReport", LastGeneratedOn = DateTime.Parse("2025-03-31 11:18:59.673"), LastGeneratedBy = 123, HasStaticFile = true, IsGenerating = false, IsGenerated = true },
                 new ReportsModel { ReportID = 8,  ReportFileName = "Demo", ReportName = "Employee Report", SpName = "sp_GetEmployeeReport", LastGeneratedOn = DateTime.Parse("2025-03-28 16:19:07.733"), LastGeneratedBy = 0, HasStaticFile = false, IsGenerating = false, IsGenerated = false },
                 new ReportsModel { ReportID = 9,  ReportFileName = "Demo", ReportName = "Expense Report", SpName = "sp_GetExpenseReport", LastGeneratedOn = DateTime.Parse("2025-03-28 16:19:07.733"), LastGeneratedBy = 0, HasStaticFile = false, IsGenerating = false, IsGenerated = false },
                 new ReportsModel { ReportID = 10, ReportFileName = "Demo",  ReportName = "Revenue Report", SpName = "sp_GetRevenueReport", LastGeneratedOn = DateTime.Parse("2025-03-28 16:19:07.733"), LastGeneratedBy = 0, HasStaticFile = false, IsGenerating = false, IsGenerated = false },
                 
             });
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