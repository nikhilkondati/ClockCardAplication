namespace ClockCardAplication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Customer",
                c => new
                    {
                        customerId = c.Int(nullable: false, identity: true),
                        userId = c.Int(),
                        firstName = c.String(nullable: false),
                        lastName = c.String(nullable: false),
                        status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.customerId)
                .ForeignKey("dbo.User", t => t.userId)
                .Index(t => t.userId);
            
            CreateTable(
                "dbo.Project",
                c => new
                    {
                        projectId = c.Int(nullable: false, identity: true),
                        userId = c.Int(),
                        customerId = c.Int(),
                        name = c.String(nullable: false),
                        status = c.Int(nullable: false),
                        startDate = c.DateTime(nullable: false),
                        expectedEndDate = c.DateTime(nullable: false),
                        timeSpentInHours = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.projectId)
                .ForeignKey("dbo.Customer", t => t.customerId)
                .ForeignKey("dbo.User", t => t.userId)
                .Index(t => t.userId)
                .Index(t => t.customerId);
            
            CreateTable(
                "dbo.TimeLog",
                c => new
                    {
                        timeLogId = c.Int(nullable: false, identity: true),
                        userId = c.Int(),
                        customerId = c.Int(),
                        projectId = c.Int(),
                        date = c.DateTime(nullable: false),
                        timeSpentInHours = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.timeLogId)
                .ForeignKey("dbo.Customer", t => t.customerId)
                .ForeignKey("dbo.Project", t => t.projectId)
                .ForeignKey("dbo.User", t => t.userId)
                .Index(t => t.userId)
                .Index(t => t.customerId)
                .Index(t => t.projectId);
            
            CreateTable(
                "dbo.User",
                c => new
                    {
                        userId = c.Int(nullable: false, identity: true),
                        firstName = c.String(nullable: false),
                        lastName = c.String(nullable: false),
                        email = c.String(nullable: false),
                        password = c.String(nullable: false),
                        repeatPassword = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.userId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Customer", "userId", "dbo.User");
            DropForeignKey("dbo.Project", "userId", "dbo.User");
            DropForeignKey("dbo.TimeLog", "userId", "dbo.User");
            DropForeignKey("dbo.TimeLog", "projectId", "dbo.Project");
            DropForeignKey("dbo.TimeLog", "customerId", "dbo.Customer");
            DropForeignKey("dbo.Project", "customerId", "dbo.Customer");
            DropIndex("dbo.TimeLog", new[] { "projectId" });
            DropIndex("dbo.TimeLog", new[] { "customerId" });
            DropIndex("dbo.TimeLog", new[] { "userId" });
            DropIndex("dbo.Project", new[] { "customerId" });
            DropIndex("dbo.Project", new[] { "userId" });
            DropIndex("dbo.Customer", new[] { "userId" });
            DropTable("dbo.User");
            DropTable("dbo.TimeLog");
            DropTable("dbo.Project");
            DropTable("dbo.Customer");
        }
    }
}
