USE StoreDB;
GO


-- 1. Non-clustered index 

CREATE NONCLUSTERED INDEX IX_Customers_Email
ON sales.customers(email);
GO


--2. Composite index 
   

CREATE NONCLUSTERED INDEX IX_Products_Category_Brand
ON production.products(category_id, brand_id);
GO


--3. Index on sales.orders(order_date)

CREATE NONCLUSTERED INDEX IX_Orders_OrderDate
ON sales.orders(order_date)
INCLUDE (customer_id, store_id, order_status);
GO


--4. Customer log table + trigger

CREATE TABLE sales.customer_log (
    log_id INT IDENTITY(1,1) PRIMARY KEY,
    customer_id INT,
    action VARCHAR(50),
    log_date DATETIME DEFAULT GETDATE()
);
GO

CREATE TRIGGER trg_AfterInsert_Customers
ON sales.customers
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO sales.customer_log (customer_id, action)
    SELECT customer_id, 'Customer Created'
    FROM inserted;
END;
GO


--5. Price history table + trigger

CREATE TABLE production.price_history (
    history_id INT IDENTITY(1,1) PRIMARY KEY,
    product_id INT,
    old_price DECIMAL(10,2),
    new_price DECIMAL(10,2),
    change_date DATETIME DEFAULT GETDATE(),
    changed_by VARCHAR(100)
);
GO

CREATE TRIGGER trg_Product_Price_Change
ON production.products
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO production.price_history (
        product_id,
        old_price,
        new_price,
        changed_by
    )
    SELECT
        d.product_id,
        d.list_price,
        i.list_price,
        SUSER_NAME()
    FROM deleted d
    JOIN inserted i
        ON d.product_id = i.product_id
    WHERE d.list_price <> i.list_price;
END;
GO


--6. INSTEAD OF DELETE trigger on categories
CREATE TRIGGER trg_Prevent_Category_Delete
ON production.categories
INSTEAD OF DELETE
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM production.products p
        JOIN deleted d ON p.category_id = d.category_id
    )
    BEGIN
        RAISERROR (
            'Cannot delete category: associated products exist.',
            16, 1
        );
        RETURN;
    END

    DELETE FROM production.categories
    WHERE category_id IN (SELECT category_id FROM deleted);
END;
GO


--7. Reduce stock when order item is inserted
CREATE TRIGGER trg_Update_Stock_On_OrderItem
ON sales.order_items
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE s
    SET s.quantity = s.quantity - i.quantity
    FROM production.stocks s
    JOIN inserted i
        ON s.product_id = i.product_id
    JOIN sales.orders o
        ON i.order_id = o.order_id
    WHERE s.store_id = o.store_id;
END;
GO


--8. Order audit table + trigger
CREATE TABLE sales.order_audit (
    audit_id INT IDENTITY(1,1) PRIMARY KEY,
    order_id INT,
    customer_id INT,
    store_id INT,
    staff_id INT,
    order_date DATE,
    audit_timestamp DATETIME DEFAULT GETDATE()
);
GO

CREATE TRIGGER trg_Order_Audit
ON sales.orders
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO sales.order_audit (
        order_id,
        customer_id,
        store_id,
        staff_id,
        order_date
    )
    SELECT
        order_id,
        customer_id,
        store_id,
        staff_id,
        order_date
    FROM inserted;
END;
GO
