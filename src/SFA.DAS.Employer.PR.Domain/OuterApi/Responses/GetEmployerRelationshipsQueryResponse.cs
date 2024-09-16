using SFA.DAS.Employer.PR.Domain.Models;

namespace SFA.DAS.Employer.PR.Domain.OuterApi.Responses;

public record GetEmployerRelationshipsQueryResponse(List<LegalEntity> AccountLegalEntities);