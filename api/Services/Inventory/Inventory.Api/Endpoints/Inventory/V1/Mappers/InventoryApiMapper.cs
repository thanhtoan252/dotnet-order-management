using Inventory.Application.Items.Models;
using CmdDto = Inventory.Api.Endpoints.Inventory.V1.Commands.DTOs;
using QryDto = Inventory.Api.Endpoints.Inventory.V1.Queries.DTOs;

namespace Inventory.Api.Endpoints.Inventory.V1.Mappers;

internal static class InventoryApiMapper
{
    internal static CreateInventoryItemInput ToInput(this CmdDto.CreateInventoryItemRequest request)
    {
        return new CreateInventoryItemInput(
            request.ProductId,
            request.Sku,
            request.ProductName,
            request.InitialQuantity);
    }

    internal static ReceiveStockInput ToInput(this CmdDto.ReceiveStockRequest request)
    {
        return new ReceiveStockInput(request.Quantity);
    }

    internal static AdjustStockInput ToInput(this CmdDto.AdjustStockRequest request)
    {
        return new AdjustStockInput(request.OnHand, request.Reason);
    }

    internal static CmdDto.InventoryItemResponse ToCommandDto(this InventoryItemResult item)
    {
        return new CmdDto.InventoryItemResponse
        {
            ProductId = item.ProductId,
            Sku = item.Sku,
            ProductName = item.ProductName,
            OnHand = item.OnHand,
            Reserved = item.Reserved,
            Available = item.Available
        };
    }

    internal static QryDto.InventoryItemResponse ToQueryDto(this InventoryItemResult item)
    {
        return new QryDto.InventoryItemResponse
        {
            ProductId = item.ProductId,
            Sku = item.Sku,
            ProductName = item.ProductName,
            OnHand = item.OnHand,
            Reserved = item.Reserved,
            Available = item.Available
        };
    }
}
