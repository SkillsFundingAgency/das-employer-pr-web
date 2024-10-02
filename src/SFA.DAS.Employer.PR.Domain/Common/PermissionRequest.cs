namespace SFA.DAS.Employer.PR.Domain.Common;

public static class PermissionRequest
{
    public enum RequestStatus : short
    {
        New,
        Sent,
        Accepted,
        Declined,
        Expired,
        Deleted
    }
}
