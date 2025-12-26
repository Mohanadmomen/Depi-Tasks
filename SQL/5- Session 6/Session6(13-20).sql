-- 13. CUSTOMER ORDER HISTORY PROCEDURE
CREATE PROCEDURE sp_GetCustomerOrderHistory
    @CustomerID INT,
    @StartDate DATE = NULL,
    @EndDate DATE = NULL
AS
BEGIN
    SELECT o.order_id, o.order_date, o.required_date, o.shipped_date,
           SUM(oi.quantity * oi.list_price * (1-oi.discount/100.0)) AS OrderTotal
    FROM sales.orders o
    JOIN sales.order_items oi ON o.order_id = oi.order_id
    WHERE o.customer_id = @CustomerID
      AND (@StartDate IS NULL OR o.order_date >= @StartDate)
      AND (@EndDate IS NULL OR o.order_date <= @EndDate)
    GROUP BY o.order_id, o.order_date, o.required_date, o.shipped_date
    ORDER BY o.order_date;
END;
GO

-- 14. INVENTORY RESTOCK PROCEDURE
CREATE PROCEDURE sp_RestockProduct
    @StoreID INT,
    @ProductID INT,
    @RestockQty INT,
    @OldQty INT OUTPUT,
    @NewQty INT OUTPUT,
    @Success BIT OUTPUT
AS
BEGIN
    SELECT @OldQty = quantity
    FROM production.stocks
    WHERE store_id = @StoreID AND product_id = @ProductID;

    IF @OldQty IS NULL
    BEGIN
        SET @Success = 0;
        SET @NewQty = NULL;
    END
    ELSE
    BEGIN
        UPDATE production.stocks
        SET quantity = quantity + @RestockQty
        WHERE store_id = @StoreID AND product_id = @ProductID;

        SELECT @NewQty = quantity
        FROM production.stocks
        WHERE store_id = @StoreID AND product_id = @ProductID;

        SET @Success = 1;
    END
END;
GO


-- 15. ORDER PROCESSING PROCEDURE
CREATE PROCEDURE sp_ProcessNewOrder
    @CustomerID INT,
    @ProductID INT,
    @Quantity INT,
    @StoreID INT
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @StaffID INT;
        SELECT TOP 1 @StaffID = staff_id FROM sales.staffs WHERE store_id = @StoreID ORDER BY staff_id;

        DECLARE @OrderID INT;
        INSERT INTO sales.orders (customer_id, order_status, order_date, required_date, store_id, staff_id)
        VALUES (@CustomerID, 1, GETDATE(), DATEADD(DAY,7,GETDATE()), @StoreID, @StaffID);

        SET @OrderID = SCOPE_IDENTITY();

        INSERT INTO sales.order_items (order_id, item_id, product_id, quantity, list_price, discount)
        SELECT @OrderID, 1, @ProductID, @Quantity, list_price, 0
        FROM production.products
        WHERE product_id = @ProductID;

        UPDATE production.stocks
        SET quantity = quantity - @Quantity
        WHERE store_id = @StoreID AND product_id = @ProductID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        PRINT ERROR_MESSAGE();
    END CATCH
END;
GO

-- 16. DYNAMIC PRODUCT SEARCH PROCEDURE
CREATE PROCEDURE sp_SearchProducts
    @NameSearch NVARCHAR(255) = NULL,
    @CategoryID INT = NULL,
    @MinPrice DECIMAL(10,2) = NULL,
    @MaxPrice DECIMAL(10,2) = NULL,
    @SortColumn NVARCHAR(50) = 'product_name'
AS
BEGIN
    DECLARE @SQL NVARCHAR(MAX);
    SET @SQL = 'SELECT p.product_id, p.product_name, p.list_price, b.brand_name, c.category_name
                FROM production.products p
                JOIN production.brands b ON p.brand_id = b.brand_id
                JOIN production.categories c ON p.category_id = c.category_id
                WHERE 1=1';

    IF @NameSearch IS NOT NULL
        SET @SQL = @SQL + ' AND p.product_name LIKE ''%' + @NameSearch + '%''';

    IF @CategoryID IS NOT NULL
        SET @SQL = @SQL + ' AND p.category_id = ' + CAST(@CategoryID AS NVARCHAR);

    IF @MinPrice IS NOT NULL
        SET @SQL = @SQL + ' AND p.list_price >= ' + CAST(@MinPrice AS NVARCHAR);

    IF @MaxPrice IS NOT NULL
        SET @SQL = @SQL + ' AND p.list_price <= ' + CAST(@MaxPrice AS NVARCHAR);

    SET @SQL = @SQL + ' ORDER BY ' + @SortColumn;

    EXEC sp_executesql @SQL;
