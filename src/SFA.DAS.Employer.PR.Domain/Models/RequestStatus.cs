namespace SFA.DAS.Employer.PR.Domain.Models;

public enum RequestStatus : short
{
    New,
    Sent,
    Accepted,
    Declined,
    Expired,
    Deleted
}
