global using Billing.TimeCafe.API.Auth;
global using Billing.TimeCafe.API.DTOs;
global using Billing.TimeCafe.API.Extensions;
global using Billing.TimeCafe.API.Filters;
global using Billing.TimeCafe.Application.CQRS.Balances.Commands;
global using Billing.TimeCafe.Application.CQRS.Balances.Queries;
global using Billing.TimeCafe.Application.CQRS.Payments.Commands;
global using Billing.TimeCafe.Application.CQRS.Transactions.Queries;
global using Billing.TimeCafe.Application.DependencyInjection;
global using Billing.TimeCafe.Application.Services.Payments;
global using Billing.TimeCafe.Domain.Enums;
global using Billing.TimeCafe.Infrastructure;
global using Billing.TimeCafe.Infrastructure.Data;

global using BuildingBlocks.Extensions;
global using BuildingBlocks.Middleware;

global using Carter;

global using MassTransit;

global using MediatR;

global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.OpenApi.Any;
global using Microsoft.OpenApi.Models;

global using Scalar.AspNetCore;

global using Serilog;

global using Swashbuckle.AspNetCore.Filters;
global using Swashbuckle.AspNetCore.SwaggerGen;

global using BuildingBlocks.Events;