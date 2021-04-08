namespace DOCUMAT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AgentMatchingTable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SessionTravails", "AgentID", "dbo.Agents");
            CreateTable(
                "dbo.AgentMatchingTables",
                c => new
                    {
                        AgentMatchingTableID = c.Int(nullable: false, identity: true),
                        Casa_id = c.Int(nullable: false),
                        Rabat_id = c.Int(nullable: false),
                        Matricule = c.String(nullable: false),
                        Nom = c.String(nullable: false),
                        Prenom = c.String(nullable: false),
                        Genre = c.String(nullable: false),
                        DateNaiss = c.DateTime(),
                        StatutMat = c.String(),
                        Affectation = c.Int(nullable: false),
                        CheminPhoto = c.String(),
                        Login = c.String(nullable: false),
                        Mdp = c.String(nullable: false),
                        DateCreation = c.DateTime(nullable: false),
                        DateModif = c.DateTime(nullable: false),
                        TIMESTAMP = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.AgentMatchingTableID);
            
            AddColumn("dbo.SessionTravails", "AgentMatchingTable_AgentMatchingTableID", c => c.Int());
            CreateIndex("dbo.SessionTravails", "AgentMatchingTable_AgentMatchingTableID");
            AddForeignKey("dbo.SessionTravails", "AgentMatchingTable_AgentMatchingTableID", "dbo.AgentMatchingTables", "AgentMatchingTableID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SessionTravails", "AgentMatchingTable_AgentMatchingTableID", "dbo.AgentMatchingTables");
            DropIndex("dbo.SessionTravails", new[] { "AgentMatchingTable_AgentMatchingTableID" });
            DropColumn("dbo.SessionTravails", "AgentMatchingTable_AgentMatchingTableID");
            DropTable("dbo.AgentMatchingTables");
            AddForeignKey("dbo.SessionTravails", "AgentID", "dbo.Agents", "AgentID", cascadeDelete: true);
        }
    }
}
