using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Employer.PR.Web.Constants;

[ExcludeFromCodeCoverage]
public static class TempDataKeys
{
    public const string NameOfProviderAdded = nameof(NameOfProviderAdded);
    public const string NameOfProviderUpdated = nameof(NameOfProviderUpdated);
    public const string RequestTypeActioned = nameof(RequestTypeActioned);
    public const string RequestAction = nameof(RequestAction);
    public const string RequestDeclinedConfirmation = nameof(RequestDeclinedConfirmation);
    public const string ValidRequest = nameof(ValidRequest);
}
