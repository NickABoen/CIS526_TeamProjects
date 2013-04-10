namespace CIS726_Assignment2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedShortNameforElectiveListMig : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ElectiveLists", "shortName", c => c.String(nullable: false, maxLength: 20));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ElectiveLists", "shortName");
        }
    }
}