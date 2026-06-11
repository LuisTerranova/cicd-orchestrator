using Orchestrator.Api.Extensions;
using Orchestrator.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Orchestrator.Infrastructure.Persistence;
using Orchestrator.Infrastructure.Auth;

namespace Orchestrator.Api.Endpoints;

public class AuthEndpoints : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/login", LoginAsync);
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        IRunnerTokenGenerator tokenGenerator,
        OrchestratorDbContext dbContext,
        CancellationToken ct
    )
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username, ct);

        if (user == null || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Results.Json(new { error = "Invalid credentials" }, statusCode: StatusCodes.Status401Unauthorized);
        }

        // Generate token using the User's Id
        var token = tokenGenerator.GenerateToken(user.Id);
        return Results.Ok(new LoginResponse(token));
    }

    public sealed record LoginRequest(string Username, string Password);
    public sealed record LoginResponse(string Token);
}
