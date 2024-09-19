namespace SFA.DAS.Employer.PR.Domain.OuterApi.Responses;

public record GetAccountLegalEntitiesResponse(List<AccountLegalEntity> LegalEntities);

public record AccountLegalEntity(string AccountLegalEntityName, string AccountLegalEntityPublicHashedId, long AccountLegalEntityId);
