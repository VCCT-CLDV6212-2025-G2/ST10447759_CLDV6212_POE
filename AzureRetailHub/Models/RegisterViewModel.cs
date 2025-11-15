using System.ComponentModel.DataAnnotations;

namespace AzureRetailHub.Models
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = "";

        [Display(Name = "Full Name")]
        public string? FullName { get; set; }

        public string? Phone { get; set; }
    }
}