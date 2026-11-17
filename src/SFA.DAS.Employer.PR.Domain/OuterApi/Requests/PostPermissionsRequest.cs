using SFA.DAS.Employer.PR.Domain.Models;

namespace SFA.DAS.Employer.PR.Domain.OuterApi.Requests;
public class PostPermissionsRequest
{
    public Guid UserRef { get; }

    public long Ukprn { get; }

    public long AccountLegalEntityId { get; }

    public List<Operation> Operations { get; }
    public string Email { get; }

    public PostPermissionsRequest(Guid userRef, long ukprn, long accountLegalEntityId, List<Operation> operations, string email)
    {
        UserRef = userRef;
        Ukprn = ukprn;
        AccountLegalEntityId = accountLegalEntityId;
        Operations = operations;
        Email = email;
    }
}