global using Auth.TimeCafe.Application.Contracts;
global using Auth.TimeCafe.Application.DTOs;
global using Auth.TimeCafe.Domain.Contracts;
global using Auth.TimeCafe.Domain.Models;
global using Auth.TimeCafe.Domain.Permissions;

global using BuildingBlocks.Behaviors;
global using BuildingBlocks.Extensions;
global using MassTransit;
global using FluentValidation;
global using BuildingBlocks.Events;

global using MediatR;

global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.WebUtilities;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

global using System.Reflection;
global using System.Security.Claims;
global using System.Text;
