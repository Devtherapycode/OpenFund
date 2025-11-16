namespace OpenFund.API.Models;

public record ApiResponse(string Message, Dictionary<string, ICollection<string>> Errors);
public record ApiResponse<T>(T Data);