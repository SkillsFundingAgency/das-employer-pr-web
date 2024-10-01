using FluentValidation;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.Validators;

public class ChangePermissionsValidator : AbstractValidator<ChangePermissionsSubmitModel>
{
    public const string NotChangedPermissionsErrorMessage = "You must change the permissions for add apprentice records or recruit apprentices for this provider";

    public ChangePermissionsValidator()
    {
        RuleFor(s => s.PermissionToAddCohorts)
            .Must(ChangePermissionsNotChanged)
            .WithMessage(NotChangedPermissionsErrorMessage);
    }

    private static bool ChangePermissionsNotChanged(ChangePermissionsSubmitModel viewModel, string? addRecords)
    {
        return !(viewModel.PermissionToAddCohorts == viewModel.PermissionToAddCohortsOriginal && viewModel.PermissionToRecruit == viewModel.PermissionToRecruitOriginal);
    }
}