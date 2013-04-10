namespace CIS726_Assignment2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WorkingOnUsersMig : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.Users");

            CreateTable(
                "dbo.Users",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        username = c.String(nullable: false, maxLength: 100),
                        realName = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.ID);
        }
        
        public override void Down()
        {
            DropTable("dbo.Users");

            CreateTable(
                "dbo.Users",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        username = c.String(nullable: false, maxLength: 100),
                        realName = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.ID);
           
        }
    }
}