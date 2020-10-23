namespace DOCUMAT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class nombreRegistre : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Versements", "NombreRegistreR3", c => c.Int(nullable: false));
            AddColumn("dbo.Versements", "NombreRegistreR4", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Versements", "NombreRegistreR4");
            DropColumn("dbo.Versements", "NombreRegistreR3");
        }
    }
}
