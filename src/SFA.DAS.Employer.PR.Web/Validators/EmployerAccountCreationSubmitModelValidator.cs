using FluentValidation;
using SFA.DAS.Employer.PR.Web.Models.Requests;

namespace SFA.DAS.Employer.PR.Web.Validators;

public class EmployerAccountCreationSubmitModelValidator : AbstractValidator<EmployerAccountCreationSubmitModel>
{
    public const string AcceptAgreementErrorMessage = "You must accept the employer agreement";

    public EmployerAccountCreationSubmitModelValidator()
    {
        RuleFor(s => s.HasAcceptedTerms)
            .Cascade(CascadeMode.Stop)
            .NotEqual(false)
            .WithMessage(AcceptAgreementErrorMessage);
    }
}