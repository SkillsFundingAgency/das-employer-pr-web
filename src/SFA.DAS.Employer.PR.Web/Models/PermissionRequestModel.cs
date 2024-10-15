using SFA.DAS.Employer.PR.Domain.Common;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Constants;

namespace SFA.DAS.Employer.PR.Web.Models;

public class PermissionRequestModel : PermissionDetailsModel
{
    public required Guid RequestId { get; set; }
    public required RequestType RequestType { get; set; }
}
