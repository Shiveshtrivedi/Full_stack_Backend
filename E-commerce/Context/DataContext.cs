using E_commerce.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
namespace E_commerce.Context
{
    public class DataContext:DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
             public DbSet<User> Users { get;set;}
             public DbSet<Product> Products { get;set;}
             public DbSet<Order> Orders { get;set;}
             public DbSet<OrderDetail> OrderDetails { get;set;}
             public DbSet<Cart> Carts { get;set;}
             public DbSet<CartItem> CartItems { get;set;}
             public DbSet<Review> Reviews { get;set;}
             public DbSet<ShippingAddress> ShippingAddresses { get;set;}
             public DbSet<WishList> WishLists { get; set; }
             public DbSet<Sale> Sales { get; set; }
             public DbSet<Revenue> Revenues { get; set; }
             public DbSet<Inventory> Inventories { get; set; }
             public DbSet<AdminHistory> Histories { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Sale>()
               .HasOne(s => s.User)
               .WithMany() 
               .HasForeignKey(s => s.UserId)
               .OnDelete(DeleteBehavior.Restrict);  

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)"); 

            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.Price)
                .HasColumnType("decimal(18,2)");  // Set precision and scale for Price in OrderDetail

            modelBuilder.Entity<Sale>()
                .Property(s => s.TotalAmount)
                .HasColumnType("decimal(18,2)");  // Set precision for TotalAmount in Sale

            modelBuilder.Entity<Revenue>()
                .Property(r => r.TotalRevenue)
                .HasColumnType("decimal(18,2)");  // Set precision for TotalRevenue in Revenue

            modelBuilder.Entity<Inventory>()
               .HasOne(i => i.Product)
               .WithMany()
               .HasForeignKey(i => i.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
        }

    }
    }

