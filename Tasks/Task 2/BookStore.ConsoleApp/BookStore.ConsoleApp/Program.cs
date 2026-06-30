using BookStore.ConsoleApp.Models;
using BookStore.ConsoleApp.Repositories;
using BookStore.ConsoleApp.Services;

namespace BookStore.ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 1. Initialize Repositories and Services
            var bookRepo = new InMemoryRepository<Book>();
            var customerRepo = new InMemoryRepository<Customer>();
            var purchaseRepo = new InMemoryRepository<Purchase>();

            var stockManager = new StockManager();
            var bookService = new BookService();

            // Wire up Requirement #10 (Listen for the out of stock alarm!)
            stockManager.OnOutOfStock += (book) =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[SYSTEM ALERT]: The book '{book.Title}' is officially OUT OF STOCK!");
                Console.ResetColor();
            };

            // Seed initial data
            SeedData(bookRepo, customerRepo);

            bool isRunning = true;
            while (isRunning)
            {
                Console.WriteLine("\n==========================================");
                Console.WriteLine("         BOOKSTORE MANAGEMENT SYSTEM      ");
                Console.WriteLine("==========================================");
                Console.WriteLine("1. List All Books");
                Console.WriteLine("2. Add a New Book");
                Console.WriteLine("3. Remove a Book");
                Console.WriteLine("4. Search Books by Title");
                Console.WriteLine("5. Filter Books (Category / Price)");
                Console.WriteLine("6. Register Customer & Record Purchase");
                Console.WriteLine("7. View Store Analytics (Revenue & Top Stats)");
                Console.WriteLine("8. Exit Application");
                Console.WriteLine("==========================================");

                int choice = InputValidator.ReadValidInt("Enter your choice (1-8): ", 1, 8);

                switch (choice)
                {
                    case 1:
                        ListBooks(bookRepo.GetAll());
                        break;
                    case 2:
                        AddNewBook(bookRepo);
                        break;
                    case 3:
                        RemoveBook(bookRepo);
                        break;
                    case 4:
                        SearchBooks(bookRepo);
                        break;
                    case 5:
                        FilterBooksMenu(bookRepo, bookService);
                        break;
                    case 6:
                        CreatePurchase(bookRepo, customerRepo, purchaseRepo, stockManager);
                        break;
                    case 7:
                        ShowAnalytics(bookRepo, customerRepo, purchaseRepo);
                        break;
                    case 8:
                        isRunning = false;
                        Console.WriteLine("Exiting... Have a great day!");
                        break;
                }
            }
        }

        static void SeedData(InMemoryRepository<Book> bookRepo, InMemoryRepository<Customer> customerRepo)
        {
            bookRepo.Add(new PaperbackBook { Id = 1, Title = "Clean Code", Author = "Robert C. Martin", Category = "Programming", Price = 45.00m, Stock = 5 });
            bookRepo.Add(new PaperbackBook { Id = 2, Title = "Dune", Author = "Frank Herbert", Category = "Sci-Fi", Price = 30.00m, Stock = 2 });
            bookRepo.Add(new PaperbackBook { Id = 3, Title = "Design Patterns", Author = "GoF", Category = "Programming", Price = 60.00m, Stock = 1 });

            customerRepo.Add(new Customer { Id = 1, FullName = "Alice Smith", Email = "alice@test.com" });
        }

        static void ListBooks(List<Book> books)
        {
            Console.WriteLine("\n--- CURRENT INVENTORY ---");
            if (!books.Any())
            {
                Console.WriteLine("No books found.");
                return;
            }

            foreach (var b in books)
            {
                Console.WriteLine($"ID: {b.Id} | Title: {b.Title} | Author: {b.Author} | Cat: {b.Category} | Price: ${b.Price} | Stock: {b.Stock} | Format: {b.GetBookType()}");
            }
        }

        static void AddNewBook(InMemoryRepository<Book> repo)
        {
            Console.WriteLine("\n--- ADD NEW BOOK ---");
            int id = repo.GetAll().Any() ? repo.GetAll().Max(b => b.Id) + 1 : 1;
            string title = InputValidator.ReadRequiredString("Enter Title: ");
            string author = InputValidator.ReadRequiredString("Enter Author: ");
            string cat = InputValidator.ReadRequiredString("Enter Category: ");
            decimal price = InputValidator.ReadValidDecimal("Enter Price: ", 0.01m);
            int stock = InputValidator.ReadValidInt("Enter Stock Quantity: ", 1, 1000);

            var newBook = new PaperbackBook
            {
                Id = id,
                Title = title,
                Author = author,
                Category = cat,
                Price = price,
                Stock = stock
            };

            repo.Add(newBook);
            Console.WriteLine("Book added successfully!");
        }

        static void RemoveBook(InMemoryRepository<Book> repo)
        {
            Console.WriteLine("\n--- REMOVE BOOK ---");
            int id = InputValidator.ReadValidInt("Enter Book ID to remove: ", 1, int.MaxValue);
            var book = repo.GetById(id);
            if (book != null)
            {
                repo.Remove(book);
                Console.WriteLine("Book removed successfully.");
            }
            else
            {
                Console.WriteLine("Book not found!");
            }
        }

        static void SearchBooks(InMemoryRepository<Book> repo)
        {
            string keyword = InputValidator.ReadRequiredString("\nEnter search keyword: ").ToLower();
            var results = repo.GetAll().Where(b => b.Title.ToLower().Contains(keyword)).ToList();
            ListBooks(results);
        }

        static void FilterBooksMenu(InMemoryRepository<Book> repo, BookService service)
        {
            Console.WriteLine("\n1. Filter by Category");
            Console.WriteLine("2. Filter by Max Price");
            int choice = InputValidator.ReadValidInt("Choose filter (1-2): ", 1, 2);

            var allBooks = repo.GetAll();
            if (choice == 1)
            {
                string cat = InputValidator.ReadRequiredString("Enter exact category name: ");
                // Requirement #9: Passing custom lambda rule
                ListBooks(service.ApplyCustomFilter(allBooks, b => b.Category.Equals(cat, StringComparison.OrdinalIgnoreCase)));
            }
            else
            {
                decimal maxPrice = InputValidator.ReadValidDecimal("Enter maximum price: ", 0);
                ListBooks(service.ApplyCustomFilter(allBooks, b => b.Price <= maxPrice));
            }
        }

        static void CreatePurchase(InMemoryRepository<Book> bRepo, InMemoryRepository<Customer> cRepo, InMemoryRepository<Purchase> pRepo, StockManager stockMgr)
        {
            Console.WriteLine("\n--- RECORD NEW PURCHASE ---");
            string name = InputValidator.ReadRequiredString("Enter Customer Name: ");
            string email = InputValidator.ReadRequiredString("Enter Customer Email: ");

            // Find or create customer
            var customer = cRepo.GetAll().FirstOrDefault(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (customer == null)
            {
                int newCustId = cRepo.GetAll().Any() ? cRepo.GetAll().Max(c => c.Id) + 1 : 1;
                customer = new Customer { Id = newCustId, FullName = name, Email = email };
                cRepo.Add(customer);
            }

            int purchaseId = pRepo.GetAll().Any() ? pRepo.GetAll().Max(p => p.Id) + 1 : 1;
            var purchase = new Purchase { Id = purchaseId, CustomerId = customer.Id };

            bool addingItems = true;
            while (addingItems)
            {
                ListBooks(bRepo.GetAll());
                int bookId = InputValidator.ReadValidInt("\nEnter Book ID to buy (or 0 to finish cart): ", 0, int.MaxValue);
                if (bookId == 0) break;

                var book = bRepo.GetById(bookId);
                if (book == null)
                {
                    Console.WriteLine("Invalid Book ID.");
                    continue;
                }

                int qty = InputValidator.ReadValidInt($"Enter quantity for '{book.Title}' (Stock: {book.Stock}): ", 1, book.Stock);

                if (stockMgr.TrySellBook(book, qty))
                {
                    purchase.Items.Add(new PurchaseItem { BookId = book.Id, Quantity = qty, UnitPrice = book.Price });
                    Console.WriteLine("Item added to cart.");
                }
            }

            if (purchase.Items.Any())
            {
                pRepo.Add(purchase);
                Console.WriteLine($"\nPurchase recorded successfully! Total Bill: ${purchase.GetTotal()}");
            }
            else
            {
                Console.WriteLine("Purchase cancelled (empty cart).");
            }
        }

        static void ShowAnalytics(InMemoryRepository<Book> bRepo, InMemoryRepository<Customer> cRepo, InMemoryRepository<Purchase> pRepo)
        {
            Console.WriteLine("\n--- STORE ANALYTICS ---");
            var purchases = pRepo.GetAll();
            decimal totalRev = purchases.Sum(p => p.GetTotal());

            Console.WriteLine($"Total Store Revenue: ${totalRev}");

            // Best selling book
            var allItems = purchases.SelectMany(p => p.Items);
            var bestSellerGroup = allItems.GroupBy(i => i.BookId).OrderByDescending(g => g.Sum(i => i.Quantity)).FirstOrDefault();
            if (bestSellerGroup != null)
            {
                var bestBook = bRepo.GetById(bestSellerGroup.Key);
                Console.WriteLine($"Best-Selling Book: {bestBook?.Title} ({bestSellerGroup.Sum(i => i.Quantity)} copies sold)");
            }

            // Top Customer
            var topCustGroup = purchases.GroupBy(p => p.CustomerId).OrderByDescending(g => g.Sum(p => p.GetTotal())).FirstOrDefault();
            if (topCustGroup != null)
            {
                var topCust = cRepo.GetById(topCustGroup.Key);
                Console.WriteLine($"Top Spending Customer: {topCust?.FullName} (Spent ${topCustGroup.Sum(p => p.GetTotal())})");
            }
        }
    }
}