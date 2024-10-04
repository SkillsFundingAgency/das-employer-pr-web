using SFA.DAS.Employer.PR.Domain.Common;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Constants;

namespace SFA.DAS.Employer.PR.Web.Helpers;

public static class ReviewRequestHelper
{
    public static bool IsValidRequest(GetPermissionRequestResponse? response, RequestType requestType)
    {
        return response != null &&
               response.RequestType == requestType &&
              (response!.Status == RequestStatus.New || response.Status == RequestStatus.Sent);
    }

    public static void MapOperationsToDescriptions<T>(ref T model, Operation[] operations) where T : IReviewRequest
    {
        model.AddApprenticeRecordsText = operations.Contains(Operation.CreateCohort) ? ReviewRequest.Yes : ReviewRequest.No;
        model.RecruitApprenticesText = operations.Contains(Operation.Recruitment)
            ? ReviewRequest.Yes
            : SetRecruitRequiresReviewText(operations);
    }

    private static string SetRecruitRequiresReviewText(Operation[] operations)
    {
        return operations.Contains(Operation.RecruitmentRequiresReview)
                ? ReviewRequest.YesWithEmployerReview
                : ReviewRequest.No;
    }
}
