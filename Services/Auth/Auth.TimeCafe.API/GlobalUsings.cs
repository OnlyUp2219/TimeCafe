global using Auth.TimeCafe.API.Extensions;
global using Auth.TimeCafe.API.Services;
global using Auth.TimeCafe.Application;
global using Auth.TimeCafe.Application.DTO;
global using Auth.TimeCafe.Domain.Events;
global using Auth.TimeCafe.Domain.Models;
global using Auth.TimeCafe.Domain.Services;
global using Auth.TimeCafe.Domain.Contracts;
global using Auth.TimeCafe.Infrastructure.Data;
global using Auth.TimeCafe.Infrastructure.Services;

global using BuildingBlocks.Middleware;

global using Carter;

global using MassTransit;

global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Logging;
global using Microsoft.IdentityModel.Tokens;

global using Swashbuckle.AspNetCore.Filters;

global using System.Text;
global using Microsoft.AspNetCore.Authentication;

global using System.Security.Claims;

global using Auth.TimeCafe.Application.CQRS.Sender.Queries;

global using MediatR;
