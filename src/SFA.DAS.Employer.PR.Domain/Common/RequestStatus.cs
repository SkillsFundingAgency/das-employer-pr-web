namespace SFA.DAS.Employer.PR.Domain.Common;

public enum RequestStatus : short
{
    New,
    Sent,
    Accepted,
    Declined,
    Expired,
    Deleted
}