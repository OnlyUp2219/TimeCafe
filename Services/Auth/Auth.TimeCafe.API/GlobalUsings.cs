global using Auth.TimeCafe.API.DTOs;
global using Auth.TimeCafe.API.Extensions;
global using Auth.TimeCafe.API.Middleware;
global using Auth.TimeCafe.API.Services;
global using Auth.TimeCafe.Application;
global using Auth.TimeCafe.Application.Contracts;
global using Auth.TimeCafe.Application.CQRS.Account.Commands;
global using Auth.TimeCafe.Application.CQRS.Auth.Commands;
global using Auth.TimeCafe.Application.Permissions;
global using Auth.TimeCafe.Domain.Contracts;
global using Auth.TimeCafe.Domain.Models;
global using Auth.TimeCafe.Domain.Permissions;
global using Auth.TimeCafe.Infrastructure.Data;
global using Auth.TimeCafe.Infrastructure.Permissions;
global using Auth.TimeCafe.Infrastructure.Services;
global using Auth.TimeCafe.Infrastructure.Services.Email;
global using Auth.TimeCafe.Infrastructure.Services.Phone;
global using BuildingBlocks.Events;
global using BuildingBlocks.Extensions;
global using BuildingBlocks.Middleware;

global using Carter;

global using MassTransit;

global using MediatR;

global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.Logging;
global using Microsoft.IdentityModel.Tokens;

global using Scalar.AspNetCore;

global using Serilog;

global using Swashbuckle.AspNetCore.Filters;

global using System.Security.Claims;
global using System.Text;
global using System.Threading.RateLimiting;
