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

        public DocumatContext() : base("name=DOCUMAT") {}
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
        public static void AddTraitement(string Table, int TabeID, int AgentId, int TypeTraitement,string Observation = null)
        {
            using (var ct = new Models.DocumatContext())
            {
                Models.Traitement traitement = new Models.Traitement()
                {
                    DateCreation = DateTime.Now,
                    DateModif = DateTime.Now,
                    TableID = TabeID,
                    TableSelect = Table,
                    AgentID = AgentId,
                    TypeTraitement = TypeTraitement,
                    Observation = Observation
                };
                ct.Traitement.Add(traitement);
                ct.SaveChanges();
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
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.

            //Ajout de l'age,t admmin par défaut 
            context.Agent.Add(new Agent()
            {
                Nom = "ADMIN0",
                Prenom = "ADMIN0",
                DateCreation = DateTime.Now,
                DateModif = DateTime.Now,
                Affectation = 5, // superviseur
                Login = "Admin0",
                Mdp = "Admin0@DOCUMAT",
                Matricule = "ADMIN0",
                DateNaiss = DateTime.Now,
                Genre = Enum.GetName(typeof(Enumeration.Genre), 1),
                StatutMat = Enum.GetName(typeof(Enumeration.StatutMatrimonial), 0)
            });
            context.SaveChanges();

            // Ajout des Regions du Maroc .......
            List<Region> Regions = new List<Region>()
            {
                new Region { DateCreation = DateTime.Now, DateModif = DateTime.Now, Nom = "Région de Tanger-Tétouan-Al Hoceïma"},
                new Region { DateCreation = DateTime.Now, DateModif = DateTime.Now, Nom = "Région de l'Oriental"},
                new Region { DateCreation = DateTime.Now, DateModif = DateTime.Now, Nom = "Région de Fès-Meknès"},
                new Region { DateCreation = DateTime.Now, DateModif = DateTime.Now, Nom = "Région de Rabat-Salé-Kénitra"},
                new Region { DateCreation = DateTime.Now, DateModif = DateTime.Now, Nom = "Région de Béni Mellal-Khénifra"},
                new Region { DateCreation = DateTime.Now, DateModif = DateTime.Now, Nom = "Région de Casablanca-Settat"},
                new Region { DateCreation = DateTime.Now, DateModif = DateTime.Now, Nom = "Région de Drâa-Tafilalet"},
                new Region { DateCreation = DateTime.Now, DateModif = DateTime.Now, Nom = "Région de Souss-Massa"},
                new Region { DateCreation = DateTime.Now, DateModif = DateTime.Now, Nom = "Région de Guelmim-Oued Noun"},
                new Region { DateCreation = DateTime.Now, DateModif = DateTime.Now, Nom = "Région de Laâyoune-Sakia El Hamra"},
                new Region { DateCreation = DateTime.Now, DateModif = DateTime.Now, Nom = "Région de Dakhla-Oued Ed Dahab"}
            };

            foreach (var region in Regions)
            {
                context.Region.Add(region);
            }
            context.SaveChanges();

            // Ajout des Services des Regions  ajoutés
            List<Service> Services = new List<Service>()
            {
                //Trouver des noms de code unique à chaque service
                // Insertion des services de la region "Région de Tanger-Tétouan-Al Hoceïma"
                new Service { Nom = "ALHOCEIMA", NombreR3 = 15, NombreR4 = 37, Code = "AL", CheminDossier= @"DOCUMAT\ALHOCEIMA", DateCreation = DateTime.Now, DateModif = DateTime.Now,
                    RegionID = context.Region.FirstOrDefault(r=>r.Nom == "Région de Tanger-Tétouan-Al Hoceïma").RegionID},

                new Service { Nom = "LARACHE", NombreR3 = 24, NombreR4 = 60, Code = "LA", CheminDossier=@"DOCUMAT\LARACHE", DateCreation = DateTime.Now, DateModif = DateTime.Now,
                    RegionID = context.Region.FirstOrDefault(r=>r.Nom == "Région de Tanger-Tétouan-Al Hoceïma").RegionID},

                new Service { Nom = "MDIQ FNIDEQ", NombreR3 = 3, NombreR4 = 28, Code = "LA", CheminDossier=@"DOCUMAT\MDIQ FNIDEQ", DateCreation = DateTime.Now, DateModif = DateTime.Now,
                    RegionID = context.Region.FirstOrDefault(r=>r.Nom == "Région de Tanger-Tétouan-Al Hoceïma").RegionID},

                new Service { Nom = "TANGER", NombreR3 = 266, NombreR4 = 18, Code = "TA", CheminDossier=@"DOCUMAT\TANGER", DateCreation = DateTime.Now, DateModif = DateTime.Now,
                    RegionID = context.Region.FirstOrDefault(r=>r.Nom == "Région de Tanger-Tétouan-Al Hoceïma").RegionID},

                new Service { Nom = "TANGER BANI MAKADA", NombreR3 = 9, NombreR4 = 57, Code = "TM", CheminDossier=@"DOCUMAT\TANGER BANI MAKADA", DateCreation = DateTime.Now, DateModif = DateTime.Now,
                    RegionID = context.Region.FirstOrDefault(r=>r.Nom == "Région de Tanger-Tétouan-Al Hoceïma").RegionID},

                new Service { Nom = "TETOUAN", NombreR3 = 20, NombreR4 = 68, Code = "TE", CheminDossier=@"DOCUMAT\TETOUAN", DateCreation = DateTime.Now, DateModif = DateTime.Now,
                    RegionID = context.Region.FirstOrDefault(r=>r.Nom == "Région de Tanger-Tétouan-Al Hoceïma").RegionID},

                // Insertion des services de la region "Région de l'Oriental"
                new Service { Nom = "BERKANE", NombreR3 = 7, NombreR4 = 82, Code = "BE", CheminDossier=@"DOCUMAT\BERKANE", DateCreation = DateTime.Now, DateModif = DateTime.Now,
                    RegionID = context.Region.FirstOrDefault(r=>r.Nom == "Région de l'Oriental").RegionID},

                new Service { Nom = "GUERCIF", NombreR3 = 6, NombreR4 = 13, Code = "GU", CheminDossier=@"DOCUMAT\GUERCIF", DateCreation = DateTime.Now, DateModif = DateTime.Now,
                    RegionID = context.Region.FirstOrDefault(r=>r.Nom == "Région de l'Oriental").RegionID},

                new Service { Nom = "NADOR", NombreR3 = 30, NombreR4 = 58, Code = "NA", CheminDossier=@"DOCUMAT\NADOR", DateCreation = DateTime.Now, DateModif = DateTime.Now,
                    RegionID = context.Region.FirstOrDefault(r=>r.Nom == "Région de l'Oriental").RegionID},

                new Service { Nom = "OUJDA", NombreR3 = 29, NombreR4 = 225, Code = "OU", CheminDossier=@"DOCUMAT\OUJDA", DateCreation = DateTime.Now, DateModif = DateTime.Now,
                    RegionID = context.Region.FirstOrDefault(r=>r.Nom == "Région de l'Oriental").RegionID},

                new Service { Nom = "OUJDA ANGAD", NombreR3 = 4, NombreR4 = 49, Code = "OA", CheminDossier=@"DOCUMAT\OUJDA ANGAD", DateCreation = DateTime.Now, DateModif = DateTime.Now,
                    RegionID = context.Region.FirstOrDefault(r=>r.Nom == "Région de l'Oriental").RegionID},

                new Service { Nom = "TAOURIRT", NombreR3 = 5, NombreR4 = 19, Code = "TR", CheminDossier=@"DOCUMAT\TAOURIRT", DateCreation = DateTime.Now, DateModif = DateTime.Now,
                    RegionID = context.Region.FirstOrDefault(r=>r.Nom == "Région de l'Oriental").RegionID}
            };

            foreach (var service in Services)
            {
                context.Service.Add(service);
            }
            context.SaveChanges();

            base.Seed(context);
        }
    }
}
