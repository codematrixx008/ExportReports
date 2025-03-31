namespace API1.Model
{
    public class ReportsModel
    {
        public int ReportID { get; set; }
        public string ReportName { get; set; }
        public string SpName { get; set; }
        public DateTime LastGeneratedOn { get; set; }
        public int LastGeneratedBy { get; set; }
        public bool HasStaticFile { get; set; }
        public bool IsGenerating { get; set; }
        public bool IsGenerated { get; set; }
    }
    public class ExportReportsModel
    {
        public string ReportName { get; set; }
        public string SpName { get; set; }
        public string ExportType { get; set; }
    }
}
