namespace DOCUMAT.Migrations
{
    using DOCUMAT.Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<DOCUMAT.Models.DocumatContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(DOCUMAT.Models.DocumatContext context)
        {
            if (true == false)
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
                    new Service { Nom="ALHOCEIMA", NomComplet = "AL HOCEIMA", Code = "E00045", NombreR3 =15, NombreR4 = 37, CheminDossier= @"DOCUMAT\ALHOCEIMA", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 1},
                    new Service { Nom="LARACHE", NomComplet = "LARACHE", Code = "L00042", NombreR3 =24, NombreR4 = 60, CheminDossier= @"DOCUMAT\LARACHE", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 1},
                    new Service { Nom="MDIQFNIDEQ", NomComplet = "MDIQ FNIDEQ", Code = "M00075", NombreR3 =3, NombreR4 = 28, CheminDossier= @"DOCUMAT\MDIQFNIDEQ", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 1},
                    new Service { Nom="TANGERBANI", NomComplet = "TANGER BANI", Code = "TAB001", NombreR3 =9, NombreR4 = 57, CheminDossier= @"DOCUMAT\TANGERBANI", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 1},
                    new Service { Nom="TANGER", NomComplet = "TANGER", Code = "T00040", NombreR3 =18, NombreR4 = 266, CheminDossier= @"DOCUMAT\TANGER", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 1},
                    new Service { Nom="TETOUAN", NomComplet = "TETOUAN", Code = "T00044", NombreR3 =20, NombreR4 = 68, CheminDossier= @"DOCUMAT\TETOUAN", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 1},
                    new Service { Nom="BERKANE", NomComplet = "BERKANE", Code = "B00049", NombreR3 =7, NombreR4 = 82, CheminDossier= @"DOCUMAT\BERKANE", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 2},
                    new Service { Nom="GUERCIF", NomComplet = "GUERCIF", Code = "G00077", NombreR3 =6, NombreR4 = 13, CheminDossier= @"DOCUMAT\GUERCIF", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 2},
                    new Service { Nom="NADOR", NomComplet = "NADOR", Code = "N00050", NombreR3 =30, NombreR4 = 58, CheminDossier= @"DOCUMAT\NADOR", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 2},
                    new Service { Nom="OUJDAANGAD", NomComplet = "OUJDA ANGAD", Code = "OAG001", NombreR3 =4, NombreR4 = 49, CheminDossier= @"DOCUMAT\OUJDAANGAD", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 2},
                    new Service { Nom="OUJDA", NomComplet = "OUJDA", Code = "O00048", NombreR3 =29, NombreR4 = 225, CheminDossier= @"DOCUMAT\OUJDA", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 2},
                    new Service { Nom="TAOURIRT", NomComplet = "TAOURIRT", Code = "T00051", NombreR3 =5, NombreR4 = 19, CheminDossier= @"DOCUMAT\TAOURIRT", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 2},
                    new Service { Nom="BOULMANE", NomComplet = "BOULMANE MISSOUR ", Code = "B00019", NombreR3 =4, NombreR4 = 14, CheminDossier= @"DOCUMAT\BOULMANE", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { Nom="ELHAJEB", NomComplet = "EL HAJEB", Code = "E00022", NombreR3 =5, NombreR4 = 42, CheminDossier= @"DOCUMAT\ELHAJEB", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { Nom="FES", NomComplet = "FES", Code = "F00015", NombreR3 =20, NombreR4 = 322, CheminDossier= @"DOCUMAT\FES", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { Nom="KARIAT", NomComplet = "KARIAT BA MOHAMED", Code = "KBM001", NombreR3 =19, NombreR4 = 8, CheminDossier= @"DOCUMAT\KARIAT", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { Nom="IFRANE", NomComplet = "IFRANE", Code = "I00023", NombreR3 =3, NombreR4 = 14, CheminDossier= @"DOCUMAT\IFRANE", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { Nom="MEKNESISMAILIA", NomComplet = "MEKNES AL ISMAILIA", Code = "M00021", NombreR3 =5, NombreR4 = 94, CheminDossier= @"DOCUMAT\MEKNESISMAILIA", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { Nom="MEKNESMENZEH", NomComplet = "MEKNES EL MENZEH", Code = "M00020", NombreR3 =22, NombreR4 = 219, CheminDossier= @"DOCUMAT\MEKNESMENZEH", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { Nom="SEFROU", NomComplet = "SEFROU", Code = "S00018", NombreR3 =0, NombreR4 = 62, CheminDossier= @"DOCUMAT\SEFROU", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { Nom="TAOUNATE", NomComplet = "TAOUNATE", Code = "T00047", NombreR3 =14, NombreR4 = 20, CheminDossier= @"DOCUMAT\TAOUNATE", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { Nom="TAZA", NomComplet = "TAZA", Code = "T00046", NombreR3 =13, NombreR4 = 73, CheminDossier= @"DOCUMAT\TAZA", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { Nom="YAAKOUB", NomComplet = "ZOUAGHA MOULAY YAAKOUB", Code = "Z00017", NombreR3 =4, NombreR4 = 103, CheminDossier= @"DOCUMAT\YAAKOUB", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 3},
                    new Service { Nom="SKHIRAT", NomComplet = "HARHOURA SKHIRAT", Code = "HSK001", NombreR3 =4, NombreR4 = 56, CheminDossier= @"DOCUMAT\SKHIRAT", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { Nom="KENITRA", NomComplet = "KENITRA", Code = "K00059", NombreR3 =25, NombreR4 = 289, CheminDossier= @"DOCUMAT\KENITRA", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { Nom="KHEMISSET", NomComplet = "KHEMISSET", Code = "K0005", NombreR3 =0, NombreR4 = 80, CheminDossier= @"DOCUMAT\KHEMISSET", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { Nom="RABATRYAD", NomComplet = "RABAT RYAD", Code = "RAR001", NombreR3 =8, NombreR4 = 95, CheminDossier= @"DOCUMAT\RABATRYAD", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { Nom="RABATHASSAN", NomComplet = "RABAT HASSAN", Code = "R00001", NombreR3 =0, NombreR4 = 0, CheminDossier= @"DOCUMAT\RABATHASSAN", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { Nom="ROUMANI", NomComplet = "ROUMANI", Code = "ROU001", NombreR3 =30, NombreR4 = 58, CheminDossier= @"DOCUMAT\ROUMANI", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { Nom="SALAALJADIDA", NomComplet = "SALA AL JADIDA", Code = "S0003", NombreR3 =9, NombreR4 = 73, CheminDossier= @"DOCUMAT\SALAALJADIDA", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { Nom="SALEMEDINA", NomComplet = "SALE MEDINA", Code = "S00002", NombreR3 =3, NombreR4 = 149, CheminDossier= @"DOCUMAT\SALEMEDINA", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { Nom="SIDIKACEM", NomComplet = "SIDI KACEM", Code = "S00060", NombreR3 =20, NombreR4 = 76, CheminDossier= @"DOCUMAT\SIDIKACEM", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { Nom="SIDISLIMANE", NomComplet = "SIDI SLIMANE", Code = "S00079", NombreR3 =6, NombreR4 = 36, CheminDossier= @"DOCUMAT\SIDISLIMANE", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { Nom="SOUKLARBAA", NomComplet = "SOUK LARBAA", Code = "SOL001", NombreR3 =9, NombreR4 = 31, CheminDossier= @"DOCUMAT\SOUKLARBAA", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { Nom="TEMARA", NomComplet = "TEMARA", Code = "S0004", NombreR3 =13, NombreR4 = 164, CheminDossier= @"DOCUMAT\TEMARA", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { Nom="TIFELT", NomComplet = "TIFELT", Code = "TIF001", NombreR3 =4, NombreR4 = 11, CheminDossier= @"DOCUMAT\TIFELT", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 4},
                    new Service { Nom="AZILAL", NomComplet = "AZILAL", Code = "A00062", NombreR3 =7, NombreR4 = 15, CheminDossier= @"DOCUMAT\AZILAL", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 5},
                    new Service { Nom="BENIMELLAL", NomComplet = "BENI MELLAL", Code = "B00061", NombreR3 =41, NombreR4 = 137, CheminDossier= @"DOCUMAT\BENIMELLAL", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 5},
                    new Service { Nom="FQUIHBENSALEH", NomComplet = "FQUIH BEN SALEH", Code = "F00084", NombreR3 =14, NombreR4 = 56, CheminDossier= @"DOCUMAT\FQUIHBENSALEH", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 5},
                    new Service { Nom="KHENIFRA", NomComplet = "KHENIFRA", Code = "K00024", NombreR3 =24, NombreR4 = 45, CheminDossier= @"DOCUMAT\KHENIFRA", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 5},
                    new Service { Nom="KHOURIBGA", NomComplet = "KHOURIBGA", Code = "K00057", NombreR3 =28, NombreR4 = 95, CheminDossier= @"DOCUMAT\KHOURIBGA", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 5},
                    new Service { Nom="SOUKOULED", NomComplet = "SOUK SEBT OULED NEMMA", Code = "SSN001", NombreR3 =4, NombreR4 = 7, CheminDossier= @"DOCUMAT\SOUKOULED", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 5},
                    new Service { Nom="BENSLIMANE", NomComplet = "BEN SLIMANE", Code = "B00058", NombreR3 =19, NombreR4 = 108, CheminDossier= @"DOCUMAT\BENSLIMANE", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom="BERRECHID", NomComplet = "BERRECHID", Code = "B00081", NombreR3 =23, NombreR4 = 95, CheminDossier= @"DOCUMAT\BERRECHID", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom="CASAAINCHOCK", NomComplet = "CASA AIN CHOCK", Code = "C00011", NombreR3 =7, NombreR4 = 140, CheminDossier= @"DOCUMAT\CASAAINCHOCK", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom="CASAAINSEBAA", NomComplet = "CASA AIN SEBAA", Code = "C00072", NombreR3 =4, NombreR4 = 205, CheminDossier= @"DOCUMAT\CASAAINSEBAA", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom="CASAALFIDA", NomComplet = "CASA AL FIDA", Code = "CAF001", NombreR3 =1, NombreR4 = 30, CheminDossier= @"DOCUMAT\CASAALFIDA", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom="CASASIDIOTHMANE", NomComplet = "CASA SIDI OTHMANE", Code = "C00010", NombreR3 =9, NombreR4 = 262, CheminDossier= @"DOCUMAT\CASASIDIOTHMANE", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom="CASABERNOUSSI", NomComplet = "CASA BERNOUSSI", Code = "C00013", NombreR3 =5, NombreR4 = 135, CheminDossier= @"DOCUMAT\CASABERNOUSSI", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom="CASAANFA", NomComplet = "CASA ANFA", Code = "C0006", NombreR3 =0, NombreR4 = 906, CheminDossier= @"DOCUMAT\CASAANFA", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom="CASAHAYHASSANI", NomComplet = "CASA HAY HASSANI", Code = "C0008", NombreR3 =8, NombreR4 = 172, CheminDossier= @"DOCUMAT\CASAHAYHASSANI", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom="CASAMAARIF", NomComplet = "CASA MAARIF", Code = "CAM001", NombreR3 =2, NombreR4 = 49, CheminDossier= @"DOCUMAT\CASAMAARIF", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom="CASANOUACER", NomComplet = "CASA NOUACER", Code = "C00073", NombreR3 =7, NombreR4 = 95, CheminDossier= @"DOCUMAT\CASANOUACER", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom="ELJADIDA", NomComplet = "EL JADIDA", Code = "E00055", NombreR3 =64, NombreR4 = 251, CheminDossier= @"DOCUMAT\ELJADIDA", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom="MOHAMMADIA", NomComplet = "MOHAMMADIA", Code = "M0014", NombreR3 =10, NombreR4 = 109, CheminDossier= @"DOCUMAT\MOHAMMADIA", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom="SETTAT", NomComplet = "SETTAT", Code = "S00056", NombreR3 =31, NombreR4 = 137, CheminDossier= @"DOCUMAT\SETTAT", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom="SIDIBENNOUR", NomComplet = "SIDI BENNOUR", Code = "S00082", NombreR3 =51, NombreR4 = 78, CheminDossier= @"DOCUMAT\SIDIBENNOUR", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom="ZEMAMRA", NomComplet = "SIDI SMAIL ZEMAMRA", Code = "SSZ001", NombreR3 =0, NombreR4 = 28, CheminDossier= @"DOCUMAT\ZEMAMRA", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 6},
                    new Service { Nom="CHICHAOUA", NomComplet = "CHICHAOUA", Code = "C00030", NombreR3 =3, NombreR4 = 7, CheminDossier= @"DOCUMAT\CHICHAOUA", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { Nom="ESSAOUIRA", NomComplet = "ESSAOUIRA", Code = "E00032", NombreR3 =13, NombreR4 = 35, CheminDossier= @"DOCUMAT\ESSAOUIRA", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { Nom="KELAADESSRAGHNA", NomComplet = "EL KELAA DES SRAGHNA", Code = "K00031", NombreR3 =18, NombreR4 = 89, CheminDossier= @"DOCUMAT\KELAADESSRAGHNA", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { Nom="BENGUERIR", NomComplet = "BEN GUERIR", Code = "BGU001", NombreR3 =7, NombreR4 = 31, CheminDossier= @"DOCUMAT\BENGUERIR", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { Nom="ALHAOUZ", NomComplet = "AL HAOUZ", Code = "M00029", NombreR3 =6, NombreR4 = 33, CheminDossier= @"DOCUMAT\ALHAOUZ", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { Nom="MARRAKECHGELIZ", NomComplet = "MARRAKECH GELIZ", Code = "MAG001", NombreR3 =3, NombreR4 = 0, CheminDossier= @"DOCUMAT\MARRAKECHGELIZ", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { Nom="MARRAKECHMENARA", NomComplet = "MARRAKECH MENARA", Code = "M00026", NombreR3 =33, NombreR4 = 402, CheminDossier= @"DOCUMAT\MARRAKECHMENARA", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { Nom="MARRAKECHSYBA", NomComplet = "MARRAKECH SYBA", Code = "MAS001", NombreR3 =16, NombreR4 = 101, CheminDossier= @"DOCUMAT\MARRAKECHSYBA", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { Nom="SAFI", NomComplet = "SAFI", Code = "S00054", NombreR3 =44, NombreR4 = 133, CheminDossier= @"DOCUMAT\SAFI", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 7},
                    new Service { Nom="ERRACHIDIA", NomComplet = "ERRACHIDIA", Code = "E00025", NombreR3 =13, NombreR4 = 30, CheminDossier= @"DOCUMAT\ERRACHIDIA", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 8},
                    new Service { Nom="MIDELT", NomComplet = "MIDELT", Code = "M00080", NombreR3 =14, NombreR4 = 27, CheminDossier= @"DOCUMAT\MIDELT", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 8},
                    new Service { Nom="OUARZAZATE", NomComplet = "OUARZAZATE", Code = "O00038", NombreR3 =12, NombreR4 = 29, CheminDossier= @"DOCUMAT\OUARZAZATE", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 8},
                    new Service { Nom="AGADIR", NomComplet = "AGADIR", Code = "A00033", NombreR3 =31, NombreR4 = 250, CheminDossier= @"DOCUMAT\AGADIR", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 9},
                    new Service { Nom="CHTOUKAITBAHA", NomComplet = "CHTOUK AIT BAHA", Code = "C00035", NombreR3 =6, NombreR4 = 16, CheminDossier= @"DOCUMAT\CHTOUKAITBAHA", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 9},
                    new Service { Nom="INEZEGANE", NomComplet = "INEZEGANE", Code = "I00034", NombreR3 =0, NombreR4 = 55, CheminDossier= @"DOCUMAT\INEZEGANE", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 9},
                    new Service { Nom="TAROUDANT", NomComplet = "TAROUDANT", Code = "T00036", NombreR3 =19, NombreR4 = 58, CheminDossier= @"DOCUMAT\TAROUDANT", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 9},
                    new Service { Nom="TIZNIT", NomComplet = "TIZNIT", Code = "T00037", NombreR3 =25, NombreR4 = 50, CheminDossier= @"DOCUMAT\TIZNIT", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 9},
                    new Service { Nom="GUELMIM", NomComplet = "GUELMIM", Code = "G00064", NombreR3 =18, NombreR4 = 9, CheminDossier= @"DOCUMAT\GUELMIM", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 10},
                    new Service { Nom="LAAYOUNE", NomComplet = "LAAYOUNE", Code = "L00068", NombreR3 =18, NombreR4 = 34, CheminDossier= @"DOCUMAT\LAAYOUNE", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 11},
                    new Service { Nom="DAKHLA", NomComplet = "DAKHLA", Code = "DAK001", NombreR3 =3, NombreR4 = 9, CheminDossier= @"DOCUMAT\DAKHLA", DateCreation = DateTime.Now, DateModif = DateTime.Now,RegionID = 12},
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
