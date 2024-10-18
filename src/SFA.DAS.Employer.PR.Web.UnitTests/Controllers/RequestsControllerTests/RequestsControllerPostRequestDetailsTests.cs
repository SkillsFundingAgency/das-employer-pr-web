using AutoFixture.NUnit3;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Controllers;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Models.Requests;
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.RequestsControllerTests;
public class RequestsControllerPostRequestDetailsTests
{
    static readonly string CreateAccountCheckDetailsLink = Guid.NewGuid().ToString();

    [Test, MoqAutoData]
    public async Task Post_Validated_ReturnsExpectedModel(
        [Frozen] Mock<IValidator<EmployerAccountCreationSubmitModel>> validatorMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RequestsController sut,
         Guid requestId,
         CancellationToken cancellationToken)
    {
        validatorMock.Setup(v => v.Validate(It.IsAny<EmployerAccountCreationSubmitModel>())).Returns(new ValidationResult());

        sut.AddDefaultContext();

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.CreateAccountCheckDetails, CreateAccountCheckDetailsLink);

        EmployerAccountCreationSubmitModel submitModel = new() { HasAcceptedTerms = true };

        var result = await sut.PostRequestDetails(requestId, submitModel, cancellationToken);

        RedirectToRouteResult? redirectToRouteResult = result.As<RedirectToRouteResult>();
        redirectToRouteResult.RouteName.Should().Be(RouteNames.CreateAccountCheckDetails);
        outerApiClientMock.Verify(x => x.GetPermissionRequest(requestId, cancellationToken), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task Post_ValidationFailed_ReturnsExpectedModel(
        [Frozen] Mock<IValidator<EmployerAccountCreationSubmitModel>> validatorMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RequestsController sut,
        GetPermissionRequestResponse permissionRequest,
        CancellationToken cancellationToken)
    {
        var requestId = permissionRequest.RequestId;

        validatorMock.Setup(v => v.Validate(It.IsAny<EmployerAccountCreationSubmitModel>())).Returns(new ValidationResult(new List<ValidationFailure>()
             {
                 new("TestField","Test Message") { ErrorCode = "1001"}
             }));

        outerApiClientMock.Setup(o => o.GetPermissionRequest(requestId, cancellationToken))
            .ReturnsAsync(permissionRequest);

        sut.AddDefaultContext();

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.CreateAccountCheckDetails, CreateAccountCheckDetailsLink);

        EmployerAccountCreationSubmitModel submitModel = new() { HasAcceptedTerms = false };

        var result = await sut.PostRequestDetails(requestId, submitModel, cancellationToken);

        ViewResult? viewResult = result.As<ViewResult>();
        EmployerAccountCreationModel? viewModel = viewResult.Model as EmployerAccountCreationModel;

        viewModel.Should().BeEquivalentTo(permissionRequest, options =>
            options.ExcludingMissingMembers()
                .Excluding(x => x.ProviderName)
                .Excluding(x => x.EmployerOrganisationName));

        viewModel.ProviderName.Should().Be(permissionRequest.ProviderName.ToUpper());
        viewModel.EmployerOrganisationName.Should().Be(permissionRequest.EmployerOrganisationName!.ToUpper());
        outerApiClientMock.Verify(x => x.GetPermissionRequest(requestId, cancellationToken), Times.Once);
    }
}
