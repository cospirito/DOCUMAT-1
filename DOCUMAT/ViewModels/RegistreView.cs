using DOCUMAT.DataModels;
using DOCUMAT.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows;

namespace DOCUMAT.ViewModels
{
    public class RegistreView : IView<RegistreView, Registre>
    {
        private string DossierRacine = ConfigurationManager.AppSettings["CheminDossier_Scan"];
        string fichier_sid = Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Scan"], "sid_scanners_users.tmp");

        public int NumeroOrdre { get; set; }

        public Registre Registre { get; set; }
        public Versement Versement { get; set; }
        public Agent AgentTraitant { get; set; }
        public Service ServiceVersant { get; set; }
        public Agent ChefService { get; set; }
        public string CheminDossier { get; private set; }
        public StatutRegistre StatutActuel { get; set; }

        public int NombreImageCompte { get; set; }
        public int NombreImageScan { get; set; }
        public string StatutName { get; private set; }

        public RegistreView()
        {
            Registre = new Registre();
        }

        public void Add()
        {

        }

        public int AddRegistre()
        {
            try
            {
                using (var ct = new DocumatContext())
                {
                    //Vérification qu'un numéro de registre identique n'existe pas *
                    if (ct.Registre.Any(r => r.Numero == Registre.Numero && r.VersementID == Registre.VersementID
                        && r.Type.ToUpper() == Registre.Type.ToUpper() && r.QrCode.Trim().ToUpper() != "DEFAUT".ToUpper()))
                    {
                        throw new Exception("Un registre ayant le même numéro de volume existe déja !!!");
                    }

                    // Recherche d'un registre defaut du même versement ayant le même type et le même Numéro Volume 
                    Models.Registre registreExistant = ct.Registre.FirstOrDefault(r => r.Numero == Registre.Numero && r.VersementID == Registre.VersementID
                        && r.Type.ToUpper() == Registre.Type.ToUpper() && r.QrCode.Trim().ToUpper() == "DEFAUT".ToUpper());

                    if (registreExistant != null)
                    {
                        Registre = registreExistant;
                    }
                    else
                    {
                        Registre.QrCode = "DEFAUT";
                        Registre.CheminDossier = Registre.QrCode;
                        ct.Registre.Add(Registre);
                    }

                    ct.SaveChanges();

                    // Procédure d'Ajout du QrCode
                    //recup le code de service 
                    Versement versement = ct.Versement.FirstOrDefault(v => v.VersementID == Registre.VersementID);
                    Livraison livraison = ct.Livraison.FirstOrDefault(l => l.LivraisonID == versement.LivraisonID);
                    Service service = ct.Service.FirstOrDefault(s => s.ServiceID == livraison.ServiceID);

                    //Composition du Code QR ou du Dossier 
                    // -- Numero de Volume du registre + Le code de Service (Renseigner manuellement) + Id du registre nouvellement créer
                    Registre.QrCode = Registre.Numero + service.Code + Registre.RegistreID;

                    //Procédure de création du dossier de scan du registre 
                    string DossierRacine = ConfigurationManager.AppSettings["CheminDossier_Scan"];
                    string DossierService = "";
                    string DossierRegistre = "";
                    if (Registre.Type == "R3")
                    {
                        DossierService = service.CheminDossier + "/R3";
                        DossierRegistre = DossierService + "/" + Registre.QrCode.ToString();
                    }
                    else
                    {
                        DossierService = service.CheminDossier + "/R4";
                        DossierRegistre = DossierService + "/" + Registre.QrCode.ToString();
                    }

                    if (Directory.Exists(DossierRacine + "/" + DossierService))
                    {
                        if (!Directory.Exists(DossierRacine + "/" + DossierRegistre))
                        {
                            Directory.CreateDirectory(DossierRacine + "/" + DossierRegistre);
                            Registre.CheminDossier = DossierRegistre;
                        }
                        else
                        {
                            Registre.CheminDossier = DossierRegistre;
                            MessageBox.Show("Un Dossier ayant un nom de registre conforme a été trouvé, et pris par défaut", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Le Dossier du service est introuvable, il a peut être été supprimé, veuillez le créer à nouveau dans la partie service !!!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    // Création du statut
                    StatutRegistre statutRegistre = new StatutRegistre();
                    statutRegistre.Code = (int)Enumeration.Registre.CREE;
                    statutRegistre.DateCreation = DateTime.Now;
                    statutRegistre.DateDebut = DateTime.Now;
                    statutRegistre.DateFin = DateTime.Now;
                    statutRegistre.DateModif = DateTime.Now;
                    statutRegistre.RegistreID = Registre.RegistreID;
                    ct.StatutRegistre.Add(statutRegistre);
                    ct.SaveChanges();
                    return Registre.RegistreID; 
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Dispose()
        {
        }

        public bool GetView(int Id)
        {
            using (var ct = new DocumatContext())
            {
                Registre = ct.Registre.FirstOrDefault(r => r.RegistreID == Id);

                if (Registre != null)
                {
                    return true;
                }
                else
                {
                    throw new Exception("Ce registre est introuvable !!!");
                } 
            }
        }

        public RegistreView GetViewRegistre(int Id)
        {
            return GetViewsList().FirstOrDefault(r => r.Registre.RegistreID == Id);
        }


        public List<RegistreView> GetViewsList()
        {
            try
            {
                using (var ct = new DocumatContext())
                {
                    List<Registre> registres = new List<Registre>();
                    if (Versement != null)
                    {
                        registres = ct.Registre.Where(r => r.VersementID == Versement.VersementID &&
                                                            r.QrCode != "DEFAUT").ToList();
                    }
                    else
                    {
                        registres = ct.Registre.Where(r => r.QrCode != "DEFAUT").ToList();
                    }

                    List<RegistreView> RegistreViews = new List<RegistreView>();

                    // Numero Ordre des Registres
                    foreach (Registre reg in registres)
                    {
                        RegistreView regV = new RegistreView();
                        regV.Registre = reg;

                        //Récupération du versement du registre  
                        if (Versement != null)
                        {
                            // Récupération des informations sur le service 
                            regV.Versement = Versement;
                            Livraison livraison = ct.Livraison.FirstOrDefault(l => l.LivraisonID == Versement.LivraisonID);
                            regV.ServiceVersant = ct.Service.FirstOrDefault(s => s.ServiceID == livraison.ServiceID);
                        }
                        else
                        {
                            regV.Versement = ct.Versement.FirstOrDefault(v => v.VersementID == reg.VersementID);
                            Livraison livraison = ct.Livraison.FirstOrDefault(l => l.LivraisonID == regV.Versement.LivraisonID);
                            regV.ServiceVersant = ct.Service.FirstOrDefault(s => s.ServiceID == livraison.ServiceID);
                        }

                        // récupération des images liées au registre dans la Base de Données
                        regV.NombreImageCompte = ct.Image.Where(i => i.RegistreID == regV.Registre.RegistreID).Count();

                        //Nombre de page scanné dans le Dossier du registre
                        regV.NombreImageScan = 0;

                        try
                        {
                            DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Scan"], reg.CheminDossier));
                            //DirectoryInfo directoryInfo = new DirectoryInfo(ConfigurationManager.AppSettings["CheminDossier_Scan"] + "/"+ reg.CheminDossier);                       
                            if (directoryInfo.Exists)
                            {
                                regV.NombreImageScan = directoryInfo.GetFiles().Where(f => f.Extension.ToLower() == ".tif"
                                     || f.Extension.ToLower() == ".png" || f.Extension.ToLower() == ".jpg" || f.Extension.ToLower() == ".pdf").ToList().Count();
                            }
                            else
                            {
                                MessageBox.Show("Le repertoire du Registre de type : \"" + reg.Type + "\", de code : " + reg.QrCode + " ,dont le chemin est \""
                                                    + reg.CheminDossier + "\" est introuvable", "AVERTISSEMENT", MessageBoxButton.OK, MessageBoxImage.Warning);
                                if (MessageBox.Show("Voulez vous suspendre les requêtes de récuparation des autres registre afin de régler le problème ?", "SUSPENDRE", MessageBoxButton.YesNo, MessageBoxImage.Question)
                                    == MessageBoxResult.Yes)
                                {
                                    return RegistreViews;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message + "\n Repertoire : " + reg.CheminDossier, "AVERTISSEMENT", MessageBoxButton.OK, MessageBoxImage.Warning);
                            if (MessageBox.Show("Voulez vous suspendre les requêtes de récuparation des autres registre afin de régler le problème ?", "SUSPENDRE", MessageBoxButton.YesNo, MessageBoxImage.Question)
                                == MessageBoxResult.Yes)
                            {
                                return RegistreViews;
                            }
                        }

                        //Définition du chemin d'accès au dossier
                        regV.CheminDossier = Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Scan"], reg.CheminDossier);


                        // Affichage de statut en string
                        regV.StatutActuel = ct.StatutRegistre.FirstOrDefault(s => s.Code == reg.StatutActuel && s.RegistreID == reg.RegistreID);
                        regV.StatutName = Enum.GetName(typeof(Models.Enumeration.Registre), regV.StatutActuel.Code);

                        RegistreViews.Add(regV);
                    }
                    return RegistreViews; 
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
                return new List<RegistreView>();
            }
        }

        public static RegistreView GetViewRegistreByStatus(int CodeStatut, int IdRegistre)
        {
            using (var ct = new DocumatContext())
            {
                Registre registre = ct.Registre.FirstOrDefault(r => r.RegistreID == IdRegistre && r.QrCode.Trim().ToUpper() != "DEFAUT".ToUpper().Trim() && CodeStatut == (int)Enumeration.Registre.CREE);
                RegistreView regV = new RegistreView();

                if (registre != null)
                {
                    regV.Registre = registre;

                    //Récupération du versement du registre                      
                    regV.Versement = ct.Versement.FirstOrDefault(v => v.VersementID == registre.VersementID);
                    Livraison livraison = ct.Livraison.FirstOrDefault(l => l.LivraisonID == regV.Versement.LivraisonID);
                    regV.ServiceVersant = ct.Service.FirstOrDefault(s => s.ServiceID == livraison.ServiceID);

                    // récupération des images liées au registre dans la Base de Données
                    regV.NombreImageCompte = ct.Image.Where(i => i.RegistreID == regV.Registre.RegistreID).Count();

                    //Nombre de page scanné dans le Dossier du registre
                    regV.NombreImageScan = 0;

                    try
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Scan"], registre.CheminDossier));
                        //DirectoryInfo directoryInfo = new DirectoryInfo(ConfigurationManager.AppSettings["CheminDossier_Scan"] + "/"+ reg.CheminDossier);                       
                        if (directoryInfo.Exists)
                        {
                            regV.NombreImageScan = directoryInfo.GetFiles().Where(f => f.Extension.ToLower() == ".tif"
                                 || f.Extension.ToLower() == ".png" || f.Extension.ToLower() == ".jpg" || f.Extension.ToLower() == ".pdf").ToList().Count();
                        }
                        else
                        {
                            return null;
                        }
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }

                    //Définition du chemin d'accès au dossier
                    regV.CheminDossier = Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Scan"], registre.CheminDossier);


                    // Affichage de statut en string
                    regV.StatutActuel = ct.StatutRegistre.FirstOrDefault(s => s.Code == registre.StatutActuel && s.RegistreID == registre.RegistreID);
                    regV.StatutName = Enum.GetName(typeof(Models.Enumeration.Registre), regV.StatutActuel.Code);
                }

                return regV;
            }
        }

        public List<RegistreView> GetViewsListByStatus(int CodeStatut)
        {
            try
            {
                using (var ct = new DocumatContext())
                {
                    List<Registre> registres = new List<Registre>();
                    if (Versement != null)
                    {
                        registres = ct.Registre.Where(r => r.VersementID == Versement.VersementID
                                                            && r.StatutActuel == CodeStatut && r.QrCode != "DEFAUT").ToList();
                    }
                    else
                    {
                        registres = ct.Registre.Where(r => r.StatutActuel == CodeStatut && r.QrCode != "DEFAUT").ToList();
                    }

                    List<RegistreView> RegistreViews = new List<RegistreView>();

                    // Numero Ordre des Registres
                    foreach (Registre reg in registres)
                    {
                        RegistreView regV = new RegistreView();
                        regV.Registre = reg;

                        //Récupération du versement du registre  
                        if (Versement != null)
                        {
                            // Récupération des informations sur le service 
                            regV.Versement = Versement;
                            Livraison livraison = ct.Livraison.FirstOrDefault(l => l.LivraisonID == Versement.LivraisonID);
                            regV.ServiceVersant = ct.Service.FirstOrDefault(s => s.ServiceID == livraison.ServiceID);
                        }
                        else
                        {
                            regV.Versement = ct.Versement.FirstOrDefault(v => v.VersementID == reg.VersementID);
                            Livraison livraison = ct.Livraison.FirstOrDefault(l => l.LivraisonID == regV.Versement.LivraisonID);
                            regV.ServiceVersant = ct.Service.FirstOrDefault(s => s.ServiceID == livraison.ServiceID);
                        }

                        // récupération des images liées au registre dans la Base de Données
                        regV.NombreImageCompte = ct.Image.Where(i => i.RegistreID == regV.Registre.RegistreID).Count();

                        //Nombre de page scanné dans le Dossier du registre
                        regV.NombreImageScan = 0;

                        try
                        {
                            DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Scan"], reg.CheminDossier));
                            //DirectoryInfo directoryInfo = new DirectoryInfo(ConfigurationManager.AppSettings["CheminDossier_Scan"] + "/"+ reg.CheminDossier);                       
                            if (directoryInfo.Exists)
                            {
                                regV.NombreImageScan = directoryInfo.GetFiles().Where(f => f.Extension.ToLower() == ".tif"
                                     || f.Extension.ToLower() == ".png" || f.Extension.ToLower() == ".jpg" || f.Extension.ToLower() == ".pdf").ToList().Count();
                            }
                            else
                            {
                                MessageBox.Show("Le repertoire du Registre de type : \"" + reg.Type + "\", de code : " + reg.QrCode + " ,dont le chemin est \""
                                                    + reg.CheminDossier + "\" est introuvable", "AVERTISSEMENT", MessageBoxButton.OK, MessageBoxImage.Warning);
                                if (MessageBox.Show("Voulez vous suspendre les requêtes de récuparation des autres registre afin de régler le problème ?", "SUSPENDRE", MessageBoxButton.YesNo, MessageBoxImage.Question)
                                    == MessageBoxResult.Yes)
                                {
                                    return RegistreViews;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message + "\n Repertoire : " + reg.CheminDossier, "AVERTISSEMENT", MessageBoxButton.OK, MessageBoxImage.Warning);
                            if (MessageBox.Show("Voulez vous suspendre les requêtes de récuparation des autres registre afin de régler le problème ?", "SUSPENDRE", MessageBoxButton.YesNo, MessageBoxImage.Question)
                                == MessageBoxResult.Yes)
                            {
                                return RegistreViews;
                            }
                        }

                        //Définition du chemin d'accès au dossier
                        regV.CheminDossier = Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Scan"], reg.CheminDossier);

                        // Affichage de statut en string
                        regV.StatutName = Enum.GetName(typeof(Models.Enumeration.Registre), reg.StatutActuel);

                        RegistreViews.Add(regV);
                    }
                    return RegistreViews; 
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
                return new List<RegistreView>();
            }
        }

        public List<RegistreView> GetViewsListByScanAgent(string login)
        {
            //Récupération des Fichiers .mets de Tout les Répertoires
            DirectoryInfo AlldirectoryInfo = new DirectoryInfo(Path.Combine(DossierRacine, "DOCUMAT"));
            List<DirectoryInfo> ServiceDirectoryInfo = AlldirectoryInfo.GetDirectories().ToList();
            List<FileInfo> filesmets = new List<FileInfo>();
            List<RegistreView> registreViewsAgentScan = new List<RegistreView>();
            string loginAgent = "", sidAgent = "", FullLine = "";

            // Vérification de l'agent dans le fichier
            if (File.Exists(fichier_sid))
            {
                using (StreamReader fileStream = new StreamReader(fichier_sid))
                {
                    while (!fileStream.EndOfStream)
                    {
                        string line = fileStream.ReadLine().Trim();
                        string lineUser = line.Split(' ')[0].ToLower();
                        string lineSid = line.Split(' ')[line.Split(' ').Length - 1].ToLower();

                        if (login.ToLower().Trim() == lineUser)
                        {
                            loginAgent = lineUser;
                            sidAgent = lineSid;
                            FullLine = line;
                            break;
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Fichier Configuration de sid manquant ", "Fichier agent de scan introuvable", MessageBoxButton.OK, MessageBoxImage.Warning);
                return registreViewsAgentScan;
            }

            // Cas où le Login Agent est introuvable
            if (string.IsNullOrWhiteSpace(loginAgent))
            {
                MessageBox.Show("Votre Login ne se Trouve pas dans le Fichier de configuration ", "Login introuvable", MessageBoxButton.OK, MessageBoxImage.Warning);
                return registreViewsAgentScan;
            }

            foreach (var svdir in ServiceDirectoryInfo)
            {
                List<DirectoryInfo> typedirs = svdir.GetDirectories().ToList();
                foreach (var tpdir in typedirs)
                {
                    List<DirectoryInfo> RegistresDirectories = tpdir.GetDirectories().ToList();
                    foreach (var regdir in RegistresDirectories)
                    {
                        filesmets.AddRange(regdir.GetFiles().Where(f => f.Extension.Trim().ToLower() == ".mets".ToLower()));
                    }
                }
            }

            foreach (var file in filesmets)
            {
                IdentityReference sid = null;
                string owner = null;
                bool fileAgent = false;
                try
                {
                    FileSecurity fileSecurity = File.GetAccessControl(file.FullName);
                    sid = fileSecurity.GetOwner(typeof(SecurityIdentifier));
                    NTAccount ntAccount = sid.Translate(typeof(NTAccount)) as NTAccount;
                    owner = ntAccount.Value;
                }
                catch (IdentityNotMappedException ex)
                {
                    if (sid != null)
                    {
                        owner = sid.ToString();
                    }
                }

                // Recherche du sid agent
                if (sidAgent.Trim().ToUpper() == owner.Trim().ToUpper())
                {
                    fileAgent = true;
                }


                if (fileAgent)
                {
                    using (var ct = new DocumatContext())
                    {
                        Models.Registre registre = ct.Registre.FirstOrDefault(r => r.QrCode.ToLower().Trim() == file.Directory.Name.Trim().ToLower()
                                                                              && r.StatutActuel == (int)Enumeration.Registre.CREE);

                        if (registre != null)
                        {
                            registreViewsAgentScan.Add(GetViewRegistreByStatus((int)Enumeration.Registre.CREE, registre.RegistreID));
                        }
                    }
                }
            }

            return registreViewsAgentScan;
        }


        public bool Update(Registre UpRegistre)
        {
            using (var ct = new DocumatContext())
            {
                if (Registre.DateDepotDebut != UpRegistre.DateDepotDebut || Registre.DateDepotFin != UpRegistre.DateDepotFin
                || Registre.NombrePage != UpRegistre.NombrePage || Registre.NumeroDepotDebut != UpRegistre.NumeroDepotDebut
                || Registre.NumeroDepotFin != UpRegistre.NumeroDepotFin || Registre.Observation != UpRegistre.Observation
                || Registre.Numero != UpRegistre.Numero || Registre.NombrePageDeclaree != UpRegistre.NombrePageDeclaree)
                {
                    Models.Registre registre = ct.Registre.FirstOrDefault(r => r.RegistreID == Registre.RegistreID);
                    if(registre != null)
                    {
                        registre.Numero = UpRegistre.Numero;
                        registre.NombrePageDeclaree = UpRegistre.NombrePageDeclaree;
                        registre.NombrePage = UpRegistre.NombrePage;
                        registre.NumeroDepotDebut = UpRegistre.NumeroDepotDebut;
                        registre.DateDepotDebut = UpRegistre.DateDepotDebut;
                        registre.NumeroDepotFin = UpRegistre.NumeroDepotFin;
                        registre.DateDepotFin = UpRegistre.DateDepotFin;
                        registre.Observation = UpRegistre.Observation;
                        registre.DateModif = DateTime.Now;
                        ct.SaveChanges();
                        return true;
                    }
                }
                else
                {

                } throw new Exception("Aucun changement n'a été effectué");
            }
        }

        public List<RegistreView> GetViewsListByTraite(int codeTraite, bool isTraite, int CodeStatus)
        {
            using (var ct = new DocumatContext())
            {
                RegistreView RegistreView1 = new RegistreView();
                List<RegistreView> registreViewIsTraites = new List<RegistreView>();
                List<RegistreView> registreViewNotTraites = new List<RegistreView>();
                List<RegistreView> RegistreViews = RegistreView1.GetViewsListByStatus(CodeStatus).ToList();

                foreach (RegistreView registreView in RegistreViews)
                {
                    // Si le registre a le traitement demandé par le codeTraite alors on le met dans registreViewIsTraites sinon 
                    // il va dans les registreViewNotTraites
                    Models.Traitement traitement = ct.Traitement.FirstOrDefault(t => t.TableSelect == "Registres" && t.TableID == registreView.Registre.RegistreID
                       && t.TypeTraitement == codeTraite);
                    if (traitement != null)
                    {
                        registreView.AgentTraitant = ct.Agent.FirstOrDefault(a => a.AgentID == traitement.AgentID);
                        registreViewIsTraites.Add(registreView);
                    }
                    else
                    {
                        registreViewNotTraites.Add(registreView);
                    }
                }

                if (isTraite)
                {
                    return registreViewIsTraites;
                }
                else
                {
                    return registreViewNotTraites;
                } 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CodeStatut"></param>
        /// <returns></returns>
        public static List<RegistreDataModel> GetRegistreData(int? CodeStatut = null)
        {
            try
            {
                using (var ct = new DocumatContext())
                {
                    // Nombre d'index du Registres 
                    string reqAllreg = "SELECT RG.*, RG.DateCreation AS DateCreationRegistre,RG.DateModif AS DateModifRegistre, " +
                                         "VS.NumeroVers ,VS.NomAgentVersant, VS.PrenomsAgentVersant, VS.DateVers, VS.cheminBordereau, VS.NombreRegistreR3, VS.NombreRegistreR4, VS.DateCreation AS DateCreationVersement, VS.DateModif AS DateModifVersement, " +
                                         "LV.LivraisonID ,LV.Numero AS NumeroLivraison,LV.DateLivraison, LV.ServiceID, LV.DateCreation AS DateCreationLivraison, LV.DateModif AS DateModifLivraison, " +
                                         "SV.Code ,SV.Nom ,SV.NomComplet,SV.Description ,SV.NomChefDeService ,SV.PrenomChefDeService, SV.DateCreation AS DateCreationService, SV.DateModif AS DateModifService, " +
                                         "SV.CheminDossier AS CheminDossierService,SV.NombreR3,SV.NombreR4,RE.RegionID,RE.Nom AS NomRegion, RE.DateCreation AS DateCreationRegion, RE.DateModif AS DateModifRegion " +
                                         "FROM REGISTRES RG " +
                                         "INNER JOIN VERSEMENTS VS ON VS.VersementID = RG.VersementID " +
                                         "INNER JOIN LIVRAISONS LV ON LV.LivraisonID = VS.LivraisonID " +
                                         "INNER JOIN SERVICES SV ON SV.ServiceID = LV.ServiceID " +
                                         "INNER JOIN REGIONS RE ON RE.RegionID = SV.RegionID ";

                    string whereClause = "";

                    if (CodeStatut != null)
                    {
                        whereClause = $"WHERE RG.StatutActuel = {CodeStatut} ";
                    }

                    List<RegistreDataModel> dataObjects = ct.Database.SqlQuery<RegistreDataModel>(reqAllreg + whereClause).ToList();

                    foreach (var reg in dataObjects)
                    {
                        if (CodeStatut != null && CodeStatut == (int)Enumeration.Registre.CREE)
                        {
                            // Recherche du Nombre de Page Scannées
                            if (!string.IsNullOrWhiteSpace(reg.CheminDossier))
                            {
                                DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Scan"], reg.CheminDossier));
                                //DirectoryInfo directoryInfo = new DirectoryInfo(ConfigurationManager.AppSettings["CheminDossier_Scan"] + "/"+ reg.CheminDossier);                       
                                if (directoryInfo.Exists)
                                {
                                    reg.NombrePageScan = directoryInfo.GetFiles().Where(f => f.Extension.ToLower() == ".tif"
                                         || f.Extension.ToLower() == ".png" || f.Extension.ToLower() == ".jpg" || f.Extension.ToLower() == ".pdf").ToList().Count();
                                }
                            }
                        }

                        //Définition du statut du registre
                        reg.StatutName = Enum.GetName(typeof(Models.Enumeration.Registre), reg.StatutActuel);
                    }

                    return dataObjects;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
                return null;
            }
        }

        /// <summary>
        ///  Récupération des registres par group par codestatut
        /// </summary>
        /// <param name="limitStart"></param>
        /// <param name="limitLenght"></param>
        /// <param name="CodeStatut"></param>
        /// <returns></returns>
        public static List<RegistreDataModel> GetRegistreData(int limitId, int limitLenght, int? CodeStatut = null)
        {
            try
            {
                using (var ct = new DocumatContext())
                {
                    // Nombre d'index du Registres 
                    string reqAllreg =  $"SELECT TOP {limitLenght}  RG.*, RG.DateCreation AS DateCreationRegistre,RG.DateModif AS DateModifRegistre, " +
                                         "VS.NumeroVers ,VS.NomAgentVersant, VS.PrenomsAgentVersant, VS.DateVers, VS.cheminBordereau, VS.NombreRegistreR3, VS.NombreRegistreR4, VS.DateCreation AS DateCreationVersement, VS.DateModif AS DateModifVersement, " +
                                         "LV.LivraisonID ,LV.Numero AS NumeroLivraison,LV.DateLivraison, LV.ServiceID, LV.DateCreation AS DateCreationLivraison, LV.DateModif AS DateModifLivraison, " +
                                         "SV.Code ,SV.Nom ,SV.NomComplet,SV.Description ,SV.NomChefDeService ,SV.PrenomChefDeService, SV.DateCreation AS DateCreationService, SV.DateModif AS DateModifService, " +
                                         "SV.CheminDossier AS CheminDossierService,SV.NombreR3,SV.NombreR4,RE.RegionID,RE.Nom AS NomRegion, RE.DateCreation AS DateCreationRegion, RE.DateModif AS DateModifRegion " +
                                         "FROM REGISTRES RG " +
                                         "INNER JOIN VERSEMENTS VS ON VS.VersementID = RG.VersementID " +
                                         "INNER JOIN LIVRAISONS LV ON LV.LivraisonID = VS.LivraisonID " +
                                         "INNER JOIN SERVICES SV ON SV.ServiceID = LV.ServiceID " +
                                         "INNER JOIN REGIONS RE ON RE.RegionID = SV.RegionID ";

                    // Définition de la limite de récupération
                    string whereClause = (limitId > 0) ? $"WHERE RG.RegistreID > {limitId} " : "";

                    if (CodeStatut != null)
                    {
                        whereClause += (limitId > 0) ? $" AND RG.StatutActuel = {CodeStatut}" : $"WHERE RG.StatutActuel = {CodeStatut}";
                    }
                    whereClause += " ORDER BY RG.RegistreID";

                    List<RegistreDataModel> dataObjects = ct.Database.SqlQuery<RegistreDataModel>(reqAllreg + whereClause).ToList();

                    foreach (var reg in dataObjects)
                    {
                        if (CodeStatut != null && CodeStatut == (int)Enumeration.Registre.CREE)
                        {
                            // Recherche du Nombre de Page Scannées
                            if (!string.IsNullOrWhiteSpace(reg.CheminDossier))
                            {
                                DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Scan"], reg.CheminDossier));
                                //DirectoryInfo directoryInfo = new DirectoryInfo(ConfigurationManager.AppSettings["CheminDossier_Scan"] + "/"+ reg.CheminDossier);                       
                                if (directoryInfo.Exists)
                                {
                                    reg.NombrePageScan = directoryInfo.GetFiles().Where(f => f.Extension.ToLower() == ".tif"
                                         || f.Extension.ToLower() == ".png" || f.Extension.ToLower() == ".jpg" || f.Extension.ToLower() == ".pdf").ToList().Count();
                                }
                            }
                        }

                        //Définition du statut du registre
                        reg.StatutName = Enum.GetName(typeof(Models.Enumeration.Registre), reg.StatutActuel);
                        reg.PercentPageTraiteIndex = Math.Round((((double)(reg.NombrePageIndex + reg.NombrePageInst) / (double)reg.NombrePageScan) * 100), 1);
                    }

                    return dataObjects;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
                return null;
            }
        }

        public static List<RegistreDataModel> GetRegistreDataByTrait(int codeTraite, int? CodeStatut = null)
        {
            try
            {
                using (var ct = new DocumatContext())
                {
                    // Nombre d'index du Registres 
                    string reqAllreg = "SELECT RG.*, RG.DateCreation AS DateCreationRegistre,RG.DateModif AS DateModifRegistre, " +
                                         "VS.NumeroVers ,VS.NomAgentVersant, VS.PrenomsAgentVersant, VS.DateVers, VS.cheminBordereau, VS.NombreRegistreR3, VS.NombreRegistreR4, VS.DateCreation AS DateCreationVersement, VS.DateModif AS DateModifVersement, " +
                                         "LV.LivraisonID ,LV.Numero AS NumeroLivraison,LV.DateLivraison, LV.ServiceID, LV.DateCreation AS DateCreationLivraison, LV.DateModif AS DateModifLivraison, " +
                                         "SV.Code ,SV.Nom ,SV.NomComplet,SV.Description ,SV.NomChefDeService ,SV.PrenomChefDeService, SV.DateCreation AS DateCreationService, SV.DateModif AS DateModifService, " +
                                         "SV.CheminDossier AS CheminDossierService,SV.NombreR3,SV.NombreR4,RE.RegionID,RE.Nom AS NomRegion, RE.DateCreation AS DateCreationRegion, RE.DateModif AS DateModifRegion " +
                                         "FROM REGISTRES RG " +
                                         "INNER JOIN VERSEMENTS VS ON VS.VersementID = RG.VersementID " +
                                         "INNER JOIN LIVRAISONS LV ON LV.LivraisonID = VS.LivraisonID " +
                                         "INNER JOIN SERVICES SV ON SV.ServiceID = LV.ServiceID " +
                                         "INNER JOIN REGIONS RE ON RE.RegionID = SV.RegionID " +
                                         "INNER JOIN TRAITEMENTS TR ON TR.TableID = RG.RegistreID ";

                    string whereClause = $"WHERE Lower(TR.TableSelect) = Lower('Registres') AND TR.TypeTraitement = {codeTraite}";

                    if (CodeStatut != null)
                    {
                        whereClause += $" AND RG.StatutActuel = {CodeStatut} ";
                    }

                    List<RegistreDataModel> dataObjects = ct.Database.SqlQuery<RegistreDataModel>(reqAllreg + whereClause).ToList();

                    foreach (var reg in dataObjects)
                    {
                        if (CodeStatut != null && CodeStatut == (int)Enumeration.Registre.CREE)
                        {
                            // Recherche du Nombre de Page Scannées
                            if (!string.IsNullOrWhiteSpace(reg.CheminDossier))
                            {
                                DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Scan"], reg.CheminDossier));
                                //DirectoryInfo directoryInfo = new DirectoryInfo(ConfigurationManager.AppSettings["CheminDossier_Scan"] + "/"+ reg.CheminDossier);                       
                                if (directoryInfo.Exists)
                                {
                                    reg.NombrePageScan = directoryInfo.GetFiles().Where(f => f.Extension.ToLower() == ".tif"
                                         || f.Extension.ToLower() == ".png" || f.Extension.ToLower() == ".jpg" || f.Extension.ToLower() == ".pdf").ToList().Count();
                                }
                            }
                        }

                        //Définition du statut du registre
                        reg.StatutName = Enum.GetName(typeof(Models.Enumeration.Registre), reg.StatutActuel);
                    }

                    return dataObjects;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
                return null;
            }
        }

        public static List<RegistreDataModel> GetRegistreDataByTrait(int codeTraite,int limitId, int limitLenght, int? CodeStatut = null)
        {
            try
            {
                using (var ct = new DocumatContext())
                {
                    // Nombre d'index du Registres 
                    string reqAllreg =  $"SELECT TOP {limitLenght} RG.*, RG.DateCreation AS DateCreationRegistre,RG.DateModif AS DateModifRegistre, " +
                                         "VS.NumeroVers ,VS.NomAgentVersant, VS.PrenomsAgentVersant, VS.DateVers, VS.cheminBordereau, VS.NombreRegistreR3, VS.NombreRegistreR4, VS.DateCreation AS DateCreationVersement, VS.DateModif AS DateModifVersement, " +
                                         "LV.LivraisonID ,LV.Numero AS NumeroLivraison,LV.DateLivraison, LV.ServiceID, LV.DateCreation AS DateCreationLivraison, LV.DateModif AS DateModifLivraison, " +
                                         "SV.Code ,SV.Nom ,SV.NomComplet,SV.Description ,SV.NomChefDeService ,SV.PrenomChefDeService, SV.DateCreation AS DateCreationService, SV.DateModif AS DateModifService, " +
                                         "SV.CheminDossier AS CheminDossierService,SV.NombreR3,SV.NombreR4,RE.RegionID,RE.Nom AS NomRegion, RE.DateCreation AS DateCreationRegion, RE.DateModif AS DateModifRegion " +
                                         "FROM REGISTRES RG " +
                                         "INNER JOIN VERSEMENTS VS ON VS.VersementID = RG.VersementID " +
                                         "INNER JOIN LIVRAISONS LV ON LV.LivraisonID = VS.LivraisonID " +
                                         "INNER JOIN SERVICES SV ON SV.ServiceID = LV.ServiceID " +
                                         "INNER JOIN REGIONS RE ON RE.RegionID = SV.RegionID " +
                                         "INNER JOIN TRAITEMENTS TR ON TR.TableID = RG.RegistreID ";

                    string whereClause = $"WHERE Lower(TR.TableSelect) = Lower('Registres') AND TR.TypeTraitement = {codeTraite}";
                    whereClause += (limitId > 0) ? $"AND RG.RegistreID > {limitId} " : "";

                    if (CodeStatut != null)
                    {
                        whereClause += $" AND RG.StatutActuel = {CodeStatut}";
                    }
                    whereClause += " ORDER BY RG.RegistreID";

                    List<RegistreDataModel> dataObjects = ct.Database.SqlQuery<RegistreDataModel>(reqAllreg + whereClause).ToList();

                    foreach (var reg in dataObjects)
                    {
                        if (CodeStatut != null && CodeStatut == (int)Enumeration.Registre.CREE)
                        {
                            // Recherche du Nombre de Page Scannées
                            if (!string.IsNullOrWhiteSpace(reg.CheminDossier))
                            {
                                DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Scan"], reg.CheminDossier));
                                //DirectoryInfo directoryInfo = new DirectoryInfo(ConfigurationManager.AppSettings["CheminDossier_Scan"] + "/"+ reg.CheminDossier);                       
                                if (directoryInfo.Exists)
                                {
                                    reg.NombrePageScan = directoryInfo.GetFiles().Where(f => f.Extension.ToLower() == ".tif"
                                         || f.Extension.ToLower() == ".png" || f.Extension.ToLower() == ".jpg" || f.Extension.ToLower() == ".pdf").ToList().Count();
                                }
                            }
                        }

                        //Définition du statut du registre
                        reg.StatutName = Enum.GetName(typeof(Models.Enumeration.Registre), reg.StatutActuel);
                    }

                    return dataObjects;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
                return null;
            }
        }

        public static List<RegistreDataModel> GetRegistreDataByTraitbyAgent(int idAgent, int codeTraite, int? CodeStatut = null)
        {
            try
            {
                using (var ct = new DocumatContext())
                {
                    // Nombre d'index du Registres 
                    string reqAllreg = "SELECT RG.*, RG.DateCreation AS DateCreationRegistre,RG.DateModif AS DateModifRegistre, " +
                                         "VS.NumeroVers ,VS.NomAgentVersant, VS.PrenomsAgentVersant, VS.DateVers, VS.cheminBordereau, VS.NombreRegistreR3, VS.NombreRegistreR4, VS.DateCreation AS DateCreationVersement, VS.DateModif AS DateModifVersement, " +
                                         "LV.LivraisonID ,LV.Numero AS NumeroLivraison,LV.DateLivraison, LV.ServiceID, LV.DateCreation AS DateCreationLivraison, LV.DateModif AS DateModifLivraison, " +
                                         "SV.Code ,SV.Nom ,SV.NomComplet,SV.Description ,SV.NomChefDeService ,SV.PrenomChefDeService, SV.DateCreation AS DateCreationService, SV.DateModif AS DateModifService, " +
                                         "SV.CheminDossier AS CheminDossierService,SV.NombreR3,SV.NombreR4,RE.RegionID,RE.Nom AS NomRegion, RE.DateCreation AS DateCreationRegion, RE.DateModif AS DateModifRegion " +
                                         "FROM REGISTRES RG " +
                                         "INNER JOIN VERSEMENTS VS ON VS.VersementID = RG.VersementID " +
                                         "INNER JOIN LIVRAISONS LV ON LV.LivraisonID = VS.LivraisonID " +
                                         "INNER JOIN SERVICES SV ON SV.ServiceID = LV.ServiceID " +
                                         "INNER JOIN REGIONS RE ON RE.RegionID = SV.RegionID " +
                                         "INNER JOIN TRAITEMENTS TR ON TR.TableID = RG.RegistreID ";

                    string whereClause = $"WHERE Lower(TR.TableSelect) = Lower('Registres') AND TR.TypeTraitement = {codeTraite} " +
                                         $"AND TR.AgentID = {idAgent} ";

                    if (CodeStatut != null)
                    {
                        whereClause += $" AND RG.StatutActuel = {CodeStatut} ";
                    }

                    List<RegistreDataModel> dataObjects = ct.Database.SqlQuery<RegistreDataModel>(reqAllreg + whereClause).ToList();

                    foreach (var reg in dataObjects)
                    {
                        if (CodeStatut != null && CodeStatut == (int)Enumeration.Registre.CREE)
                        {
                            // Recherche du Nombre de Page Scannées
                            if (!string.IsNullOrWhiteSpace(reg.CheminDossier))
                            {
                                DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Scan"], reg.CheminDossier));
                                //DirectoryInfo directoryInfo = new DirectoryInfo(ConfigurationManager.AppSettings["CheminDossier_Scan"] + "/"+ reg.CheminDossier);                       
                                if (directoryInfo.Exists)
                                {
                                    reg.NombrePageScan = directoryInfo.GetFiles().Where(f => f.Extension.ToLower() == ".tif"
                                         || f.Extension.ToLower() == ".png" || f.Extension.ToLower() == ".jpg" || f.Extension.ToLower() == ".pdf").ToList().Count();
                                }
                            }
                        }

                        //Définition du statut du registre
                        reg.StatutName = Enum.GetName(typeof(Models.Enumeration.Registre), reg.StatutActuel);
                    }

                    return dataObjects;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
                return null;
            }
        }

        public static List<RegistreDataModel> GetRegistreDataByTraitbyAgent(int idAgent, int codeTraite, int limitId, int limitLenght, int? CodeStatut = null)
        {
            try
            {
                using (var ct = new DocumatContext())
                {
                    // Nombre d'index du Registres 
                    string reqAllreg = $"SELECT TOP {limitLenght} RG.*, RG.DateCreation AS DateCreationRegistre,RG.DateModif AS DateModifRegistre, " +
                                        "VS.NumeroVers ,VS.NomAgentVersant, VS.PrenomsAgentVersant, VS.DateVers, VS.cheminBordereau, VS.NombreRegistreR3, VS.NombreRegistreR4, VS.DateCreation AS DateCreationVersement, VS.DateModif AS DateModifVersement, " +
                                        "LV.LivraisonID ,LV.Numero AS NumeroLivraison,LV.DateLivraison, LV.ServiceID, LV.DateCreation AS DateCreationLivraison, LV.DateModif AS DateModifLivraison, " +
                                        "SV.Code ,SV.Nom ,SV.NomComplet,SV.Description ,SV.NomChefDeService ,SV.PrenomChefDeService, SV.DateCreation AS DateCreationService, SV.DateModif AS DateModifService, " +
                                        "SV.CheminDossier AS CheminDossierService,SV.NombreR3,SV.NombreR4,RE.RegionID,RE.Nom AS NomRegion, RE.DateCreation AS DateCreationRegion, RE.DateModif AS DateModifRegion " +
                                        "FROM REGISTRES RG " +
                                        "INNER JOIN VERSEMENTS VS ON VS.VersementID = RG.VersementID " +
                                        "INNER JOIN LIVRAISONS LV ON LV.LivraisonID = VS.LivraisonID " +
                                        "INNER JOIN SERVICES SV ON SV.ServiceID = LV.ServiceID " +
                                        "INNER JOIN REGIONS RE ON RE.RegionID = SV.RegionID " +
                                        "INNER JOIN TRAITEMENTS TR ON TR.TableID = RG.RegistreID ";

                    string whereClause = $"WHERE Lower(TR.TableSelect) = Lower('Registres') AND TR.TypeTraitement = {codeTraite} " +
                                         $"AND TR.AgentID = {idAgent} ";
                    whereClause += (limitId > 0) ? $"AND RG.RegistreID > {limitId} " : "";

                    if (CodeStatut != null)
                    {
                        whereClause += $" AND RG.StatutActuel = {CodeStatut}";
                    }
                    whereClause += " ORDER BY RG.RegistreID";

                    List<RegistreDataModel> dataObjects = ct.Database.SqlQuery<RegistreDataModel>(reqAllreg + whereClause).ToList();

                    foreach (var reg in dataObjects)
                    {
                        if (CodeStatut != null && CodeStatut == (int)Enumeration.Registre.CREE)
                        {
                            // Recherche du Nombre de Page Scannées
                            if (!string.IsNullOrWhiteSpace(reg.CheminDossier))
                            {
                                DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Scan"], reg.CheminDossier));
                                //DirectoryInfo directoryInfo = new DirectoryInfo(ConfigurationManager.AppSettings["CheminDossier_Scan"] + "/"+ reg.CheminDossier);                       
                                if (directoryInfo.Exists)
                                {
                                    reg.NombrePageScan = directoryInfo.GetFiles().Where(f => f.Extension.ToLower() == ".tif"
                                         || f.Extension.ToLower() == ".png" || f.Extension.ToLower() == ".jpg" || f.Extension.ToLower() == ".pdf").ToList().Count();
                                }
                            }
                        }

                        //Définition du statut du registre
                        reg.StatutName = Enum.GetName(typeof(Models.Enumeration.Registre), reg.StatutActuel);
                    }

                    return dataObjects;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
                return null;
            }
        }


        public static List<RegistreDataModel> GetRegistreDataByUnite(int uniteID,int limitId, int limitLenght, int? CodeStatut = null)
        {
            try
            {
                using (var ct = new DocumatContext())
                {
                    // Nombre d'index du Registres 
                    string reqAllreg = $"SELECT TOP {limitLenght}  RG.*, RG.DateCreation AS DateCreationRegistre,RG.DateModif AS DateModifRegistre, " +
                                         "VS.NumeroVers ,VS.NomAgentVersant, VS.PrenomsAgentVersant, VS.DateVers, VS.cheminBordereau, VS.NombreRegistreR3, VS.NombreRegistreR4, VS.DateCreation AS DateCreationVersement, VS.DateModif AS DateModifVersement, " +
                                         "LV.LivraisonID ,LV.Numero AS NumeroLivraison,LV.DateLivraison, LV.ServiceID, LV.DateCreation AS DateCreationLivraison, LV.DateModif AS DateModifLivraison, " +
                                         "SV.Code ,SV.Nom ,SV.NomComplet,SV.Description ,SV.NomChefDeService ,SV.PrenomChefDeService, SV.DateCreation AS DateCreationService, SV.DateModif AS DateModifService, " +
                                         "SV.CheminDossier AS CheminDossierService,SV.NombreR3,SV.NombreR4,RE.RegionID,RE.Nom AS NomRegion, RE.DateCreation AS DateCreationRegion, RE.DateModif AS DateModifRegion " +
                                         "FROM REGISTRES RG " +
                                         "INNER JOIN VERSEMENTS VS ON VS.VersementID = RG.VersementID " +
                                         "INNER JOIN LIVRAISONS LV ON LV.LivraisonID = VS.LivraisonID " +
                                         "INNER JOIN SERVICES SV ON SV.ServiceID = LV.ServiceID " +
                                         "INNER JOIN REGIONS RE ON RE.RegionID = SV.RegionID " +
                                         $"WHERE RG.ID_Unite = {uniteID}";

                    // Définition de la limite de récupération
                    string whereClause = (limitId > 0) ? $"AND RG.RegistreID > {limitId} " : "";

                    if (CodeStatut != null)
                    {
                        whereClause += $" AND RG.StatutActuel = {CodeStatut}";
                    }
                    whereClause += " ORDER BY RG.RegistreID";

                    List<RegistreDataModel> dataObjects = ct.Database.SqlQuery<RegistreDataModel>(reqAllreg + whereClause).ToList();

                    foreach (var reg in dataObjects)
                    {
                        if (CodeStatut != null && CodeStatut == (int)Enumeration.Registre.CREE)
                        {
                            // Recherche du Nombre de Page Scannées
                            if (!string.IsNullOrWhiteSpace(reg.CheminDossier))
                            {
                                DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Scan"], reg.CheminDossier));
                                //DirectoryInfo directoryInfo = new DirectoryInfo(ConfigurationManager.AppSettings["CheminDossier_Scan"] + "/"+ reg.CheminDossier);                       
                                if (directoryInfo.Exists)
                                {
                                    reg.NombrePageScan = directoryInfo.GetFiles().Where(f => f.Extension.ToLower() == ".tif"
                                         || f.Extension.ToLower() == ".png" || f.Extension.ToLower() == ".jpg" || f.Extension.ToLower() == ".pdf").ToList().Count();
                                }
                            } 
                        }

                        //Définition du statut du registre
                        reg.StatutName = Enum.GetName(typeof(Models.Enumeration.Registre), reg.StatutActuel);
                    }

                    return dataObjects;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
                return null;
            }
        }


        public static List<RegistreDataModel> GetRegistreDataControlePhase2(int uniteID, int limitId, int limitLenght, int CodeStatut)
        {
            try
            {
                using (var ct = new DocumatContext())
                {
                    // Nombre d'index du Registres 
                    string reqAllreg = $"SELECT TOP {limitLenght}  RG.*, RG.DateCreation AS DateCreationRegistre,RG.DateModif AS DateModifRegistre, " +
                                         "VS.NumeroVers ,VS.NomAgentVersant, VS.PrenomsAgentVersant, VS.DateVers, VS.cheminBordereau, VS.NombreRegistreR3, VS.NombreRegistreR4, VS.DateCreation AS DateCreationVersement, VS.DateModif AS DateModifVersement, " +
                                         "LV.LivraisonID ,LV.Numero AS NumeroLivraison,LV.DateLivraison, LV.ServiceID, LV.DateCreation AS DateCreationLivraison, LV.DateModif AS DateModifLivraison, " +
                                         "SV.Code ,SV.Nom ,SV.NomComplet,SV.Description ,SV.NomChefDeService ,SV.PrenomChefDeService, SV.DateCreation AS DateCreationService, SV.DateModif AS DateModifService, " +
                                         "SV.CheminDossier AS CheminDossierService,SV.NombreR3,SV.NombreR4,RE.RegionID,RE.Nom AS NomRegion, RE.DateCreation AS DateCreationRegion, RE.DateModif AS DateModifRegion " +
                                         "FROM REGISTRES RG " +
                                         "INNER JOIN VERSEMENTS VS ON VS.VersementID = RG.VersementID " +
                                         "INNER JOIN LIVRAISONS LV ON LV.LivraisonID = VS.LivraisonID " +
                                         "INNER JOIN SERVICES SV ON SV.ServiceID = LV.ServiceID " +
                                         "INNER JOIN REGIONS RE ON RE.RegionID = SV.RegionID " +
                                         "INNER JOIN CORRECTIONS CR ON CR.RegistreID = RG.RegistreID " +
                                         $"WHERE RG.ID_Unite = {uniteID} AND CR.ImageID IS NULL AND CR.SequenceID IS NULL AND CR.StatutCorrection = 0 AND CR.PhaseCorrection <> 3" +
                                         $"AND RG.StatutActuel = {CodeStatut}";

                    // Définition de la limite de récupération
                    string whereClause = (limitId > 0) ? $"AND RG.RegistreID > {limitId} " : "";
                    whereClause += " ORDER BY RG.RegistreID";
                    List<RegistreDataModel> dataObjects = ct.Database.SqlQuery<RegistreDataModel>(reqAllreg + whereClause).ToList();

                    foreach (var reg in dataObjects)
                    {
                        //Définition du statut du registre
                        reg.StatutName = Enum.GetName(typeof(Models.Enumeration.Registre), reg.StatutActuel);
                    }

                    return dataObjects;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
                return null;
            }
        }

        public static List<RegistreDataModel> GetRegistreDataControlePhaseFinal(int uniteID, int limitId, int limitLenght, int CodeStatut)
        {
            try
            {
                using (var ct = new DocumatContext())
                {
                    // Nombre d'index du Registres 
                    string reqAllreg =  $"SELECT TOP {limitLenght}  RG.*, RG.DateCreation AS DateCreationRegistre,RG.DateModif AS DateModifRegistre, " +
                                         "VS.NumeroVers ,VS.NomAgentVersant, VS.PrenomsAgentVersant, VS.DateVers, VS.cheminBordereau, VS.NombreRegistreR3, VS.NombreRegistreR4, VS.DateCreation AS DateCreationVersement, VS.DateModif AS DateModifVersement, " +
                                         "LV.LivraisonID ,LV.Numero AS NumeroLivraison,LV.DateLivraison, LV.ServiceID, LV.DateCreation AS DateCreationLivraison, LV.DateModif AS DateModifLivraison, " +
                                         "SV.Code ,SV.Nom ,SV.NomComplet,SV.Description ,SV.NomChefDeService ,SV.PrenomChefDeService, SV.DateCreation AS DateCreationService, SV.DateModif AS DateModifService, " +
                                         "SV.CheminDossier AS CheminDossierService,SV.NombreR3,SV.NombreR4,RE.RegionID,RE.Nom AS NomRegion, RE.DateCreation AS DateCreationRegion, RE.DateModif AS DateModifRegion " +
                                         "FROM REGISTRES RG " +
                                         "INNER JOIN VERSEMENTS VS ON VS.VersementID = RG.VersementID " +
                                         "INNER JOIN LIVRAISONS LV ON LV.LivraisonID = VS.LivraisonID " +
                                         "INNER JOIN SERVICES SV ON SV.ServiceID = LV.ServiceID " +
                                         "INNER JOIN REGIONS RE ON RE.RegionID = SV.RegionID " +
                                         "INNER JOIN CORRECTIONS CR ON CR.RegistreID = RG.RegistreID " +
                                         $"WHERE RG.ID_Unite = {uniteID} AND CR.ImageID IS NULL AND CR.SequenceID IS NULL AND CR.PhaseCorrection = 3 " +
                                         $"AND RG.StatutActuel = {CodeStatut}";

                    // Définition de la limite de récupération
                    string whereClause = (limitId > 0) ? $"AND RG.RegistreID > {limitId} " : "";
                    whereClause += " ORDER BY RG.RegistreID";
                    List<RegistreDataModel> dataObjects = ct.Database.SqlQuery<RegistreDataModel>(reqAllreg + whereClause).ToList();

                    foreach (var reg in dataObjects)
                    {
                        //Définition du statut du registre
                        reg.StatutName = Enum.GetName(typeof(Models.Enumeration.Registre), reg.StatutActuel);
                    }

                    return dataObjects;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
                return null;
            }
        }


        public static List<RegistreDataModel> GetRegistreDataCorrectionPhase1(int uniteID, int limitId, int limitLenght, int CodeStatut)
        {
            try
            {
                using (var ct = new DocumatContext())
                {
                    // Nombre d'index du Registres 
                    string reqAllreg =  $"SELECT TOP {limitLenght}  RG.*, RG.DateCreation AS DateCreationRegistre,RG.DateModif AS DateModifRegistre, " +
                                         "VS.NumeroVers ,VS.NomAgentVersant, VS.PrenomsAgentVersant, VS.DateVers, VS.cheminBordereau, VS.NombreRegistreR3, VS.NombreRegistreR4, VS.DateCreation AS DateCreationVersement, VS.DateModif AS DateModifVersement, " +
                                         "LV.LivraisonID ,LV.Numero AS NumeroLivraison,LV.DateLivraison, LV.ServiceID, LV.DateCreation AS DateCreationLivraison, LV.DateModif AS DateModifLivraison, " +
                                         "SV.Code ,SV.Nom ,SV.NomComplet,SV.Description ,SV.NomChefDeService ,SV.PrenomChefDeService, SV.DateCreation AS DateCreationService, SV.DateModif AS DateModifService, " +
                                         "SV.CheminDossier AS CheminDossierService,SV.NombreR3,SV.NombreR4,RE.RegionID,RE.Nom AS NomRegion, RE.DateCreation AS DateCreationRegion, RE.DateModif AS DateModifRegion " +
                                         "FROM REGISTRES RG " +
                                         "INNER JOIN VERSEMENTS VS ON VS.VersementID = RG.VersementID " +
                                         "INNER JOIN LIVRAISONS LV ON LV.LivraisonID = VS.LivraisonID " +
                                         "INNER JOIN SERVICES SV ON SV.ServiceID = LV.ServiceID " +
                                         "INNER JOIN REGIONS RE ON RE.RegionID = SV.RegionID " +
                                         "INNER JOIN CORRECTIONS CR ON CR.RegistreID = RG.RegistreID " +
                                        $"WHERE RG.ID_Unite = {uniteID} AND CR.ImageID IS NULL AND CR.SequenceID IS NULL AND CR.StatutCorrection = 1 AND CR.PhaseCorrection = 1 " +
                                        $"AND RG.StatutActuel = {CodeStatut}";

                    // Définition de la limite de récupération
                    string whereClause = (limitId > 0) ? $"AND RG.RegistreID > {limitId} " : "";
                    whereClause += " ORDER BY RG.RegistreID";
                    List<RegistreDataModel> dataObjects = ct.Database.SqlQuery<RegistreDataModel>(reqAllreg + whereClause).ToList();

                    foreach (var reg in dataObjects)
                    {
                        //Définition du statut du registre
                        reg.StatutName = Enum.GetName(typeof(Models.Enumeration.Registre), reg.StatutActuel);
                    }

                    return dataObjects;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
                return null;
            }
        }

        public static List<RegistreDataModel> GetRegistreDataByScanAgent(string login)
        {
            //Récupération des Fichiers .mets de Tout les Répertoires            
            string DossierRacine = ConfigurationManager.AppSettings["CheminDossier_Scan"];
            string fichier_sid = Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Scan"], "sid_scanners_users.tmp");
            DirectoryInfo AlldirectoryInfo = new DirectoryInfo(Path.Combine(DossierRacine, "DOCUMAT"));
            List<DirectoryInfo> ServiceDirectoryInfo = AlldirectoryInfo.GetDirectories().ToList();
            List<FileInfo> filesmets = new List<FileInfo>();
            List<RegistreDataModel> registreViewsAgentScan = new List<RegistreDataModel>();
            string loginAgent = "", sidAgent = "", FullLine = "", ListRegistreScanAgent = "";

            // Vérification de l'agent dans le fichier
            if (File.Exists(fichier_sid))
            {
                using (StreamReader fileStream = new StreamReader(fichier_sid))
                {
                    while (!fileStream.EndOfStream)
                    {
                        string line = fileStream.ReadLine().Trim();
                        string lineUser = line.Split(' ')[0].ToLower();
                        string lineSid = line.Split(' ')[line.Split(' ').Length - 1].ToLower();

                        if (login.ToLower().Trim() == lineUser)
                        {
                            loginAgent = lineUser;
                            sidAgent = lineSid;
                            FullLine = line;
                            break;
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Fichier Configuration de sid manquant ", "Fichier agent de scan introuvable", MessageBoxButton.OK, MessageBoxImage.Warning);
                return registreViewsAgentScan;
            }

            // Cas où le Login Agent est introuvable
            if (string.IsNullOrWhiteSpace(loginAgent))
            {
                MessageBox.Show($"Votre Login ne se Trouve pas dans le Fichier de Config : {fichier_sid}", "Login introuvable", MessageBoxButton.OK, MessageBoxImage.Warning);
                return registreViewsAgentScan;
            }

            // Récupération des Fichier .mets
            foreach (var svdir in ServiceDirectoryInfo)
            {
                List<DirectoryInfo> typedirs = svdir.GetDirectories().ToList();
                foreach (var tpdir in typedirs)
                {
                    List<DirectoryInfo> RegistresDirectories = tpdir.GetDirectories().ToList();
                    foreach (var regdir in RegistresDirectories)
                    {
                        filesmets.AddRange(regdir.GetFiles().Where(f => f.Extension.Trim().ToLower() == ".mets".ToLower()));
                    }
                }
            }

            // Pour chaque fichier .mets Récupérer, 
            foreach (var file in filesmets)
            {
                IdentityReference fileSid = null;
                string owner = null;
                bool fileAgent = false;

                try
                {
                    FileSecurity fileSecurity = File.GetAccessControl(file.FullName);
                    fileSid = fileSecurity.GetOwner(typeof(SecurityIdentifier));
                    NTAccount ntAccount = fileSid.Translate(typeof(NTAccount)) as NTAccount;
                    owner = ntAccount.Value;
                }
                catch (IdentityNotMappedException ex)
                {
                    if (fileSid != null)
                    {
                        owner = fileSid.ToString();
                    }
                }

                // Recherche du sid agent
                if (sidAgent.Trim().ToUpper() == fileSid.Value.Trim().ToUpper())
                {
                    fileAgent = true;
                }

                if (fileAgent)
                {
                    ListRegistreScanAgent += $"'{file.Directory.Name.Trim().ToLower()}',";
                }
            }

            if(ListRegistreScanAgent.Length != 0)
            {
                ListRegistreScanAgent = ListRegistreScanAgent.Remove(ListRegistreScanAgent.Length - 1);

                using (var ct = new DocumatContext())
                {
                    // Récupération des registres 
                    string reqAllreg = "SELECT RG.*, RG.DateCreation AS DateCreationRegistre,RG.DateModif AS DateModifRegistre, " +
                                       "VS.NumeroVers ,VS.NomAgentVersant, VS.PrenomsAgentVersant, VS.DateVers, VS.cheminBordereau, VS.NombreRegistreR3, VS.NombreRegistreR4, VS.DateCreation AS DateCreationVersement, VS.DateModif AS DateModifVersement, " +
                                       "LV.LivraisonID ,LV.Numero AS NumeroLivraison,LV.DateLivraison, LV.ServiceID, LV.DateCreation AS DateCreationLivraison, LV.DateModif AS DateModifLivraison, " +
                                       "SV.Code ,SV.Nom ,SV.NomComplet,SV.Description ,SV.NomChefDeService ,SV.PrenomChefDeService, SV.DateCreation AS DateCreationService, SV.DateModif AS DateModifService, " +
                                       "SV.CheminDossier AS CheminDossierService,SV.NombreR3,SV.NombreR4,RE.RegionID,RE.Nom AS NomRegion, RE.DateCreation AS DateCreationRegion, RE.DateModif AS DateModifRegion " +
                                       "FROM REGISTRES RG " +
                                       "INNER JOIN VERSEMENTS VS ON VS.VersementID = RG.VersementID " +
                                       "INNER JOIN LIVRAISONS LV ON LV.LivraisonID = VS.LivraisonID " +
                                       "INNER JOIN SERVICES SV ON SV.ServiceID = LV.ServiceID " +
                                       "INNER JOIN REGIONS RE ON RE.RegionID = SV.RegionID " +
                                       $"WHERE RG.StatutActuel = {(int)Enumeration.Registre.CREE} AND RG.QrCode IN ({ListRegistreScanAgent})";

                    List<RegistreDataModel> dataObjects = ct.Database.SqlQuery<RegistreDataModel>(reqAllreg).ToList();

                    foreach (var reg in dataObjects)
                    {
                        if (!string.IsNullOrWhiteSpace(reg.CheminDossier))
                        {
                            DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Scan"], reg.CheminDossier));
                            //DirectoryInfo directoryInfo = new DirectoryInfo(ConfigurationManager.AppSettings["CheminDossier_Scan"] + "/"+ reg.CheminDossier);                       
                            if (directoryInfo.Exists)
                            {
                                reg.NombrePageScan = directoryInfo.GetFiles().Where(f => f.Extension.ToLower() == ".tif"
                                     || f.Extension.ToLower() == ".png" || f.Extension.ToLower() == ".jpg" || f.Extension.ToLower() == ".pdf").ToList().Count();
                            }
                        }
                    }

                    return dataObjects;
                }
            }
            else
            {
                return registreViewsAgentScan;
            }
        }
    }
}
