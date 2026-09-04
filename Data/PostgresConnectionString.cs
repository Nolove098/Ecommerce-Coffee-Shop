using Npgsql;

namespace SaleStore.Data;

internal static class PostgresConnectionString
{
    private const int DefaultPostgresPort = 5432;

    public static string Normalize(
        string? configuredValue,
        bool forceSupabaseSessionPooler = false,
        string? projectRef = null,
        string? poolerRegion = null)
    {
        if (string.IsNullOrWhiteSpace(configuredValue))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection must be provided through secure configuration.");
        }

        var value = configuredValue.Trim();

        try
        {
            var normalized = IsPostgresUri(value)
                ? ConvertUri(value)
                : ValidateNpgsql(value);

            return forceSupabaseSessionPooler
                ? UseSupabaseSessionPooler(normalized, projectRef, poolerRegion)
                : normalized;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is ArgumentException or FormatException or UriFormatException)
        {
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is not a valid PostgreSQL URI or Npgsql connection string.");
        }
    }

    private static bool IsPostgresUri(string value) =>
        value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
        value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase);

    private static string ConvertUri(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            !(uri.Scheme.Equals("postgres", StringComparison.OrdinalIgnoreCase) ||
              uri.Scheme.Equals("postgresql", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection contains an invalid PostgreSQL URI.");
        }

        if (string.IsNullOrWhiteSpace(uri.Host))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection PostgreSQL URI must include a host.");
        }

        var userInfoSeparator = uri.UserInfo.IndexOf(':');
        var encodedUsername = userInfoSeparator >= 0
            ? uri.UserInfo[..userInfoSeparator]
            : uri.UserInfo;
        var encodedPassword = userInfoSeparator >= 0
            ? uri.UserInfo[(userInfoSeparator + 1)..]
            : string.Empty;

        var username = Uri.UnescapeDataString(encodedUsername);
        var password = Uri.UnescapeDataString(encodedPassword);
        var database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(database))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection PostgreSQL URI must include a username and database name.");
        }

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : DefaultPostgresPort,
            Database = database,
            Username = username,
            Password = password,
            SslMode = SslMode.Require
        };

        return builder.ConnectionString;
    }

    private static string ValidateNpgsql(string value)
    {
        var builder = new NpgsqlConnectionStringBuilder(value);

        if (string.IsNullOrWhiteSpace(builder.Host) ||
            string.IsNullOrWhiteSpace(builder.Database))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection Npgsql format must include Host and Database.");
        }

        return builder.ConnectionString;
    }

    private static string UseSupabaseSessionPooler(
        string normalized,
        string? projectRef,
        string? poolerRegion)
    {
        if (string.IsNullOrWhiteSpace(projectRef) || string.IsNullOrWhiteSpace(poolerRegion))
        {
            throw new InvalidOperationException(
                "Supabase:ProjectRef and Supabase:PoolerRegion are required when Supabase:ForceSessionPooler is enabled.");
        }

        var builder = new NpgsqlConnectionStringBuilder(normalized);
        var expectedDirectHost = $"db.{projectRef}.supabase.co";

        // Leave an already configured pooler (or a non-Supabase endpoint)
        // untouched. Only the exact scoped project's direct host is rewritten.
        if (!builder.Host.Equals(expectedDirectHost, StringComparison.OrdinalIgnoreCase))
            return builder.ConnectionString;

        builder.Host = $"aws-1-{poolerRegion}.pooler.supabase.com";
        builder.Port = DefaultPostgresPort;
        builder.Username = $"postgres.{projectRef}";
        builder.SslMode = SslMode.Require;
        return builder.ConnectionString;
    }
}
