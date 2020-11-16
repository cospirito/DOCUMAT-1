using DOCUMAT.Migrations;
using DOCUMAT.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace DOCUMAT.ViewModels
{
    public class RegistreView : IView<RegistreView, Registre>
    {
        private string DossierRacine = ConfigurationManager.AppSettings["CheminDossier_Scan"];
        private string FileLogName = ConfigurationManager.AppSettings["Nom_Log_Scan"];

        public int NumeroOrdre {get ; set ;}
        public DocumatContext context {get ; set; }

        public Registre Registre { get; set; }
        public Versement Versement { get; set; }
        public Agent AgentTraitant { get; set; }
        public Service ServiceVersant { get; set; }
        public  Agent ChefService { get; set; }
        public string CheminDossier { get; private set; }
        public StatutRegistre StatutActuel { get; set; }

        public int NombreImageCompte { get; set; }
        public int NombreImageScan { get; set; }
        public string StatutName { get; private set; }

        public RegistreView()
        {
            Registre = new Registre();
            context = new DocumatContext();
        }
        public void Add()
        {
           
        }

        public int AddRegistre()
        {
            //Vérification qu'un numéro de registre identique n'existe pas *
            if (context.Registre.Any(r => r.Numero == Registre.Numero && r.VersementID == Registre.VersementID))
                throw new Exception("Un registre ayant le même numéro de volume existe déja !!!");

            Registre.QrCode = "DEFAUT";
            Registre.CheminDossier = Registre.QrCode;
            context.Registre.Add(Registre);
            context.SaveChanges();

            // Procédure d'Ajout du QrCode
            //recup le code de service 
            Versement versement = context.Versement.FirstOrDefault(v => v.VersementID == Registre.VersementID );
            Livraison livraison = context.Livraison.FirstOrDefault(l => l.LivraisonID == versement.LivraisonID );
            Service service = context.Service.FirstOrDefault(s => s.ServiceID == livraison.ServiceID);

            //Composition du Code QR ou du Dossier 
            // -- Numero de Volume du registre + Le code de Service (Renseigner manuellement) + Id du registre nouvellement créer
            Registre.QrCode = Registre.Numero + service.Code + Registre.RegistreID;
            Registre.CheminDossier = Registre.QrCode;


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
                if(!Directory.Exists(DossierRacine + "/" + DossierRegistre))
                {
                    Directory.CreateDirectory(DossierRacine + "/" + DossierRegistre);
                    Registre.CheminDossier = DossierRegistre;
                }
                else
                {
                    Registre.CheminDossier = DossierRegistre;
                    MessageBox.Show("Un Dossier ayant un nom de registre conforme a été trouvé, et pris par défaut","Notification",MessageBoxButton.OK,MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Le Dossier du service est introuvable, il a peut être été supprimé, veuillez le créer à nouveau dans la partie service !!!","Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            }            

            // Création du statut
            StatutRegistre statutRegistre = new StatutRegistre();
            statutRegistre.Code = (int)Enumeration.Registre.CREE;
            statutRegistre.DateCreation = DateTime.Now;
            statutRegistre.DateDebut = DateTime.Now;
            statutRegistre.DateFin = DateTime.Now;
            statutRegistre.DateModif = DateTime.Now;
            statutRegistre.RegistreID = Registre.RegistreID;
            context.StatutRegistre.Add(statutRegistre);
            context.SaveChanges();
            return Registre.RegistreID;
        }

        public void Dispose()
        {
            context.Dispose();
        }

        public bool GetView(int Id)
        {
            Registre = context.Registre.FirstOrDefault(r => r.RegistreID == Id);

            if (Registre != null)
                return true;
            else
                throw new Exception("Ce registre est introuvable !!!");
        }

        public RegistreView GetViewRegistre(int Id)
        {
            return GetViewsList().FirstOrDefault(r => r.Registre.RegistreID == Id);
        }


        public List<RegistreView> GetViewsList()
        {
            try
            {
                List<Registre> registres = new List<Registre>();
                if (Versement != null)               
                    registres = context.Registre.Where(r=>r.VersementID == Versement.VersementID).ToList();
                else
                    registres = context.Registre.ToList();

                List<RegistreView> RegistreViews = new List<RegistreView>();

                foreach (Registre reg in registres)
                {
                    RegistreView regV = new RegistreView();
                    regV.Registre = reg;
                    
                    //Récupération du versement du registre  
                    if(Versement != null)
                    {
                    // Récupération des informations sur le service 
                        regV.Versement = Versement;
                        Livraison livraison = context.Livraison.FirstOrDefault(l => l.LivraisonID == Versement.LivraisonID);
                        regV.ServiceVersant = context.Service.FirstOrDefault(s => s.ServiceID == livraison.ServiceID);                                    
                    }
                    else
                    {
                        regV.Versement = context.Versement.FirstOrDefault(v => v.VersementID == reg.VersementID);
                        Livraison livraison = context.Livraison.FirstOrDefault(l => l.LivraisonID == regV.Versement.LivraisonID);
                        regV.ServiceVersant = context.Service.FirstOrDefault(s => s.ServiceID == livraison.ServiceID);
                    }

                    // récupération des images liées au registre dans la Base de Données
                    regV.NombreImageCompte = context.Image.Where(i => i.RegistreID == regV.Registre.RegistreID).Count();
                    
                    //Nombre de page scanné dans le Dossier du registre
                    regV.NombreImageScan = 0;

                    try
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Scan"], reg.CheminDossier));
                        //DirectoryInfo directoryInfo = new DirectoryInfo(ConfigurationManager.AppSettings["CheminDossier_Scan"] + "/"+ reg.CheminDossier);                       
                        if (directoryInfo.Exists)
                        {
                            regV.NombreImageScan = directoryInfo.GetFiles().Where(f=>f.Extension.ToLower() == ".tif" 
                                 || f.Extension.ToLower() == ".png" || f.Extension.ToLower() == ".jpg").ToList().Count();
                        }
                        else
                        {
                            MessageBox.Show("Le repertoire du Registre de type : \"" + reg.Type + " \", de code : " + reg.QrCode + " ,dont le chemin est \""
                                                + reg.CheminDossier + "\" est introuvable","AVERTISSEMENT",MessageBoxButton.OK,MessageBoxImage.Warning);
                            if (MessageBox.Show("Voulez vous suspendre les requêtes de récuparation des autres registre afin de régler le problème ?", "SUSPENDRE", MessageBoxButton.YesNo,MessageBoxImage.Question)
                                == MessageBoxResult.Yes)
                            {
                                return RegistreViews;
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message + "\n Repertoire : " + reg.CheminDossier,"AVERTISSEMENT",MessageBoxButton.OK,MessageBoxImage.Warning);
                        if (MessageBox.Show("Voulez vous suspendre les requêtes de récuparation des autres registre afin de régler le problème ?", "SUSPENDRE", MessageBoxButton.YesNo, MessageBoxImage.Question)
                            == MessageBoxResult.Yes)
                        {
                            return RegistreViews;
                        }
                    }

                    //Définition du chemin d'accès au dossier
                    regV.CheminDossier = Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Scan"], reg.CheminDossier);


                    // Affichage de statut en string
                    regV.StatutActuel = context.StatutRegistre.FirstOrDefault(s => s.Code == reg.StatutActuel && s.RegistreID == reg.RegistreID);
                    regV.StatutName = Enum.GetName(typeof(Models.Enumeration.Registre),regV.StatutActuel.Code);
                    
                    RegistreViews.Add(regV);
                }
                return RegistreViews;
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
                return new List<RegistreView>();
            }
        }

        public List<RegistreView> GetViewsListByTraite(int codeTraite,bool isTraite)
        {
            RegistreView RegistreView1 = new RegistreView();
            List<RegistreView> registreViewIsTraites = new List<RegistreView>();
            List<RegistreView> registreViewNotTraites = new List<RegistreView>();
            List<RegistreView> RegistreViews =  RegistreView1.GetViewsList().ToList();

            foreach(RegistreView registreView in RegistreViews)
            {
                // Si le registre a le traitement demandé par le codeTraite alors on le met dans registreViewIsTraites sinon 
                // il va dans les registreViewNotTraites
                Models.Traitement traitement = context.Traitement.FirstOrDefault(t => t.TableSelect == "Registres" && t.TableID == registreView.Registre.RegistreID
                   && t.TypeTraitement == codeTraite);
                if (traitement != null)
                {
                    registreView.AgentTraitant = context.Agent.FirstOrDefault(a => a.AgentID == traitement.AgentID);
                    registreViewIsTraites.Add(registreView);
                }
                else
                {
                    registreViewNotTraites.Add(registreView);
                }
            }

            if(isTraite)
            {
                return registreViewIsTraites;
            }
            else
            {
                return registreViewNotTraites;
            }
        }

        public List<RegistreView> GetViewsListByScanAgent()
        {
            RegistreView RegistreView1 = new RegistreView();
            List<RegistreView> RegistreViews = RegistreView1.GetViewsList().Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.PREINDEXE).ToList();
            List<RegistreView> regionViewsAgentScan = new List<RegistreView>();

            foreach (var regV in RegistreViews)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(DossierRacine, regV.Registre.CheminDossier));
                FileInfo logFile = directoryInfo.GetFiles().FirstOrDefault(f => f.Name.ToLower() == FileLogName.ToLower());
                if (logFile != null)
                {
                    //Chargement du fichier XML 
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(logFile.FullName);

                    //Récupération de l'agent de scan
                    string LoginAgent = xmlDocument.GetElementsByTagName("dc:creator").Item(0).InnerText;

                    Models.Agent agentScan = regV.context.Agent.FirstOrDefault(a => a.Login.ToLower() == LoginAgent && a.Affectation == (int)Enumeration.AffectationAgent.SCANNE);
                    if (agentScan != null)
                    {
                        regV.AgentTraitant = agentScan;
                        regionViewsAgentScan.Add(regV);
                    }
                }
            }

            return regionViewsAgentScan;
        }


        public bool Update(Registre UpRegistre)
        {
            if(Registre.DateDepotDebut != UpRegistre.DateDepotDebut || Registre.DateDepotFin != UpRegistre.DateDepotFin
                || Registre.NombrePage != UpRegistre.NombrePage || Registre.NumeroDepotDebut != UpRegistre.NumeroDepotDebut
                || Registre.NumeroDepotFin != UpRegistre.NumeroDepotFin || Registre.Observation != UpRegistre.Observation 
                || Registre.Numero != UpRegistre.Numero || Registre.NombrePageDeclaree != UpRegistre.NombrePageDeclaree)
            {
                if(context.Registre.Any(r => r.Numero == UpRegistre.Numero && r.VersementID == Registre.VersementID && r.RegistreID != Registre.RegistreID))
                    throw new Exception("Ce numéro de Volume est utilisé par un autre registre !!!");

                Registre.Numero = UpRegistre.Numero;
                Registre.NombrePageDeclaree = UpRegistre.NombrePageDeclaree;
                Registre.NombrePage = UpRegistre.NombrePage;
                Registre.NumeroDepotDebut = UpRegistre.NumeroDepotDebut;
                Registre.DateDepotDebut = UpRegistre.DateDepotDebut;
                Registre.NumeroDepotFin = UpRegistre.NumeroDepotFin;
                Registre.DateDepotFin = UpRegistre.DateDepotFin;
                //Registre.Type = UpRegistre.Type;
                Registre.Observation = UpRegistre.Observation;                
                Registre.DateModif = DateTime.Now;
                context.SaveChanges();
                return true;
            }
            else
            {
                throw new Exception("Aucun changement n'a été effectué");
            }
        }
    }
}
