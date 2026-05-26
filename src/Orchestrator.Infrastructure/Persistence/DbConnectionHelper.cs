using System;
using Npgsql;

namespace Orchestrator.Infrastructure.Persistence;

public static class DbConnectionHelper
{
    public static string FormatConnectionString(string connectionStringOrUri)
    {
        if (string.IsNullOrEmpty(connectionStringOrUri))
            return connectionStringOrUri;

        if (
            !connectionStringOrUri.StartsWith("postgresql://")
            && !connectionStringOrUri.StartsWith("postgres://")
        )
            return connectionStringOrUri;

        try
        {
            var uri = new Uri(connectionStringOrUri);
            var userInfo = uri.UserInfo.Split(':', 2);
            var username = userInfo[0];
            var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";

            var host = uri.Host;
            var port = uri.Port != -1 ? uri.Port : 5432;
            var database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = host,
                Port = port,
                Database = database,
                Username = username,
                Password = password,
            };

            // Parse query parameters (e.g. sslmode)
            var query = uri.Query;
            if (!string.IsNullOrEmpty(query))
            {
                var queryParams = query.TrimStart('?').Split('&');
                foreach (var param in queryParams)
                {
                    var kv = param.Split('=', 2);
                    if (kv.Length == 2)
                    {
                        var key = kv[0].Trim();
                        var val = Uri.UnescapeDataString(kv[1].Trim());

                        if (key.Equals("sslmode", StringComparison.OrdinalIgnoreCase))
                        {
                            if (val.Equals("require", StringComparison.OrdinalIgnoreCase))
                            {
                                builder.SslMode = SslMode.Require;
                            }
                            else if (val.Equals("disable", StringComparison.OrdinalIgnoreCase))
                            {
                                builder.SslMode = SslMode.Disable;
                            }
                            else if (val.Equals("prefer", StringComparison.OrdinalIgnoreCase))
                            {
                                builder.SslMode = SslMode.Prefer;
                            }
                            else if (val.Equals("allow", StringComparison.OrdinalIgnoreCase))
                            {
                                builder.SslMode = SslMode.Allow;
                            }
                            else if (val.Equals("verify-ca", StringComparison.OrdinalIgnoreCase))
                            {
                                builder.SslMode = SslMode.VerifyCA;
                            }
                            else if (val.Equals("verify-full", StringComparison.OrdinalIgnoreCase))
                            {
                                builder.SslMode = SslMode.VerifyFull;
                            }
                        }
                        else
                        {
                            builder[key] = val;
                        }
                    }
                }
            }

            return builder.ToString();
        }
        catch
        {
            return connectionStringOrUri; // Fallback to original connection string if parsing fails
        }
    }
}
