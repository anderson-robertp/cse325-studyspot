public class UserListDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public bool IsActive { get; set; }
    public string Role { get; set; } = "";
}