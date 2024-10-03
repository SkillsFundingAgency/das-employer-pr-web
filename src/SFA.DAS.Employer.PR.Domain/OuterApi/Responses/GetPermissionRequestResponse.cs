using SFA.DAS.Employer.PR.Domain.Models;
using static SFA.DAS.Employer.PR.Domain.Common.PermissionRequest;

namespace SFA.DAS.Employer.PR.Domain.OuterApi.Responses;

public class GetPermissionRequestResponse
{
    public Guid RequestId { get; set; }
    public RequestType RequestType { get; set; }
    public long Ukprn { get; set; }
    public required string ProviderName { get; set; }
    public required string RequestedBy { get; set; }
    public DateTime RequestedDate { get; set; }
    public long? AccountLegalEntityId { get; set; }
    public string? EmployerOrganisationName { get; set; }
    public string? EmployerContactFirstName { get; set; }
    public string? EmployerContactLastName { get; set; }
    public string? EmployerContactEmail { get; set; }
    public string? EmployerPAYE { get; set; }
    public string? EmployerAORN { get; set; }
    public RequestStatus Status { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public Operation[] Operations { get; set; } = [];
}