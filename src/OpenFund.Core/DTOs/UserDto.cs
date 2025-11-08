namespace OpenFund.Core.DTOs;

public class UserDto
{
    public string Id { get; private set; }
    public string Email { get; private set; }

    public UserDto(string id, string email)
    {
        Id = id;
        Email = email;
    }
}