global using Auth.TimeCafe.Application.Contracts;
global using Auth.TimeCafe.Application.DTO;
global using Auth.TimeCafe.Application.Permissions;
global using Auth.TimeCafe.Domain.Contracts;
global using Auth.TimeCafe.Domain.Models;
global using Auth.TimeCafe.Domain.Permissions;
global using Auth.TimeCafe.Infrastructure.Data;
global using Auth.TimeCafe.Infrastructure.Services;

global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Infrastructure;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.IdentityModel.Tokens;

global using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

global using System.IdentityModel.Tokens.Jwt;
global using System.Net.Http.Headers;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.Text;
global using System.Text.Json;
