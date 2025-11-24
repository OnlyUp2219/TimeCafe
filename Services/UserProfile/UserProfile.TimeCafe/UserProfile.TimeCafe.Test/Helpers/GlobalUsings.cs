global using System.Text.Json;

global using FluentAssertions;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Caching.Distributed;
global using Microsoft.Extensions.Logging;

global using Moq;

global using UserProfile.TimeCafe.Application.CQRS.Profiles.Commands;
global using UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;
global using UserProfile.TimeCafe.Domain.Constants;
global using UserProfile.TimeCafe.Domain.Contracts;
global using UserProfile.TimeCafe.Domain.Models;
global using UserProfile.TimeCafe.Infrastructure.Data;
global using UserProfile.TimeCafe.Infrastructure.Repositories;
global using UserProfile.TimeCafe.Test.Helpers;

global using Xunit;
