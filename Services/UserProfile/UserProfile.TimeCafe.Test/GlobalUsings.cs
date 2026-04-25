global using System.Net;
global using System.Net.Http.Json;
global using FluentAssertions;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Moq;
global using UserProfile.TimeCafe.Application.CQRS.AdditionalInfos.Commands;
global using UserProfile.TimeCafe.Application.CQRS.AdditionalInfos.Queries;
global using UserProfile.TimeCafe.Application.CQRS.Photos.Commands;
global using UserProfile.TimeCafe.Application.CQRS.Photos.Queries;
global using UserProfile.TimeCafe.Application.CQRS.Profiles.Commands;
global using UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;
global using UserProfile.TimeCafe.Domain.Contracts;
global using UserProfile.TimeCafe.Domain.Models;
global using UserProfile.TimeCafe.Infrastructure.Repositories;
global using UserProfile.TimeCafe.Test.Integration.Helpers;
global using Xunit;
global using static UserProfile.TimeCafe.Test.Integration.Helpers.TestData;

