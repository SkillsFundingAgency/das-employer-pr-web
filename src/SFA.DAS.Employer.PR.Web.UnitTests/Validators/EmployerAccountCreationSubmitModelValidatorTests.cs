using FluentValidation.TestHelper;
using SFA.DAS.Employer.PR.Web.Models.Requests;
using SFA.DAS.Employer.PR.Web.Validators;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Validators;
public class EmployerAccountCreationSubmitModelValidatorTests
{
    [Test, MoqAutoData]
    public void TestValidate_AcceptAddAccount_Valid()
    {
        var model = new EmployerAccountCreationSubmitModel { HasAcceptedTerms = true };

        var sut = new EmployerAccountCreationSubmitModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(c => c.HasAcceptedTerms);
    }

    [Test]
    public void TestValidator_AcceptAddAccount_ReturnsExpectedMessage()
    {
        var model = new EmployerAccountCreationSubmitModel { HasAcceptedTerms = false };
        var sut = new EmployerAccountCreationSubmitModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.HasAcceptedTerms)
            .WithErrorMessage(EmployerAccountCreationSubmitModelValidator.AcceptAgreementErrorMessage);
    }
}
