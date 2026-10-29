namespace AttendEdgeWebService.Infrastructure.Utils
{
    public static class Enums
    {
        public enum Role
        {
            System_Owner = 1,
            Super_Admin = 2,
            Admin = 3,
            Employee = 4,
        }

        public enum Status
        {
            Pending = 1,
            Approved = 2,
            Rejected = 3
        }
    }
}
