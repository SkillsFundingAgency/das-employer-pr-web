using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Models;
public class PermissionModelTests
{
    [Test]
    [MoqInlineAutoData(Operation.CreateCohort, Operation.Recruitment, ManageRequests.YesWithEmployerRecordReview, ManageRequests.Yes)]
    [MoqInlineAutoData(Operation.CreateCohort, Operation.RecruitmentRequiresReview, ManageRequests.YesWithEmployerRecordReview, ManageRequests.YesWithEmployerAdvertReview)]
    [MoqInlineAutoData(Operation.CreateCohort, null, ManageRequests.YesWithEmployerRecordReview, ManageRequests.No)]
    [MoqInlineAutoData(Operation.Recruitment, null, ManageRequests.No, ManageRequests.Yes)]
    [MoqInlineAutoData(Operation.RecruitmentRequiresReview, null, ManageRequests.No, ManageRequests.YesWithEmployerAdvertReview)]
    [MoqInlineAutoData(null, null, ManageRequests.No, ManageRequests.No)]
    public void Operator_ConvertsTo_PermissionsModel(Operation? operation1,
        Operation? operation2,
        string expectedPermissionToAddApprenticesText,
        string expectedPermissionToRecruitApprenticesText,
        string providerName,
        long ukprn)
    {
        var permission = new ProviderPermission { Operations = new List<Operation>(), ProviderName = providerName, Ukprn = ukprn };
        if (operation1 != null) permission.Operations.Add(operation1.Value);
        if (operation2 != null) permission.Operations.Add(operation2.Value);

        PermissionModel sut = permission;
        sut.Ukprn.Should().Be(ukprn);
        sut.ProviderName.Should().Be(providerName);
        sut.PermissionToAddRecords.Should().Be(expectedPermissionToAddApprenticesText);
        sut.PermissionToRecruitApprentices.Should().Be(expectedPermissionToRecruitApprenticesText);
        sut.ActionLink.Should().Be("#");
    }
}
