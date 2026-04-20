using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStockQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed the new Inventory service by re-emitting ProductCreatedIntegrationEvent for every
            // existing product. The historical StockQuantity becomes the InitialStockQuantity. The
            // OutboxProcessor will publish these to Kafka; ProductCreatedConsumer in Inventory is
            // idempotent (skips if an InventoryItem already exists), so this is safe to retry.
            migrationBuilder.Sql(@"
INSERT INTO OutboxMessages (Id, Topic, PartitionKey, EventType, Payload, CreatedAt, RetryCount)
SELECT NEWID(),
       'catalog.product-created',
       CONVERT(nvarchar(36), p.Id),
       'Shared.Contracts.IntegrationEvents.ProductCreatedIntegrationEvent, Shared.Contracts',
       CONCAT('{""EventId"":""', NEWID(),
              '"",""OccurredOn"":""', CONVERT(nvarchar(33), SYSUTCDATETIME(), 127), 'Z',
              '"",""ProductId"":""', p.Id,
              '"",""Sku"":""', STRING_ESCAPE(p.SKU, 'json'),
              '"",""Name"":""', STRING_ESCAPE(p.Name, 'json'),
              '"",""InitialStockQuantity"":', p.StockQuantity, '}'),
       SYSUTCDATETIME(),
       0
FROM Products p
WHERE p.IsDeleted = 0
  AND NOT EXISTS (
      SELECT 1 FROM OutboxMessages o
      WHERE o.Topic = 'catalog.product-created'
        AND o.PartitionKey = CONVERT(nvarchar(36), p.Id)
  );
");

            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
