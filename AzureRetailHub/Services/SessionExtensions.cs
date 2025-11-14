/*
 * ST10447759
 * CLDV6212
 * Part 3
 * This file adds Get<T> and Set<T> methods to ISession
 */
using System.Text.Json;

namespace AzureRetailHub.Services
{
    public static class SessionExtensions
    {
        // This method saves an object to the session
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // This method loads an object from the session
        public static T? Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}