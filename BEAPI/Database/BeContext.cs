using BEAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Database
{
    public class BeContext : DbContext
    {
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Value> Values { get; set; }
        public DbSet<ListOfValue> ListOfValues { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<PaymentHistory> PaymentHistories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductVariantValue> ProductVariantValues { get; set; }

        public BeContext(DbContextOptions<BeContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Elder)
                .WithMany()
                .HasForeignKey(c => c.ElderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Customer)
                .WithMany()
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Address>()
                .HasOne(a => a.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentHistory>()
                .HasOne(ph => ph.User)
                .WithMany()
                .HasForeignKey(ph => ph.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Role>().HasData(
                 new Role { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "User" },
                 new Role { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Elder" },
                 new Role { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Admin" }
     );
        }
    }
}
