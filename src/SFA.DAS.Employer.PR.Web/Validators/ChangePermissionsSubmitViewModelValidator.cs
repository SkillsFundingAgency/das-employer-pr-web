using FluentValidation;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.Validators;

public class ChangePermissionsSubmitViewModelValidator : AbstractValidator<ChangePermissionsSubmitViewModel>
{
    public const string NotChangedPermissionsErrorMessage = "You must change the permissions for add apprentice records or recruit apprentices for this provider";

    public ChangePermissionsSubmitViewModelValidator()
    {
        RuleFor(s => s.PermissionToAddCohorts)
            .Must(ChangePermissionsNotChanged)
            .WithMessage(NotChangedPermissionsErrorMessage);
    }

    private static bool ChangePermissionsNotChanged(ChangePermissionsSubmitViewModel model, string? addRecords)
    {
        return !(model.PermissionToAddCohorts == model.PermissionToAddCohortsOriginal && model.PermissionToRecruit == model.PermissionToRecruitOriginal);
    }
}