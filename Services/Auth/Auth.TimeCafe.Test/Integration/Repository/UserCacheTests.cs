using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using Auth.TimeCafe.Test.Integration.Helpers;

namespace Auth.TimeCafe.Test.Integration.Repository;

public class UserCacheTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Cache_ShouldBeInvalidated_OnRegisterUser()
    {
        // 0. Логин админа
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Auth.TimeCafe.Domain.Models.ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole<Guid>>>();
        
        var adminEmail = $"admin_{Guid.NewGuid():N}@example.com";
        var adminUser = new Auth.TimeCafe.Domain.Models.ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        await userManager.CreateAsync(adminUser, "P@ssw0rd!");

        var adminRole = "Admin";
        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole<Guid>(adminRole));
            // В идеале мы должны добавить claim Permission = AccountAdminRead к роли, но для теста может быть проще
            // просто добавить claim напрямую пользователю.
        }
        await userManager.AddToRoleAsync(adminUser, adminRole);
        // Добавим claim напрямую
        await userManager.AddClaimAsync(adminUser, new System.Security.Claims.Claim(BuildingBlocks.Permissions.CustomClaimTypes.Permissions, BuildingBlocks.Permissions.Permissions.AccountAdminRead));

        var loginPayload = new { Email = adminEmail, Password = "P@ssw0rd!" };
        var loginResp = await Client.PostAsJsonAsync("/auth/login-jwt", loginPayload);
        loginResp.EnsureSuccessStatusCode();
        var loginJsonStr = await loginResp.Content.ReadAsStringAsync();
        var adminToken = JsonDocument.Parse(loginJsonStr).RootElement.GetProperty("accessToken").GetString();
        
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/auth/admin/users?page=1&size=20");
        requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // 1. Получить список пользователей (Initial GET)
        var initialGetResponse = await Client.SendAsync(requestMessage);
        var initialJsonStr = await initialGetResponse.Content.ReadAsStringAsync();
        
        if (initialGetResponse.StatusCode == HttpStatusCode.InternalServerError)
        {
            throw new Exception($"Internal Server Error: {initialJsonStr}");
        }
        
        initialGetResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        var initialCount = 0;
        
        if (initialGetResponse.IsSuccessStatusCode)
        {
            try
            {
                var root = JsonDocument.Parse(initialJsonStr).RootElement;
                if (root.TryGetProperty("value", out var valueElem))
                {
                    root = valueElem;
                }
                var items = root.GetProperty("items").EnumerateArray().ToList();
                initialCount = items.Count;
            }
            catch(Exception)
            {
                throw new Exception($"Failed to parse initial GET: {initialJsonStr}");
            }
        }

        // 2. Добавление пользователя (POST)
        var newUsername = $"testuser_{Guid.NewGuid():N}";
        var newEmail = $"{newUsername}@example.com";
        var createPayload = new
        {
            username = newUsername,
            email = newEmail,
            password = "Password123!"
        };

        var createResponse = await Client.PostAsJsonAsync("/auth/registerWithUsername-mock", createPayload);
        var createResponseStr = await createResponse.Content.ReadAsStringAsync();
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK, $"Create response failed: {createResponseStr}");

        // 3. Получить данные (GET after POST)
        var requestMessageAfterCreate = new HttpRequestMessage(HttpMethod.Get, "/auth/admin/users?page=1&size=20");
        requestMessageAfterCreate.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        var getAfterCreateResponse = await Client.SendAsync(requestMessageAfterCreate);
        getAfterCreateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getAfterCreateJsonStr = await getAfterCreateResponse.Content.ReadAsStringAsync();
        var rootAfterCreate = JsonDocument.Parse(getAfterCreateJsonStr).RootElement;
        if (rootAfterCreate.TryGetProperty("value", out var valElem))
        {
            rootAfterCreate = valElem;
        }
        var itemsAfterCreate = rootAfterCreate.GetProperty("items").EnumerateArray().ToList();
        
        itemsAfterCreate.Count.Should().BeGreaterThan(initialCount, $"Новый пользователь должен появиться в списке, кэш инвалидирован. Получен JSON: {getAfterCreateJsonStr}");
        var createdItem = itemsAfterCreate.FirstOrDefault(x => x.GetProperty("email").GetString() == newEmail);
        createdItem.ValueKind.Should().NotBe(JsonValueKind.Undefined);
    }
}
