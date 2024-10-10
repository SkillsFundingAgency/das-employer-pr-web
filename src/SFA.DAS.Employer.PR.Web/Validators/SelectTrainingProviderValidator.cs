using FluentValidation;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.Validators;

public class SelectTrainingProviderValidator : AbstractValidator<SelectTrainingProviderViewModel>
{
    public const string NoTrainingProviderSelectedErrorMessage = "Select a training provider";

    public SelectTrainingProviderValidator()
    {
        RuleFor(s => s.SearchTerm)
            .NotEmpty()
            .WithMessage(NoTrainingProviderSelectedErrorMessage);
    }
}