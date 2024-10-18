﻿using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Constants;

namespace SFA.DAS.Employer.PR.Web.Models.Requests;

public class EmployerAccountCreationModel : EmployerAccountCreationSubmitModel
{
    public Guid RequestId { get; set; }

    public long Ukprn { get; set; }
    public required string ProviderName { get; set; }

    public string? EmployerOrganisationName { get; set; }

    public string? EmployerPAYE { get; set; }
    public string? EmployerAORN { get; set; }
    public Operation[] Operations { get; set; } = [];

    public List<string> OperationDescriptions
    {
        get
        {
            var operationDescriptions = new List<string>();

            foreach (var operation in Operations)
            {
                switch (operation)
                {
                    case Operation.CreateCohort:
                        operationDescriptions.Add(PermissionDescriptions.AddApprenticeRecords);
                        break;
                    case Operation.Recruitment:
                        operationDescriptions.Add(PermissionDescriptions.RecruitApprentices);
                        break;
                    case Operation.RecruitmentRequiresReview:
                        operationDescriptions.Add(PermissionDescriptions.RecruitApprenticesWithReview);
                        break;
                }
            }
            return operationDescriptions;
        }
    }
}

public class EmployerAccountCreationSubmitModel
{
    public required bool HasAcceptedTerms { get; set; }
    public string? EmployerContactFirstName { get; set; }
    public string? EmployerContactLastName { get; set; }
}
