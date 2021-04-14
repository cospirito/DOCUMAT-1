using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOCUMAT.Models
{
    public class DocumatContext : DbContext
    {
        // définition des noms de table
        public static string TbRegion = "Regions";
        public static string TbService = "Services";
        public static string TbLivraison = "Livraisons";
        public static string TbVersement = "Versements";
        public static string TbRegistre = "Registres";
        public static string TbImage = "Images";
        public static string TbSequence = "Sequences";
        public static string TbAgent = "Agents";
        public static string TbStatutRegistre = "StatutRegistres";
        public static string TbStatutImage = "StatutImages";
        public static string TbControle = "Controles";
        public static string TbCorrection = "Corrections";
        public static string TbManquantImage = "ManquantImages";
        public static string TbManquantSequence = "ManquantSequences";
        public static string TbTraitement = "Traitements";
        public static string TbConfigs = "Configs";
        public static string TbSessionTravail = "SessionTravails";
        public static string TbUnite = "Unites";
        public static string TbTranche = "Tranches";
        public static string TbFacture = "Factures";
        public static string TbMappageImage = "MappageImages";
        public static string TbMappageSequence = "MappageImages";

        public DbSet<Region> Region { get; set; }
        public DbSet<Service> Service { get; set; }
        public DbSet<Livraison> Livraison { get; set; }
        public DbSet<Versement> Versement { get; set; }
        public DbSet<Registre> Registre { get; set; }
        public DbSet<Image> Image { get; set; }
        public DbSet<Sequence> Sequence { get; set; }
        public DbSet<Agent> Agent { get; set; }
        public DbSet<StatutRegistre> StatutRegistre { get; set; }
        public DbSet<StatutImage> StatutImage { get; set; }
        public DbSet<Controle> Controle { get; set; }
        public DbSet<Correction> Correction { get; set; }
        public DbSet<ManquantImage> ManquantImage { get; set; }
        public DbSet<ManquantSequence> ManquantSequences { get; set; }
        public DbSet<Traitement> Traitement { get; set; }
        public DbSet<Configs> Configs { get; set; }
        public DbSet<SessionTravail> SessionTravails { get; set; }
        public DbSet<Unite> Unites { get; set; }
        public DbSet<Tranche> Tranches { get; set; }
        public DbSet<Facture> Factures { get; set; }
        public DbSet<MappageImage> mappageImages { get; set; }
        public DbSet<MappageSequence> mappageSequences { get; set; }
        public DbSet<AgentMatchingTable> agentMatchingTables { get; set; }

        public DocumatContext() : base("name=DOCUMAT") 
        {
            base.Database.CommandTimeout = 300;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<DocumatContext>(null);
            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Fonction static permettant de renseigner rapidemen un traitement
        /// </summary>
        /// <param name="Table"> Table Traitée </param>
        /// <param name="TabeID"> Id de L'élement de la Table Traitée</param>
        /// <param name="AgentId"> Id de l'agent à la base du traitement </param>
        /// <param name="TypeTraitement"> Int du type de traitement CUD et plus voir Enumeration TypeTraitement </param>
        public static void AddTraitement(string Table, int TabeID, int AgentId, int TypeTraitement,string Observation = null,DateTime? dateCreation = null,
                                         DateTime? dateModif = null)
        {
            using (var ct = new DocumatContext())
            {
                // Ajout d'un traitement Manuel
                ct.Database.ExecuteSqlCommand($"INSERT INTO {TbTraitement} ([TableID],[AgentID],[TypeTraitement]," +
                    $"[TableSelect],[Observation],[DateCreation],[DateModif]) VALUES ({TabeID},{AgentId},{TypeTraitement}" +
                    $",'{Table}','{Observation}',GETDATE(),GETDATE());");
            }
        }

        //redéfinir si voulu, le nom des tables à gérer
        //protected override void OnModelCreating(DbModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);
        //    modelBuilder.Entity<Service>().ToTable("SERVICE", "dbo");
        //}
    }

    // Définition d'une classe d'initilalisation de la bd
    public class DocumatDbInitializer : CreateDatabaseIfNotExists<DocumatContext>
    {
        //public DocumatDbInitializer(DocumatContext context) :base()
        //{
        //    Seed(context);
        //}

        protected override void Seed(DocumatContext context)
        {
        }
    }
}
