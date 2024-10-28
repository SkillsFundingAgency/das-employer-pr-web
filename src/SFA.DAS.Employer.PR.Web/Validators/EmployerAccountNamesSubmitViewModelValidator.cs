using FluentValidation;
using SFA.DAS.Employer.PR.Domain.Constants;
using SFA.DAS.Employer.PR.Web.Models.Requests;

namespace SFA.DAS.Employer.PR.Web.Validators;

public class EmployerAccountNamesSubmitViewModelValidator : AbstractValidator<EmployerUserNamesViewModel>
{
    public const string FirstNameEmptyErrorMessage = "You must enter a first name";
    public const string FirstNameMustExcludeSpecialCharacters = "First name must include valid characters";
    public const string LastNameEmptyErrorMessage = "You must enter a last name";
    public const string LastNameMustExcludeSpecialCharacters = "Last name must include valid characters";

    public EmployerAccountNamesSubmitViewModelValidator()
    {
        RuleFor(s => s.EmployerContactFirstName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(FirstNameEmptyErrorMessage)
            .Matches(RegularExpressions.ExcludedCharactersRegex)
            .WithMessage(FirstNameMustExcludeSpecialCharacters);

        RuleFor(s => s.EmployerContactLastName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(LastNameEmptyErrorMessage)
            .Matches(RegularExpressions.ExcludedCharactersRegex)
            .WithMessage(LastNameMustExcludeSpecialCharacters);

    }
}