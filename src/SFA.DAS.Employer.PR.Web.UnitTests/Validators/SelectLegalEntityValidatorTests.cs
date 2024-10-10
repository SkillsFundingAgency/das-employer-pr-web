using FluentValidation.TestHelper;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Validators;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Validators;
public class SelectLegalEntityValidatorTests
{
    [Test, MoqAutoData]
    public void TestValidate_LegalEntitySet_Valid(string legalEntityId)
    {
        var model = new SelectLegalEntitiesSubmitViewModel
        {
            LegalEntityPublicHashedId = legalEntityId
        };

        var sut = new SelectLegalEntityValidator();
        var result = sut.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(c => c.LegalEntityPublicHashedId);
    }

    [Test]
    public void TestValidator_LegalEntityInvalid_ReturnsExpectedMessage()
    {
        var model = new SelectLegalEntitiesSubmitViewModel();
        var sut = new SelectLegalEntityValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.LegalEntityPublicHashedId)
            .WithErrorMessage(SelectLegalEntityValidator.NoOrganisationSelectedErrorMessage);
    }
}
