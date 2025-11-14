/*
 * ST10447759
 * CLDV6212
 * Part 3
 */
using Microsoft.AspNetCore.Identity;

namespace AzureRetailHub.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string? FullName { get; set; }
    }
}