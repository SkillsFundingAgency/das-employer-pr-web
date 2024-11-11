using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Constants;

namespace SFA.DAS.Employer.PR.Web.Models.Requests;

public class EmployerAccountCreationViewModel : EmployerAccountCreationSubmitModel
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

    public string? ChangeNameLink { get; set; }
    public string? DeclineCreateAccountLink { get; set; }

    public string EmployerAgreementLink { get; set; } = null!;
}

public class EmployerAccountCreationSubmitModel : EmployerUserNamesBase
{
    public required bool HasAcceptedTerms { get; set; }
}

public class EmployerUserNamesViewModel : EmployerUserNamesBase
{
}

public class EmployerAccountNamesSubmitModel : EmployerUserNamesBase
{
}

public class EmployerUserNamesBase
{
    public string? EmployerContactFirstName { get; set; }
    public string? EmployerContactLastName { get; set; }
}
