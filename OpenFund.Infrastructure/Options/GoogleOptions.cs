namespace OpenFund.Infrastructure.Options;

public class GoogleOptions
{
    public required string InitialAuthAddress { get; init; }
    public required string TokenRetrievalAddress { get; init; }    
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
}