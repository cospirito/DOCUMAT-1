namespace DOCUMAT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class setDateNullableOnDateofSessionTravailTable : DbMigration
    {
        public override void Up()
        {
            //AddColumn("dbo.Traitements", "TableSelect", c => c.String());
            AlterColumn("dbo.SessionTravails", "DateDebut", c => c.DateTime());
            AlterColumn("dbo.SessionTravails", "DateFin", c => c.DateTime());
            //DropColumn("dbo.Traitements", "Table");
        }
        
        public override void Down()
        {
            //AddColumn("dbo.Traitements", "Table", c => c.String());
            AlterColumn("dbo.SessionTravails", "DateFin", c => c.DateTime(nullable: false));
            AlterColumn("dbo.SessionTravails", "DateDebut", c => c.DateTime(nullable: false));
            //DropColumn("dbo.Traitements", "TableSelect");
        }
    }
}
