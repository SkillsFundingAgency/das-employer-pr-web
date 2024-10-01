﻿using SFA.DAS.Employer.PR.Domain.Models;

namespace SFA.DAS.Employer.PR.Domain.OuterApi.Responses;

public class GetRequestResponse
{
    public Guid RequestId { get; set; }
    public required string RequestType { get; set; }
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
    public required string Status { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public Operation[] Operations { get; set; } = [];
}

public static class GetRequestResponseExtensions
{
    public static bool ValidateRequest(this GetRequestResponse response)
    {
        return response.Status == nameof(RequestStatus.Sent) || response.Status == nameof(RequestStatus.New);
    }
}

public enum RequestStatus : short
{
    New,
    Sent,
    Accepted,
    Declined,
    Expired,
    Deleted
}
