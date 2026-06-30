using Microsoft.EntityFrameworkCore;
using BookStore.Data.Models;

namespace BookStore.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Connected directly to your local SQL Server instance!
                optionsBuilder.UseSqlServer("Server=DESKTOP-MM542JE;Database=BookStoreEFCoreDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Enforce Unique Customer Email (Requirement #103 & #166)
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique();

            // Precision settings for money values
            modelBuilder.Entity<Book>().Property(b => b.Price).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseItem>().Property(pi => pi.UnitPrice).HasPrecision(18, 2);

            // Prevent deleting categories if books belong to them
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Category)
                .WithMany(c => c.Books)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Book>()
                .HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Database Check Constraints
            modelBuilder.Entity<Book>().ToTable(t => t.HasCheckConstraint("CHK_EF_Price", "[Price] > 0"));
            modelBuilder.Entity<Book>().ToTable(t => t.HasCheckConstraint("CHK_EF_Stock", "[Stock] >= 0"));

            // Seed initial sample data
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, CategoryName = "Programming" },
                new Category { CategoryId = 2, CategoryName = "Sci-Fi" }
            );

            modelBuilder.Entity<Author>().HasData(
                new Author { AuthorId = 1, AuthorName = "Robert C. Martin" },
                new Author { AuthorId = 2, AuthorName = "Frank Herbert" }
            );

            modelBuilder.Entity<Book>().HasData(
                new Book { BookId = 1, Title = "Clean Code", CategoryId = 1, AuthorId = 1, Price = 45.00m, Stock = 10, IsDeleted = false },
                new Book { BookId = 2, Title = "Dune", CategoryId = 2, AuthorId = 2, Price = 30.00m, Stock = 5, IsDeleted = false }
            );

            modelBuilder.Entity<Customer>().HasData(
                new Customer { CustomerId = 1, FullName = "Ahmed Ali", Email = "ahmed@test.com", City = "Cairo" }
            );
        }
    }
}