using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookStore.Data.DTOs
{
    public record OrderItemInputDto(
        [Required] int BookId,
        [Required][Range(1, 100)] int Quantity
    );

    public record PlaceOrderDto(
        [Required][MinLength(1)] List<OrderItemInputDto> Items
    );

    public record PurchaseItemDto(
        int PurchaseItemId,
        int BookId,
        string BookTitle,
        int Quantity,
        decimal UnitPrice
    );

    public record PurchaseDto(
        int PurchaseId,
        int CustomerId,
        string CustomerEmail,
        DateTime PurchaseDate,
        List<PurchaseItemDto> Items,
        decimal TotalAmount
    );
}
