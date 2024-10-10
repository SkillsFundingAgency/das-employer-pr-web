using FluentValidation.TestHelper;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Validators;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Validators;

internal class ReviewAddAccountRequestSubmitViewModelValidatorTests
{
    [Test, MoqAutoData]
    public void TestValidate_AcceptAddAccount_Valid(string searchTerm)
    {
        var model = new ReviewAddAccountRequestSubmitViewModel()
        {
            AcceptAddAccountRequest = true
        };

        var sut = new ReviewAddAccountRequestSubmitViewModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(c => c.AcceptAddAccountRequest);
    }

    [Test]
    public void TestValidator_AcceptAddAccount_ReturnsExpectedMessage()
    {
        var model = new ReviewAddAccountRequestSubmitViewModel();
        var sut = new ReviewAddAccountRequestSubmitViewModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.AcceptAddAccountRequest)
            .WithErrorMessage(ReviewAddAccountRequestSubmitViewModelValidator.AcceptAddAccountRequestValidationMessage);
    }
}
