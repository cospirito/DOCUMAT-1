namespace DOCUMAT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TbTraitementAddObservation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Traitements", "Observation", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Traitements", "Observation");
        }
    }
}
