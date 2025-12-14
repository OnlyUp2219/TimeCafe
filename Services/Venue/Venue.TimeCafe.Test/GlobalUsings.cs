global using FluentAssertions;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Caching.Distributed;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;

global using Moq;

global using System.Net;
global using System.Net.Http.Json;
global using System.Text.Json;

global using Venue.TimeCafe.Application.CQRS.Promotions.Commands;
global using Venue.TimeCafe.Application.CQRS.Promotions.Queries;
global using Venue.TimeCafe.Application.CQRS.Tariffs.Commands;
global using Venue.TimeCafe.Application.CQRS.Tariffs.Queries;
global using Venue.TimeCafe.Application.CQRS.Themes.Commands;
global using Venue.TimeCafe.Application.CQRS.Themes.Queries;
global using Venue.TimeCafe.Application.CQRS.Visits.Commands;
global using Venue.TimeCafe.Application.CQRS.Visits.Queries;
global using Venue.TimeCafe.Domain.Contracts.Repositories;
global using Venue.TimeCafe.Domain.Models;
global using Venue.TimeCafe.Infrastructure.Data;
global using Venue.TimeCafe.Infrastructure.Repositories;
global using Venue.TimeCafe.Test.Helpers;
global using Venue.TimeCafe.Test.Integration.Helpers;

global using Xunit;
