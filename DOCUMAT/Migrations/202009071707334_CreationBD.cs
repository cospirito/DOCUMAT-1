namespace DOCUMAT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreationBD : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Agents",
                c => new
                    {
                        AgentID = c.Int(nullable: false, identity: true),
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
                .PrimaryKey(t => t.AgentID);
            
            CreateTable(
                "dbo.SessionTravails",
                c => new
                    {
                        SessionTravailID = c.Int(nullable: false, identity: true),
                        DateDebut = c.DateTime(nullable: false),
                        DateFin = c.DateTime(nullable: false),
                        AgentID = c.Int(nullable: false),
                        DateCreation = c.DateTime(nullable: false),
                        DateModif = c.DateTime(nullable: false),
                        TIMESTAMP = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.SessionTravailID)
                .ForeignKey("dbo.Agents", t => t.AgentID, cascadeDelete: true)
                .Index(t => t.AgentID);
            
            CreateTable(
                "dbo.Configs",
                c => new
                    {
                        ConfigsID = c.Int(nullable: false, identity: true),
                        NomApp = c.String(),
                        NomBD = c.String(),
                        NomEntreprise = c.String(),
                        VersionApp = c.String(),
                        VersionBD = c.String(),
                        DateCreation = c.DateTime(nullable: false),
                        DateModif = c.DateTime(nullable: false),
                        TIMESTAMP = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.ConfigsID);
            
            CreateTable(
                "dbo.Controles",
                c => new
                    {
                        ControleID = c.Int(nullable: false, identity: true),
                        RegistreId = c.Int(),
                        ImageID = c.Int(),
                        SequenceID = c.Int(),
                        NumeroPageImage_idx = c.Int(),
                        NomPageImage_idx = c.Int(),
                        RejetImage_idx = c.Int(),
                        MotifRejetImage_idx = c.String(),
                        OrdreSequence_idx = c.Int(),
                        DateSequence_idx = c.Int(),
                        RefSequence_idx = c.Int(),
                        RefRejetees_idx = c.String(),
                        PhaseControle = c.Int(),
                        StatutControle = c.Int(),
                        ASupprimer = c.Int(),
                        DateControle = c.DateTime(),
                        DateCreation = c.DateTime(nullable: false),
                        DateModif = c.DateTime(nullable: false),
                        TIMESTAMP = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.ControleID);
            
            CreateTable(
                "dbo.Corrections",
                c => new
                    {
                        CorrectionID = c.Int(nullable: false, identity: true),
                        RegistreId = c.Int(),
                        ImageID = c.Int(),
                        SequenceID = c.Int(),
                        NumeroPageImage_idx = c.Int(),
                        NomPageImage_idx = c.Int(),
                        RejetImage_idx = c.Int(),
                        MotifRejetImage_idx = c.String(),
                        OrdreSequence_idx = c.Int(),
                        DateSequence_idx = c.Int(),
                        RefSequence_idx = c.Int(),
                        RefRejetees_idx = c.String(),
                        PhaseCorrection = c.Int(),
                        StatutCorrection = c.Int(),
                        ASupprimer = c.Int(),
                        DateCorrection = c.DateTime(),
                        DateCreation = c.DateTime(nullable: false),
                        DateModif = c.DateTime(nullable: false),
                        TIMESTAMP = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.CorrectionID);
            
            CreateTable(
                "dbo.Images",
                c => new
                    {
                        ImageID = c.Int(nullable: false, identity: true),
                        CheminImage = c.String(),
                        Type = c.String(),
                        Taille = c.Single(nullable: false),
                        DateScan = c.DateTime(nullable: false),
                        NumeroPage = c.Int(nullable: false),
                        NomPage = c.String(),
                        typeInstance = c.String(),
                        ObservationInstance = c.String(),
                        StatutActuel = c.Int(nullable: false),
                        DebutSequence = c.Int(nullable: false),
                        FinSequence = c.Int(nullable: false),
                        DateDebutSequence = c.DateTime(nullable: false),
                        RegistreID = c.Int(nullable: false),
                        DateCreation = c.DateTime(nullable: false),
                        DateModif = c.DateTime(nullable: false),
                        TIMESTAMP = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.ImageID)
                .ForeignKey("dbo.Registres", t => t.RegistreID, cascadeDelete: true)
                .Index(t => t.RegistreID);
            
            CreateTable(
                "dbo.Registres",
                c => new
                    {
                        RegistreID = c.Int(nullable: false, identity: true),
                        QrCode = c.String(),
                        Numero = c.Int(nullable: false),
                        DateDepotDebut = c.DateTime(nullable: false),
                        DateDepotFin = c.DateTime(nullable: false),
                        NumeroDepotDebut = c.Int(nullable: false),
                        NumeroDepotFin = c.Int(nullable: false),
                        NombrePageDeclaree = c.Int(nullable: false),
                        NombrePage = c.Int(nullable: false),
                        Observation = c.String(),
                        StatutActuel = c.Int(nullable: false),
                        CheminDossier = c.String(),
                        Type = c.String(),
                        VersementID = c.Int(nullable: false),
                        DateCreation = c.DateTime(nullable: false),
                        DateModif = c.DateTime(nullable: false),
                        TIMESTAMP = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.RegistreID)
                .ForeignKey("dbo.Versements", t => t.VersementID, cascadeDelete: true)
                .Index(t => t.VersementID);
            
            CreateTable(
                "dbo.StatutRegistres",
                c => new
                    {
                        StatutRegistreID = c.Int(nullable: false, identity: true),
                        Code = c.Int(nullable: false),
                        DateDebut = c.DateTime(nullable: false),
                        DateFin = c.DateTime(),
                        RegistreID = c.Int(nullable: false),
                        DateCreation = c.DateTime(nullable: false),
                        DateModif = c.DateTime(nullable: false),
                        TIMESTAMP = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.StatutRegistreID)
                .ForeignKey("dbo.Registres", t => t.RegistreID, cascadeDelete: true)
                .Index(t => t.RegistreID);
            
            CreateTable(
                "dbo.Versements",
                c => new
                    {
                        VersementID = c.Int(nullable: false, identity: true),
                        NumeroVers = c.Int(nullable: false),
                        NomAgentVersant = c.String(),
                        PrenomsAgentVersant = c.String(),
                        DateVers = c.DateTime(nullable: false),
                        cheminBordereau = c.String(),
                        LivraisonID = c.Int(nullable: false),
                        DateCreation = c.DateTime(nullable: false),
                        DateModif = c.DateTime(nullable: false),
                        TIMESTAMP = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.VersementID)
                .ForeignKey("dbo.Livraisons", t => t.LivraisonID, cascadeDelete: true)
                .Index(t => t.LivraisonID);
            
            CreateTable(
                "dbo.Livraisons",
                c => new
                    {
                        LivraisonID = c.Int(nullable: false, identity: true),
                        Numero = c.Int(nullable: false),
                        DateLivraison = c.DateTime(nullable: false),
                        ServiceID = c.Int(nullable: false),
                        DateCreation = c.DateTime(nullable: false),
                        DateModif = c.DateTime(nullable: false),
                        TIMESTAMP = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.LivraisonID)
                .ForeignKey("dbo.Services", t => t.ServiceID, cascadeDelete: true)
                .Index(t => t.ServiceID);
            
            CreateTable(
                "dbo.Services",
                c => new
                    {
                        ServiceID = c.Int(nullable: false, identity: true),
                        Code = c.String(nullable: false),
                        Nom = c.String(nullable: false),
                        Description = c.String(),
                        NomChefDeService = c.String(),
                        PrenomChefDeService = c.String(),
                        CheminDossier = c.String(nullable: false),
                        NombreR3 = c.Int(nullable: false),
                        NombreR4 = c.Int(nullable: false),
                        RegionID = c.Int(nullable: false),
                        DateCreation = c.DateTime(nullable: false),
                        DateModif = c.DateTime(nullable: false),
                        TIMESTAMP = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.ServiceID)
                .ForeignKey("dbo.Regions", t => t.RegionID, cascadeDelete: true)
                .Index(t => t.RegionID);
            
            CreateTable(
                "dbo.Regions",
                c => new
                    {
                        RegionID = c.Int(nullable: false, identity: true),
                        Nom = c.String(nullable: false),
                        DateCreation = c.DateTime(nullable: false),
                        DateModif = c.DateTime(nullable: false),
                        TIMESTAMP = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.RegionID);
            
            CreateTable(
                "dbo.StatutImages",
                c => new
                    {
                        StatutImageID = c.Int(nullable: false, identity: true),
                        Code = c.Int(nullable: false),
                        DateDebut = c.DateTime(nullable: false),
                        DateFin = c.DateTime(),
                        ImageID = c.Int(nullable: false),
                        DateCreation = c.DateTime(nullable: false),
                        DateModif = c.DateTime(nullable: false),
                        TIMESTAMP = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.StatutImageID)
                .ForeignKey("dbo.Images", t => t.ImageID, cascadeDelete: true)
                .Index(t => t.ImageID);
            
            CreateTable(
                "dbo.ManquantImages",
                c => new
                    {
                        ManquantImageID = c.Int(nullable: false, identity: true),
                        IdRegistre = c.Int(nullable: false),
                        IdImage = c.Int(),
                        NumeroPage = c.Int(nullable: false),
                        DebutSequence = c.Int(nullable: false),
                        FinSequence = c.Int(nullable: false),
                        statutManquant = c.Int(nullable: false),
                        DateSequenceDebut = c.DateTime(),
                        DateDeclareManquant = c.DateTime(nullable: false),
                        DateCorrectionManquant = c.DateTime(),
                        DateCreation = c.DateTime(nullable: false),
                        DateModif = c.DateTime(nullable: false),
                        TIMESTAMP = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.ManquantImageID);
            
            CreateTable(
                "dbo.ManquantSequences",
                c => new
                    {
                        ManquantSequenceID = c.Int(nullable: false, identity: true),
                        IdImage = c.Int(nullable: false),
                        IdSequence = c.Int(),
                        NumeroOrdre = c.Int(nullable: false),
                        statutManquant = c.Int(nullable: false),
                        DateDeclareManquant = c.DateTime(),
                        DateCorrectionManquant = c.DateTime(),
                        DateCreation = c.DateTime(nullable: false),
                        DateModif = c.DateTime(nullable: false),
                        TIMESTAMP = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.ManquantSequenceID);
            
            CreateTable(
                "dbo.Sequences",
                c => new
                    {
                        SequenceID = c.Int(nullable: false, identity: true),
                        NUmeroOdre = c.Int(nullable: false),
                        DateSequence = c.DateTime(nullable: false),
                        References = c.String(),
                        NombreDeReferences = c.Int(nullable: false),
                        isSpeciale = c.String(),
                        ImageID = c.Int(nullable: false),
                        PhaseActuelle = c.Int(nullable: false),
                        DateCreation = c.DateTime(nullable: false),
                        DateModif = c.DateTime(nullable: false),
                        TIMESTAMP = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.SequenceID)
                .ForeignKey("dbo.Images", t => t.ImageID, cascadeDelete: true)
                .Index(t => t.ImageID);
            
            CreateTable(
                "dbo.Traitements",
                c => new
                    {
                        TraitementID = c.Int(nullable: false, identity: true),
                        RegistreID = c.Int(nullable: false),
                        AgentID = c.Int(nullable: false),
                        TypeTraitement = c.Int(nullable: false),
                        DateCreation = c.DateTime(nullable: false),
                        DateModif = c.DateTime(nullable: false),
                        TIMESTAMP = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.TraitementID)
                .ForeignKey("dbo.Agents", t => t.AgentID, cascadeDelete: true)
                .ForeignKey("dbo.Registres", t => t.RegistreID, cascadeDelete: true)
                .Index(t => t.RegistreID)
                .Index(t => t.AgentID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Traitements", "RegistreID", "dbo.Registres");
            DropForeignKey("dbo.Traitements", "AgentID", "dbo.Agents");
            DropForeignKey("dbo.Sequences", "ImageID", "dbo.Images");
            DropForeignKey("dbo.StatutImages", "ImageID", "dbo.Images");
            DropForeignKey("dbo.Registres", "VersementID", "dbo.Versements");
            DropForeignKey("dbo.Versements", "LivraisonID", "dbo.Livraisons");
            DropForeignKey("dbo.Livraisons", "ServiceID", "dbo.Services");
            DropForeignKey("dbo.Services", "RegionID", "dbo.Regions");
            DropForeignKey("dbo.StatutRegistres", "RegistreID", "dbo.Registres");
            DropForeignKey("dbo.Images", "RegistreID", "dbo.Registres");
            DropForeignKey("dbo.SessionTravails", "AgentID", "dbo.Agents");
            DropIndex("dbo.Traitements", new[] { "AgentID" });
            DropIndex("dbo.Traitements", new[] { "RegistreID" });
            DropIndex("dbo.Sequences", new[] { "ImageID" });
            DropIndex("dbo.StatutImages", new[] { "ImageID" });
            DropIndex("dbo.Services", new[] { "RegionID" });
            DropIndex("dbo.Livraisons", new[] { "ServiceID" });
            DropIndex("dbo.Versements", new[] { "LivraisonID" });
            DropIndex("dbo.StatutRegistres", new[] { "RegistreID" });
            DropIndex("dbo.Registres", new[] { "VersementID" });
            DropIndex("dbo.Images", new[] { "RegistreID" });
            DropIndex("dbo.SessionTravails", new[] { "AgentID" });
            DropTable("dbo.Traitements");
            DropTable("dbo.Sequences");
            DropTable("dbo.ManquantSequences");
            DropTable("dbo.ManquantImages");
            DropTable("dbo.StatutImages");
            DropTable("dbo.Regions");
            DropTable("dbo.Services");
            DropTable("dbo.Livraisons");
            DropTable("dbo.Versements");
            DropTable("dbo.StatutRegistres");
            DropTable("dbo.Registres");
            DropTable("dbo.Images");
            DropTable("dbo.Corrections");
            DropTable("dbo.Controles");
            DropTable("dbo.Configs");
            DropTable("dbo.SessionTravails");
            DropTable("dbo.Agents");
        }
    }
}
