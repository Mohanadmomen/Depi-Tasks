namespace BookStore.ConsoleApp.Models
{
    public class PaperbackBook : Book
    {
        public override string GetBookType()
        {
            return "Paperback (Physical)";
        }
    }
}