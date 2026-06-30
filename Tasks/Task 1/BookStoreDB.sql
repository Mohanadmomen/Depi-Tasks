-- ============================================================================
-- TASK 1: DESIGN THE DATABASE SCHEMA
-- ============================================================================

CREATE DATABASE BookStoreDB;
GO

USE BookStoreDB;
GO

-- Categories Table
CREATE TABLE Categories (
    CategoryID INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) NOT NULL UNIQUE
);

-- Authors Table
CREATE TABLE Authors (
    AuthorID INT PRIMARY KEY IDENTITY(1,1),
    AuthorName NVARCHAR(150) NOT NULL
);

-- Books Table (Enforces price > 0, stock >= 0, and soft deletions)
CREATE TABLE Books (
    BookID INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(200) NOT NULL,
    CategoryID INT NOT NULL,
    AuthorID INT NOT NULL,
    Price DECIMAL(10, 2) NOT NULL CONSTRAINT CHK_Books_Price CHECK (Price > 0),
    Stock INT NOT NULL CONSTRAINT CHK_Books_Stock CHECK (Stock >= 0),
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Books_Categories FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID),
    CONSTRAINT FK_Books_Authors FOREIGN KEY (AuthorID) REFERENCES Authors(AuthorID)
);

-- Customers Table (Enforces unique email addresses)
CREATE TABLE Customers (
    CustomerID INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(150) NOT NULL,
    Email NVARCHAR(150) NOT NULL CONSTRAINT UQ_Customers_Email UNIQUE,
    City NVARCHAR(100) NOT NULL
);

-- Purchases Table
CREATE TABLE Purchases (
    PurchaseID INT PRIMARY KEY IDENTITY(1,1),
    CustomerID INT NOT NULL,
    PurchaseDate DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Purchases_Customers FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID)
);

-- PurchaseItems Table (Snapshots UnitPrice to preserve historical pricing)
CREATE TABLE PurchaseItems (
    PurchaseItemID INT PRIMARY KEY IDENTITY(1,1),
    PurchaseID INT NOT NULL,
    BookID INT NOT NULL,
    Quantity INT NOT NULL CONSTRAINT CHK_PurchaseItems_Quantity CHECK (Quantity > 0),
    UnitPrice DECIMAL(10, 2) NOT NULL CONSTRAINT CHK_PurchaseItems_UnitPrice CHECK (UnitPrice > 0),
    CONSTRAINT FK_PurchaseItems_Purchases FOREIGN KEY (PurchaseID) REFERENCES Purchases(PurchaseID),
    CONSTRAINT FK_PurchaseItems_Books FOREIGN KEY (BookID) REFERENCES Books(BookID)
);
GO

-- ============================================================================
-- TASK 2: INSERT SAMPLE DATA
-- ============================================================================

INSERT INTO Categories (CategoryName) VALUES 
('Programming'), ('Sci-Fi'), ('Fiction'), ('History'), ('Biography'), ('Mystery');

INSERT INTO Authors (AuthorName) VALUES 
('Robert C. Martin'), ('Andrew Hunt'), ('Frank Herbert'), ('Isaac Asimov'), ('Agatha Christie');

-- 6 Books in 'Programming' (CategoryID = 1) to satisfy query #9 (>5 books)
INSERT INTO Books (Title, CategoryID, AuthorID, Price, Stock) VALUES 
('Clean Code', 1, 1, 45.00, 10),
('Clean Architecture', 1, 1, 50.00, 5),
('The Pragmatic Programmer', 1, 2, 55.00, 12),
('Refactoring', 1, 1, 60.00, 8),
('Design Patterns', 1, 2, 65.00, 3),
('Code Complete', 1, 2, 70.00, 15),
('Dune', 2, 3, 30.00, 20),
('Foundation', 2, 4, 25.00, 18),
('Murder on the Orient Express', 6, 5, 20.00, 2);

