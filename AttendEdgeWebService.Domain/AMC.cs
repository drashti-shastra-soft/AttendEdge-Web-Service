using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendEdgeWebService.Domain
{
    public class AMC : AuditTrail
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CompanyId { get; set; }
        public string Company { get; set; }
        public string ContactPerson { get; set; }
        public string MobileNumber { get; set; }
        public string EmailAddress { get; set; }
        public DateTime ContractStartDate { get; set; }
        public DateTime ContractEndDate { get; set; }
        public string ServicesCovered { get; set; }
        public string ContractDocumentPath { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public string Status { get; set; }
        public int AuditId { get; set; }
    }
}
