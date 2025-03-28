using System.Data;
using API1.Model;
using Microsoft.AspNetCore.Mvc;

namespace API1.Interface
{
    public interface IReports
    {
        Task<IEnumerable<ReportsModel>> GetAllReportsAsync();
        Task<bool> UpdateReportGeneratingStatus(int reportId, bool isGenerating);
        Task<bool> UpdateLastGeneratedOnAndBy(int reportId, DateTime lastGeneratedOn, int lastGeneratedBy);
        Task<MemoryStream> GetCsvStreamForReport(ExportReportsModel model);
        Task<MemoryStream> GetWorkbookStreamForReport(ExportReportsModel reportModal);
        Task<DataTable> GetWorkbookStreamForReportDataTable(ExportReportsModel reportModal);
    }
}
