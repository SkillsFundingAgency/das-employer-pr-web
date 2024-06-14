using FluentValidation;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.Validators;

public class ChangePermissionsSubmitViewModelValidator : AbstractValidator<ChangePermissionsSubmitViewModel>
{
    public const string NotChangedPermissionsErrorMessage = "You have not changed your permissions";

    public ChangePermissionsSubmitViewModelValidator()
    {
        RuleFor(s => s.AddRecords)
            .Must(SetPermissionsNotChanged)
            .WithMessage(NotChangedPermissionsErrorMessage);

    }

    private static bool SetPermissionsNotChanged(ChangePermissionsSubmitViewModel model, string? addRecords)
    {
        return !(model.AddRecords == model.AddRecordsOriginal && model.RecruitApprentices == model.RecruitApprenticesOriginal);
    }
}