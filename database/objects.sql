CREATE OR ALTER VIEW dbo.vw_OrderSummary
AS
SELECT
    o.Id AS OrderId,
    o.UserId,
    u.FullName AS CustomerName,
    u.Email AS CustomerEmail,
    o.OrderDate,
    o.Status,
    o.DeliveryMethod,
    o.TotalAmount
FROM dbo.Orders AS o
INNER JOIN dbo.AspNetUsers AS u ON u.Id = o.UserId;
GO

CREATE OR ALTER VIEW dbo.vw_TopProducts
AS
SELECT
    p.Id AS ProductId,
    p.Name AS ProductName,
    SUM(oi.Quantity) AS UnitsSold,
    SUM(oi.UnitPrice * oi.Quantity) AS GrossSales
FROM dbo.OrderItems AS oi
INNER JOIN dbo.Orders AS o ON o.Id = oi.OrderId
INNER JOIN dbo.Products AS p ON p.Id = oi.ProductId
WHERE o.Status <> N'Cancelled'
GROUP BY p.Id, p.Name;
GO

CREATE OR ALTER PROCEDURE dbo.sp_PlaceOrder
    @UserId NVARCHAR(450),
    @DeliveryMethod NVARCHAR(20),
    @DeliveryAddress NVARCHAR(300) = NULL,
    @Phone NVARCHAR(32),
    @Note NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @DeliveryMethod NOT IN (N'Pickup', N'Delivery')
            THROW 50001, 'Delivery method is invalid.', 1;

        IF @DeliveryMethod = N'Delivery' AND NULLIF(LTRIM(RTRIM(@DeliveryAddress)), N'') IS NULL
            THROW 50002, 'Delivery address is required.', 1;

        IF NULLIF(LTRIM(RTRIM(@Phone)), N'') IS NULL
            THROW 50003, 'Phone number is required.', 1;

        IF NOT EXISTS (
            SELECT 1
            FROM dbo.CartItems WITH (UPDLOCK, HOLDLOCK)
            WHERE UserId = @UserId
        )
            THROW 50004, 'The cart is empty.', 1;

        IF EXISTS (
            SELECT 1
            FROM dbo.CartItems AS ci WITH (UPDLOCK, HOLDLOCK)
            INNER JOIN dbo.Products AS p WITH (UPDLOCK, HOLDLOCK) ON p.Id = ci.ProductId
            WHERE ci.UserId = @UserId
              AND (ci.Quantity < 1 OR ci.Quantity > 20 OR ci.Quantity > p.StockQuantity OR p.IsAvailable = 0)
        )
            THROW 50005, 'One or more products are no longer available in the requested quantity.', 1;

        DECLARE @TotalAmount DECIMAL(10, 2);
        SELECT @TotalAmount = SUM(CONVERT(DECIMAL(10, 2), p.Price * ci.Quantity))
        FROM dbo.CartItems AS ci
        INNER JOIN dbo.Products AS p ON p.Id = ci.ProductId
        WHERE ci.UserId = @UserId;

        DECLARE @CreatedOrder TABLE (Id INT NOT NULL);
        INSERT INTO dbo.Orders (UserId, OrderDate, Status, DeliveryMethod, DeliveryAddress, Phone, Note, TotalAmount)
        OUTPUT INSERTED.Id INTO @CreatedOrder (Id)
        VALUES (@UserId, SYSUTCDATETIME(), N'Pending', @DeliveryMethod,
            CASE WHEN @DeliveryMethod = N'Delivery' THEN @DeliveryAddress ELSE NULL END,
            @Phone, @Note, @TotalAmount);

        DECLARE @OrderId INT = (SELECT Id FROM @CreatedOrder);

        INSERT INTO dbo.OrderItems (OrderId, ProductId, Quantity, UnitPrice)
        SELECT @OrderId, ci.ProductId, ci.Quantity, p.Price
        FROM dbo.CartItems AS ci
        INNER JOIN dbo.Products AS p ON p.Id = ci.ProductId
        WHERE ci.UserId = @UserId;

        UPDATE p
        SET p.StockQuantity = p.StockQuantity - ci.Quantity
        FROM dbo.Products AS p
        INNER JOIN dbo.CartItems AS ci ON ci.ProductId = p.Id
        WHERE ci.UserId = @UserId;

        DELETE FROM dbo.CartItems WHERE UserId = @UserId;

        COMMIT TRANSACTION;
        SELECT @OrderId AS [Value];
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_MonthlyRevenue
    @Months INT = 12
AS
BEGIN
    SET NOCOUNT ON;
    IF @Months < 1 OR @Months > 60
        THROW 50006, 'Months must be between 1 and 60.', 1;

    SELECT
        YEAR(OrderDate) AS [Year],
        MONTH(OrderDate) AS [Month],
        COUNT_BIG(*) AS OrderCount,
        SUM(TotalAmount) AS Revenue
    FROM dbo.Orders
    WHERE Status <> N'Cancelled'
      AND OrderDate >= DATEADD(MONTH, -@Months + 1, DATEFROMPARTS(YEAR(GETUTCDATE()), MONTH(GETUTCDATE()), 1))
    GROUP BY YEAR(OrderDate), MONTH(OrderDate)
    ORDER BY [Year], [Month];
END;
GO