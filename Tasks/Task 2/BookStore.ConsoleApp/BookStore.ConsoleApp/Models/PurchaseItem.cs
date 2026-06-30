namespace BookStore.ConsoleApp.Models
{
    public class PurchaseItem
    {
        public int BookId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}