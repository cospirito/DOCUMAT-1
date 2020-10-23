namespace DOCUMAT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveDependancetTablTraitement : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Traitements", "AgentID", "dbo.Agents");
            DropForeignKey("dbo.Traitements", "RegistreID", "dbo.Registres");
            DropIndex("dbo.Traitements", new[] { "RegistreID" });
            DropIndex("dbo.Traitements", new[] { "AgentID" });
        }
        
        public override void Down()
        {
            CreateIndex("dbo.Traitements", "AgentID");
            CreateIndex("dbo.Traitements", "RegistreID");
            AddForeignKey("dbo.Traitements", "RegistreID", "dbo.Registres", "RegistreID", cascadeDelete: true);
            AddForeignKey("dbo.Traitements", "AgentID", "dbo.Agents", "AgentID", cascadeDelete: true);
        }
    }
}
