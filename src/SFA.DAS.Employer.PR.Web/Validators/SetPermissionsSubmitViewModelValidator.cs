using FluentValidation;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.Validators;

public class SetPermissionsSubmitViewModelValidator : AbstractValidator<SetPermissionsSubmitViewModel>
{
    public const string AddRecordsNotSelectedErrorMessage = "Select the permissions you want to set for Add apprentice records";
    public const string RecruitApprenticesNotSelectedErrorMessage = "Select the permissions you want to set for Recruit apprentices";
    public const string BothSelectionsAreNoErrorMessage = "You must select yes for at least one permission";

    public SetPermissionsSubmitViewModelValidator()
    {
        RuleFor(s => s.AddRecords)
             .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(AddRecordsNotSelectedErrorMessage)
            .Must(SetPermissionsBothNoFalse)
            .WithMessage(BothSelectionsAreNoErrorMessage);

        RuleFor(s => s.RecruitApprentices)
            .NotEmpty()
            .WithMessage(RecruitApprenticesNotSelectedErrorMessage);
    }

    private static bool SetPermissionsBothNoFalse(SetPermissionsSubmitViewModel model, string? addRecords)
    {
        return !(model.AddRecords == SetPermissions.AddRecords.No && model.RecruitApprentices == SetPermissions.RecruitApprentices.No);
    }
}