using System.Data.Entity.Migrations;

public partial class InitialCreate : DbMigration
{
    public override void Up()
    {
        CreateTable(
            "dbo.Roles",
            c => new
            {
                Id = c.Int(nullable: false, identity: true),
                Name = c.String(nullable: false, maxLength: 50),
            })
            .PrimaryKey(t => t.Id);

        CreateTable(
            "dbo.Users",
            c => new
            {
                Id = c.Int(nullable: false, identity: true),
                FullName = c.String(nullable: false, maxLength: 100),
                Login = c.String(nullable: false, maxLength: 50),
                PasswordHash = c.String(nullable: false),
                RoleId = c.Int(nullable: false),
                IsArchived = c.Boolean(nullable: false),
            })
            .PrimaryKey(t => t.Id)
            .ForeignKey("dbo.Roles", t => t.RoleId, cascadeDelete: true)
            .Index(t => t.RoleId);

        CreateTable(
            "dbo.Products",
            c => new
            {
                Id = c.Int(nullable: false, identity: true),
                Name = c.String(nullable: false, maxLength: 200),
                Code = c.String(maxLength: 50),
                IsArchived = c.Boolean(nullable: false),
            })
            .PrimaryKey(t => t.Id);

        CreateTable(
            "dbo.RawMaterials",
            c => new
            {
                Id = c.Int(nullable: false, identity: true),
                Name = c.String(nullable: false, maxLength: 200),
                Code = c.String(maxLength: 50),
                IsArchived = c.Boolean(nullable: false),
            })
            .PrimaryKey(t => t.Id);

        CreateTable(
            "dbo.Equipment",
            c => new
            {
                Id = c.Int(nullable: false, identity: true),
                Name = c.String(nullable: false, maxLength: 150),
                Type = c.String(),
                IsArchived = c.Boolean(nullable: false),
            })
            .PrimaryKey(t => t.Id);

        CreateTable(
            "dbo.Recipes",
            c => new
            {
                Id = c.Int(nullable: false, identity: true),
                ProductId = c.Int(nullable: false),
                Version = c.String(maxLength: 20),
                Status = c.String(maxLength: 30),
                CreatedAt = c.DateTime(nullable: false),
            })
            .PrimaryKey(t => t.Id)
            .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
            .Index(t => t.ProductId);

        CreateTable(
            "dbo.RecipeComponents",
            c => new
            {
                Id = c.Int(nullable: false, identity: true),
                RecipeId = c.Int(nullable: false),
                RawMaterialId = c.Int(nullable: false),
                Percentage = c.Decimal(nullable: false, precision: 5, scale: 2),
                LoadOrder = c.Int(nullable: false),
            })
            .PrimaryKey(t => t.Id)
            .ForeignKey("dbo.Recipes", t => t.RecipeId, cascadeDelete: true)
            .ForeignKey("dbo.RawMaterials", t => t.RawMaterialId, cascadeDelete: true)
            .Index(t => t.RecipeId)
            .Index(t => t.RawMaterialId);

        CreateTable(
            "dbo.TechnologicalCards",
            c => new
            {
                Id = c.Int(nullable: false, identity: true),
                ProductId = c.Int(nullable: false),
                Version = c.String(maxLength: 20),
                Status = c.String(maxLength: 30),
            })
            .PrimaryKey(t => t.Id)
            .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
            .Index(t => t.ProductId);

        CreateTable(
            "dbo.TechnologicalSteps",
            c => new
            {
                Id = c.Int(nullable: false, identity: true),
                CardId = c.Int(nullable: false),
                StepNumber = c.Int(nullable: false),
                StepType = c.String(maxLength: 100),
                ParametersJson = c.String(),
                IsMandatory = c.Boolean(nullable: false),
            })
            .PrimaryKey(t => t.Id)
            .ForeignKey("dbo.TechnologicalCards", t => t.CardId, cascadeDelete: true)
            .Index(t => t.CardId);

        CreateTable(
            "dbo.ProductionBatches",
            c => new
            {
                Id = c.Int(nullable: false, identity: true),
                RecipeId = c.Int(nullable: false),
                CardId = c.Int(nullable: false),
                StartDate = c.DateTime(nullable: false),
                Status = c.String(maxLength: 30),
                PlannedQuantity = c.Decimal(nullable: false, precision: 18, scale: 2),
            })
            .PrimaryKey(t => t.Id)
            .ForeignKey("dbo.Recipes", t => t.RecipeId, cascadeDelete: true)
            .ForeignKey("dbo.TechnologicalCards", t => t.CardId, cascadeDelete: true)
            .Index(t => t.RecipeId)
            .Index(t => t.CardId);

        CreateTable(
            "dbo.LabTests",
            c => new
            {
                Id = c.Int(nullable: false, identity: true),
                BatchId = c.Int(nullable: false),
                AssignedAt = c.DateTime(nullable: false),
                TestType = c.String(),
            })
            .PrimaryKey(t => t.Id)
            .ForeignKey("dbo.ProductionBatches", t => t.BatchId, cascadeDelete: true)
            .Index(t => t.BatchId);

        CreateTable(
            "dbo.LabTestParameterResults",
            c => new
            {
                Id = c.Int(nullable: false, identity: true),
                TestId = c.Int(nullable: false),
                ParameterName = c.String(),
                MinValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                MaxValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                ActualValue = c.Decimal(precision: 18, scale: 2),
                Decision = c.String(),
            })
            .PrimaryKey(t => t.Id)
            .ForeignKey("dbo.LabTests", t => t.TestId, cascadeDelete: true)
            .Index(t => t.TestId);

        // Фильтрованный уникальный индекс: только одна утверждённая рецептура на продукт
        Sql(@"
            CREATE UNIQUE INDEX IX_Recipe_Product_Approved
            ON dbo.Recipes (ProductId, Status)
            WHERE Status = 'Approved';
        ");

        // Фильтрованный уникальный индекс: только одна активная техкарта на продукт
        Sql(@"
            CREATE UNIQUE INDEX IX_Card_Product_Active
            ON dbo.TechnologicalCards (ProductId, Status)
            WHERE Status = 'Active';
        ");

        // Триггер: запрет утверждения рецептуры, если сумма долей компонентов != 100
        Sql(@"
            CREATE TRIGGER trg_Recipe_SumCheck
            ON dbo.Recipes
            AFTER UPDATE
            AS
            BEGIN
                IF UPDATE(Status)
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM inserted i
                        WHERE i.Status = 'Approved'
                        AND (SELECT SUM(Percentage) FROM dbo.RecipeComponents WHERE RecipeId = i.Id) <> 100
                    )
                    BEGIN
                        RAISERROR('Нельзя утвердить рецептуру: сумма долей компонентов не равна 100%', 16, 1);
                        ROLLBACK TRANSACTION;
                    END
                END
            END;
        ");
    }

    public override void Down()
    {
        Sql("DROP TRIGGER IF EXISTS trg_Recipe_SumCheck;");
        Sql("DROP INDEX IF EXISTS IX_Recipe_Product_Approved ON dbo.Recipes;");
        Sql("DROP INDEX IF EXISTS IX_Card_Product_Active ON dbo.TechnologicalCards;");

        DropForeignKey("dbo.LabTestParameterResults", "TestId", "dbo.LabTests");
        DropForeignKey("dbo.LabTests", "BatchId", "dbo.ProductionBatches");
        DropForeignKey("dbo.ProductionBatches", "CardId", "dbo.TechnologicalCards");
        DropForeignKey("dbo.ProductionBatches", "RecipeId", "dbo.Recipes");
        DropForeignKey("dbo.TechnologicalSteps", "CardId", "dbo.TechnologicalCards");
        DropForeignKey("dbo.TechnologicalCards", "ProductId", "dbo.Products");
        DropForeignKey("dbo.RecipeComponents", "RawMaterialId", "dbo.RawMaterials");
        DropForeignKey("dbo.RecipeComponents", "RecipeId", "dbo.Recipes");
        DropForeignKey("dbo.Recipes", "ProductId", "dbo.Products");
        DropForeignKey("dbo.Users", "RoleId", "dbo.Roles");

        DropIndex("dbo.LabTestParameterResults", new[] { "TestId" });
        DropIndex("dbo.LabTests", new[] { "BatchId" });
        DropIndex("dbo.ProductionBatches", new[] { "CardId" });
        DropIndex("dbo.ProductionBatches", new[] { "RecipeId" });
        DropIndex("dbo.TechnologicalSteps", new[] { "CardId" });
        DropIndex("dbo.TechnologicalCards", new[] { "ProductId" });
        DropIndex("dbo.RecipeComponents", new[] { "RawMaterialId" });
        DropIndex("dbo.RecipeComponents", new[] { "RecipeId" });
        DropIndex("dbo.Recipes", new[] { "ProductId" });
        DropIndex("dbo.Users", new[] { "RoleId" });

        DropTable("dbo.LabTestParameterResults");
        DropTable("dbo.LabTests");
        DropTable("dbo.ProductionBatches");
        DropTable("dbo.TechnologicalSteps");
        DropTable("dbo.TechnologicalCards");
        DropTable("dbo.RecipeComponents");
        DropTable("dbo.Recipes");
        DropTable("dbo.Equipment");
        DropTable("dbo.RawMaterials");
        DropTable("dbo.Products");
        DropTable("dbo.Users");
        DropTable("dbo.Roles");
    }
}