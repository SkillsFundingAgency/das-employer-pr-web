using FluentValidation.TestHelper;
using SFA.DAS.Employer.PR.Web.Models.Requests;
using SFA.DAS.Employer.PR.Web.Validators;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Validators;

public class EmployerAccountNamesSubmitViewModelValidatorTests
{
    private const string ValidFirstName = "Joe";
    private const string ValidLastName = "Cool";

    [Test]
    public void ContactDetailsModel_IsValid()
    {
        var model = new EmployerUserNamesViewModel
        {
            EmployerContactFirstName = ValidFirstName,
            EmployerContactLastName = ValidLastName
        };

        var sut = new EmployerAccountNamesSubmitViewModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void NoFirstNameInModel()
    {
        var model = new EmployerUserNamesViewModel
        {
            EmployerContactFirstName = string.Empty,
            EmployerContactLastName = ValidLastName
        };

        var sut = new EmployerAccountNamesSubmitViewModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.EmployerContactFirstName)
            .WithErrorMessage(EmployerAccountNamesSubmitViewModelValidator.FirstNameEmptyErrorMessage);
    }

    [TestCase("a#")]
    [TestCase("a$")]
    [TestCase("a^")]
    [TestCase("a=")]
    [TestCase("a+")]
    [TestCase("a\\")]
    [TestCase("a/")]
    [TestCase("a<")]
    [TestCase("a>")]
    public void FirstNameInvalidInModel(string firstName)
    {
        var model = new EmployerUserNamesViewModel
        {
            EmployerContactFirstName = firstName,
            EmployerContactLastName = ValidLastName
        };

        var sut = new EmployerAccountNamesSubmitViewModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.EmployerContactFirstName)
            .WithErrorMessage(EmployerAccountNamesSubmitViewModelValidator.FirstNameMustExcludeSpecialCharacters);
    }

    [Test]
    public void NoLastNameInModel()
    {
        var model = new EmployerUserNamesViewModel
        {
            EmployerContactFirstName = ValidFirstName,
            EmployerContactLastName = string.Empty
        };

        var sut = new EmployerAccountNamesSubmitViewModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.EmployerContactLastName)
            .WithErrorMessage(EmployerAccountNamesSubmitViewModelValidator.LastNameEmptyErrorMessage);
    }

    [TestCase("a#")]
    [TestCase("a$")]
    [TestCase("a^")]
    [TestCase("a=")]
    [TestCase("a+")]
    [TestCase("a\\")]
    [TestCase("a/")]
    [TestCase("a<")]
    [TestCase("a>")]
    public void LastNameInvalidInModel(string lastName)
    {
        var model = new EmployerUserNamesViewModel
        {
            EmployerContactFirstName = ValidFirstName,
            EmployerContactLastName = lastName
        };

        var sut = new EmployerAccountNamesSubmitViewModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.EmployerContactLastName)
            .WithErrorMessage(EmployerAccountNamesSubmitViewModelValidator.LastNameMustExcludeSpecialCharacters);
    }
}
