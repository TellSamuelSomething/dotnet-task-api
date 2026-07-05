using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TaskAPI.DTOs;

namespace TaskAPI.Tests;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTasks_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/tasks");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest
        {
            Username = "admin",
            Password = "password123"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(result?.Token);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest
        {
            Username = "wrong",
            Password = "wrong"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateTask_WithToken_Returns201()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/v1/tasks", new CreateTaskRequest
        {
            Title = "Integration Test Task"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private async Task<string> GetTokenAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest
        {
            Username = "admin",
            Password = "password123"
        });
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return result!.Token;
    }
}
