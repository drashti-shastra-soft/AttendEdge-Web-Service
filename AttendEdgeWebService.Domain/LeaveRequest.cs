using System;

namespace AttendEdgeWebService.Domain
{
    public class LeaveRequest : AuditTrail
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string LeaveType { get; set; }
        public string HalfType { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public string LeaveStatus { get; set; }
        public string AdminNote { get; set; }
        public int? ProcessedByAdminId { get; set; }
        public bool IsHalfDay { get; set; }
    }
}
