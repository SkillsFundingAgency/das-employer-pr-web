﻿using SFA.DAS.Employer.PR.Domain.Interfaces;

namespace SFA.DAS.Employer.PR.Web.Models;

public sealed class ReviewPermissionsRequestViewModel : ReviewPermissionsRequestSubmitViewModel, IReviewRequest
{
    public required string ProviderName { get; set; }
    public string? AddApprenticeRecordsText { get; set; }
    public string? RecruitApprenticesText { get; set; }
    public required string ViewYourTrainingProvidersLink { get; set; }
}

public class ReviewPermissionsRequestSubmitViewModel : PermissionDescriptionsModel
{
    public bool? AcceptPermissions { get; set; }
}
