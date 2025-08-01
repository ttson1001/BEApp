using BEAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Database
{
    public class BeContext: DbContext
    {
        public DbSet<Address> addresses { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Value> Values { get; set; }
        public DbSet<ListOfValue> ListOfValues  { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<PaymentHistory> PaymentHistories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductVariantValue> ProductVariantValues { get; set; }

        public BeContext(DbContextOptions options) : base(options) { }
    }
}
