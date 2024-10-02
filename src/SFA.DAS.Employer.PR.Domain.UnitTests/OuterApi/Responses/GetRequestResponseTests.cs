using NUnit.Framework;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;

namespace SFA.DAS.Employer.PR.Domain.UnitTests.OuterApi.Responses;

public sealed class GetRequestResponseTests
{
    [Test]
    public void ValidateRequest_ShouldReturnTrue_WhenStatusIsSent()
    {
        var requestResponse = new GetRequestResponse
        {
            RequestId = Guid.NewGuid(),
            RequestType = "Test",
            Ukprn = 12345678,
            ProviderName = "Test Provider",
            RequestedBy = "RequestedBy",
            RequestedDate = DateTime.Now,
            Status = nameof(RequestStatus.Sent),
            AccountLegalEntityId = 1,
            EmployerOrganisationName = "EmployerOrganisationName",
            EmployerContactFirstName = "EmployerContactFirstName",
            EmployerContactLastName = "EmployerContactLastName",
            EmployerContactEmail = "EmployerContactEmail",
            EmployerPAYE = "EmployerPAYE",
            EmployerAORN = "EmployerAORN",
            UpdatedDate = DateTime.UtcNow
        };

        var isValid = requestResponse.ValidateRequest();

        Assert.That(isValid, Is.True);
    }

    [Test]
    public void ValidateRequest_ShouldReturnTrue_WhenStatusIsNew()
    {
        var requestResponse = new GetRequestResponse
        {
            RequestId = Guid.NewGuid(),
            RequestType = "Test",
            Ukprn = 12345678,
            ProviderName = "Test Provider",
            RequestedBy = "RequestedBy",
            RequestedDate = DateTime.Now,
            Status = nameof(RequestStatus.New),
        };

        var isValid = requestResponse.ValidateRequest();

        Assert.That(isValid, Is.True);
    }

    [Test]
    public void ValidateRequest_ShouldReturnFalse_WhenStatusIsNotSentOrNew()
    {
        var requestResponse = new GetRequestResponse
        {
            RequestId = Guid.NewGuid(),
            RequestType = "Test",
            Ukprn = 12345678,
            ProviderName = "Test Provider",
            RequestedBy = "RequestedBy",
            RequestedDate = DateTime.Now,
            Status = nameof(RequestStatus.Expired),
        };

        var isValid = requestResponse.ValidateRequest();

        Assert.That(isValid, Is.False);
    }

    [Test]
    public void ValidateRequest_ShouldReturnFalse_WhenStatusIsNullOrEmpty()
    {
        var requestResponse = new GetRequestResponse
        {
            RequestId = Guid.NewGuid(),
            RequestType = "Test",
            Ukprn = 12345678,
            ProviderName = "Test Provider",
            RequestedBy = "User D",
            RequestedDate = DateTime.Now,
            Status = string.Empty
        };

        var isValid = requestResponse.ValidateRequest();

        Assert.That(isValid, Is.False);
    }
}
