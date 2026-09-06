using Npgsql;
using SaleStore.Data;

namespace SaleStore.Tests.Data;

public class PostgresConnectionStringTests
{
    [Theory]
    [InlineData("postgres")]
    [InlineData("postgresql")]
    [InlineData("POSTGRESQL")]
    public void Normalize_WhenUriHasEncodedComponents_PreservesDecodedValuesAndRequiresTls(string scheme)
    {
        var input = $" {scheme}://synthetic%40user:dummy%3Ap%40ss%2Bword@db.example.test/coffee%20demo ";

        var result = new NpgsqlConnectionStringBuilder(PostgresConnectionString.Normalize(input));

        Assert.Equal("db.example.test", result.Host);
        Assert.Equal(5432, result.Port);
        Assert.Equal("synthetic@user", result.Username);
        Assert.Equal("dummy:p@ss+word", result.Password);
        Assert.Equal("coffee demo", result.Database);
        Assert.Equal(SslMode.Require, result.SslMode);
    }

    [Fact]
    public void Normalize_WhenUriUsesCustomPort_PreservesPort()
    {
        var result = new NpgsqlConnectionStringBuilder(
            PostgresConnectionString.Normalize("postgres://synthetic:dummy@db.example.test:6543/coffee"));

        Assert.Equal(6543, result.Port);
    }

    [Fact]
    public void Normalize_WhenNpgsqlFormat_PreservesQuotedCredentialsAndOptions()
    {
        const string input = "Host=db.example.test;Port=6543;Database=coffee;Username=synthetic;Password=\"dummy;value=42\";SSL Mode=VerifyFull;Timeout=17;Pooling=false";

        var result = new NpgsqlConnectionStringBuilder(PostgresConnectionString.Normalize(input));

        Assert.Equal("dummy;value=42", result.Password);
        Assert.Equal("synthetic", result.Username);
        Assert.Equal("coffee", result.Database);
        Assert.Equal("db.example.test", result.Host);
        Assert.Equal(6543, result.Port);
        Assert.Equal(SslMode.VerifyFull, result.SslMode);
        Assert.Equal(17, result.Timeout);
        Assert.False(result.Pooling);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Host=db.example.test;Password=dummy-sensitive-marker")]
    [InlineData("Database=coffee;Password=dummy-sensitive-marker")]
    [InlineData("Host=db.example.test;Database=coffee;Port=dummy-sensitive-marker")]
    [InlineData("postgres://synthetic:dummy-sensitive-marker@db.example.test/")]
    [InlineData("postgres://:dummy-sensitive-marker@db.example.test/coffee")]
    [InlineData("https://synthetic:dummy-sensitive-marker@db.example.test/coffee")]
    public void Normalize_WhenInvalid_ThrowsConfigurationErrorWithoutEchoingInput(string? input)
    {
        var exception = Assert.Throws<InvalidOperationException>(() => PostgresConnectionString.Normalize(input));

        Assert.Contains("ConnectionStrings:DefaultConnection", exception.Message);
        Assert.DoesNotContain("dummy-sensitive-marker", exception.ToString());
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void Normalize_WhenScopedDirectHostAndPoolerEnabled_RewritesOnlyEndpointAndRetainsCredentials()
    {
        const string input = "Host=DB.synthetic-project.supabase.co;Port=6543;Database=coffee;Username=postgres;Password=dummy;Timeout=19";

        var result = new NpgsqlConnectionStringBuilder(PostgresConnectionString.Normalize(
            input, true, "synthetic-project", "synthetic-region"));

        Assert.Equal("aws-1-synthetic-region.pooler.supabase.com", result.Host);
        Assert.Equal(5432, result.Port);
        Assert.Equal("postgres.synthetic-project", result.Username);
        Assert.Equal("dummy", result.Password);
        Assert.Equal("coffee", result.Database);
        Assert.Equal(19, result.Timeout);
        Assert.Equal(SslMode.Require, result.SslMode);
    }

    [Theory]
    [InlineData("db.other-project.supabase.co")]
    [InlineData("db.synthetic-project.supabase.co.example.test")]
    [InlineData("aws-1-synthetic-region.pooler.supabase.com")]
    [InlineData("db.example.test")]
    public void Normalize_WhenHostIsNotExactScopedDirectEndpoint_DoesNotRewrite(string host)
    {
        var input = $"Host={host};Port=6543;Database=coffee;Username=synthetic;Password=dummy;SSL Mode=VerifyFull";

        var result = PostgresConnectionString.Normalize(input, true, "synthetic-project", "synthetic-region");

        Assert.Equal(new NpgsqlConnectionStringBuilder(input).ConnectionString, result);
    }

    [Fact]
    public void Normalize_WhenPoolerDisabled_DoesNotRequirePoolerConfigurationOrRewriteHost()
    {
        const string input = "Host=db.synthetic-project.supabase.co;Database=coffee";

        Assert.Equal(new NpgsqlConnectionStringBuilder(input).ConnectionString,
            PostgresConnectionString.Normalize(input));
    }

    [Theory]
    [InlineData(null, "synthetic-region")]
    [InlineData("synthetic-project", " ")]
    public void Normalize_WhenPoolerEnabledWithoutConfiguration_Throws(string? project, string? region)
    {
        Assert.Throws<InvalidOperationException>(() => PostgresConnectionString.Normalize(
            "Host=db.example.test;Database=coffee", true, project, region));
    }
}
