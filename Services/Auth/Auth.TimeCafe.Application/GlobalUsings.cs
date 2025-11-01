global using Auth.TimeCafe.Application.CQRS.Auth.Commands;
global using Auth.TimeCafe.Application.DTO;
global using Auth.TimeCafe.Domain.Contracts;
global using Auth.TimeCafe.Domain.Models;
global using Auth.TimeCafe.Application.Contracts;

global using BuildingBlocks.Behaviors;
global using BuildingBlocks.Common;
global using BuildingBlocks.Extensions;

global using FluentValidation;

global using MediatR;

global using System.Security.Claims;

global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Identity.UI.Services;
global using Microsoft.AspNetCore.WebUtilities;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

global using Swashbuckle.AspNetCore.Filters;

global using System.Reflection;
global using System.Text;
