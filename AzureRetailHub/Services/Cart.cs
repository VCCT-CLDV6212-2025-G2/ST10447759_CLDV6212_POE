/*
 * ST10447759
 * CLDV6212
 * Part 3
 */
using AzureRetailHub.Models;
using Microsoft.AspNetCore.Http;

namespace AzureRetailHub.Services
{
    public class Cart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        // --- Static method to get cart from session ---
        public static Cart GetCart(IServiceProvider services)
        {
            var session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext?.Session
                ?? throw new InvalidOperationException("Session not available");

            var cart = session.Get<Cart>("Cart") ?? new Cart();
            cart.Session = session; // Store session for saving
            return cart;
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public ISession? Session { get; set; }

        public void AddItem(Product product, int quantity)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == product.Id);
            if (item == null)
            {
                Items.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = quantity
                });
            }
            else
            {
                item.Quantity += quantity;
            }
            Save();
        }

        public void RemoveItem(string productId)
        {
            Items.RemoveAll(i => i.ProductId == productId);
            Save();
        }

        // ... inside your Cart.cs class ...

        // NEW METHOD: Validates items in cart against the master product list
        public void ValidateCart(List<Product> products)
        {
            var itemsToRemove = new List<CartItem>();
            foreach (var item in Items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product == null)
                {
                    // Product no longer exists, remove it
                    itemsToRemove.Add(item);
                }
                else
                {
                    // Product exists, update its name and price
                    item.ProductName = product.Name;
                    item.Price = product.Price;
                }
            }

            foreach (var item in itemsToRemove)
            {
                Items.Remove(item);
            }
            Save();
        }

        public decimal GetTotal() => Items.Sum(i => i.Total);

        public void Clear()
        {
            Items.Clear();
            Save();
        }

        // Save cart back to session
        private void Save()
        {
            Session?.Set("Cart", this);
        }
    }
}