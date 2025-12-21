
USE StoreDB
GO







--1.Write a query that classifies all products into price categories:

SELECT 
    product_name,
    list_price,
    CASE 
        WHEN list_price < 300 THEN 'Economy'
        WHEN list_price BETWEEN 300 AND 999 THEN 'Standard'
        WHEN list_price BETWEEN 1000 AND 2499 THEN 'Premium'
        ELSE 'Luxury'
    END AS PriceCategory
FROM production.products;


--2.Create a query that shows order processing information with user-friendly status descriptions:

SELECT 
    order_id,
    order_date,
    CASE order_status
        WHEN 1 THEN 'Order Received'
        WHEN 2 THEN 'In Preparation'
        WHEN 3 THEN 'Order Cancelled'
        WHEN 4 THEN 'Order Delivered'
    END AS StatusDescription,
    CASE 
        WHEN order_status = 1 
             AND DATEDIFF(DAY, order_date, GETDATE()) > 5 THEN 'URGENT'
        WHEN order_status = 2 
             AND DATEDIFF(DAY, order_date, GETDATE()) > 3 THEN 'HIGH'
        ELSE 'NORMAL'
    END AS PriorityLevel
FROM sales.orders;


--3.Write a query that categorizes staff based on the number of orders they've handled:

SELECT 
    s.staff_id,
    s.first_name + ' ' + s.last_name AS StaffName,
    COUNT(o.order_id) AS TotalOrders,
    CASE 
        WHEN COUNT(o.order_id) = 0 THEN 'New Staff'
        WHEN COUNT(o.order_id) BETWEEN 1 AND 10 THEN 'Junior Staff'
        WHEN COUNT(o.order_id) BETWEEN 11 AND 25 THEN 'Senior Staff'
        ELSE 'Expert Staff'
    END AS StaffLevel
FROM sales.staffs s
LEFT JOIN sales.orders o
    ON s.staff_id = o.staff_id
GROUP BY s.staff_id, s.first_name, s.last_name;


--4.Create a query that handles missing customer contact information:



SELECT 
    customer_id,
    first_name,
    last_name,
    ISNULL(phone, 'Phone Not Available') AS Phone,
    email,
    COALESCE(phone, email, 'No Contact Method') AS PreferredContact
FROM sales.customers;




-- 5.Write a query that safely calculates price per unit in stock:

SELECT 
    p.product_name,
    s.quantity,
    ISNULL(p.list_price / NULLIF(s.quantity, 0), 0) AS PricePerUnit,
    CASE 
        WHEN s.quantity = 0 THEN 'Out of Stock'
        WHEN s.quantity < 10 THEN 'Low Stock'
        ELSE 'In Stock'
    END AS StockStatus
FROM production.products p
JOIN production.stocks s
    ON p.product_id = s.product_id
WHERE s.store_id = 1;

--6.Create a query that formats complete addresses safely:
SELECT 
    customer_id,
    COALESCE(street, '') + ', ' +
    COALESCE(city, '') + ', ' +
    COALESCE(state, '') + 
    CASE 
        WHEN zip_code IS NULL THEN ''
        ELSE ' ' + zip_code
    END AS FormattedAddress
FROM sales.customers;


--7.Use a CTE to find customers who have spent more than $1,500 total:



WITH CustomerSpending AS (
    SELECT 
        o.customer_id,
        SUM(oi.quantity * oi.list_price) AS TotalSpent
    FROM sales.orders o
    JOIN sales.order_items oi
        ON o.order_id = oi.order_id
    GROUP BY o.customer_id
)
SELECT 
    c.customer_id,
    c.first_name + ' ' + c.last_name AS CustomerName,
    cs.TotalSpent
FROM CustomerSpending cs
JOIN sales.customers c
    ON cs.customer_id = c.customer_id
WHERE cs.TotalSpent > 1500
ORDER BY cs.TotalSpent DESC;



