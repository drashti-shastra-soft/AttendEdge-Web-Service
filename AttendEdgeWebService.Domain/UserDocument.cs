using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendEdgeWebService.Domain
{
    public class UserDocument : AuditTrail
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Base64String { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public int AuditId { get; set; }
    }
}
