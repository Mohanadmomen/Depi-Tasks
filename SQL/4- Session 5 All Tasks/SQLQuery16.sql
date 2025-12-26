-- 1. CUSTOMER SPENDING ANALYSIS 

DECLARE @CustomerID INT = 1;
DECLARE @TotalSpent DECIMAL(18,2);

SELECT @TotalSpent = SUM(oi.quantity * oi.list_price * (1-oi.discount/100.0))
FROM sales.order_items oi
JOIN sales.orders o ON oi.order_id = o.order_id
WHERE o.customer_id = @CustomerID;

PRINT 'Customer ' + CAST(@CustomerID AS VARCHAR) + ' spent: $' + CAST(@TotalSpent AS VARCHAR);

IF @TotalSpent > 5000
    PRINT 'VIP Customer';
ELSE
    PRINT 'Regular Customer';

-- 2. PRODUCT PRICE THRESHOLD REPORT
DECLARE @PriceThreshold DECIMAL(10,2) = 1500;
DECLARE @ProductCount INT;

SELECT @ProductCount = COUNT(*) 
FROM production.products
WHERE list_price > @PriceThreshold;

PRINT 'Products above $' + CAST(@PriceThreshold AS VARCHAR) + ': ' + CAST(@ProductCount AS VARCHAR);

-- 3. STAFF PERFORMANCE CALCULATOR
DECLARE @StaffID INT = 2;
DECLARE @Year INT = 2017;
DECLARE @StaffTotal DECIMAL(18,2);

SELECT 
    @StaffTotal = ISNULL(
        SUM(oi.quantity * oi.list_price * (1 - oi.discount / 100.0)),
        0
    )
FROM sales.order_items oi
JOIN sales.orders o ON oi.order_id = o.order_id
WHERE o.staff_id = @StaffID
  AND YEAR(o.order_date) = @Year;

PRINT 'Staff ' + CAST(@StaffID AS VARCHAR(10)) +
      ' total sales in ' + CAST(@Year AS VARCHAR(4)) +
      ': $' + CAST(@StaffTotal AS VARCHAR(20));


-- 4. GLOBAL VARIABLES INFORMATION
SELECT 
    @@SERVERNAME AS ServerName,
    @@VERSION AS SQLVersion,
    @@ROWCOUNT AS RowsAffectedByLastStatement;

-- 5. CHECK INVENTORY LEVEL
DECLARE @StoreID INT = 1;
DECLARE @ProductID INT = 1;
DECLARE @Qty INT;

SELECT @Qty = quantity
FROM production.stocks
WHERE store_id = @StoreID AND product_id = @ProductID;

IF @Qty > 20
    PRINT 'Well stocked';
ELSE IF @Qty BETWEEN 10 AND 20
    PRINT 'Moderate stock';
ELSE
    PRINT 'Low stock - reorder needed';

-- 6. LOW-STOCK UPDATE LOOP
DECLARE @Counter INT = 0;
DECLARE @BatchSize INT = 3;

WHILE EXISTS (SELECT 1 FROM production.stocks WHERE quantity < 5)
BEGIN
    UPDATE TOP (@BatchSize) production.stocks
    SET quantity = quantity + 10
    WHERE quantity < 5;

    SET @Counter = @Counter + @BatchSize;
    PRINT CAST(@Counter AS VARCHAR) + ' low-stock items updated';
END

-- 7. PRODUCT PRICE CATEGORIZATION
SELECT 
    product_id,
    product_name,
    list_price,
    CASE 
        WHEN list_price < 300 THEN 'Budget'
        WHEN list_price BETWEEN 300 AND 800 THEN 'Mid-Range'
        WHEN list_price BETWEEN 801 AND 2000 THEN 'Premium'
        ELSE 'Luxury'
    END AS PriceCategory
FROM production.products;

-- 8. CUSTOMER ORDER VALIDATION
DECLARE @CheckCustomerID INT = 5;
DECLARE @OrderCount INT;

IF EXISTS (SELECT 1 FROM sales.customers WHERE customer_id = @CheckCustomerID)
BEGIN
    SELECT @OrderCount = COUNT(*) FROM sales.orders WHERE customer_id = @CheckCustomerID;
    PRINT 'Customer ' + CAST(@CheckCustomerID AS VARCHAR) + ' has ' + CAST(@OrderCount AS VARCHAR) + ' orders';
END
ELSE
    PRINT 'Customer does not exist';

-- 9. SHIPPING COST FUNCTION
CREATE FUNCTION dbo.CalculateShipping (@OrderTotal DECIMAL(18,2))
RETURNS DECIMAL(10,2)
AS
BEGIN
    DECLARE @Shipping DECIMAL(10,2);

    IF @OrderTotal > 100
        SET @Shipping = 0;
    ELSE IF @OrderTotal BETWEEN 50 AND 99.99
        SET @Shipping = 5.99;
    ELSE
        SET @Shipping = 12.99;

    RETURN @Shipping;
END;
GO

-- 10. INLINE TABLE-VALUED FUNCTION: PRODUCTS BY PRICE RANGE
CREATE FUNCTION dbo.GetProductsByPriceRange (@MinPrice DECIMAL(10,2), @MaxPrice DECIMAL(10,2))
RETURNS TABLE
AS
RETURN
(
    SELECT p.product_id, p.product_name, p.list_price, b.brand_name, c.category_name
    FROM production.products p
    JOIN production.brands b ON p.brand_id = b.brand_id
    JOIN production.categories c ON p.category_id = c.category_id
    WHERE p.list_price BETWEEN @MinPrice AND @MaxPrice
);
GO

-- 11. Customer Sales Summary Function
CREATE FUNCTION dbo.GetCustomerYearlySummary(@CustomerID INT)
RETURNS @Summary TABLE (
    Year INT,
    TotalOrders INT,
    TotalSpent DECIMAL(18,2),
    AvgOrderValue DECIMAL(18,2)
)
AS
BEGIN
    INSERT INTO @Summary (Year, TotalOrders, TotalSpent, AvgOrderValue)
    SELECT 
        YEAR(o.order_date) AS Year,
        COUNT(*) AS TotalOrders,
        SUM(oi.quantity * oi.list_price * (1-oi.discount/100.0)) AS TotalSpent,
        AVG(oi.quantity * oi.list_price * (1-oi.discount/100.0)) AS AvgOrderValue
    FROM sales.orders o
    JOIN sales.order_items oi ON o.order_id = oi.order_id
    WHERE o.customer_id = @CustomerID
    GROUP BY YEAR(o.order_date);

    RETURN;
END;
GO

-- 12. DISCOUNT FUNCTION
CREATE FUNCTION dbo.CalculateBulkDiscount(@Qty INT)
RETURNS INT
AS
BEGIN
    DECLARE @Discount INT;

    IF @Qty BETWEEN 1 AND 2
        SET @Discount = 0;
    ELSE IF @Qty BETWEEN 3 AND 5
        SET @Discount = 5;
    ELSE IF @Qty BETWEEN 6 AND 9
        SET @Discount = 10;
    ELSE
        SET @Discount = 15;

    RETURN @Discount;
END;
GO



