namespace SFA.DAS.Employer.PR.Web.Models;

public class SelectTrainingProviderModel : SelectTrainingProviderSubmitModel, IBackLink
{
    public string BackLink { get; set; }

    public SelectTrainingProviderModel(string backLink, string? name, string? ukprn)
    {
        BackLink = backLink;
        Name = name;
        Ukprn = ukprn;
    }
}

public class SelectTrainingProviderSubmitModel
{
    public string? SearchTerm { get; set; }
    public string? Name { get; set; }
    public string? Ukprn { get; set; }
}