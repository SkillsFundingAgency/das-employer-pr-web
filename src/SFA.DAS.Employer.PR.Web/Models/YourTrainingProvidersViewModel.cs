using SFA.DAS.Employer.PR.Domain.Models;

namespace SFA.DAS.Employer.PR.Web.Models;

public class YourTrainingProvidersViewModel
{
    public bool IsOwner { get; set; }

    public List<LegalEntityModel> LegalEntities { get; set; } = [];

    public static implicit operator YourTrainingProvidersViewModel(List<AccountLegalEntity> legalEntities)
    {
        var model = new YourTrainingProvidersViewModel();
        foreach (var legalEntity in legalEntities)
        {
            model.LegalEntities.Add(legalEntity);
        }

        return model;
    }
}