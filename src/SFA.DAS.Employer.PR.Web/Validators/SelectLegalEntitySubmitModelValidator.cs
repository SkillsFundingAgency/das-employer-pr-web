using FluentValidation;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.Validators;

public class SelectLegalEntitySubmitModelValidator : AbstractValidator<SelectLegalEntitiesSubmitViewModel>
{
    public const string NoOrganisationSelectedErrorMessage = "You must select an organisation";

    public SelectLegalEntitySubmitModelValidator()
    {
        RuleFor(s => s.LegalEntityId)
            .NotEmpty()
            .WithMessage(NoOrganisationSelectedErrorMessage);
    }
}
