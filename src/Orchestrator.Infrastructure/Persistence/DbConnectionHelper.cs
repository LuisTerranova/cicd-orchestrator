using System.Text.RegularExpressions;
using Npgsql;

namespace Orchestrator.Infrastructure.Persistence;

public static partial class DbConnectionHelper
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
                            builder.SslMode = val.ToLowerInvariant() switch
                            {
                                "require" => SslMode.Require,
                                "disable" => SslMode.Disable,
                                "prefer" => SslMode.Prefer,
                                "allow" => SslMode.Allow,
                                "verify-ca" or "verify_ca" => SslMode.VerifyCA,
                                "verify-full" or "verify_full" => SslMode.VerifyFull,
                                _ => builder.SslMode,
                            };
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
            return connectionStringOrUri;
        }
    }

    public static string SanitizeConnectionString(string connStr)
    {
        if (string.IsNullOrEmpty(connStr))
            return connStr;

        return PasswordPattern().Replace(connStr, "Password=***");
    }

    [GeneratedRegex(@"Password\s*=\s*[^;]+", RegexOptions.IgnoreCase)]
    private static partial Regex PasswordPattern();
}
