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
    public async Task Register_WithValidDetails_ReturnsToken()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest
        {
            Username = $"user_{Guid.NewGuid():N}",
            Password = "password123"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(result?.Token);
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_Returns409()
    {
        var username = $"user_{Guid.NewGuid():N}";

        await _client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest
        {
            Username = username,
            Password = "password123"
        });

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest
        {
            Username = username,
            Password = "password123"
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest
        {
            Username = "nonexistent",
            Password = "wrong"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateTask_WithToken_Returns201()
    {
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/v1/tasks", new CreateTaskRequest
        {
            Title = "Integration Test Task"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateTask_WithPriorityAndDueDate_ReturnsCorrectValues()
    {
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var dueDate = DateTime.UtcNow.AddDays(7);
        var response = await _client.PostAsJsonAsync("/api/v1/tasks", new CreateTaskRequest
        {
            Title = "High Priority Task",
            Priority = Models.Priority.High,
            DueDate = dueDate
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.Equal(Models.Priority.High, result?.Priority);
        Assert.NotNull(result?.DueDate);
    }

    private async Task<string> RegisterAndGetTokenAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest
        {
            Username = $"user_{Guid.NewGuid():N}",
            Password = "password123"
        });
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return result!.Token;
    }
}
