using FluentValidation;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.Validators;

public class SelectTrainingProviderSubmitModelValidator : AbstractValidator<SelectTrainingProviderSubmitModel>
{
    public const string NoTrainingProviderSelectedErrorMessage = "Select a training provider";

    public SelectTrainingProviderSubmitModelValidator()
    {
        RuleFor(s => s.SearchTerm)
            .NotEmpty()
            .WithMessage(NoTrainingProviderSelectedErrorMessage);
    }
}