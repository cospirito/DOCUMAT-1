namespace DOCUMAT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateTableAgentMapping : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SessionTravails", "AgentMatchingTable_AgentMatchingTableID", "dbo.AgentMatchingTables");
            DropIndex("dbo.SessionTravails", new[] { "AgentMatchingTable_AgentMatchingTableID" });
            DropColumn("dbo.SessionTravails", "AgentMatchingTable_AgentMatchingTableID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SessionTravails", "AgentMatchingTable_AgentMatchingTableID", c => c.Int());
            CreateIndex("dbo.SessionTravails", "AgentMatchingTable_AgentMatchingTableID");
            AddForeignKey("dbo.SessionTravails", "AgentMatchingTable_AgentMatchingTableID", "dbo.AgentMatchingTables", "AgentMatchingTableID");
        }
    }
}
