-- 1. Recent orders with customer details.
SELECT TOP (20) o.Id, u.FullName, u.Email, o.OrderDate, o.Status, o.TotalAmount
FROM dbo.Orders AS o
JOIN dbo.AspNetUsers AS u ON u.Id = o.UserId
ORDER BY o.OrderDate DESC;

-- 2. Revenue and order count by month.
SELECT YEAR(OrderDate) AS [Year], MONTH(OrderDate) AS [Month], COUNT(*) AS OrderCount, SUM(TotalAmount) AS Revenue
FROM dbo.Orders
WHERE Status <> N'Cancelled'
GROUP BY YEAR(OrderDate), MONTH(OrderDate)
ORDER BY [Year], [Month];

-- 3. Products whose total sales exceed ten units.
SELECT p.Name, SUM(oi.Quantity) AS UnitsSold
FROM dbo.OrderItems AS oi
JOIN dbo.Products AS p ON p.Id = oi.ProductId
JOIN dbo.Orders AS o ON o.Id = oi.OrderId
WHERE o.Status <> N'Cancelled'
GROUP BY p.Id, p.Name
HAVING SUM(oi.Quantity) > 10
ORDER BY UnitsSold DESC;

-- 4. Customers whose lifetime order value is above the average customer value.
WITH CustomerTotals AS (
    SELECT UserId, SUM(TotalAmount) AS LifetimeValue
    FROM dbo.Orders
    WHERE Status <> N'Cancelled'
    GROUP BY UserId
)
SELECT u.FullName, u.Email, ct.LifetimeValue
FROM CustomerTotals AS ct
JOIN dbo.AspNetUsers AS u ON u.Id = ct.UserId
WHERE ct.LifetimeValue > (SELECT AVG(LifetimeValue) FROM CustomerTotals)
ORDER BY ct.LifetimeValue DESC;

-- 5. Rank products by revenue in each category.
WITH ProductRevenue AS (
    SELECT p.CategoryId, p.Name, SUM(oi.UnitPrice * oi.Quantity) AS Revenue
    FROM dbo.OrderItems AS oi
    JOIN dbo.Orders AS o ON o.Id = oi.OrderId
    JOIN dbo.Products AS p ON p.Id = oi.ProductId
    WHERE o.Status <> N'Cancelled'
    GROUP BY p.CategoryId, p.Id, p.Name
)
SELECT CategoryId, Name, Revenue,
       DENSE_RANK() OVER (PARTITION BY CategoryId ORDER BY Revenue DESC) AS CategoryRank
FROM ProductRevenue;

-- 6. Current low-stock items.
SELECT p.Name, c.Name AS Category, p.StockQuantity
FROM dbo.Products AS p
JOIN dbo.Categories AS c ON c.Id = p.CategoryId
WHERE p.IsAvailable = 1 AND p.StockQuantity <= 5
ORDER BY p.StockQuantity, p.Name;

-- 7. Average order value by delivery method.
SELECT DeliveryMethod, COUNT(*) AS OrderCount, AVG(TotalAmount) AS AverageOrderValue
FROM dbo.Orders
WHERE Status <> N'Cancelled'
GROUP BY DeliveryMethod;

-- 8. Items that have never appeared in an order.
SELECT p.Id, p.Name, p.Price
FROM dbo.Products AS p
WHERE NOT EXISTS (SELECT 1 FROM dbo.OrderItems AS oi WHERE oi.ProductId = p.Id)
ORDER BY p.Name;

-- 9. Monthly revenue with a running total.
WITH Monthly AS (
    SELECT DATEFROMPARTS(YEAR(OrderDate), MONTH(OrderDate), 1) AS MonthStart, SUM(TotalAmount) AS Revenue
    FROM dbo.Orders
    WHERE Status <> N'Cancelled'
    GROUP BY DATEFROMPARTS(YEAR(OrderDate), MONTH(OrderDate), 1)
)
SELECT MonthStart, Revenue,
       SUM(Revenue) OVER (ORDER BY MonthStart ROWS UNBOUNDED PRECEDING) AS RunningRevenue
FROM Monthly
ORDER BY MonthStart;

-- 10. Order composition with the price snapshot captured at purchase time.
SELECT o.Id AS OrderId, p.Name, oi.Quantity, oi.UnitPrice,
       oi.Quantity * oi.UnitPrice AS LineTotal, o.OrderDate
FROM dbo.OrderItems AS oi
JOIN dbo.Orders AS o ON o.Id = oi.OrderId
JOIN dbo.Products AS p ON p.Id = oi.ProductId
ORDER BY o.OrderDate DESC, o.Id;