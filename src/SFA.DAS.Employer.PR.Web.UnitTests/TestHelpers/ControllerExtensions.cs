﻿using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Authentication;
using System.Text.Json;

namespace SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
public static class ControllerExtensions
{
    public static Mock<IUrlHelper> AddUrlHelperMock(this Controller controller)
    {
        var urlHelperMock = new Mock<IUrlHelper>();
        controller.Url = urlHelperMock.Object;
        return urlHelperMock;
    }

    public static Mock<IUrlHelper> AddUrlForRoute(this Mock<IUrlHelper> urlHelperMock, string routeName, string url = TestConstants.DefaultUrl)
    {
        urlHelperMock
            .Setup(m => m.RouteUrl(It.Is<UrlRouteContext>(c => c.RouteName!.Equals(routeName))))
            .Returns(url);
        return urlHelperMock;
    }

    public static Controller AddDefaultContext(this Controller controller)
    {
        Fixture fixture = new();
        var employerIdentifier = fixture
            .Build<EmployerIdentifier>()
            .With(e => e.AccountId, TestConstants.DefaultAccountId)
            .With(e => e.EmployerName, TestConstants.DefaultAccountName)
            .Create();

        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifier } };
        var accountClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonSerializer.Serialize(employerAccounts));
        var emailClaim = new Claim(ClaimTypes.Email, fixture.Create<string>());
        var nameClaim = new Claim(ClaimTypes.NameIdentifier, fixture.Create<string>());

        var httpContext = new DefaultHttpContext();
        var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[]
        {
            accountClaim,
            emailClaim,
            nameClaim
        })});
        httpContext.User = claimsPrinciple;
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        return controller;
    }
}
