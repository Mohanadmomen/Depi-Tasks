namespace BookStore.Data.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public List<Book> Books { get; set; } = new();
    }

    public class Author
    {
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public List<Book> Books { get; set; } = new();
    }

    public class Book
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public int AuthorId { get; set; }
        public Author? Author { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class Customer
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;

        // --- ADD THESE TWO LINES FOR AUTHENTICATION ---
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Customer"; // Will be "Admin" or "Customer"

        public List<Purchase> Purchases { get; set; } = new();
    }

    public class Purchase
    {
        public int PurchaseId { get; set; }
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public DateTime PurchaseDate { get; set; } = DateTime.Now;
        public List<PurchaseItem> Items { get; set; } = new();
    }

    public class PurchaseItem
    {
        public int PurchaseItemId { get; set; }
        public int PurchaseId { get; set; }
        public Purchase? Purchase { get; set; }
        public int BookId { get; set; }
        public Book? Book { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}