using System;
using System.Collections.Generic;
using System.Linq;

namespace BankSystem
{
  
    public class BankAccount
    {
        public const string BankCode = "BNK001";
        public readonly DateTime CreatedDate;

        private static int s_accountCounter = 1000;
        private int _accountNumber;

        
        private string _fullName = "";
        private string _nationalID = "";
        private string _phoneNumber = "";
        private string _address = "";
        private decimal _balance = 0;

        public string FullName
        {
            get { return _fullName; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Full Name must not be null or empty.");
                _fullName = value;
            }
        }

        public string NationalID
        {
            get { return _nationalID; }
            set
            {
                if (!IsValidNationalID(value))
                    throw new ArgumentException("National ID must be exactly 14 digits.");
                _nationalID = value;
            }
        }

        public string PhoneNumber
        {
            get { return _phoneNumber; }
            set
            {
                if (!IsValidPhoneNumber(value))
                    throw new ArgumentException("Phone Number must start with '01' and be 11 digits long.");
                _phoneNumber = value;
            }
        }

        public decimal Balance
        {
            get { return _balance; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Balance must be greater than or equal to 0.");
                _balance = value;
            }
        }

        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }

        
        public BankAccount()
        {
            CreatedDate = DateTime.Now;
            _accountNumber = s_accountCounter++;

          
            _fullName = "Mohanad";
            _nationalID = "00000000000000";
            _phoneNumber = "01000000000";
            _balance = 0;
        }

        public BankAccount(string fullName, string nationalID, string phoneNumber, string address, decimal balance)
        {
            CreatedDate = DateTime.Now;
            _accountNumber = s_accountCounter++;

           
            FullName = fullName;
            NationalID = nationalID;
            PhoneNumber = phoneNumber;
            Address = address;
            Balance = balance;
        }

        public BankAccount(string fullName, string nationalID, string phoneNumber, string address)
            : this(fullName, nationalID, phoneNumber, address, 0)
        {
        }

        public bool IsValidNationalID(string id)
        {
            return !string.IsNullOrEmpty(id) && id.Length == 14 && id.All(char.IsDigit);
        }

        public bool IsValidPhoneNumber(string phone)
        {
            return !string.IsNullOrEmpty(phone)
                   && phone.Length == 11
                   && phone.StartsWith("01")
                   && phone.All(char.IsDigit);
        }

      
        public virtual void ShowAccountDetails()
        {
            Console.WriteLine($"\n--- ACCOUNT ({_accountNumber}) ---");
            Console.WriteLine($"Type:           Base Account");
            Console.WriteLine($"Name:           {FullName}");
            Console.WriteLine($"Balance:        ${Balance:N2}");
        }
    }


    public class SavingAccount : BankAccount
    {
        public decimal InterestRate { get; set; }

        public SavingAccount(string name, string id, string phone, string address, decimal balance, decimal interestRate)
            : base(name, id, phone, address, balance)
        {
            InterestRate = interestRate;
        }

        public override void ShowAccountDetails()
        {
            base.ShowAccountDetails();
            Console.WriteLine($"Account Type:   Savings");
            Console.WriteLine($"Interest Rate:  {InterestRate}%");
        }

        public void CalculateInterest()
        {
            decimal interest = Balance * (InterestRate / 100);
            Console.WriteLine($"-> Calculated Interest: ${interest:N2}");
        }
    }

  
    public class CurrentAccount : BankAccount
    {
        public decimal OverdraftLimit { get; set; }

        public CurrentAccount(string name, string id, string phone, string address, decimal balance, decimal overdraftLimit)
            : base(name, id, phone, address, balance)
        {
            OverdraftLimit = overdraftLimit;
        }

        public override void ShowAccountDetails()
        {
            base.ShowAccountDetails();
            Console.WriteLine($"Account Type:   Current");
            Console.WriteLine($"Overdraft Limit: ${OverdraftLimit:N2}");
        }
    }

    // ==========================================
    // 4. Main Program
    // ==========================================
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                SavingAccount saver = new SavingAccount(
                    "Ahmed", "11111111111111", "01000000001", "123 St", 5000m, 5.5m);

                CurrentAccount spender = new CurrentAccount(
                    "Sarah", "22222222222222", "01000000002", "456n", 2000m, 1000m);

                List<BankAccount> accounts = new List<BankAccount>();
                accounts.Add(saver);
                accounts.Add(spender);

                foreach (BankAccount account in accounts)
                {
                    account.ShowAccountDetails();

                    if (account is SavingAccount savingsObj)
                    {
                        savingsObj.CalculateInterest();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.ReadLine();
        }
    }
}