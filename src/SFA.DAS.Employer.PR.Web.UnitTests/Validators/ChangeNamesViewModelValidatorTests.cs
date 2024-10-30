using FluentValidation.TestHelper;
using SFA.DAS.Employer.PR.Web.Models.Requests;
using SFA.DAS.Employer.PR.Web.Validators;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Validators;

public class ChangeNamesViewModelValidatorTests
{
    private const string ValidFirstName = "Joe";
    private const string ValidLastName = "Cool";

    [Test]
    public void ContactDetailsModel_IsValid()
    {
        var model = new ChangeNamesViewModel
        {
            EmployerContactFirstName = ValidFirstName,
            EmployerContactLastName = ValidLastName
        };

        var sut = new ChangeNamesViewModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void NoFirstNameInModel()
    {
        var model = new ChangeNamesViewModel
        {
            EmployerContactFirstName = string.Empty,
            EmployerContactLastName = ValidLastName
        };

        var sut = new ChangeNamesViewModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.EmployerContactFirstName)
            .WithErrorMessage(ChangeNamesViewModelValidator.FirstNameEmptyErrorMessage);
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
        var model = new ChangeNamesViewModel
        {
            EmployerContactFirstName = firstName,
            EmployerContactLastName = ValidLastName
        };

        var sut = new ChangeNamesViewModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.EmployerContactFirstName)
            .WithErrorMessage(ChangeNamesViewModelValidator.FirstNameMustExcludeSpecialCharacters);
    }

    [Test]
    public void NoLastNameInModel()
    {
        var model = new ChangeNamesViewModel
        {
            EmployerContactFirstName = ValidFirstName,
            EmployerContactLastName = string.Empty
        };

        var sut = new ChangeNamesViewModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.EmployerContactLastName)
            .WithErrorMessage(ChangeNamesViewModelValidator.LastNameEmptyErrorMessage);
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
        var model = new ChangeNamesViewModel
        {
            EmployerContactFirstName = ValidFirstName,
            EmployerContactLastName = lastName
        };

        var sut = new ChangeNamesViewModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.EmployerContactLastName)
            .WithErrorMessage(ChangeNamesViewModelValidator.LastNameMustExcludeSpecialCharacters);
    }
}
