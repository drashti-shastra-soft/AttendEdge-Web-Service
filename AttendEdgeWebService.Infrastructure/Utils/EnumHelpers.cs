using System.Linq;

namespace AttendEdgeWebService.Infrastructure.Utils
{
    public static class EnumHelpers
    {
        public static string GetEnumString(Enums.Status result)
        {
            switch (result)
            {
                case Enums.Status.Pending:
                    return nameof(Enums.Status.Pending);
                case Enums.Status.Approved:
                    return nameof(Enums.Status.Approved);
                case Enums.Status.Rejected:
                    return nameof(Enums.Status.Rejected);
                default:
                    return null;
            }
        }

    }
}
