using System;
using System.Linq; // Used for verifying digits

namespace BankSystem
{
    public class BankAccount
    {
        public const string BankCode = "BNK001"; 
        public readonly DateTime CreatedDate;    
        
        private static int s_accountCounter = 1000; 

        private int _accountNumber;
        
        // Private backing fields
        private string _fullName;
        private string _nationalID;
        private string _phoneNumber;
        private string _address;
        private decimal _balance;

   
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
            
            _fullName = "Unknown";
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

        public void ShowAccountDetails()
        {
            Console.WriteLine("\n--- ACCOUNT DETAILS ---");
            Console.WriteLine($"Bank Code:      {BankCode}");
            Console.WriteLine($"Account No:     {_accountNumber}");
            Console.WriteLine($"Created Date:   {CreatedDate}");
            Console.WriteLine($"Name:           {FullName}");
            Console.WriteLine($"National ID:    {NationalID}");
            Console.WriteLine($"Phone:          {PhoneNumber}");
            Console.WriteLine($"Address:        {Address ?? "N/A"}"); // Handle null address
            Console.WriteLine($"Balance:        ${Balance:N2}");
            Console.WriteLine("-----------------------");
        }
    }

 
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Object 1: 
                Console.WriteLine("Creating Account 1...");
                BankAccount account1 = new BankAccount(
                    "AHMED", 
                    "12345678901234", 
                    "01098765432", 
                    "123 Skynet Blvd", 
                    5000.50m
                );

                // Object 2:
                Console.WriteLine("Creating Account 2...");
                BankAccount account2 = new BankAccount(
                    "MOhanad", 
                    "98765432109876", 
                    "01123456789", 
                    "456 Main St"
                );

                
                account1.ShowAccountDetails();
                account2.ShowAccountDetails();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError creating account: {ex.Message}");
            }

            
            Console.ReadLine();
        }
    }
}
