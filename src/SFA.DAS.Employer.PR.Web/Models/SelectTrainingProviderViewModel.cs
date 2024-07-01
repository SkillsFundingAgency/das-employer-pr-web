namespace SFA.DAS.Employer.PR.Web.Models;

public class SelectTrainingProviderViewModel : SelectTrainingProviderSubmitViewModel, IBackLink
{
    public string BackLink { get; set; }

    public SelectTrainingProviderViewModel(string backLink, string? name, string? ukprn)
    {
        BackLink = backLink;
        Name = name;
        Ukprn = ukprn;
    }
}

public class SelectTrainingProviderSubmitViewModel
{
    public string? SearchTerm { get; set; }
    public string? Name { get; set; }
    public string? Ukprn { get; set; }
}