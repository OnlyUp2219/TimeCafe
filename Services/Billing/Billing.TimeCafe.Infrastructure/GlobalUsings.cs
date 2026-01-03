global using Billing.TimeCafe.Application.Services.Payments;
global using Billing.TimeCafe.Domain.Constants;
global using Billing.TimeCafe.Domain.Enums;
global using Billing.TimeCafe.Domain.Models;
global using Billing.TimeCafe.Domain.Repositories;
global using Billing.TimeCafe.Infrastructure.Consumers;
global using Billing.TimeCafe.Infrastructure.Data;
global using Billing.TimeCafe.Infrastructure.Repositories;
global using Billing.TimeCafe.Infrastructure.Services.Stripe;

global using BuildingBlocks.Events;
global using BuildingBlocks.Helpers;

global using MassTransit;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Caching.Distributed;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
