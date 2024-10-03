using SFA.DAS.Employer.PR.Domain.Models;

namespace SFA.DAS.Employer.PR.Web.Models;

public class PermissionRequestModel
{
    public long Ukprn { get; set; }
    public Guid RequestId { get; set; }
    public Operation[] Operations { get; set; } = [];

    public static implicit operator PermissionRequestModel(PermissionRequest source)
    {
        return new PermissionRequestModel()
        {
            Ukprn = source.Ukprn,
            RequestId = source.RequestId,
            Operations = source.Operations
        };
    }
}
