namespace DOCUMAT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TableTraitementModif : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Traitements", "TableID", c => c.Int(nullable: false));
            AddColumn("dbo.Traitements", "Table", c => c.String());
            DropColumn("dbo.Traitements", "RegistreID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Traitements", "RegistreID", c => c.Int(nullable: false));
            DropColumn("dbo.Traitements", "Table");
            DropColumn("dbo.Traitements", "TableID");
        }
    }
}
