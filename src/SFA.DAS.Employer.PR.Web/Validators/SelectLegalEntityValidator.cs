using FluentValidation;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.Validators;

public class SelectLegalEntityValidator : AbstractValidator<SelectLegalEntitiesSubmitViewModel>
{
    public const string NoOrganisationSelectedErrorMessage = "Select an organisation";

    public SelectLegalEntityValidator()
    {
        RuleFor(s => s.LegalEntityPublicHashedId)
            .NotEmpty()
            .WithMessage(NoOrganisationSelectedErrorMessage);
    }
}