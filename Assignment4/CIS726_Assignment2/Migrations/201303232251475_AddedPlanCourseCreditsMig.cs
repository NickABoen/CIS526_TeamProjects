namespace CIS726_Assignment2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPlanCourseCreditsMig : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PlanCourses", "credits", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PlanCourses", "credits");
        }
    }
}
