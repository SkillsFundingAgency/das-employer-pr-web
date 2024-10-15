using SFA.DAS.Employer.PR.Domain.Common;

namespace SFA.DAS.Employer.PR.Domain.Models;

public sealed class PermissionRequest 
{
    public required long Ukprn { get; set; }
    public required string ProviderName { get; set; }
    public required Guid RequestId { get; set; }
    public Operation[] Operations { get; set; } = [];
    public RequestType RequestType { get; set; }
}
