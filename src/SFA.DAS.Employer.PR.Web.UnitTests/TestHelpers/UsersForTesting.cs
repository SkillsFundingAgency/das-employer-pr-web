﻿using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Authentication;
using System.Text.Json;

namespace SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
public static class UsersForTesting
{
    public static ClaimsPrincipal GetUserWithClaims(string employerAccountId, EmployerUserRole? roleToUse)
    {
        var familyName = "validFamilyName";
        var givenName = "validGivenName";
        var familyNameClaim = new Claim(EmployerClaims.FamilyName, familyName);
        var givenNameClaim = new Claim(EmployerClaims.GivenName, givenName);
        var nameClaim = new Claim(EmployerClaims.UserDisplayNameClaimTypeIdentifier, $"{givenName} {familyName}");

        var emailClaim = new Claim(ClaimTypes.Email, "valid_email");
        var userIdClaimTypeIdentifier = new Claim(EmployerClaims.UserIdClaimTypeIdentifier, Guid.NewGuid().ToString());

        var role = roleToUse != null ? roleToUse.ToString() : "role";

        EmployerIdentifier employerIdentifier = new(employerAccountId.ToString(), "das_account_name", role!);
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifier } };

        var accountsClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonSerializer.Serialize(employerAccounts));

        ClaimsPrincipal claimsPrincipal = new(new ClaimsIdentity[1]
        {
            new(new Claim[6]
            {
                givenNameClaim,
                familyNameClaim,
                nameClaim,
                emailClaim,
                userIdClaimTypeIdentifier,
                accountsClaim,

            }, "Test")
        });

        return claimsPrincipal;
    }
}
