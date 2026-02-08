using System;
using System.Collections.Generic;
using System.Linq;

namespace BankSystem
{
        
    public abstract class BankAccount
    {
        private static int s_accountCounter = 10000;
        public int AccountNumber { get; private set; }
        public decimal Balance { get; protected set; }
        public DateTime DateOpened { get; private set; }

        protected List<string> _transactions = new List<string>();

        public BankAccount(decimal initialBalance)
        {
            AccountNumber = s_accountCounter++;
            Balance = initialBalance;
            DateOpened = DateTime.Now;
            AddTransaction($"Account opened with ${initialBalance:N2}");
        }

        public virtual void Deposit(decimal amount)
        {
            if (amount <= 0) throw new ArgumentException("Deposit amount must be positive.");
            Balance += amount;
            AddTransaction($"Deposited: ${amount:N2}");
        }

       
        public abstract void Withdraw(decimal amount);

        public void Transfer(BankAccount targetAccount, decimal amount)
        {
            if (targetAccount == null) throw new ArgumentException("Target account cannot be null.");

            
            this.Withdraw(amount);

           
            targetAccount.Deposit(amount);

            this.AddTransaction($"Transferred ${amount:N2} to Acc #{targetAccount.AccountNumber}");
            targetAccount.AddTransaction($"Received ${amount:N2} from Acc #{this.AccountNumber}");
        }

        protected void AddTransaction(string description)
        {
            _transactions.Add($"{DateTime.Now}: {description} | Bal: ${Balance:N2}");
        }

        public void ShowTransactions()
        {
            Console.WriteLine($"\n--- Transaction History (Acc: {AccountNumber}) ---");
            foreach (var log in _transactions)
            {
                Console.WriteLine(log);
            }
        }

        public abstract void ShowDetails();
    }

    public class SavingsAccount : BankAccount
    {
        public decimal InterestRate { get; set; }

        public SavingsAccount(decimal initialBalance, decimal interestRate) : base(initialBalance)
        {
            InterestRate = interestRate;
        }

        public override void Withdraw(decimal amount)
        {
            if (amount <= 0) throw new ArgumentException("Amount must be positive.");
            if (Balance < amount) throw new InvalidOperationException("Insufficient funds.");

            Balance -= amount;
            AddTransaction($"Withdrew: ${amount:N2}");
        }

        public void CalculateMonthlyInterest()
        {
            decimal interest = Balance * (InterestRate / 100 / 12);
            Deposit(interest);
            AddTransaction($"Monthly Interest Added: ${interest:N2}");
            Console.WriteLine($"Interest Added: ${interest:N2}");
        }

        public override void ShowDetails()
        {
            Console.WriteLine($"[Savings] Acc: {AccountNumber} | Bal: ${Balance:N2} | Rate: {InterestRate}%");
        }
    }

    public class CurrentAccount : BankAccount
    {
        public decimal OverdraftLimit { get; set; }

        public CurrentAccount(decimal initialBalance, decimal overdraftLimit) : base(initialBalance)
        {
            OverdraftLimit = overdraftLimit;
        }

        public override void Withdraw(decimal amount)
        {
            if (amount <= 0) throw new ArgumentException("Amount must be positive.");

            
            if ((Balance + OverdraftLimit) < amount)
                throw new InvalidOperationException("Exceeds overdraft limit.");

            Balance -= amount;
            AddTransaction($"Withdrew: ${amount:N2}");
        }

        public override void ShowDetails()
        {
            Console.WriteLine($"[Current] Acc: {AccountNumber} | Bal: ${Balance:N2} | Overdraft: ${OverdraftLimit:N2}");
        }
    }

    // ==========================================
    // 2. Customer Class
    // ==========================================
    public class Customer
    {
        private static int s_idCounter = 1;
        public int CustomerID { get; private set; }
        public string FullName { get; set; }
        public string NationalID { get; private set; }
        public DateTime DateOfBirth { get; set; }

