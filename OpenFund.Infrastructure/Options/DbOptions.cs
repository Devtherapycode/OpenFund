namespace OpenFund.Infrastructure.Options;

public sealed class DbOptions
{
    public required string ConnectionString { get; init; }
}