END;
GO


--17 Staff Bonus Calculation System


DECLARE @StartDate DATE = '2024-01-01';
DECLARE @EndDate   DATE = '2024-03-31';

DECLARE @LowRate  DECIMAL(5,2) = 0.02;
DECLARE @MidRate  DECIMAL(5,2) = 0.05;
DECLARE @HighRate DECIMAL(5,2) = 0.08;

SELECT 
    s.staff_id,
    s.first_name + ' ' + s.last_name AS StaffName,
    SUM(oi.quantity * oi.list_price * (1 - oi.discount / 100.0)) AS TotalSales,
    CASE
        WHEN SUM(oi.quantity * oi.list_price * (1 - oi.discount / 100.0)) < 50000
            THEN SUM(oi.quantity * oi.list_price * (1 - oi.discount / 100.0)) * @LowRate
        WHEN SUM(oi.quantity * oi.list_price * (1 - oi.discount / 100.0)) BETWEEN 50000 AND 100000
            THEN SUM(oi.quantity * oi.list_price * (1 - oi.discount / 100.0)) * @MidRate
        ELSE
            SUM(oi.quantity * oi.list_price * (1 - oi.discount / 100.0)) * @HighRate
    END AS BonusAmount
FROM sales.staffs s
JOIN sales.orders o ON s.staff_id = o.staff_id
JOIN sales.order_items oi ON o.order_id = oi.order_id
WHERE o.order_date BETWEEN @StartDate AND @EndDate
  AND o.order_status = 4
GROUP BY s.staff_id, s.first_name, s.last_name;





--18 Smart Inventory Management

SELECT 
    p.product_id,
    p.product_name,
    c.category_name,
    s.quantity AS CurrentStock,
    CASE
        WHEN c.category_name = 'Bikes' THEN
            CASE 
                WHEN s.quantity < 10 THEN 50
                ELSE 0
            END
        WHEN c.category_name = 'Accessories' THEN
            CASE 
                WHEN s.quantity < 20 THEN 100
                ELSE 0
            END
        ELSE
            CASE
                WHEN s.quantity < 15 THEN 30
                ELSE 0
            END
    END AS ReorderQuantity
FROM production.stocks s
JOIN production.products p ON s.product_id = p.product_id
JOIN production.categories c ON p.category_id = c.category_id;



--19 Customer Loyalty Tier Assignment
SELECT 
    c.customer_id,
    c.first_name + ' ' + c.last_name AS CustomerName,
    ISNULL(SUM(oi.quantity * oi.list_price * (1 - oi.discount / 100.0)), 0) AS TotalSpent,
    CASE
        WHEN SUM(oi.quantity * oi.list_price * (1 - oi.discount / 100.0)) IS NULL
            THEN 'No Activity'
        WHEN SUM(oi.quantity * oi.list_price * (1 - oi.discount / 100.0)) < 5000
            THEN 'Bronze'
        WHEN SUM(oi.quantity * oi.list_price * (1 - oi.discount / 100.0)) BETWEEN 5000 AND 20000
            THEN 'Silver'
        ELSE 'Gold'
    END AS LoyaltyTier
FROM sales.customers c
LEFT JOIN sales.orders o ON c.customer_id = o.customer_id
LEFT JOIN sales.order_items oi ON o.order_id = oi.order_id
GROUP BY c.customer_id, c.first_name, c.last_name;

--20 Product Lifecycle Management (Stored Procedure)
CREATE PROCEDURE production.DiscontinueProduct
    @ProductID INT,
    @ReplacementProductID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Check for pending or processing orders
    IF EXISTS (
        SELECT 1
        FROM sales.order_items oi
        JOIN sales.orders o ON oi.order_id = o.order_id
        WHERE oi.product_id = @ProductID
          AND o.order_status IN (1,2)
    )
    BEGIN
        IF @ReplacementProductID IS NULL
        BEGIN
            PRINT 'ERROR: Product has pending orders and no replacement provided.';
            RETURN;
        END
        ELSE
        BEGIN
            UPDATE sales.order_items
            SET product_id = @ReplacementProductID
            WHERE product_id = @ProductID;

            PRINT 'Pending orders updated with replacement product.';
        END
    END

    -- Clear stock
    DELETE FROM production.stocks
    WHERE product_id = @ProductID;

    -- Remove product
    DELETE FROM production.products
    WHERE product_id = @ProductID;

    PRINT 'Product successfully discontinued and inventory cleared.';
END;
