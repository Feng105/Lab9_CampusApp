namespace CampusApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StudentCampusValidation : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Students", "Name", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.Students", "Address", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.UniversityCampus", "Name", c => c.String(nullable: false, maxLength: 50));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.UniversityCampus", "Name", c => c.String());
            AlterColumn("dbo.Students", "Address", c => c.String());
            AlterColumn("dbo.Students", "Name", c => c.String());
        }
    }
}
