namespace SFA.DAS.Employer.PR.Domain.Models;

public sealed class PermissionRequest 
{
    public required long Ukprn { get; set; }
    public required Guid RequestId { get; set; }
    public Operation[] Operations { get; set; } = [];
}
