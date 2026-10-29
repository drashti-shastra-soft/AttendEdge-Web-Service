using System;

namespace AttendEdgeWebService.Domain
{
    public class Contact : AuditTrail
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string MobileNumber { get; set; }
        public string Message { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; }
        public int AuditId { get; set; }
    }
}
