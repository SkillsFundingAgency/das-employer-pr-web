﻿using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Models;

public sealed class ProviderRequestModelTests
{
    [Test]
    [MoqAutoData]
    public void Operator_ConvertsTo_ProviderRequestModel(ProviderRequest request)
    {
        ProviderRequestModel sut = request;

        Assert.Multiple(() =>
        {
            Assert.That(sut.Ukprn, Is.EqualTo(request.Ukprn));
            Assert.That(sut.RequestId, Is.EqualTo(request.RequestId));
            Assert.That(sut.Operations.Count, Is.EqualTo(request.Operations.Count()));
        });        
    }
}
