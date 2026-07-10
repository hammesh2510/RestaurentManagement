using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Tax> Taxes { get; set; }
        public DbSet<RestaurantTable> RestaurantTables { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Delete Behaviors to avoid Multiple Cascade paths on SQL Server
            
            // Employee and User
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithOne(u => u.Employee)
                .HasForeignKey<Employee>(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reservation and Customer
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Customer)
                .WithMany(c => c.Reservations)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reservation and Table
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Table)
                .WithMany(t => t.Reservations)
                .HasForeignKey(r => r.TableId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order and Customer
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order and Table
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Table)
                .WithMany(t => t.Orders)
                .HasForeignKey(o => o.TableId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order and CreatedByUser
            modelBuilder.Entity<Order>()
                .HasOne(o => o.CreatedByUser)
                .WithMany()
                .HasForeignKey(o => o.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderItem and MenuItem
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.MenuItem)
                .WithMany(m => m.OrderItems)
                .HasForeignKey(oi => oi.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderItem and Order
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Payment and Order
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // MenuItem and Category
            modelBuilder.Entity<MenuItem>()
                .HasOne(m => m.Category)
                .WithMany(c => c.MenuItems)
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // MenuItem and Tax
            modelBuilder.Entity<MenuItem>()
                .HasOne(m => m.Tax)
                .WithMany(t => t.MenuItems)
                .HasForeignKey(m => m.TaxId)
                .OnDelete(DeleteBehavior.SetNull);

            // Purchase and Supplier
            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Purchases)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // PurchaseItem and Purchase
            modelBuilder.Entity<PurchaseItem>()
                .HasOne(pi => pi.Purchase)
                .WithMany(p => p.PurchaseItems)
                .HasForeignKey(pi => pi.PurchaseId)
                .OnDelete(DeleteBehavior.Cascade);

            // PurchaseItem and Inventory
            modelBuilder.Entity<PurchaseItem>()
                .HasOne(pi => pi.Inventory)
                .WithMany(i => i.PurchaseItems)
                .HasForeignKey(pi => pi.InventoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ensure Decimal Precisions (extra safety check in EF Core fluent configuration)
            modelBuilder.Entity<Employee>().Property(e => e.Salary).HasPrecision(18, 2);
            modelBuilder.Entity<Tax>().Property(t => t.Percentage).HasPrecision(5, 2);
            modelBuilder.Entity<MenuItem>().Property(m => m.Price).HasPrecision(18, 2);
            modelBuilder.Entity<Order>().Property(o => o.SubTotal).HasPrecision(18, 2);
            modelBuilder.Entity<Order>().Property(o => o.DiscountAmount).HasPrecision(18, 2);
            modelBuilder.Entity<Order>().Property(o => o.TaxAmount).HasPrecision(18, 2);
            modelBuilder.Entity<Order>().Property(o => o.GrandTotal).HasPrecision(18, 2);
            modelBuilder.Entity<OrderItem>().Property(oi => oi.Price).HasPrecision(18, 2);
            modelBuilder.Entity<Payment>().Property(p => p.AmountPaid).HasPrecision(18, 2);
            modelBuilder.Entity<Inventory>().Property(i => i.CurrentStock).HasPrecision(18, 2);
            modelBuilder.Entity<Inventory>().Property(i => i.MinStock).HasPrecision(18, 2);
            modelBuilder.Entity<Purchase>().Property(p => p.TotalAmount).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseItem>().Property(pi => pi.Quantity).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseItem>().Property(pi => pi.PricePerUnit).HasPrecision(18, 2);
            modelBuilder.Entity<Expense>().Property(e => e.Amount).HasPrecision(18, 2);
        }
    }
}
