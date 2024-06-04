using FluentValidation.TestHelper;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Validators;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Validators;
public class SelectLegalEntitySubmitModelValidatorTests
{
    [Test, MoqAutoData]
    public void TestValidate_LegalEntitySet_Valid(long legalEntityId)
    {
        var model = new SelectLegalEntitiesSubmitViewModel
        {
            LegalEntityId = legalEntityId
        };

        var sut = new SelectLegalEntitySubmitModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(c => c.LegalEntityId);
    }


    [Test]
    public void TestValidator_LegalEntityInvalid_ReturnsExpectedMessage()
    {
        var model = new SelectLegalEntitiesSubmitViewModel();
        var sut = new SelectLegalEntitySubmitModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.LegalEntityId)
            .WithErrorMessage(SelectLegalEntitySubmitModelValidator.NoOrganisationSelectedErrorMessage);
    }
}
