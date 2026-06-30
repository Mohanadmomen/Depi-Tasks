namespace BookStore.ConsoleApp.Models
{
    public class Purchase : BaseEntity
    {
        public int CustomerId { get; set; }
        public DateTime PurchaseDate { get; set; } = DateTime.Now;
        public List<PurchaseItem> Items { get; set; } = new();

        public decimal GetTotal()
        {
            decimal total = 0;
            foreach (var item in Items)
            {
                total += item.Quantity * item.UnitPrice;
            }
            return total;
        }
    }
}