﻿using BuildingBlocks.CQRS.Command;
using Ordering.Application.Data;
using Ordering.Application.Dtos;
using Ordering.Application.Exceptions;
using Ordering.Domain.Models;
using Ordering.Domain.ValueObjects;

namespace Ordering.Application.Orders.Commands.UpdateOrder;
public class UpdateOrderHandler(IApplicationDbContext dbContext)
    : ICommandHandler<UpdateOrderCommand, UpdateOrderResult>
{
    public async Task<UpdateOrderResult> Handle(UpdateOrderCommand command, CancellationToken cancellationToken)
    {
        //Update Order entity from command object
        //save to database
        //return result

        var orderId = OrderId.Of(command.Order.Id);
        var order = await dbContext.Orders
            .FindAsync([orderId], cancellationToken: cancellationToken);

        if (order is null)
        {
            throw new OrderNotFoundException(command.Order.Id);
        }

        UpdateOrderWithNewValues(order, command.Order);

        dbContext.Orders.Update(order);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateOrderResult(true);        
    }

    public void UpdateOrderWithNewValues(Order order, OrderDto orderDto)
    {
        var updatedShippingAddress = Address.Of(new CreateAddressDto(orderDto.ShippingAddress.AddressLine, orderDto.ShippingAddress.Country, orderDto.ShippingAddress.State, orderDto.ShippingAddress.ZipCode));
        var updatedBillingAddress = Address.Of(new CreateAddressDto(orderDto.BillingAddress.AddressLine, orderDto.BillingAddress.Country, orderDto.BillingAddress.State, orderDto.BillingAddress.ZipCode));
        var updatedPayment = Payment.Of(new CreatePaymentValueObjectDto( orderDto.Payment.CardName, orderDto.Payment.CardNumber, orderDto.Payment.Expiration, orderDto.Payment.Cvv, orderDto.Payment.PaymentMethod));

        order.Update(
            customerId: order.CustomerId,
            orderName: OrderName.Of(orderDto.OrderName),
            shippingAddress: updatedShippingAddress,
            billingAddress: updatedBillingAddress,
            payment: updatedPayment,
            status: orderDto.Status);
    }
}
