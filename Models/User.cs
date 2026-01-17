using System.ComponentModel.DataAnnotations;


public class User
{
    public int Id { get; set; }

    [Required]
    public required string FullName { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string PasswordHash { get; set; }

    public string Role { get; set; } = "User";

    public bool Drink { get; set; } = true;
    public bool Lunch { get; set; } = false;
}


