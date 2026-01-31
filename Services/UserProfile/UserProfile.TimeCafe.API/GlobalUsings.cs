global using Amazon.S3;

global using BuildingBlocks.Authorization;

global using BuildingBlocks.Extensions;
global using BuildingBlocks.Middleware;

global using Carter;

global using MassTransit;

global using MediatR;

global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Options;

global using Scalar.AspNetCore;
global using Microsoft.OpenApi.Models;

global using Serilog;

global using Swashbuckle.AspNetCore.Filters;

global using UserProfile.TimeCafe.API.DTOs;
global using UserProfile.TimeCafe.API.Extensions;
global using UserProfile.TimeCafe.Application;
global using UserProfile.TimeCafe.Application.CQRS.AdditionalInfos.Commands;
global using UserProfile.TimeCafe.Application.CQRS.AdditionalInfos.Queries;
global using UserProfile.TimeCafe.Application.CQRS.Photos.Commands;
global using UserProfile.TimeCafe.Application.CQRS.Photos.Queries;
global using UserProfile.TimeCafe.Application.CQRS.Profiles.Commands;
global using UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;
global using UserProfile.TimeCafe.Domain.Contracts;
global using UserProfile.TimeCafe.Domain.Models;
global using UserProfile.TimeCafe.Infrastructure;
global using UserProfile.TimeCafe.Infrastructure.Data;
global using UserProfile.TimeCafe.Infrastructure.Services;
