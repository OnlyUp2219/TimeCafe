global using System.Net;
global using System.Net.Http.Json;
global using System.Text.Json;

global using Billing.TimeCafe.API;
global using Billing.TimeCafe.Application.CQRS.Balances.Commands;
global using Billing.TimeCafe.Application.CQRS.Balances.Queries;
global using Billing.TimeCafe.Application.CQRS.Payments.Commands;
global using Billing.TimeCafe.Application.CQRS.Payments.Queries;
global using Billing.TimeCafe.Application.CQRS.Transactions.Queries;
global using Billing.TimeCafe.Application.Services.Payments;
global using Billing.TimeCafe.Domain.Models;
global using Billing.TimeCafe.Domain.Enums;
global using Billing.TimeCafe.Infrastructure;
global using Billing.TimeCafe.Infrastructure.Data;

global using FluentAssertions;

global using MassTransit;

global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.AspNetCore.TestHost;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Caching.Distributed;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;

global using StackExchange.Redis;

global using Xunit;