-- Cairo has the highest customer count to satisfy query #8
INSERT INTO Customers (FullName, Email, City) VALUES 
('Ahmed Ali', 'ahmed@test.com', 'Cairo'),
('Mohamed Sara', 'sara@test.com', 'Cairo'),
('Mahmoud Omar', 'omar@test.com', 'Cairo'),
('Mona Hassan', 'mona@test.com', 'Alexandria'),
('Youssef Ibrahim', 'youssef@test.com', 'Giza'),
('Khaled Kareem', 'khaled@test.com', 'Ismailia'); -- Never purchased (Query #11)

-- Multi-book purchases across different months
INSERT INTO Purchases (CustomerID, PurchaseDate) VALUES 
(1, '2026-01-15 10:00:00'),
(2, '2026-01-20 14:30:00'),
(1, '2026-02-10 11:15:00'),
(3, '2026-02-18 16:45:00'),
(4, '2026-03-05 09:20:00');

INSERT INTO PurchaseItems (PurchaseID, BookID, Quantity, UnitPrice) VALUES 
(1, 1, 2, 45.00),
(1, 7, 1, 30.00),
(2, 3, 1, 55.00),
(3, 1, 3, 45.00),
(4, 2, 1, 50.00),
(4, 8, 2, 25.00),
(5, 9, 4, 20.00);
GO

-- ============================================================================
-- TASK 3: LIST ALL BOOKS SORTED BY PRICE FROM HIGHEST TO LOWEST
-- ============================================================================

SELECT BookID, Title, Price, Stock 
FROM Books 
WHERE IsDeleted = 0 
ORDER BY Price DESC;
GO

-- ============================================================================
-- TASK 4: SHOW BOOK TITLES IN UPPERCASE AND AUTHOR NAMES IN LOWERCASE
-- ============================================================================

SELECT 
    UPPER(b.Title) AS BookTitle, 
    LOWER(a.AuthorName) AS AuthorName
FROM Books b
JOIN Authors a ON b.AuthorID = a.AuthorID
WHERE b.IsDeleted = 0;
GO

-- ============================================================================
-- TASK 5: SHOW EVERY BOOK WITH ITS CATEGORY AND ITS AUTHOR
-- ============================================================================

SELECT 
    b.Title, 
    c.CategoryName, 
    a.AuthorName,
    b.Price
FROM Books b
JOIN Categories c ON b.CategoryID = c.CategoryID
JOIN Authors a ON b.AuthorID = a.AuthorID
WHERE b.IsDeleted = 0;
GO

-- ============================================================================
-- TASK 6: SHOW EVERY CUSTOMER WITH THE NUMBER OF PURCHASES THEY HAVE MADE
-- ============================================================================

SELECT 
    c.CustomerID, 
    c.FullName, 
    c.Email, 
    COUNT(p.PurchaseID) AS TotalPurchases
FROM Customers c
LEFT JOIN Purchases p ON c.CustomerID = p.CustomerID
GROUP BY c.CustomerID, c.FullName, c.Email;
GO

-- ============================================================================
-- TASK 7: LIST THE TOP 5 BEST-SELLING BOOKS
-- ============================================================================

SELECT TOP 5 
    b.BookID, 
    b.Title, 
    SUM(pi.Quantity) AS TotalCopiesSold
FROM Books b
JOIN PurchaseItems pi ON b.BookID = pi.BookID
GROUP BY b.BookID, b.Title
ORDER BY TotalCopiesSold DESC;
GO

-- ============================================================================
-- TASK 8: FIND THE CITY WITH THE HIGHEST NUMBER OF CUSTOMERS
-- ============================================================================

SELECT TOP 1 
    City, 
    COUNT(CustomerID) AS CustomerCount
FROM Customers
GROUP BY City
ORDER BY CustomerCount DESC;
GO

-- ============================================================================
-- TASK 9: LIST CATEGORIES THAT HAVE MORE THAN 5 BOOKS
-- ============================================================================

SELECT 
    c.CategoryName, 
    COUNT(b.BookID) AS BookCount
FROM Categories c
JOIN Books b ON c.CategoryID = b.CategoryID
WHERE b.IsDeleted = 0
GROUP BY c.CategoryName
HAVING COUNT(b.BookID) > 5;
GO

-- ============================================================================
-- TASK 10: FIND ALL BOOKS THAT COST MORE THAN THE AVERAGE BOOK PRICE
-- ============================================================================

SELECT BookID, Title, Price
FROM Books
WHERE IsDeleted = 0 
  AND Price > (SELECT AVG(Price) FROM Books WHERE IsDeleted = 0);
GO

-- ============================================================================
-- TASK 11: FIND CUSTOMERS WHO HAVE NEVER MADE A PURCHASE
-- ============================================================================

SELECT CustomerID, FullName, Email
FROM Customers
WHERE CustomerID NOT IN (SELECT CustomerID FROM Purchases);
GO

-- ============================================================================
-- TASK 12: SHOW THE TOTAL REVENUE FOR EACH MONTH
-- ============================================================================

SELECT 
    YEAR(p.PurchaseDate) AS PurchaseYear,
    MONTH(p.PurchaseDate) AS PurchaseMonth,
    SUM(pi.Quantity * pi.UnitPrice) AS MonthlyRevenue
FROM Purchases p
JOIN PurchaseItems pi ON p.PurchaseID = pi.PurchaseID
GROUP BY YEAR(p.PurchaseDate), MONTH(p.PurchaseDate)
ORDER BY PurchaseYear, PurchaseMonth;
GO

-- ============================================================================
-- TASK 13: CREATE A VIEW THAT COMBINES TITLE, CATEGORY, AUTHOR, AND PRICE
-- ============================================================================

CREATE VIEW vw_BookDetails AS
SELECT 
    b.Title AS BookTitle,
    c.CategoryName,
    a.AuthorName,
    b.Price
FROM Books b
JOIN Categories c ON b.CategoryID = c.CategoryID
JOIN Authors a ON b.AuthorID = a.AuthorID
WHERE b.IsDeleted = 0;
GO

-- ============================================================================
-- TASK 14: STORED PROCEDURE FOR CUSTOMER PURCHASES AND TOTALS
-- ============================================================================

CREATE PROCEDURE sp_GetCustomerPurchases
    @CustomerID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        p.PurchaseID,
        p.PurchaseDate,
        b.Title AS BookTitle,
        pi.Quantity,
        pi.UnitPrice,
        (pi.Quantity * pi.UnitPrice) AS TotalItemPrice
    FROM Purchases p
    JOIN PurchaseItems pi ON p.PurchaseID = pi.PurchaseID
    JOIN Books b ON pi.BookID = b.BookID
    WHERE p.CustomerID = @CustomerID
    ORDER BY p.PurchaseDate DESC;
END;
GO