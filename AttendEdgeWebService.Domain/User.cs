using System;
using System.Collections.Generic;
using System.Web;

namespace AttendEdgeWebService.Domain
{
    public class User : AuditTrail
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int CompanyId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string PlainTextPassword { get; set; }
        public string MobileNumber { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; }
        public string Token { get; set; }
        public string Status { get; set; }
        public string Role { get; set; }
        public string Company { get; set; }
        public int AuditId { get; set; }

        public List<UserDocument> Images = new List<UserDocument>();
    }
}
