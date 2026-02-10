global using Billing.TimeCafe.API;
global using Billing.TimeCafe.Application.CQRS.Balances.Commands;
global using Billing.TimeCafe.Application.CQRS.Balances.Queries;
global using Billing.TimeCafe.Application.CQRS.Payments.Commands;
global using Billing.TimeCafe.Application.CQRS.Payments.Queries;
global using Billing.TimeCafe.Application.CQRS.Transactions.Queries;
global using Billing.TimeCafe.Application.Services.Payments;
global using Billing.TimeCafe.Domain.Constants;
global using Billing.TimeCafe.Domain.Enums;
global using Billing.TimeCafe.Domain.Models;
global using Billing.TimeCafe.Domain.Repositories;
global using Billing.TimeCafe.Infrastructure;
global using Billing.TimeCafe.Infrastructure.Data;
global using Billing.TimeCafe.Test.Integration;
global using Billing.TimeCafe.Test.TestData;

global using BuildingBlocks.Exceptions;

global using FluentAssertions;

global using FluentValidation;

global using MassTransit;

global using MediatR;

global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.AspNetCore.TestHost;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Caching.Distributed;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Diagnostics.HealthChecks;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Options;

global using StackExchange.Redis;

global using System.Net;
global using System.Net.Http.Json;
global using System.Text.Json;

global using Xunit;

global using static Billing.TimeCafe.Test.TestData.DefaultsGuid;
global using static Billing.TimeCafe.Test.TestData.InvalidDataGuid;

global using BalanceModel = Billing.TimeCafe.Domain.Models.Balance;
global using PaymentModel = Billing.TimeCafe.Domain.Models.Payment;
global using TransactionModel = Billing.TimeCafe.Domain.Models.Transaction;
