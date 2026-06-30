namespace BookStore.Data.DTOs
{
    public class CreateBookDto
    {
        public string Title { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public int AuthorId { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}