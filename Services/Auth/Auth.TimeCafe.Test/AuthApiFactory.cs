using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Auth.TimeCafe.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Auth.TimeCafe.Test;

public class AuthApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((ctx, cfg) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["Kestrel:Endpoints:Https:Certificate:Path"] = string.Empty,
                ["Kestrel:Endpoints:Https:Certificate:Password"] = string.Empty,
                ["ConnectionStrings:DefaultConnection"] = string.Empty 
            };
            cfg.AddInMemoryCollection(overrides);
        });

        builder.ConfigureTestServices(services =>
        {
            var dbContextOptionsDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IDbContextOptionsConfiguration<ApplicationDbContext>));

            if (dbContextOptionsDescriptor != null)
            {
                services.Remove(dbContextOptionsDescriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("AuthTestsDb"));
        });
    }
}
