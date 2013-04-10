namespace CIS726_Assignment2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPrerequisitesMig : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PrerequisiteCourses",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        prerequisiteCourseID = c.Int(nullable: false),
                        prerequisiteForCourseID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Courses", t => t.prerequisiteForCourseID, cascadeDelete: true)
                .ForeignKey("dbo.Courses", t => t.prerequisiteCourseID, cascadeDelete: false)
                .Index(t => t.prerequisiteForCourseID)
                .Index(t => t.prerequisiteCourseID);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.PrerequisiteCourses", new[] { "prerequisiteCourseID" });
            DropIndex("dbo.PrerequisiteCourses", new[] { "prerequisiteForCourseID" });
            DropForeignKey("dbo.PrerequisiteCourses", "prerequisiteCourseID", "dbo.Courses");
            DropForeignKey("dbo.PrerequisiteCourses", "prerequisiteForCourseID", "dbo.Courses");
            DropTable("dbo.PrerequisiteCourses");
        }
    }
}