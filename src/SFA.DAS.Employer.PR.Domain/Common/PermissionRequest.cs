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

    public enum RequestType : short
    {
        CreateAccount,
        AddAccount,
        Permission
    }
}
