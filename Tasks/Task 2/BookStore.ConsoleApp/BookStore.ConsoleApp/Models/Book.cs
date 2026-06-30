namespace BookStore.ConsoleApp.Models
{
    public abstract class Book : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }

        // This is the magic OOP line. Every book format must describe itself!
        public abstract string GetBookType();
    }
}