namespace SurveyChatbot.Models;

public class User
{
    public long Id { get; init; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Username { get; set; }

    public User()
    { }

    public User(string? firstName, string? lastName, string? username)
    {
        FirstName = firstName;
        LastName = lastName;
        Username = username;
    }

    public User(long id, string? firstName, string? lastName, string? username)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Username = username;
    }
}