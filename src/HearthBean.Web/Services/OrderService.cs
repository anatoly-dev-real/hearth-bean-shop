using System.Data;
using HearthBean.Web.Data;
using HearthBean.Web.Data.Entities;
using HearthBean.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;

namespace HearthBean.Web.Services;

public sealed class OrderService(AppDbContext db, ILogger<OrderService> logger)
{
    public async Task<int> PlaceAsync(string userId, CheckoutViewModel checkout, CancellationToken cancellationToken = default)
    {
        if (checkout.DeliveryMethod == "Delivery" && string.IsNullOrWhiteSpace(checkout.DeliveryAddress))
        {
            throw new InvalidOperationException("Delivery address is required.");
        }

        int orderId;
        await db.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            await using var command = db.Database.GetDbConnection().CreateCommand();
            command.CommandText = "dbo.sp_PlaceOrder";
            command.CommandType = CommandType.StoredProcedure;
            if (db.Database.CurrentTransaction is not null)
            {
                command.Transaction = db.Database.CurrentTransaction.GetDbTransaction();
            }

            AddParameter(command, "@UserId", DbType.String, userId, 450);
            AddParameter(command, "@DeliveryMethod", DbType.String, checkout.DeliveryMethod, 20);
            AddParameter(command, "@DeliveryAddress", DbType.String, (object?)checkout.DeliveryAddress ?? DBNull.Value, 300);
            AddParameter(command, "@Phone", DbType.String, checkout.Phone, 32);
            AddParameter(command, "@Note", DbType.String, (object?)checkout.Note ?? DBNull.Value, 500);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            orderId = Convert.ToInt32(result, System.Globalization.CultureInfo.InvariantCulture);
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }

        logger.LogInformation("Order {OrderId} placed for customer {UserId}", orderId, userId);
        return orderId;
    }

    private static void AddParameter(System.Data.Common.DbCommand command, string name, DbType type, object value, int size)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.DbType = type;
        parameter.Size = size;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    public async Task<bool> CancelByCustomerAsync(int orderId, string userId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        var order = await db.Orders.Include(item => item.Items).ThenInclude(item => item.Product)
            .SingleOrDefaultAsync(item => item.Id == orderId && item.UserId == userId, cancellationToken);
        if (order is null || !DomainRules.CanCustomerCancel(order, userId))
        {
            return false;
        }

        DomainRules.RestoreStock(order.Items);
        order.Status = OrderStatuses.Cancelled;
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Order {OrderId} cancelled by its customer", orderId);
        return true;
    }

    public async Task<bool> SetStatusAsync(int orderId, string nextStatus, CancellationToken cancellationToken = default)
    {
        if (!OrderStatuses.All.Contains(nextStatus, StringComparer.Ordinal))
        {
            return false;
        }

        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        var order = await db.Orders.Include(item => item.Items).ThenInclude(item => item.Product)
            .SingleOrDefaultAsync(item => item.Id == orderId, cancellationToken);
        if (order is null || !DomainRules.CanTransition(order.Status, nextStatus))
        {
            return false;
        }

        if (nextStatus == OrderStatuses.Cancelled)
        {
            DomainRules.RestoreStock(order.Items);
        }

        order.Status = nextStatus;
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Order {OrderId} status changed to {Status}", orderId, nextStatus);
        return true;
    }
}