        public List<BankAccount> Accounts { get; private set; }

        public Customer(string fullName, string nationalID, DateTime dob)
        {
            CustomerID = s_idCounter++;
            FullName = fullName;
            NationalID = nationalID;
            DateOfBirth = dob;
            Accounts = new List<BankAccount>();
        }

        public void AddAccount(BankAccount account)
        {
            Accounts.Add(account);
        }

        public decimal GetTotalBalance()
        {
            return Accounts.Sum(a => a.Balance);
        }

        public bool CanBeRemoved()
        {
            return GetTotalBalance() == 0;
        }

        public void ShowCustomerReport()
        {
            Console.WriteLine($"\nID: {CustomerID} | Name: {FullName} | NatID: {NationalID} | Total Bal: ${GetTotalBalance():N2}");
            foreach (var acc in Accounts)
            {
                Console.Write("  -> ");
                acc.ShowDetails();
            }
        }
    }

    // ==========================================
    // 3. Bank Management Class
    // ==========================================
    public class Bank
    {
        public string Name { get; set; }
        public string BranchCode { get; set; }
        private List<Customer> _customers = new List<Customer>();

        public Bank(string name, string branchCode)
        {
            Name = name;
            BranchCode = branchCode;
        }

        public void AddCustomer(string name, string nationalId, DateTime dob)
        {
            var newCustomer = new Customer(name, nationalId, dob);
            _customers.Add(newCustomer);
            Console.WriteLine($"Customer {name} added with ID: {newCustomer.CustomerID}");
        }

        public void RemoveCustomer(int customerId)
        {
            var customer = GetCustomer(customerId);
            if (customer == null) return;

            if (customer.CanBeRemoved())
            {
                _customers.Remove(customer);
                Console.WriteLine("Customer removed successfully.");
            }
            else
            {
                Console.WriteLine("Error: Customer has active funds. Withdraw all funds before removing.");
            }
        }

        public Customer GetCustomer(int id)
        {
            return _customers.FirstOrDefault(c => c.CustomerID == id);
        }

        public Customer SearchCustomer(string query)
        {
            return _customers.FirstOrDefault(c =>
                c.FullName.Equals(query, StringComparison.OrdinalIgnoreCase) ||
                c.NationalID == query);
        }

        public void ShowBankReport()
        {
            Console.WriteLine($"\n=== BANK REPORT: {Name} ({BranchCode}) ===");
            foreach (var customer in _customers)
            {
                customer.ShowCustomerReport();
            }
            Console.WriteLine("===========================================");
        }
    }

    // ==========================================
    // 4. Main Program
    // ==========================================
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Bank Creation
            Bank myBank = new Bank("Global Tech Bank", "GTB-007");

            // 2. Customer 
            myBank.AddCustomer("Ahmed", "12345678901234", new DateTime(1990, 5, 20));
            myBank.AddCustomer("Mohanad", "98765432109876", new DateTime(1985, 8, 15));

            // 3. Create Accounts
            var customerJohn = myBank.SearchCustomer("John Doe");
            if (customerJohn != null)
            {
                
                var saveAcc = new SavingsAccount(1000m, 5.0m);
                customerJohn.AddAccount(saveAcc);

              
                var currAcc = new CurrentAccount(500m, 500m);
                customerJohn.AddAccount(currAcc);

               
                Console.WriteLine("\n--- Performing Transactions ---");

               
                saveAcc.Deposit(200);

                
                try { currAcc.Withdraw(800); } 
                catch (Exception ex) { Console.WriteLine(ex.Message); }

                
                saveAcc.Transfer(currAcc, 100);

                
                saveAcc.CalculateMonthlyInterest();

                
                saveAcc.ShowTransactions();
                currAcc.ShowTransactions();
            }

            
            myBank.ShowBankReport();

            
            Console.WriteLine("\n--- Testing Removal ---");
            myBank.RemoveCustomer(customerJohn.CustomerID); 

            Console.ReadLine();
        }
    }
}