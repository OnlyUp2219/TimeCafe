global using BuildingBlocks.Behaviors;
global using BuildingBlocks.Exceptions;
global using BuildingBlocks.Extensions;
global using BuildingBlocks.Validation;
global using BuildingBlocks.Contracts.CQRS;

global using FluentValidation;
global using FluentResults;

global using MediatR;

global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

global using System.Reflection;

global using UserProfile.TimeCafe.Application.Helpers;
global using UserProfile.TimeCafe.Domain.Contracts;
global using UserProfile.TimeCafe.Domain.Enums;
global using UserProfile.TimeCafe.Domain.Models;
global using UserProfile.TimeCafe.Application.CQRS.Photos.Commands;