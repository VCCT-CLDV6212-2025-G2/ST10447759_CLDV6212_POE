using AzureRetailHub.Models;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace AzureRetailHub.Services.CartHelper
{
    // This helper class makes it easy to get the user in the _Layout.cshtml
    public static class SessionUser
    {
        public static UserViewModel? GetUser(IHttpContextAccessor accessor)
        {
            var userJson = accessor.HttpContext?.Session.GetString("LoggedInUser");
            if (string.IsNullOrEmpty(userJson))
            {
                return null;
            }
            try
            {
                return JsonSerializer.Deserialize<UserViewModel>(userJson);
            }
            catch
            {
                return null;
            }
        }
    }
}