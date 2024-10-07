using FluentValidation.TestHelper;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Validators;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Validators;

public sealed class ReviewPermissionsRequestSubmitViewModelValidatorTests
{
    [Test, MoqAutoData]
    public void TestValidate_AcceptPermissions_Valid(string searchTerm)
    {
        var model = new ReviewPermissionsRequestSubmitViewModel()
        {
            AcceptPermissions = true
        };

        var sut = new ReviewPermissionsRequestSubmitViewModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(c => c.AcceptPermissions);
    }

    [Test]
    public void TestValidator_AcceptPermissions_ReturnsExpectedMessage()
    {
        var model = new ReviewPermissionsRequestSubmitViewModel();
        var sut = new ReviewPermissionsRequestSubmitViewModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.AcceptPermissions)
            .WithErrorMessage(ReviewPermissionsRequestSubmitViewModelValidator.AcceptPermissionsValidationMessage);
    }
}
