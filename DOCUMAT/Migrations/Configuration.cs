namespace DOCUMAT.Migrations
{
    using DOCUMAT.Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<DOCUMAT.Models.DocumatContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;           
        }

        protected override void Seed(DOCUMAT.Models.DocumatContext context)
        {
            if (true == true)
            {
                //  This method will be called after migrating to the latest version.

                //  You can use the DbSet<T>.AddOrUpdate() helper extension method
                //  to avoid creating duplicate seed data.

                //  Ajout des parametres de configuration de Base de l"application
                context.Configs.Add(new Configs()
                {
                    NomApp = "DOCUMAT",
                    NomBD = "DOCUMAT",
                    NomEntreprise = "FINASHORE",
                    VersionApp = "1.1",
                    VersionBD = "1",
                    DateCreation = DateTime.Now,
                    DateModif = DateTime.Now,
                });

                //Ajout de l"age,t admmin par defaut 
                context.Agent.Add(new Agent()
                {
                    Nom = "ADMIN0",
                    Prenom = "ADMIN0",
                    DateCreation = DateTime.Now,
                    DateModif = DateTime.Now,
                    Affectation = 6, // Administrateur
                    Login = "Admin0",
                    Mdp = "Admin0@DOCUMAT",
                    Matricule = "ADMIN0",
                    DateNaiss = DateTime.Now,
                    Genre = Enum.GetName(typeof(Enumeration.Genre), 1),
                    StatutMat = Enum.GetName(typeof(Enumeration.StatutMatrimonial), 0)
                }); ;
                context.SaveChanges();

                // Ajout des Regions du Maroc .......
                List<Region> Regions = new List<Region>()
                {
                    new Region { DateCreation = DateTime.Now, DateModif = DateTime.Now, Nom = "Region de Tanger-Tetouan-Al Hoceima"},
                    new Region { DateCreation = DateTime.Now, DateModif = DateTime.Now, Nom = "Region de l'Oriental"},
                    new Region { DateCreation = DateTime.Now, DateModif = DateTime.Now, Nom = "Region de Fes-Meknes"},
                    new Region { DateCreation = DateTime.Now, DateModif = DateTime.Now, Nom = "Region de Rabat-Sale-Kenitra"},
                    new Region { DateCreation = DateTime.Now, DateModif = DateTime.Now, Nom = "Region de Beni Mellal-Khenifra"},
                    new Region { DateCreation = DateTime.Now, DateModif = DateTime.Now, Nom = "Region de Casablanca-Settat"},
                    new Region { DateCreation = DateTime.Now, DateModif = DateTime.Now, Nom = "Region de Marrakech-Safi"},
                    new Region { DateCreation = DateTime.Now, DateModif = DateTime.Now, Nom = "Region de Draa-Tafilalet"},
                    new Region { DateCreation = DateTime.Now, DateModif = DateTime.Now, Nom = "Region de Souss-Massa"},
                    new Region { DateCreation = DateTime.Now, DateModif = DateTime.Now, Nom = "Region de Guelmim-Oued Noun"},
                    new Region { DateCreation = DateTime.Now, DateModif = DateTime.Now, Nom = "Region de Laayoune-Sakia El Hamra"},
                    new Region { DateCreation = DateTime.Now, DateModif = DateTime.Now, Nom = "Region de Dakhla-Oued Ed Dahab"}
                };

                foreach (var region in Regions)
                {
                    context.Region.Add(region);
                }
                context.SaveChanges();


                // Ajout des Services des Regions  ajoutes
                List<Service> Services = new List<Service>()
                {
                    new Service { Nom = "Khenifra", Code = "K00024", NombreR3 =24, NombreR4 = 45, CheminDossier= @"DOCUMAT\Khenifra", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 5},
                    new Service { Nom = "Khouribga", Code = "K00057", NombreR3 =28, NombreR4 = 95, CheminDossier= @"DOCUMAT\Khouribga", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 5},
                    new Service { Nom = "Beni mellal", Code = "B00061", NombreR3 =41, NombreR4 = 137, CheminDossier= @"DOCUMAT\Beni mellal", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 5},
                    new Service { Nom = "Azilal", Code = "A00062", NombreR3 =7, NombreR4 = 15, CheminDossier= @"DOCUMAT\Azilal", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 5},
                    new Service { Nom = "Fqih Ben saleh", Code = "F00084", NombreR3 =14, NombreR4 = 56, CheminDossier= @"DOCUMAT\Fqih Ben saleh", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 5},
                    new Service { Nom = "Casablanca - Moulay Rachid - Sidi othmane", Code = "C00010", NombreR3 =9, NombreR4 = 262, CheminDossier= @"DOCUMAT\Casablanca - Moulay Rachid - Sidi othmane", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom = "Casablanca - El fida - Derb soltane", Code = "C00011", NombreR3 =1, NombreR4 = 30, CheminDossier= @"DOCUMAT\Casablanca - El fida - Derb soltane", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom = "Casablanca - Mechouar", Code = "C00012", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Casablanca - Mechouar", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom = "Casablanca - Sidi bernoussi - Zenata", Code = "C00013", NombreR3 =5, NombreR4 = 135, CheminDossier= @"DOCUMAT\Casablanca - Sidi bernoussi - Zenata", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom = "Mohammadia", Code = "M0014", NombreR3 =10, NombreR4 = 109, CheminDossier= @"DOCUMAT\Mohammadia", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom = "El jadida", Code = "E00055", NombreR3 =64, NombreR4 = 251, CheminDossier= @"DOCUMAT\El jadida", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom = "Settat", Code = "S00056", NombreR3 =31, NombreR4 = 137, CheminDossier= @"DOCUMAT\Settat", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom = "Benslimane", Code = "B00058", NombreR3 =19, NombreR4 = 108, CheminDossier= @"DOCUMAT\Benslimane", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom = "Casablanca Anfa", Code = "C0006", NombreR3 =0, NombreR4 = 906, CheminDossier= @"DOCUMAT\Casablanca Anfa", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom = "Casablanca Hay mohammadi", Code = "C0007", NombreR3 =4, NombreR4 = 205, CheminDossier= @"DOCUMAT\Casablanca Hay mohammadi", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom = "Casablanca Ain chok", Code = "C00072", NombreR3 =7, NombreR4 = 140, CheminDossier= @"DOCUMAT\Casablanca Ain chok", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom = "Casablanca Nouacer", Code = "C00073", NombreR3 =7, NombreR4 = 95, CheminDossier= @"DOCUMAT\Casablanca Nouacer", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom = "Casablanca Mediouna", Code = "C00074", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Casablanca Mediouna", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom = "Casablanca Hay Hassani", Code = "C0008", NombreR3 =8, NombreR4 = 172, CheminDossier= @"DOCUMAT\Casablanca Hay Hassani", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom = "Berrechid", Code = "B00081", NombreR3 =23, NombreR4 = 95, CheminDossier= @"DOCUMAT\Berrechid", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom = "Sidi bennour", Code = "S00082", NombreR3 =51, NombreR4 = 78, CheminDossier= @"DOCUMAT\Sidi bennour", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom = "Casablanca Benmsik", Code = "C0009", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Casablanca Benmsik", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom = "Oued Ed-dahab", Code = "O00070", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Oued Ed-dahab", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 12},
                    new Service { Nom = "Oued Ed-dahab", Code = "O00070", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Oued Ed-dahab", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 12},
                    new Service { Nom = "Aoussred", Code = "A00071", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Aoussred", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 12},
                    new Service { Nom = "Errachidia", Code = "E00025", NombreR3 =13, NombreR4 = 30, CheminDossier= @"DOCUMAT\Errachidia", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 8},
                    new Service { Nom = "Ouarzazate", Code = "O00038", NombreR3 =12, NombreR4 = 29, CheminDossier= @"DOCUMAT\Ouarzazate", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 8},
                    new Service { Nom = "Zagora", Code = "Z00039", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Zagora", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 8},
                    new Service { Nom = "Midelt", Code = "M00080", NombreR3 =14, NombreR4 = 27, CheminDossier= @"DOCUMAT\Midelt", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 8},
                    new Service { Nom = "Tinghir", Code = "T00086", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Tinghir", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 8},
                    new Service { Nom = "Ifrane", Code = "I00023", NombreR3 =3, NombreR4 = 14, CheminDossier= @"DOCUMAT\Ifrane", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { Nom = "Fes Jdid", Code = "F00015", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Fes Jdid", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { Nom = "Fes Medina", Code = "F00016", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Fes Medina", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { Nom = "Zouagha ", Code = "Z00017", NombreR3 =4, NombreR4 = 103, CheminDossier= @"DOCUMAT\Zouagha ", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { Nom = "Sefrou", Code = "S00018", NombreR3 =0, NombreR4 = 62, CheminDossier= @"DOCUMAT\Sefrou", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { Nom = "Boulmane", Code = "B00019", NombreR3 =4, NombreR4 = 14, CheminDossier= @"DOCUMAT\Boulmane", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { Nom = "Meknes Menzah", Code = "M00020", NombreR3 =22, NombreR4 = 219, CheminDossier= @"DOCUMAT\Meknes Menzah", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { Nom = "Meknes Ismailia", Code = "M00021", NombreR3 =5, NombreR4 = 94, CheminDossier= @"DOCUMAT\Meknes Ismailia", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { Nom = "El Hajeb", Code = "E00022", NombreR3 =5, NombreR4 = 42, CheminDossier= @"DOCUMAT\El Hajeb", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { Nom = "Taza", Code = "T00046", NombreR3 =13, NombreR4 = 73, CheminDossier= @"DOCUMAT\Taza", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { Nom = "Taounate", Code = "T00047", NombreR3 =14, NombreR4 = 20, CheminDossier= @"DOCUMAT\Taounate", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { Nom = "Guelmim", Code = "G00064", NombreR3 =18, NombreR4 = 9, CheminDossier= @"DOCUMAT\Guelmim", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 10},
                    new Service { Nom = "Tan-Tan", Code = "T00065", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Tan-Tan", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 10},
                    new Service { Nom = "Assa-Zag", Code = "A00067", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Assa-Zag", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 10},
                    new Service { Nom = "Sidi Ifni", Code = "S00087", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Sidi Ifni", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 10},
                    new Service { Nom = "Smara", Code = "S00063", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Smara", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 11},
                    new Service { Nom = "Laayoune", Code = "L00068", NombreR3 =18, NombreR4 = 34, CheminDossier= @"DOCUMAT\Laayoune", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 11},
                    new Service { Nom = "Boujdour", Code = "B00069", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Boujdour", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 11},
                    new Service { Nom = "Oujda", Code = "O00048", NombreR3 =29, NombreR4 = 225, CheminDossier= @"DOCUMAT\Oujda", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 2},
                    new Service { Nom = "Berkane", Code = "B00049", NombreR3 =7, NombreR4 = 82, CheminDossier= @"DOCUMAT\Berkane", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 2},
                    new Service { Nom = "Nador", Code = "N00050", NombreR3 =30, NombreR4 = 58, CheminDossier= @"DOCUMAT\Nador", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 2},
                    new Service { Nom = "Taourirt", Code = "T00051", NombreR3 =5, NombreR4 = 19, CheminDossier= @"DOCUMAT\Taourirt", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 2},
                    new Service { Nom = "Jerada", Code = "J00052", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Jerada", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 2},
                    new Service { Nom = "Figuig", Code = "F00053", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Figuig", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 2},
                    new Service { Nom = "Driouch", Code = "D00076", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Driouch", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 2},
                    new Service { Nom = "Guercif", Code = "G00077", NombreR3 =6, NombreR4 = 13, CheminDossier= @"DOCUMAT\Guercif", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 2},
                    new Service { Nom = "Marrakech Menara", Code = "M00026", NombreR3 =33, NombreR4 = 402, CheminDossier= @"DOCUMAT\Marrakech Menara", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { Nom = "Marrakech Medina", Code = "M00027", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Marrakech Medina", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { Nom = "Marrakech Sidi youssef ", Code = "M00028", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Marrakech Sidi youssef ", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { Nom = "Marrakech El haouz", Code = "M00029", NombreR3 =6, NombreR4 = 33, CheminDossier= @"DOCUMAT\Marrakech El haouz", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { Nom = "Chichaoua", Code = "C00030", NombreR3 =3, NombreR4 = 7, CheminDossier= @"DOCUMAT\Chichaoua", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { Nom = "Kelaat es-sraghna", Code = "K00031", NombreR3 =18, NombreR4 = 89, CheminDossier= @"DOCUMAT\Kelaat es-sraghna", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { Nom = "Essaouira", Code = "E00032", NombreR3 =13, NombreR4 = 35, CheminDossier= @"DOCUMAT\Essaouira", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { Nom = "Safi", Code = "S00054", NombreR3 =44, NombreR4 = 133, CheminDossier= @"DOCUMAT\Safi", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { Nom = "Skour errhamna", Code = "S00083", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Skour errhamna", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { Nom = "Youssoufia", Code = "Y00085", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Youssoufia", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { Nom = "Rabat", Code = "R00001", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Rabat", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { Nom = "Sale Medina", Code = "S00002", NombreR3 =3, NombreR4 = 149, CheminDossier= @"DOCUMAT\Sale Medina", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { Nom = "Sala El-jadida", Code = "S0003", NombreR3 =9, NombreR4 = 73, CheminDossier= @"DOCUMAT\Sala El-jadida", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { Nom = "Skhirat Temara", Code = "S0004", NombreR3 =13, NombreR4 = 164, CheminDossier= @"DOCUMAT\Skhirat Temara", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { Nom = "Khemissat", Code = "K0005", NombreR3 =0, NombreR4 = 80, CheminDossier= @"DOCUMAT\Khemissat", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { Nom = "Kenitra", Code = "K00059", NombreR3 =25, NombreR4 = 289, CheminDossier= @"DOCUMAT\Kenitra", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { Nom = "Sidi kacem", Code = "S00060", NombreR3 =20, NombreR4 = 76, CheminDossier= @"DOCUMAT\Sidi kacem", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { Nom = "Sidi slimane", Code = "S00079", NombreR3 =6, NombreR4 = 36, CheminDossier= @"DOCUMAT\Sidi slimane", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { Nom = "Agadir", Code = "A00033", NombreR3 =31, NombreR4 = 250, CheminDossier= @"DOCUMAT\Agadir", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 9},
                    new Service { Nom = "Inezgane Ait melloul", Code = "I00034", NombreR3 =0, NombreR4 = 55, CheminDossier= @"DOCUMAT\Inezgane Ait melloul", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 9},
                    new Service { Nom = "Chtouka ait baha", Code = "C00035", NombreR3 =6, NombreR4 = 16, CheminDossier= @"DOCUMAT\Chtouka ait baha", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 9},
                    new Service { Nom = "Taroudant", Code = "T00036", NombreR3 =19, NombreR4 = 58, CheminDossier= @"DOCUMAT\Taroudant", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 9},
                    new Service { Nom = "Tiznit", Code = "T00037", NombreR3 =25, NombreR4 = 50, CheminDossier= @"DOCUMAT\Tiznit", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 9},
                    new Service { Nom = "Tata", Code = "T00066", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Tata", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 9},
                    new Service { Nom = "Tanger Asila", Code = "T00040", NombreR3 =18, NombreR4 = 266, CheminDossier= @"DOCUMAT\Tanger Asila", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 1},
                    new Service { Nom = "Beni mekada", Code = "B00041", NombreR3 =9, NombreR4 = 57, CheminDossier= @"DOCUMAT\Beni mekada", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 1},
                    new Service { Nom = "Laarache", Code = "L00042", NombreR3 =24, NombreR4 = 60, CheminDossier= @"DOCUMAT\Laarache", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 1},
                    new Service { Nom = "Chefchaouen", Code = "C00043", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Chefchaouen", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 1},
                    new Service { Nom = "Tetouan", Code = "T00044", NombreR3 =20, NombreR4 = 68, CheminDossier= @"DOCUMAT\Tetouan", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 1},
                    new Service { Nom = "El hoceima", Code = "E00045", NombreR3 =15, NombreR4 = 37, CheminDossier= @"DOCUMAT\El hoceima", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 1},
                    new Service { Nom = "M'diq - Fnideq", Code = "M00075", NombreR3 =3, NombreR4 = 28, CheminDossier= @"DOCUMAT\M'diq - Fnideq", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 1},
                    new Service { Nom = "Ouazzane", Code = "O00078", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Ouazzane", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 1},
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
}
