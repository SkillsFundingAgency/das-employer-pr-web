﻿using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Employer.PR.Web.Infrastructure.Configuration;

[ExcludeFromCodeCoverage]
public class ApplicationSettings
{
    public string? RedisConnectionString { get; set; }
    public string? DataProtectionKeysDatabase { get; set; }
}