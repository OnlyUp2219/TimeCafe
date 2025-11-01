global using Auth.TimeCafe.API.Extensions;
global using Auth.TimeCafe.API.Services;
global using Auth.TimeCafe.Application;
global using Auth.TimeCafe.Application.CQRS.Sender.Commands;
global using Auth.TimeCafe.Application.DTO;
global using Auth.TimeCafe.Domain.Contracts;
global using Auth.TimeCafe.Domain.Events;
global using Auth.TimeCafe.Domain.Models;
global using Auth.TimeCafe.Domain.Services;
global using Auth.TimeCafe.Infrastructure.Data;
global using Auth.TimeCafe.Infrastructure.Data.Repositories;
global using Auth.TimeCafe.Infrastructure.Services;

global using BuildingBlocks.Extensions;
global using BuildingBlocks.Middleware;
global using Auth.TimeCafe.Infrastructure.Services.Phone;

global using Carter;

global using MassTransit;

global using MediatR;

global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.WebUtilities;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.IdentityModel.Tokens;

global using Swashbuckle.AspNetCore.Filters;
global using Auth.TimeCafe.Infrastructure.Services.Email;
global using System.Security.Claims;
global using System.Text;
