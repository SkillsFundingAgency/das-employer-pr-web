using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Employer.PR.Domain.Models;

[ExcludeFromCodeCoverage]
public class RegisteredProvider
{
    public string Name { get; set; } = null!;
    public int Ukprn { get; set; }
}