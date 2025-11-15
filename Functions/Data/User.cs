using System.ComponentModel.DataAnnotations;

namespace AzureRetailHub.Functions.Data
{
    public class User
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; } = "";

        public string? FullName { get; set; }

        public string? Phone { get; set; }

        // This is our new, simple "Role" system
        public bool IsAdmin { get; set; } = false;

        // Navigation property for orders
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}