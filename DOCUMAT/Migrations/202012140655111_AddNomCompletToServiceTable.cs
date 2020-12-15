namespace DOCUMAT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNomCompletToServiceTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Services", "NomComplet", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Services", "NomComplet");
        }
    }
}
