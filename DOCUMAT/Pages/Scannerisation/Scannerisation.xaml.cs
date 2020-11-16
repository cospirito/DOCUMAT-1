using DOCUMAT.Models;
using DOCUMAT.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
using System.Configuration;
using System.Xml;

namespace DOCUMAT.Pages.Scannerisation
{
    /// <summary>
    /// Logique d'interaction pour Scannerisation.xaml
    /// </summary>
    public partial class Scannerisation : Page
    {
        Models.Agent Utilisateur;
        string DossierRacine = ConfigurationManager.AppSettings["CheminDossier_Scan"];
        string FileLogName = ConfigurationManager.AppSettings["Nom_Log_Scan"];

        #region FONCTIONS
        public void RefreshRegistre()
        {
            try
            {
                if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR || Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
                {
                    //Remplissage de la list de registre
                    RegistreView registreView = new RegistreView();
                    dgRegistre.ItemsSource = registreView.GetViewsList().Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.PREINDEXE);
                }
                else
                {
                    //Remplissage de la list de registre
                    RegistreView registreView = new RegistreView();
                    List<RegistreView> registreViews = registreView.GetViewsListByScanAgent().Where(rgv=>rgv.AgentTraitant.Login.ToLower() == Utilisateur.Login.ToLower()).ToList();
                    dgRegistre.ItemsSource = registreViews;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }


        private void ValiderScan(Models.Registre registre)
        {

        }


        #endregion

        public Scannerisation()
        {
            InitializeComponent();
        }

        public Scannerisation(Models.Agent user):this()
        {
            Utilisateur = user;
        }

        private void BtnActualise_Click(object sender, RoutedEventArgs e)
        {
            RefreshRegistre();
        }

        private void BtnRechercher_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TbRechercher.Text != "")
                {
                    RegistreView RegistreView = new RegistreView();
                    List<RegistreView> registreViews = new List<RegistreView>();
                    if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR || Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
                    {
                        //Remplissage de la list de registre
                        RegistreView registreView = new RegistreView();
                        registreViews = registreView.GetViewsList().Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.PREINDEXE).ToList();
                    }
                    else
                    {
                        //Remplissage de la list de registre
                        RegistreView registreView = new RegistreView();
                        registreViews = registreView.GetViewsListByScanAgent().Where(rgv => rgv.AgentTraitant.Login.ToLower() == Utilisateur.Login.ToLower()).ToList();
                    }

