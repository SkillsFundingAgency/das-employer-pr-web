using FluentValidation.TestHelper;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Validators;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Validators;
public class SelectTrainingProviderValidatorTests
{
    [Test, MoqAutoData]
    public void TestValidate_LegalEntitySet_Valid(string searchTerm)
    {
        var model = new SelectTrainingProviderViewModel()
        {
            SearchTerm = searchTerm
        };

        var sut = new SelectTrainingProviderValidator();
        var result = sut.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(c => c.SearchTerm);
    }

    [Test]
    public void TestValidator_LegalEntityInvalid_ReturnsExpectedMessage()
    {
        var model = new SelectTrainingProviderViewModel();
        var sut = new SelectTrainingProviderValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.SearchTerm)
            .WithErrorMessage(SelectTrainingProviderValidator.NoTrainingProviderSelectedErrorMessage);
    }
}
