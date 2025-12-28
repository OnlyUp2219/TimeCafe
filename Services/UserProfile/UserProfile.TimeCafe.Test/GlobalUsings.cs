global using UserProfile.TimeCafe.Application.CQRS.Profiles.Commands;
global using UserProfile.TimeCafe.Infrastructure.Repositories;
global using UserProfile.TimeCafe.Test.Integration.Helpers;

global using BuildingBlocks.Authorization;

global using Microsoft.Extensions.DependencyInjection;

global using FluentAssertions;
global using Moq;
global using Xunit;

global using System.Net;
global using System.Net.Http.Json;

global using static UserProfile.TimeCafe.Test.Integration.Helpers.TestData;