using FluentValidation;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.Validators;

public class AddPermissionsValidator : AbstractValidator<AddPermissionsSubmitModel>
{
    public const string AddRecordsNotSelectedErrorMessage = "Select the permissions you want to set for Add apprentice records";
    public const string RecruitApprenticesNotSelectedErrorMessage = "Select the permissions you want to set for Recruit apprentices";
    public const string BothSelectionsAreNoErrorMessage = "You must select yes for at least one permission for add apprentice records or recruit apprentices";

    public AddPermissionsValidator()
    {
        RuleFor(s => s.PermissionToAddCohorts)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(AddRecordsNotSelectedErrorMessage)
            .Must(AddPermissionsBothNoFalse)
            .WithMessage(BothSelectionsAreNoErrorMessage);

        RuleFor(s => s.PermissionToRecruit)
            .NotEmpty()
            .WithMessage(RecruitApprenticesNotSelectedErrorMessage);
    }

    private static bool AddPermissionsBothNoFalse(AddPermissionsSubmitModel viewModel, string? addRecords)
    {
        return !(viewModel.PermissionToAddCohorts == ManageRequests.No && viewModel.PermissionToRecruit == ManageRequests.No);
    }
}