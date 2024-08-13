using SFA.DAS.Employer.PR.Domain.Models;

namespace SFA.DAS.Employer.PR.Domain.OuterApi.Permissions;
public class PostPermissionsCommand
{
    public Guid UserRef { get; }

    public long Ukprn { get; }

    public long AccountLegalEntityId { get; }

    public List<Operation> Operations { get; }

    public PostPermissionsCommand(Guid userRef, long ukprn, long accountLegalEntityId, List<Operation> operations)
    {
        UserRef = userRef;
        Ukprn = ukprn;
        AccountLegalEntityId = accountLegalEntityId;
        Operations = operations;
    }
}