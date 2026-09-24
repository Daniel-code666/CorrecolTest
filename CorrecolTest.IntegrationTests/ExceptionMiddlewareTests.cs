using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CorrecolTest.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace CorrecolTest.IntegrationTests;

public class ExceptionMiddlewareTests
{
    [Fact]
    public async Task UnexpectedExceptionReturnsSafeProblemDetails()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddProblemDetails();
        await using var app = builder.Build();
        app.UseMiddleware<ExceptionMiddleware>();
        app.Run(_ => throw new InvalidOperationException("internal-secret-connection-string"));
        await app.StartAsync();
        using var client = app.GetTestClient();
        var response = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        var text = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("internal-secret", text);
        var problem = JsonDocument.Parse(text);
        Assert.Equal(500, problem.RootElement.GetProperty("status").GetInt32());
        Assert.True(problem.RootElement.TryGetProperty("traceId", out _));
    }
}
