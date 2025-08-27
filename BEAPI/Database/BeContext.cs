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
        public DbSet<Report> Reports { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<UserConnection> UserConnections { get; set; }
        public DbSet<OrderShipmentEvent> OrderShipmentEvents { get; set; }
        public DbSet<WithdrawRequest> WithdrawRequests { get; set; }
        public BeContext(DbContextOptions<BeContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


            base.OnModelCreating(modelBuilder);
            // Disable RowVersion for Wallet entity only
            modelBuilder.Entity<Wallet>()
                .Ignore(w => w.RowVersion);
            modelBuilder.Entity<User>()
                .Property(u => u.Avatar)
                .HasMaxLength(2048);
            modelBuilder.Entity<OrderShipmentEvent>()
                .HasOne(e => e.Order)
                .WithMany(o => o.ShipmentEvents)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
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
                .WithMany(u => u.PaymentHistory)
                .HasForeignKey(ph => ph.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Report>()
                 .HasOne(r => r.User)
                 .WithMany()
                 .HasForeignKey(r => r.UserId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.Consultant)
                .WithMany()
                .HasForeignKey(r => r.ConsultantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Feedback>()
                 .HasOne(r => r.User)
                 .WithMany()                     
                 .HasForeignKey(r => r.UserId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Feedback>()
                .HasOne(r => r.Admin)
                .WithMany()                    
                .HasForeignKey(r => r.AdminId) 
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<UserPromotion>()
                .HasOne(up => up.User).WithMany(u => u.UserPromotions).HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserPromotion>()
                .HasOne(up => up.Promotion).WithMany(p => p.UserPromotions).HasForeignKey(up => up.PromotionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Promotion>().HasIndex(p => p.IsActive);

            modelBuilder.Entity<Role>().HasData(
                 new Role { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Guardian" },
                 new Role { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Elder" },
                 new Role { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Admin" },
                 new Role { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Consultant" },
                 new Role { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), Name = "Staff" }
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
                    Gender = Gender.Male,
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
                    Gender = Gender.Male,
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
                    Gender = Gender.Male,
                    CreationDate = DateTimeOffset.Parse("2025-08-02T00:00:00Z")
                }
            );
            modelBuilder.Entity<Wallet>().HasData(
                new Wallet
                {
                    Id = Guid.Parse("77777777-7777-7777-7777-777777777001"),
                    UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Amount = 0
                },
                new Wallet
                {
                    Id = Guid.Parse("77777777-7777-7777-7777-777777777002"),
                    UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Amount = 0
                },
                new Wallet
                {
                    Id = Guid.Parse("77777777-7777-7777-7777-777777777003"),
                    UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Amount = 0
                }
            );
            modelBuilder.Entity<ListOfValue>().HasData(
                new ListOfValue
                {
                    Id = Guid.Parse("e83fdb81-1ca6-49da-bd91-f42ce99fd8ee"),
                    Label = "Loại sản phẩm",
                    Note = "CATEGORY",
                    Type = MyValueType.Category,
                    CreationDate = DateTimeOffset.Parse("2025-08-02T00:00:00Z")
                },
                new ListOfValue
                {
                    Id = Guid.Parse("a23f89a1-2c34-4b2d-9876-08dcb9a3abcd"),
                    Label = "Thương hiệu",
                    Note = "BRAND",
                    Type = MyValueType.Brand,
                    CreationDate = DateTimeOffset.Parse("2025-08-02T00:00:00Z")
                },
                new ListOfValue
                {
                    Id = Guid.Parse("c47fabcd-77f2-4f55-8322-08dcb9a3cdef"),
                    Label = "Mối quan hệ",
                    Note = "RELATIONSHIP",
                    Type = MyValueType.Relationship,
                    CreationDate = DateTimeOffset.Parse("2025-08-02T00:00:00Z")
                },new ListOfValue
                {
                    Id = Guid.Parse("e12abcde-1234-4567-89ab-08dcb9a3cdef"),
                    Label = "Loại bệnh án",
                    Note = "MEDICAL_REPORT_TYPE",
                    Type = MyValueType.MedicalReport,
                    CreationDate = DateTimeOffset.Parse("2025-08-02T00:00:00Z")
                }
            );

        }
    }
}
