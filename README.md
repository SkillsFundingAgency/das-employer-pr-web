## ⛔Never push sensitive information such as client id's, secrets or keys into repositories including in the README file⛔

# Employer - Provider Relationship Web

<img src="https://avatars.githubusercontent.com/u/9841374?s=200&v=4" align="right" alt="UK Government logo">


[![Build Status](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_apis/build/status%2Fdas-employer-pr-web?repoName=SkillsFundingAgency%2Fdas-employer-pr-web&branchName=main)](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_build/latest?definitionId=3708&repoName=SkillsFundingAgency%2Fdas-employer-pr-web&branchName=main)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=SkillsFundingAgency_das-employer-pr-web&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=SkillsFundingAgency_das-employer-pr-web)
[![Confluence Project](https://img.shields.io/badge/Confluence-Project-blue)](https://skillsfundingagency.atlassian.net/wiki/spaces/NDL/pages/4368171030/Solution+Architecture+-+PR#Initial-view-of-solution-architecture-for--EP%2FPP)
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg?longCache=true&style=flat-square)](https://en.wikipedia.org/wiki/MIT_License)

## About

This is part of the employer portal. Here employer manages their relationship with the providers in form of permissions. 
There are two main permissions 
* Create cohort
* Create vacancy with or without employer's review

Also here the employer users manage permission requests raised by providers. 

## 🚀 Installation

### Pre-Requisites

* A clone of this repository
* A storage emulator like Azurite
* Visual studio 2022 or higher or similar IDE 
* .NetCore 8.0

### Dependencies

* Gov Signin for user authentication
* The Outer API [das-apim-endpoints](https://github.com/SkillsFundingAgency/das-apim-endpoints/tree/master/src/EmployerPR) should be available either running locally or accessible in an Azure tenancy.

### Config

Following config files are required to be loaded in local storage emulator
* [SFA.DAS.EmployerPR.Web](https://github.com/SkillsFundingAgency/das-employer-config/blob/master/das-employer-pr-web/SFA.DAS.EmployerPR.Web.json)
* [SFA.DAS.Employer.GovSignIn](https://github.com/SkillsFundingAgency/das-employer-config/blob/master/das-shared-config/SFA.DAS.Employer.GovSignIn.json)
* [SFA.DAS.Encoding](https://github.com/SkillsFundingAgency/das-employer-config/blob/master/das-shared-config/SFA.DAS.Encoding.json)


In the SFA.DAS.Employer.PR.Web project, if not exist already, add appSettings.Development.json file with following content:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConfigurationStorageConnectionString": "UseDevelopmentStorage=true;",
  "ConfigNames": "SFA.DAS.EmployerPR.Web,SFA.DAS.Employer.GovSignIn,SFA.DAS.Encoding",
  "EnvironmentName": "LOCAL",
  "Version": "1.0",
  "cdn": {
    "url": "https://das-at-frnt-end.azureedge.net"
  },
  "ResourceEnvironmentName": "LOCAL",
  "StubAuth": true,
  "EmployerAccountWebLocalUrl": "https://localhost:44344"
} 
```

## Technologies

* .NetCore 8.0
* Azure Table Storage
* NUnit
* Moq
* FluentAssertions
