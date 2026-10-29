namespace AttendEdgeWebService.Domain
{
    public class Company : AuditTrail
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string EmailAddress { get; set; }
        public string ContactPerson { get; set; }
        public string MobileNumber { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; }
        public string Logo { get; set; }
        public int AuditId { get; set; }

    }
}
