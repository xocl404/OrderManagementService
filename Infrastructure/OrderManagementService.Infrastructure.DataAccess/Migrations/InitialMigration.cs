using FluentMigrator;

namespace OrderManagementService.Infrastructure.DataAccess.Migrations;

[Migration(1)]
public sealed class InitialMigration : Migration
{
    public override void Up()
    {
        Execute.Sql("create type order_state as enum ('created', 'processing', 'completed', 'cancelled');");
        Execute.Sql("create type order_history_item_kind as enum ('created', 'item_added', 'item_removed', 'state_changed');");

        Create.Table("products")
            .WithColumn("product_id").AsInt64().PrimaryKey().Identity()
            .WithColumn("product_name").AsString().NotNullable()
            .WithColumn("product_price").AsDecimal(18, 2).NotNullable();

        Create.Table("orders")
            .WithColumn("order_id").AsInt64().PrimaryKey().Identity()
            .WithColumn("order_state").AsCustom("order_state").NotNullable()
            .WithColumn("order_created_at").AsDateTimeOffset().NotNullable()
            .WithColumn("order_created_by").AsString().NotNullable();

        Create.Table("order_items")
            .WithColumn("order_item_id").AsInt64().PrimaryKey().Identity()
            .WithColumn("order_id").AsInt64().NotNullable().ForeignKey("orders", "order_id")
            .WithColumn("product_id").AsInt64().NotNullable().ForeignKey("products", "product_id")
            .WithColumn("order_item_quantity").AsInt32().NotNullable()
            .WithColumn("order_item_deleted").AsBoolean().NotNullable();

        Create.Table("order_history")
            .WithColumn("order_history_item_id").AsInt64().PrimaryKey().Identity()
            .WithColumn("order_id").AsInt64().NotNullable().ForeignKey("orders", "order_id")
            .WithColumn("order_history_item_created_at").AsDateTimeOffset().NotNullable()
            .WithColumn("order_history_item_kind").AsCustom("order_history_item_kind").NotNullable()
            .WithColumn("order_history_item_payload").AsCustom("jsonb").NotNullable();
    }

    public override void Down()
    {
        Delete.Table("order_history");
        Delete.Table("order_items");
        Delete.Table("orders");
        Delete.Table("products");
        Execute.Sql("drop type order_history_item_kind;");
        Execute.Sql("drop type order_state;");
    }
}
