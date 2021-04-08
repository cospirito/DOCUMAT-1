namespace DOCUMAT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class setNullableId : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AgentMatchingTables", "Casa_id", c => c.Int());
            AlterColumn("dbo.AgentMatchingTables", "Rabat_id", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AgentMatchingTables", "Rabat_id", c => c.Int(nullable: false));
            AlterColumn("dbo.AgentMatchingTables", "Casa_id", c => c.Int(nullable: false));
        }
    }
}
