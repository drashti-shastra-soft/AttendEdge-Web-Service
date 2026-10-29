using System;

namespace AttendEdgeWebService.Domain
{
    public class Holiday : AuditTrail
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Company { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsHalfDay { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; }
        public int AuditId { get; set; }
    }
}
