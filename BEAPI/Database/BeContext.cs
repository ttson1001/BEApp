using BEAPI.Entities;
using BEAPI.Entities.Enum;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Database
{
    public class BeContext : DbContext
    {
        public DbSet<Province> Provinces { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Ward> Wards { get; set; }
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
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<ProductVariantValue> ProductVariantValues { get; set; }

        public BeContext(DbContextOptions<BeContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Elder)
                .WithMany()
                .HasForeignKey(o => o.ElderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Value>()
                .HasOne(v => v.ListOfValue)
                .WithMany(l => l.Values)
                .HasForeignKey(v => v.ListOfValueId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Value>()
                .HasOne(v => v.ChildListOfValue)
                .WithMany()
                .HasForeignKey(v => v.ChildListOfValueId)
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
                 new Role { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Guardian" },
                 new Role { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Elder" },
                 new Role { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Admin" }
            );

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    FullName = "Nguyen Van A",
                    UserName = "nguyenvana",
                    Email = "vana@example.com",
                    PhoneNumber = "0901234567",
                    PasswordHash = "$2a$11$X9X4SPGxvVdQGQKkJkGZbOGXfHP4L7lqMZtYxQz4WPrGz7G6oUu0K", // bcrypt hash 123456
                    RoleId = Guid.Parse("11111111-1111-1111-1111-111111111111"), // Role User
                    Age = 25,
                    Gender = "Male",
                    IsVerified = true,
                    CreationDate = DateTimeOffset.Parse("2025-08-02T00:00:00Z")
                },
                new User
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    FullName = "Tran Thi B",
                    UserName = "tranthib",
                    Email = "thib@example.com",
                    PhoneNumber = "0909876543",
                    PasswordHash = "$2a$11$X9X4SPGxvVdQGQKkJkGZbOGXfHP4L7lqMZtYxQz4WPrGz7G6oUu0K", // bcrypt hash 123456
                    RoleId = Guid.Parse("22222222-2222-2222-2222-222222222222"), // Role Elder
                    GuardianId = Guid.Parse("11111111-1111-1111-1111-111111111111"), // Guardian = Nguyen Van A
                    Age = 30,
                    Gender = "Female",
                    IsVerified = true,
                    CreationDate = DateTimeOffset.Parse("2025-08-02T00:00:00Z")
                },
                new User
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    FullName = "Admin System",
                    UserName = "admin1",
                    Email = "admin@example.com",
                    PhoneNumber = "0912345678",
                    PasswordHash = "$2a$11$X9X4SPGxvVdQGQKkJkGZbOGXfHP4L7lqMZtYxQz4WPrGz7G6oUu0K", // bcrypt hash 123456
                    RoleId = Guid.Parse("33333333-3333-3333-3333-333333333333"), // Role Admin
                    Age = 35,
                    IsVerified = true,
                    Gender = "Male",
                    CreationDate = DateTimeOffset.Parse("2025-08-02T00:00:00Z")
                }
            );
            modelBuilder.Entity<ListOfValue>().HasData(new ListOfValue
            {
                Id = Guid.Parse("e83fdb81-1ca6-49da-bd91-f42ce99fd8ee"),
                Label = "Loại sản Phẩm",
                Note = "CATEGORY",
                Type = MyValueType.Category,
                CreationDate = DateTimeOffset.Parse("2025-08-02T00:00:00Z")
            });
        }
    }
}
