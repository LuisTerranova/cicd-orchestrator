using Microsoft.EntityFrameworkCore;
using Orchestrator.Domain.Entities;

namespace Orchestrator.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(OrchestratorDbContext db)
    {
        if (!await db.Users.AnyAsync())
        {
            var adminUser = User.Create("admin", Orchestrator.Infrastructure.Auth.PasswordHasher.HashPassword("admin"), "Admin");
            await db.Users.AddAsync(adminUser);
        }

        if (await db.Pipelines.AnyAsync())
        {
            await db.SaveChangesAsync();
            return;
        }

        // 1. Seed default Pipeline
        var pipeline = Pipeline.Create(
            "Demo Web App CI",
            "https://github.com/luis-terranova/demo-web-app",
            "main",
            "orchestrator.yml"
        );
        pipeline.UpdateYaml(@"name: ""Demo Web Pipeline""
env:
  ENVIRONMENT: ""development""
stages:
  - name: ""build""
    image: ""ubuntu:latest""
    steps:
      - name: ""Compile Code""
        run: ""echo 'compiling...' && sleep 2""
  - name: ""test""
    image: ""ubuntu:latest""
    depends_on: [""build""]
    steps:
      - name: ""Run Tests""
        run: ""echo 'running tests...' && sleep 2""
  - name: ""deploy""
    image: ""ubuntu:latest""
    depends_on: [""test""]
    steps:
      - name: ""Publish""
        run: ""echo 'deploying...' && sleep 2""
");
        await db.Pipelines.AddAsync(pipeline);

        // 2. Seed default Runner
        var runner = Runner.Create(
            "dev-runner-01",
            ["ubuntu", "docker"],
            "linux",
            "amd64"
        );
        runner.Register(); // This sets status to Idle and sets LastSeen = UtcNow
        await db.Runners.AddAsync(runner);

        await db.SaveChangesAsync();
    }
}
