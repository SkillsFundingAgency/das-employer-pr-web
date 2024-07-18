﻿using AutoFixture.NUnit3;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Models;
public class PermissionsShutterPageViewModelTests
{
    [Test, AutoData]
    public void Constructor_BuildsViewModel(string providerName, long ukprn, long legalEntityId, string setPermissionsLink, string returnToYourTrainingProvidersLink)
    {
        var sut = new AddPermissionsShutterPageViewModel(providerName, ukprn, legalEntityId, setPermissionsLink, returnToYourTrainingProvidersLink);
        sut.ProviderName.Should().Be(providerName);
        sut.Ukprn.Should().Be(ukprn);
        sut.LegalEntityId.Should().Be(legalEntityId);
        sut.ChangePermissionsLink.Should().Be(setPermissionsLink);
        sut.ReturnToYourTrainingProvidersLink.Should().Be(returnToYourTrainingProvidersLink);
    }
}
