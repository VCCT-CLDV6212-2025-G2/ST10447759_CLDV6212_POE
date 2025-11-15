namespace AzureRetailHub.Models
{
    // This model will be stored in the session to track the logged-in user
    public class UserViewModel
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";
        public bool IsAdmin { get; set; } = false;
    }
}