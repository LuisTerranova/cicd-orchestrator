using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Orchestrator.Infrastructure.Persistence;

public class OrchestratorDbContextFactory : IDesignTimeDbContextFactory<OrchestratorDbContext>
{
    public OrchestratorDbContext CreateDbContext(string[] args)
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")))
        {
            var dir = Directory.GetCurrentDirectory();
            while (dir != null)
            {
                var envPath = Path.Combine(dir, ".env");
                if (File.Exists(envPath))
                {
                    try
                    {
                        foreach (var line in File.ReadLines(envPath))
                        {
                            var trimmed = line.Trim();
                            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                                continue;
                            var parts = trimmed.Split('=', 2);
                            if (parts.Length == 2)
                            {
                                var key = parts[0].Trim();
                                var val = parts[1].Trim().Trim('"').Trim('\'');
                                Environment.SetEnvironmentVariable(key, val);
                            }
                        }
                    }
                    catch
                    {
                        // Ignore exceptions reading .env at design time
                    }
                    break;
                }
                dir = Directory.GetParent(dir)?.FullName;
            }
        }

        var connStr =
            Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
            ?? "Host=localhost;Database=orchestrator;Username=postgres;Password=postgres;SSL Mode=Disable";

        connStr = DbConnectionHelper.FormatConnectionString(connStr);

        var options = new DbContextOptionsBuilder<OrchestratorDbContext>()
            .UseNpgsql(connStr)
            .Options;

        return new OrchestratorDbContext(options);
    }
}
