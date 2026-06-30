namespace BookStore.ConsoleApp.Services
{
    public static class InputValidator
    {
        public static int ReadValidInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int result) && result >= min && result <= max)
                {
                    return result;
                }
                Console.WriteLine($"Invalid input! Please enter a whole number between {min} and {max}.");
            }
        }

        public static decimal ReadValidDecimal(string prompt, decimal min)
        {
            while (true)
            {
                Console.Write(prompt);
                if (decimal.TryParse(Console.ReadLine(), out decimal result) && result >= min)
                {
                    return result;
                }
                Console.WriteLine($"Invalid input! Please enter a valid price greater than {min}.");
            }
        }

        public static string ReadRequiredString(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    return input.Trim();
                }
                Console.WriteLine("This field cannot be empty. Please try again.");
            }
        }
    }
}