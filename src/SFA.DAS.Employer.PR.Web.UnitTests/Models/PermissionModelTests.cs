using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Models;
public class PermissionModelTests
{
    [Test]
    [MoqInlineAutoData(Operation.CreateCohort, Operation.Recruitment, PermissionModel.PermissionToAddRecordsText, PermissionModel.PermissionToRecruitText)]
    [MoqInlineAutoData(Operation.CreateCohort, Operation.RecruitmentRequiresReview, PermissionModel.PermissionToAddRecordsText, PermissionModel.PermissionToRecruitReviewAdvertsText)]
    [MoqInlineAutoData(Operation.CreateCohort, null, PermissionModel.PermissionToAddRecordsText, PermissionModel.NoPermissionToRecruitText)]
    [MoqInlineAutoData(Operation.Recruitment, null, PermissionModel.NoPermissionToAddRecordsText, PermissionModel.PermissionToRecruitText)]
    [MoqInlineAutoData(Operation.RecruitmentRequiresReview, null, PermissionModel.NoPermissionToAddRecordsText, PermissionModel.PermissionToRecruitReviewAdvertsText)]
    [MoqInlineAutoData(null, null, PermissionModel.NoPermissionToAddRecordsText, PermissionModel.NoPermissionToRecruitText)]
    public void Operator_ConvertsTo_PermissionsModel(Operation? operation1,
        Operation? operation2,
        string expectedPermissionToAddApprenticesText,
        string expectedPermissionToRecruitApprenticesText,
        string providerName,
        long ukprn)
    {
        var permission = new Permission { Operations = new List<Operation>(), ProviderName = providerName, Ukprn = ukprn };
        if (operation1 != null) permission.Operations.Add(operation1.Value);
        if (operation2 != null) permission.Operations.Add(operation2.Value);

        PermissionModel sut = permission;
        sut.Ukprn.Should().Be(ukprn);
        sut.ProviderName.Should().Be(providerName);
        sut.PermissionToAddRecords.Should().Be(expectedPermissionToAddApprenticesText);
        sut.PermissionToRecruitApprentices.Should().Be(expectedPermissionToRecruitApprenticesText);
        sut.ChangePermissionsLink.Should().Be("#");
    }
}
