namespace HearthBean.Tests;

/// <summary>
/// Marks an integration test that needs a real SQL Server instance.
/// The test runs only when the HEARTHBEAN_TEST_SQL environment variable holds a connection
/// string whose login may create and drop databases; otherwise it is reported as skipped.
/// </summary>
public sealed class SqlServerFactAttribute : FactAttribute
{
    public const string EnvironmentVariable = "HEARTHBEAN_TEST_SQL";

    public SqlServerFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(EnvironmentVariable)))
        {
            Skip = $"Set {EnvironmentVariable} to a SQL Server connection string to run the stored procedure integration tests.";
        }
    }
}
