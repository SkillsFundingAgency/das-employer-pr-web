using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Employer.PR.Web.Constants;

[ExcludeFromCodeCoverage]
public static class SetPermissions
{
    public static class AddRecords
    {
        public const string Yes = "Yes";
        public const string No = "No";
    }

    public static class RecruitApprentices
    {
        public const string Yes = "Yes";
        public const string YesWithReview = "YesWithReview";
        public const string No = "No";
    }
}
