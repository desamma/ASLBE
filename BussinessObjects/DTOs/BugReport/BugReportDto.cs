namespace BussinessObjects.DTOs.BugReport
{
    public class CreateBugReportRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Steps { get; set; }
        public string ExpectedBehavior { get; set; }
        public string ActualBehavior { get; set; }
        public string Severity { get; set; }
    }

    public class BugReportDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Steps { get; set; }
        public string ExpectedBehavior { get; set; }
        public string ActualBehavior { get; set; }
        public string Severity { get; set; }
        public string Status { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class UpdateBugReportStatusRequest
    {
        public string Status { get; set; }
    }
}
