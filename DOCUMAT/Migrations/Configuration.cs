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
                    new Region { DateCreation = DateTime.Now, DateModif = DateTime.Now, Nom = "Region de l\'Oriental"},
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
                    new Service { NomComplet="Agadir", Nom = "Agadir", Code = "A00033", NombreR3 =31, NombreR4 = 250, CheminDossier= @"DOCUMAT\Agadir", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 9},
                    new Service { NomComplet="Aoussred", Nom = "Aoussred", Code = "A00071", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Aoussred", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 12},
                    new Service { NomComplet="Assa-Zag", Nom = "Assa Za", Code = "A00067", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Assa Za", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 10},
                    new Service { NomComplet="Azilal", Nom = "Azilal", Code = "A00062", NombreR3 =7, NombreR4 = 15, CheminDossier= @"DOCUMAT\Azilal", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 5},
                    new Service { NomComplet="Beni mekada", Nom = "Beni mekad", Code = "B00041", NombreR3 =9, NombreR4 = 57, CheminDossier= @"DOCUMAT\Beni mekad", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 1},
                    new Service { NomComplet="Beni mellal", Nom = "Beni mella", Code = "B00061", NombreR3 =41, NombreR4 = 137, CheminDossier= @"DOCUMAT\Beni mella", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 5},
                    new Service { NomComplet="Benslimane", Nom = "Benslimane", Code = "B00058", NombreR3 =19, NombreR4 = 108, CheminDossier= @"DOCUMAT\Benslimane", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { NomComplet="Berkane", Nom = "Berkane", Code = "B00049", NombreR3 =7, NombreR4 = 82, CheminDossier= @"DOCUMAT\Berkane", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 2},
                    new Service { NomComplet="Berrechid", Nom = "Berrechid", Code = "B00081", NombreR3 =23, NombreR4 = 95, CheminDossier= @"DOCUMAT\Berrechid", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { NomComplet="Boujdour", Nom = "Boujdour", Code = "B00069", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Boujdour", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 11},
                    new Service { NomComplet="Boulmane", Nom = "Boulmane", Code = "B00019", NombreR3 =4, NombreR4 = 14, CheminDossier= @"DOCUMAT\Boulmane", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { NomComplet="Casablanca - El fida - Derb soltane", Nom = "Casa-fida-Derb soltan", Code = "C00011", NombreR3 =1, NombreR4 = 30, CheminDossier= @"DOCUMAT\Casa-fida-Derb soltan", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { NomComplet="Casablanca - Mechouar", Nom = "Casa Mechoua", Code = "C00012", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Casa Mechoua", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { NomComplet="Casablanca - Moulay Rachid - Sidi othmane", Nom = "Casa-Rachid ", Code = "C00010", NombreR3 =9, NombreR4 = 262, CheminDossier= @"DOCUMAT\Casa-Rachid ", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { NomComplet="Casablanca - Sidi bernoussi - Zenata", Nom = "Casa-bernoussi Zena", Code = "C00013", NombreR3 =5, NombreR4 = 135, CheminDossier= @"DOCUMAT\Casa-bernoussi Zena", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { NomComplet="Casablanca - Ain chok", Nom = "Casa- cho", Code = "C00072", NombreR3 =7, NombreR4 = 140, CheminDossier= @"DOCUMAT\Casa- cho", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { NomComplet="Casablanca - Anfa", Nom = "Casa Anf", Code = "C0006", NombreR3 =0, NombreR4 = 906, CheminDossier= @"DOCUMAT\Casa Anf", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { NomComplet="Casablanca - Benmsik - Mediouna", Nom = "Casa- Medioun", Code = "C0009", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Casa- Medioun", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { NomComplet="Casablanca - Hay Hassani - Ain chok", Nom = "Casa-Hassani cho", Code = "C0008", NombreR3 =8, NombreR4 = 172, CheminDossier= @"DOCUMAT\Casa-Hassani cho", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { NomComplet="Casablanca - Hay mohammadi - Ain sebaa", Nom = "Casa-mohammadi s", Code = "C0007", NombreR3 =4, NombreR4 = 205, CheminDossier= @"DOCUMAT\Casa-mohammadi s", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { NomComplet="Casablanca - Mediouna", Nom = "Casa Medioun", Code = "C00074", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Casa Medioun", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { NomComplet="Casablanca - Nouacer", Nom = "Casa Nouace", Code = "C00073", NombreR3 =7, NombreR4 = 95, CheminDossier= @"DOCUMAT\Casa Nouace", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { NomComplet="Chefchaouen", Nom = "Chefchaouen", Code = "C00043", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Chefchaouen", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 1},
                    new Service { NomComplet="Chichaoua", Nom = "Chichaoua", Code = "C00030", NombreR3 =3, NombreR4 = 7, CheminDossier= @"DOCUMAT\Chichaoua", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { NomComplet="Chtouka ait baha", Nom = "Chtoait bah", Code = "C00035", NombreR3 =6, NombreR4 = 16, CheminDossier= @"DOCUMAT\Chtoait bah", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 9},
                    new Service { NomComplet="Driouch", Nom = "Driouch", Code = "D00076", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Driouch", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 2},
                    new Service { NomComplet="El Hajeb", Nom = "El H Haje", Code = "E00022", NombreR3 =5, NombreR4 = 42, CheminDossier= @"DOCUMAT\El H Haje", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { NomComplet="El hoceima", Nom = "El h hoceim", Code = "E00045", NombreR3 =15, NombreR4 = 37, CheminDossier= @"DOCUMAT\El h hoceim", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 1},
                    new Service { NomComplet="El jadida", Nom = "El j jadid", Code = "E00055", NombreR3 =64, NombreR4 = 251, CheminDossier= @"DOCUMAT\El j jadid", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { NomComplet="Errachidia", Nom = "Errachidia", Code = "E00025", NombreR3 =13, NombreR4 = 30, CheminDossier= @"DOCUMAT\Errachidia", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 8},
                    new Service { NomComplet="Essaouira", Nom = "Essaouira", Code = "E00032", NombreR3 =13, NombreR4 = 35, CheminDossier= @"DOCUMAT\Essaouira", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { NomComplet="Fes Jdid", Nom = "Fes  Jdi", Code = "F00015", NombreR3 =20, NombreR4 = 322, CheminDossier= @"DOCUMAT\Fes  Jdi", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { NomComplet="Fes Medina", Nom = "Fes  Medin", Code = "F00016", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Fes  Medin", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { NomComplet="Figuig", Nom = "Figuig", Code = "F00053", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Figuig", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 2},
                    new Service { NomComplet="Fqih Ben saleh", Nom = "FqihBen sale", Code = "F00084", NombreR3 =14, NombreR4 = 56, CheminDossier= @"DOCUMAT\FqihBen sale", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 5},
                    new Service { NomComplet="Guelmim", Nom = "Guelmim", Code = "G00064", NombreR3 =18, NombreR4 = 9, CheminDossier= @"DOCUMAT\Guelmim", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 10},
                    new Service { NomComplet="Guercif", Nom = "Guercif", Code = "G00077", NombreR3 =6, NombreR4 = 13, CheminDossier= @"DOCUMAT\Guercif", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 2},
                    new Service { NomComplet="Ifrane", Nom = "Ifrane", Code = "I00023", NombreR3 =3, NombreR4 = 14, CheminDossier= @"DOCUMAT\Ifrane", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { NomComplet="Inezgane - Ait melloul", Nom = "Inez- mellou", Code = "I00034", NombreR3 =0, NombreR4 = 55, CheminDossier= @"DOCUMAT\Inez- mellou", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 9},
                    new Service { NomComplet="Jerada", Nom = "Jerada", Code = "J00052", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Jerada", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 2},
                    new Service { NomComplet="Kelaat es-sraghna", Nom = "Kela sraghn", Code = "K00031", NombreR3 =18, NombreR4 = 89, CheminDossier= @"DOCUMAT\Kela sraghn", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { NomComplet="Kenitra", Nom = "Kenitra", Code = "K00059", NombreR3 =25, NombreR4 = 289, CheminDossier= @"DOCUMAT\Kenitra", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { NomComplet="Khemissat", Nom = "Khemissat", Code = "K0005", NombreR3 =0, NombreR4 = 80, CheminDossier= @"DOCUMAT\Khemissat", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { NomComplet="Khenifra", Nom = "Khenifra", Code = "K00024", NombreR3 =24, NombreR4 = 45, CheminDossier= @"DOCUMAT\Khenifra", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 5},
                    new Service { NomComplet="Khouribga", Nom = "Khouribga", Code = "K00057", NombreR3 =28, NombreR4 = 95, CheminDossier= @"DOCUMAT\Khouribga", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 5},
                    new Service { NomComplet="Laarache - Ksar El kebir", Nom = "Laar-El kebi", Code = "L00042", NombreR3 =24, NombreR4 = 60, CheminDossier= @"DOCUMAT\Laar-El kebi", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 1},
                    new Service { NomComplet="Laayoune", Nom = "Laayoune", Code = "L00068", NombreR3 =18, NombreR4 = 34, CheminDossier= @"DOCUMAT\Laayoune", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 11},
                    new Service { NomComplet="Marrakech - El haouz", Nom = "Marr- haou", Code = "M00029", NombreR3 =6, NombreR4 = 33, CheminDossier= @"DOCUMAT\Marr- haou", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { NomComplet="Marrakech Medina", Nom = "Marr Medin", Code = "M00027", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Marr Medin", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { NomComplet="Marrakech Menara", Nom = "Marr Menar", Code = "M00026", NombreR3 =33, NombreR4 = 402, CheminDossier= @"DOCUMAT\Marr Menar", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { NomComplet="Marrakech - Sidi youssef ben ali", Nom = "Marr-youssef al", Code = "M00028", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Marr-youssef al", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { NomComplet="M\'diq - Fnideq", Nom = "Mdi Fnide", Code = "M00075", NombreR3 =3, NombreR4 = 28, CheminDossier= @"DOCUMAT\Mdi Fnide", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 1},
                    new Service { NomComplet="Meknes - Ismailia", Nom = "Mekn Ismaili", Code = "M00021", NombreR3 =5, NombreR4 = 94, CheminDossier= @"DOCUMAT\Mekn Ismaili", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { NomComplet="Meknes - Menzah", Nom = "Mekn Menza", Code = "M00020", NombreR3 =22, NombreR4 = 219, CheminDossier= @"DOCUMAT\Mekn Menza", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { NomComplet="Midelt", Nom = "Midelt", Code = "M00080", NombreR3 =14, NombreR4 = 27, CheminDossier= @"DOCUMAT\Midelt", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 8},
                    new Service { NomComplet="Mohammadia", Nom = "Mohammadia", Code = "M0014", NombreR3 =10, NombreR4 = 109, CheminDossier= @"DOCUMAT\Mohammadia", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { NomComplet="Nador", Nom = "Nador", Code = "N00050", NombreR3 =30, NombreR4 = 58, CheminDossier= @"DOCUMAT\Nador", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 2},
                    new Service { NomComplet="Ouarzazate", Nom = "Ouarzazate", Code = "O00038", NombreR3 =12, NombreR4 = 29, CheminDossier= @"DOCUMAT\Ouarzazate", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 8},
                    new Service { NomComplet="Ouazzane", Nom = "Ouazzane", Code = "O00078", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Ouazzane", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 1},
                    new Service { NomComplet="Oued Ed-dahab", Nom = "Oued daha", Code = "O00070", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Oued daha", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 12},
                    new Service { NomComplet="Oujda", Nom = "Oujda", Code = "O00048", NombreR3 =29, NombreR4 = 225, CheminDossier= @"DOCUMAT\Oujda", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 2},
                    new Service { NomComplet="Rabat", Nom = "Rabat", Code = "R00001", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Rabat", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { NomComplet="Safi", Nom = "Safi", Code = "S00054", NombreR3 =44, NombreR4 = 133, CheminDossier= @"DOCUMAT\Safi", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { NomComplet="Sala El-jadida", Nom = "Sala jadid", Code = "S0003", NombreR3 =9, NombreR4 = 73, CheminDossier= @"DOCUMAT\Sala jadid", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { NomComplet="Sale Medina", Nom = "Sale Medin", Code = "S00002", NombreR3 =3, NombreR4 = 149, CheminDossier= @"DOCUMAT\Sale Medin", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { NomComplet="Sefrou", Nom = "Sefrou", Code = "S00018", NombreR3 =0, NombreR4 = 62, CheminDossier= @"DOCUMAT\Sefrou", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { NomComplet="Settat", Nom = "Settat", Code = "S00056", NombreR3 =31, NombreR4 = 137, CheminDossier= @"DOCUMAT\Settat", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { NomComplet="Sidi bennour", Nom = "Sidi bennou", Code = "S00082", NombreR3 =51, NombreR4 = 78, CheminDossier= @"DOCUMAT\Sidi bennou", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { NomComplet="Sidi Ifni", Nom = "Sidi Ifn", Code = "S00087", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Sidi Ifn", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 10},
                    new Service { NomComplet="Sidi kacem", Nom = "Sidi kace", Code = "S00060", NombreR3 =20, NombreR4 = 76, CheminDossier= @"DOCUMAT\Sidi kace", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { NomComplet="Sidi slimane", Nom = "Sidi sliman", Code = "S00079", NombreR3 =6, NombreR4 = 36, CheminDossier= @"DOCUMAT\Sidi sliman", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { NomComplet="Skhirat - Temara", Nom = "Skhi Temar", Code = "S0004", NombreR3 =13, NombreR4 = 164, CheminDossier= @"DOCUMAT\Skhi Temar", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { NomComplet="Skour errhamna", Nom = "Skou errhamn", Code = "S00083", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Skou errhamn", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { NomComplet="Smara", Nom = "Smara", Code = "S00063", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Smara", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 11},
                    new Service { NomComplet="Tanger - Asila", Nom = "Tang Asil", Code = "T00040", NombreR3 =18, NombreR4 = 266, CheminDossier= @"DOCUMAT\Tang Asil", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 1},
                    new Service { NomComplet="Tan-Tan", Nom = "Tan- Ta", Code = "T00065", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Tan- Ta", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 10},
                    new Service { NomComplet="Taounate", Nom = "Taounate", Code = "T00047", NombreR3 =14, NombreR4 = 20, CheminDossier= @"DOCUMAT\Taounate", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { NomComplet="Taourirt", Nom = "Taourirt", Code = "T00051", NombreR3 =5, NombreR4 = 19, CheminDossier= @"DOCUMAT\Taourirt", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 2},
                    new Service { NomComplet="Taroudant", Nom = "Taroudant", Code = "T00036", NombreR3 =19, NombreR4 = 58, CheminDossier= @"DOCUMAT\Taroudant", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 9},
                    new Service { NomComplet="Tata", Nom = "Tata", Code = "T00066", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Tata", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 9},
                    new Service { NomComplet="Taza", Nom = "Taza", Code = "T00046", NombreR3 =13, NombreR4 = 73, CheminDossier= @"DOCUMAT\Taza", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { NomComplet="Tetouan", Nom = "Tetouan", Code = "T00044", NombreR3 =20, NombreR4 = 68, CheminDossier= @"DOCUMAT\Tetouan", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 1},
                    new Service { NomComplet="Tinghir", Nom = "Tinghir", Code = "T00086", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Tinghir", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 8},
                    new Service { NomComplet="Tiznit", Nom = "Tiznit", Code = "T00037", NombreR3 =25, NombreR4 = 50, CheminDossier= @"DOCUMAT\Tiznit", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 9},
                    new Service { NomComplet="Youssoufia", Nom = "Youssoufia", Code = "Y00085", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Youssoufia", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { NomComplet="Zagora", Nom = "Zagora", Code = "Z00039", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\Zagora", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 8},
                    new Service { NomComplet="Zouagha - Moulay yaacoub", Nom = "Zoua- yaacou", Code = "Z00017", NombreR3 =4, NombreR4 = 103, CheminDossier= @"DOCUMAT\Zoua- yaacou", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
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
