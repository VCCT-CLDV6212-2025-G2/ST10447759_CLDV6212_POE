namespace AzureRetailHub.Functions.Data
{
    // This is a "safe" version of the User to send over the API
    public class UserDto
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public bool IsAdmin { get; set; } = false;
    }
}