                    switch (cbChoixRecherche.SelectedIndex)
                    {
                        case 0:
                            // Récupération des registre par code registre
                            dgRegistre.ItemsSource = registreViews.Where(r => r.Registre.QrCode.ToUpper().Contains(TbRechercher.Text.ToUpper()));
                            break;
                        case 1:
                            // Récupération des registre par service
                            List<Models.Service> Services1 = RegistreView.context.Service.ToList();
                            List<Models.Livraison> Livraisons1 = RegistreView.context.Livraison.ToList();
                            List<Models.Versement> Versements1 = RegistreView.context.Versement.ToList();
                            List<RegistreView> registreViews1 = registreViews.ToList();

                            var jointure1 = from r in registreViews1
                                            join v in Versements1 on r.Registre.VersementID equals v.VersementID into table1
                                            from v in table1.ToList()
                                            join l in Livraisons1 on v.LivraisonID equals l.LivraisonID into table2
                                            from l in table2.ToList()
                                            join s in Services1 on l.ServiceID equals s.ServiceID
                                            where s.Nom.ToUpper().Contains(TbRechercher.Text.ToUpper())
                                            select r;
                            dgRegistre.ItemsSource = jointure1;
                            break;
                        case 2:
                            // Récupération des registre par service
                            List<Models.Region> Region2 = RegistreView.context.Region.ToList();
                            List<Models.Service> Services2 = RegistreView.context.Service.ToList();
                            List<Models.Livraison> Livraisons2 = RegistreView.context.Livraison.ToList();
                            List<Models.Versement> Versements2 = RegistreView.context.Versement.ToList();
                            List<RegistreView> registreViews2 = registreViews.ToList();

                            var jointure2 = from r in registreViews2
                                            join v in Versements2 on r.Registre.VersementID equals v.VersementID into table1
                                            from v in table1.ToList()
                                            join l in Livraisons2 on v.LivraisonID equals l.LivraisonID into table2
                                            from l in table2.ToList()
                                            join s in Services2 on l.ServiceID equals s.ServiceID into table3
                                            from s in table3.ToList()
                                            join rg in Region2 on s.RegionID equals rg.RegionID
                                            where rg.Nom.ToUpper().Contains(TbRechercher.Text.ToUpper())
                                            select r;
                            dgRegistre.ItemsSource = jointure2;
                            break;
                        case 3:
                            // Récupération des registre par code registre
                            dgRegistre.ItemsSource = registreViews.Where(r => r.Registre.Numero.ToUpper().Contains(TbRechercher.Text.ToUpper()));
                            break;
                        default:
                            RefreshRegistre();
                            break;
                    }
                }
                else
                {
                    RefreshRegistre();
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void dgRegistre_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            try
            {
                // Chargement de l'image du Qrcode de la ligne
                Zen.Barcode.CodeQrBarcodeDraw qrcode = Zen.Barcode.BarcodeDrawFactory.CodeQr;
                var element = e.DetailsElement.FindName("QrCode");
                RegistreView registre = (RegistreView)e.Row.Item;
                var image = qrcode.Draw(registre.Registre.QrCode, 40);
                var imageConvertie = image.ConvertDrawingImageToWPFImage(null, null);
                ((System.Windows.Controls.Image)element).Source = imageConvertie.Source;
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void dgRegistre_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void IndexerImage_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SuperviserIndexation_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TbRechercher_TextChanged(object sender, TextChangedEventArgs e)
        {            
        }

        private void dgRegistre_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            ((RegistreView)e.Row.Item).NumeroOrdre = e.Row.GetIndex() + 1;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ContextMenu cm = this.FindResource("cmRegistre") as ContextMenu;
                dgRegistre.ContextMenu = cm;
                MenuItem menuItemImprimer = (MenuItem)cm.Items.GetItemAt(3);
                if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR || Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
                {
                    menuItemImprimer.IsEnabled = true;
                }
                else
                {
                    menuItemImprimer.IsEnabled = false;
                }
                RefreshRegistre();
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void ValiderScanRegistre_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgRegistre.SelectedItems.Count == 1)
                {
                    if (MessageBox.Show("Voulez Vous vraiment Valider le Scan du Registre ?", "CONFIRMER VALIDATION", MessageBoxButton.YesNo, MessageBoxImage.Question)
                        == MessageBoxResult.Yes)
                    {
                        // Vérification des fichiers Scannés                     
                        RegistreView registreView = (RegistreView)dgRegistre.SelectedItem;

                        // Décompte des fichiers images et comparaison;
                        DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(DossierRacine, registreView.Registre.CheminDossier));
                        List<FileInfo> filesInfos = directoryInfo.GetFiles().ToList();
                        if ((registreView.Registre.NombrePage + 2) == filesInfos.Count(f => f.Extension.ToLower() == ".jpg" || f.Extension.ToLower() == ".jpeg"
                            || f.Extension.ToLower() == ".tif" || f.Extension.ToLower() == ".png"))
                        {
                            // Vérification que la Page de Garde est Présente 
                            if (filesInfos.FirstOrDefault(f => f.Name.Remove(f.Name.Length - f.Extension.Length).ToUpper() == ConfigurationManager.AppSettings["Nom_Page_Garde"].ToUpper())
                               == null)
                            {
                                MessageBox.Show("Le Dossier de Scan ne Contient pas d'image ayant le Nom : " + ConfigurationManager.AppSettings["Nom_Page_Garde"] + ", Vérifier le Dossier de Scan et importer la" +
                                    " PAGE DE GARDE si nécessaire !", "PAGE DE GARDE Introuvable", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }

                            // Vérification que la Page de Garde est Présente 
                            if (filesInfos.FirstOrDefault(f => f.Name.Remove(f.Name.Length - f.Extension.Length).ToUpper() == ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToUpper())
                               == null)
                            {
                                MessageBox.Show("Le Dossier de Scan ne Contient pas d'image ayant le Nom : " + ConfigurationManager.AppSettings["Nom_Page_Ouverture"] + ", Vérifier le Dossier de Scan et importer la" +
                                    " PAGE D'OUVERTURE si nécessaire !", "PAGE D'OUVERTURE Introuvable", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }

                            // Vérification du fichier log de scan 
                            FileInfo logFile = filesInfos.Where(f => f.Name.ToLower() == FileLogName.ToLower()).FirstOrDefault();
                            if (logFile != null)
                            {
                                #region TRAITEMENT DU FICHIER XML DE SCAN 
                                //Chargement du fichier XML 
                                XmlDocument xmlDocument = new XmlDocument();
                                xmlDocument.Load(logFile.FullName);

                                //Récupération de l'agent de scan
                                string LoginAgent = xmlDocument.GetElementsByTagName("dc:creator").Item(0).InnerText;

                                // Récupération des meta données des pages scanné
                                XmlNodeList xmlfilesList = xmlDocument.GetElementsByTagName("mets:file");

                                // Comparaison entre le Log et la BD
                                if (xmlfilesList.Count == (registreView.Registre.NombrePage + 2))
                                {
                                    // Vérification de l'agent de scan 
                                    Models.Agent agentScan = registreView.context.Agent.FirstOrDefault(a => a.Login.ToLower() == LoginAgent && a.Affectation == (int)Enumeration.AffectationAgent.SCANNE);
                                    if (agentScan != null)
                                    {
                                        // définition de l'agent de scan                                            
                                        DocumatContext.AddTraitement(DocumatContext.TbRegistre, registreView.Registre.RegistreID, agentScan.AgentID, (int)Enumeration.TypeTraitement.REGISTRE_SCANNE, "DEFINITION DE L'AGENT DE SCAN PAR L'AGENT : " + Utilisateur.Login + ", ID : " + Utilisateur.AgentID);

                                        // Changement du status du registre
                                        Models.Registre registre = registreView.context.Registre.FirstOrDefault(r => r.RegistreID == registreView.Registre.RegistreID);
                                        // Récupération et modification de l'ancien statut du registre
                                        Models.StatutRegistre AncienStatut = registreView.context.StatutRegistre.FirstOrDefault(s => s.RegistreID == registreView.Registre.RegistreID
                                                                                && s.Code == registre.StatutActuel);
                                        AncienStatut.DateFin = AncienStatut.DateModif = DateTime.Now;

                                        // Création du nouveau statut de registre
                                        Models.StatutRegistre NewStatut = new StatutRegistre();
                                        NewStatut.Code = (int)Enumeration.Registre.SCANNE;
                                        NewStatut.DateCreation = NewStatut.DateDebut = NewStatut.DateModif = DateTime.Now;
                                        NewStatut.RegistreID = registreView.Registre.RegistreID;
                                        registreView.context.StatutRegistre.Add(NewStatut);

                                        //Changement du statut du registre
                                        registre.StatutActuel = (int)Enumeration.Registre.SCANNE;
                                        registre.DateModif = DateTime.Now;
                                        registreView.context.SaveChanges();
                                        RefreshRegistre();
                                        MessageBox.Show("Registre Validé et Prêt être à être Indexé !!!", "REGISTRE VALIDE", MessageBoxButton.OK, MessageBoxImage.Information);
                                    }
                                    else
                                    {
                                        MessageBox.Show("L'agent de scan de login : " + LoginAgent + ", est introuvable, vérifier que l'agent existe dans la base de données", "Agent Introuvable", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Le Nombre d'image est incorrect, Vérifier que le scan s'est bien déroulé et que le fichier log a bien été renseigné." +
                                                    "\n Nombre Page Incorrect " +
                                                    "\n Nombre Image Log (+PGO) : " + xmlfilesList.Count +
                                                    "\n Nombre Image Registre (+PGO) : " + (registreView.Registre.NombrePage + 2), "Nombre Page Incorrect", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                                #endregion
                            }
                            else
                            {
                                MessageBox.Show("Le fichier Métadonné de Nom : " + FileLogName + " est Introuvable. Vérifier que le Fichier a bien été importé " +
                                    "dans le Dossier du Registre et qu'il porte bien le Nom : " + FileLogName, "FICHIER LOG INTROUVABLE", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Le nombre d'image compter dans le Dossier de Scan est différent du Nombre d'image attendu, " +
                                "vérifier que toutes les images ont été scannées et importées !", "NOMBRE IMAGE INCORRECTE", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void OuvrirDossierScan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgRegistre.SelectedItems.Count == 1)
                {
                    RegistreView registreView = (RegistreView)dgRegistre.SelectedItem;
                    DirectoryInfo RegistreDossier = new DirectoryInfo(Path.Combine(DossierRacine, registreView.Registre.CheminDossier));
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    var dlg = new CommonOpenFileDialog();
                    dlg.Title = "Dossier de Scan du Registre : " + registreView.Registre.QrCode;
                    dlg.IsFolderPicker = false;
                    //dlg.InitialDirectory = currentDirectory;
                    dlg.AddToMostRecentlyUsedList = false;
                    dlg.AllowNonFileSystemItems = false;
                    //dlg.DefaultDirectory = currentDirectory;
                    //dlg.FileOk += Dlg_FileOk;
                    dlg.EnsureFileExists = true;
                    dlg.EnsurePathExists = true;
                    dlg.EnsureReadOnly = false;
                    dlg.EnsureValidNames = true;
                    dlg.Multiselect = true;
                    dlg.ShowPlacesList = true;
                    dlg.InitialDirectory = RegistreDossier.FullName;

                    if (dlg.ShowDialog() == CommonFileDialogResult.Ok){}
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void ImporterImages_Click(object sender, RoutedEventArgs e)
        {
            if(dgRegistre.SelectedItems.Count == 1)
            {
                // Importation de l'image 
                try
                {
                    RegistreView registreView = (RegistreView)dgRegistre.SelectedItem;
                    OpenFileDialog open = new OpenFileDialog();
                    string extensionLog = FileLogName.Split('.')[1];
                    open.Filter = "Fichier image et fichier(." + extensionLog + ") |*.jpg;*.png;*.tif;*." + extensionLog + ";";
                    open.Multiselect = true;

                    if(open.ShowDialog() == true)
                    {
                        List<FileInfo> filesInfos = new List<FileInfo>();
                        foreach (var filePath in open.FileNames)
                        {
                            FileInfo fileInfo = new FileInfo(filePath);
                            if (File.Exists(System.IO.Path.Combine(DossierRacine, registreView.Registre.CheminDossier, fileInfo.Name)))
                            {
                                throw new Exception("Le fichier : '" + fileInfo.Name + "' existe déja dans le répertoire du registre !!!");
                            }
                            else
                            {
                                filesInfos.Add(fileInfo);
                            }
                        }

                        // Copy des fichiers dans le dossier du registre 
                        string ListeFileImport = "";
                        foreach (var fileInfo in filesInfos)
                        {
                            File.Copy(fileInfo.FullName, System.IO.Path.Combine(DossierRacine, registreView.Registre.CheminDossier, fileInfo.Name));
                            ListeFileImport = ListeFileImport + fileInfo.Name + ",";
                        }
                        ListeFileImport = ListeFileImport.Remove(ListeFileImport.Length - 1);

                        // enregistrement du traitement 
                        DocumatContext.AddTraitement(DocumatContext.TbRegistre, registreView.Registre.RegistreID, Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION, "IMPORTATION FICHIER : " + ListeFileImport);
                        RefreshRegistre();
                        MessageBox.Show(filesInfos.Count + " Fichier(s) : " + ListeFileImport + " ont/a été importée(s) avec success !!!", "IMPORTATION EFFECTUEE", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "ERREUR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ImprimerQrCode_Click(object sender, RoutedEventArgs e)
        {
            if(dgRegistre.SelectedItems.Count == 1)
            {
                if (Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR || Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
                {
                    // Affichage de l'impression du code barre
                    Impression.QrCode PageImp = new Impression.QrCode(((RegistreView)dgRegistre.SelectedItem).Registre);
                    PageImp.Show();
                }
            }
        }

        private void btnValiderAllScan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Voulez Vous Valider le Scan de Tous les Registre ?", "VALIDER TOUT", MessageBoxButton.YesNo, MessageBoxImage.Question)
                                    == MessageBoxResult.Yes)
                {
                    if (dgRegistre.Items.Count > 0)
                    {
                        string ListRegistreValide = "";
                        foreach (RegistreView registreView in dgRegistre.Items)
                        {
                            // Décompte des fichiers images et comparaison;
                            DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(DossierRacine, registreView.Registre.CheminDossier));
                            List<FileInfo> filesInfos = directoryInfo.GetFiles().ToList();
                            if ((registreView.Registre.NombrePage + 2) == filesInfos.Count(f => f.Extension.ToLower() == ".jpg" || f.Extension.ToLower() == ".jpeg"
                                || f.Extension.ToLower() == ".tif" || f.Extension.ToLower() == ".png"))
                            {
                                // Vérification que la Page de Garde est Présente 
                                if (filesInfos.FirstOrDefault(f => f.Name.Remove(f.Name.Length - f.Extension.Length).ToUpper() == ConfigurationManager.AppSettings["Nom_Page_Garde"].ToUpper())
                                   == null)
                                {
                                    MessageBox.Show("Le Dossier de Scan ne Contient pas d'image ayant le Nom : " + ConfigurationManager.AppSettings["Nom_Page_Garde"] + ", Vérifier le Dossier de Scan et importer la" +
                                        " PAGE DE GARDE si nécessaire !", "PAGE DE GARDE Introuvable", MessageBoxButton.OK, MessageBoxImage.Information);
                                    return;
                                }

                                // Vérification que la Page de Garde est Présente 
                                if (filesInfos.FirstOrDefault(f => f.Name.Remove(f.Name.Length - f.Extension.Length).ToUpper() == ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToUpper())
                                   == null)
                                {
                                    MessageBox.Show("Le Dossier de Scan ne Contient pas d'image ayant le Nom : " + ConfigurationManager.AppSettings["Nom_Page_Ouverture"] + ", Vérifier le Dossier de Scan et importer la" +
                                        " PAGE D'OUVERTURE si nécessaire !", "PAGE D'OUVERTURE Introuvable", MessageBoxButton.OK, MessageBoxImage.Information);
                                    return;
                                }

                                // Vérification du fichier log de scan 
                                FileInfo logFile = filesInfos.Where(f => f.Name.ToLower() == FileLogName.ToLower()).FirstOrDefault();
                                if (logFile != null)
                                {
                                    #region TRAITEMENT DU FICHIER XML DE SCAN 
                                    //Chargement du fichier XML 
                                    XmlDocument xmlDocument = new XmlDocument();
                                    xmlDocument.Load(logFile.FullName);

                                    //Récupération de l'agent de scan
                                    string LoginAgent = xmlDocument.GetElementsByTagName("dc:creator").Item(0).InnerText;

                                    // Récupération des meta données des pages scanné
                                    XmlNodeList xmlfilesList = xmlDocument.GetElementsByTagName("mets:file");

                                    // Comparaison entre le Log et la BD
                                    if (xmlfilesList.Count == (registreView.Registre.NombrePage + 2))
                                    {
                                        // Vérification de l'agent de scan 
                                        Models.Agent agentScan = registreView.context.Agent.FirstOrDefault(a => a.Login.ToLower() == LoginAgent && a.Affectation == (int)Enumeration.AffectationAgent.SCANNE);
                                        if (agentScan != null)
                                        {
                                            // définition de l'agent de scan                                            
                                            DocumatContext.AddTraitement(DocumatContext.TbRegistre, registreView.Registre.RegistreID, agentScan.AgentID, (int)Enumeration.TypeTraitement.REGISTRE_SCANNE, "DEFINITION DE L'AGENT DE SCAN PAR L'AGENT : " + Utilisateur.Login + ", ID : " + Utilisateur.AgentID);

                                            // Changement du status du registre
                                            Models.Registre registre = registreView.context.Registre.FirstOrDefault(r => r.RegistreID == registreView.Registre.RegistreID);
                                            // Récupération et modification de l'ancien statut du registre
                                            Models.StatutRegistre AncienStatut = registreView.context.StatutRegistre.FirstOrDefault(s => s.RegistreID == registreView.Registre.RegistreID
                                                                                    && s.Code == registre.StatutActuel);
                                            AncienStatut.DateFin = AncienStatut.DateModif = DateTime.Now;

                                            // Création du nouveau statut de registre
                                            Models.StatutRegistre NewStatut = new StatutRegistre();
                                            NewStatut.Code = (int)Enumeration.Registre.SCANNE;
                                            NewStatut.DateCreation = NewStatut.DateDebut = NewStatut.DateModif = DateTime.Now;
                                            NewStatut.RegistreID = registreView.Registre.RegistreID;
                                            registreView.context.StatutRegistre.Add(NewStatut);

                                            //Changement du statut du registre
                                            registre.StatutActuel = (int)Enumeration.Registre.SCANNE;
                                            registre.DateModif = DateTime.Now;
                                            registreView.context.SaveChanges();
                                            RefreshRegistre();
                                            ListRegistreValide += " " + registreView.Registre.Numero + ",";
                                        }
                                        else
                                        {
                                            MessageBox.Show("L'agent de scan de login : " + LoginAgent + ", est introuvable, vérifier que l'agent existe dans la base de données", "REGISTRE N° : " + registreView.Registre.Numero, MessageBoxButton.OK, MessageBoxImage.Warning);
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Le Nombre d'image est incorrect, Vérifier que le scan s'est bien déroulé et que le fichier log a bien été renseigné." +
                                                        "\n Nombre Page Incorrect " +
                                                        "\n Nombre Image Log (+PGO) : " + xmlfilesList.Count +
                                                        "\n Nombre Image Registre (+PGO) : " + (registreView.Registre.NombrePage + 2), "REGISTRE N° : " + registreView.Registre.Numero, MessageBoxButton.OK, MessageBoxImage.Warning);
                                    }
                                    #endregion
                                }
                                else
                                {
                                    MessageBox.Show("Le fichier Métadonné de Nom : " + FileLogName + " est Introuvable. Vérifier que le Fichier a bien été importé " +
                                        "dans le Dossier du Registre et qu'il porte bien le Nom : " + FileLogName, "REGISTRE N° : " + registreView.Registre.Numero, MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Le nombre d'image compter dans le Dossier de Scan est différent du Nombre d'image attendu, " +
                                    "vérifier que toutes les images ont été scannées et importées !", "REGISTRE N° : " + registreView.Registre.Numero, MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }

                        if(ListRegistreValide != "")
                        {
                            MessageBox.Show(ListRegistreValide.ToUpper(),"REGISTRES VALIDES", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }
    }
}
