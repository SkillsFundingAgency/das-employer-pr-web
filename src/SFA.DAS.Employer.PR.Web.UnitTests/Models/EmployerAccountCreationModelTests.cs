using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Models.Requests;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Models;
public class EmployerAccountCreationModelTests
{
    [TestCase(Operation.CreateCohort, PermissionDescriptions.AddApprenticeRecords)]
    [TestCase(Operation.Recruitment, PermissionDescriptions.RecruitApprentices)]
    [TestCase(Operation.RecruitmentRequiresReview, PermissionDescriptions.RecruitApprenticesWithReview)]
    public void OperationValues_ExpectedOperationDescriptions(Operation operation, string operationDescription)
    {
        EmployerAccountCreationModel sut = new EmployerAccountCreationModel { ProviderName = "prov", HasAcceptedTerms = false, Operations = new[] { operation } };
        sut.OperationDescriptions[0].Should().Be(operationDescription);
    }
}
