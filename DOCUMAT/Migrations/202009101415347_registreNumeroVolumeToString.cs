namespace DOCUMAT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class registreNumeroVolumeToString : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Registres", "Numero", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Registres", "Numero", c => c.Int(nullable: false));
        }
    }
}
