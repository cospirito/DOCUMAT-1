using DOCUMAT.Models;
using DOCUMAT.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DOCUMAT.Pages.Image
{
    /// <summary>
    /// Logique d'interaction pour ImageViewer.xaml
    /// </summary>
    public partial class ImageViewer : Window
    {
        #region ATTRIBUTES
        Indexation.Indexation MainParent;
        Models.Registre RegistreParent;
        ImageView CurrentImageView;
        private readonly BackgroundWorker RefreshAborex = new BackgroundWorker();
        private int currentImage = 1;
        // Définition de l'aborescence
        TreeViewItem registreAbre = new TreeViewItem();
        //Fichier d'image dans disque dure
        List<FileInfo> fileInfos = new List<FileInfo>();
        // Dossier racine 
        string DossierRacine = ConfigurationManager.AppSettings["CheminDossier_Scan"];
        //Initiale des références par type de registre R3 = R / R4 = T
        string RefInitiale = "R";
        //Liste des possibles references		
        Dictionary<String, String> References = new Dictionary<String, String>();
        public bool AllisLoad = false;
        private bool editSequence = false;

        //Backround Workers
        private readonly BackgroundWorker GetAsyncIndexIndicateur = new BackgroundWorker();
        private readonly BackgroundWorker GetAsyncAgentAndRegistreInfos = new BackgroundWorker();
        private readonly BackgroundWorker LoadImageBdAsync = new BackgroundWorker();
        #endregion

        #region FONCTIONS			
        public static string GetFileFolderName(string path)
        {
            // If we have no path, return empty
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            // Make all slashes back slashes
            var normalizedPath = path.Replace('/', '\\');

            // Find the last backslash in the path
            var lastIndex = normalizedPath.LastIndexOf('\\');

            // If we don't find a backslash, return the path itself
            if (lastIndex <= 0)
            {
                return path;
            }

            // Return the name after the last back slash
            return path.Substring(lastIndex + 1);
        }

        /// <summary>
        ///  Lance la visionneuse d'image
        /// </summary>
        /// <param name="ImagePath"> Chemin de l'image à afficher </param>
        private void viewImage(string ImagePath)
        {
            FileInfo fileInfo = new FileInfo(ImagePath);
            try
            {
                if (fileInfo.Exists)
                {
                    PageImage.Source = null;
                    PageImage.Source = BitmapFrame.Create(new Uri(fileInfo.FullName), BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad);
                }
                else
                {
                    MessageBox.Show("Fichier Introuvable !!!", "Fichier Introuvable", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        /// <summary>
        ///  Permet d'afficher les informations de l'entête de la fenêtre
        /// </summary>
        private void HeaderInfosGetter()
        {
            try
            {
                if (!GetAsyncIndexIndicateur.IsBusy)
                {
                    GetAsyncIndexIndicateur.RunWorkerAsync(RegistreParent);
                }

                if (!GetAsyncAgentAndRegistreInfos.IsBusy)
                {
                    GetAsyncAgentAndRegistreInfos.RunWorkerAsync(RegistreParent);
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void setActiveAboImageIcon(int NumeroPage, string iconName)
        {
            // On rend tout les items de l'aborescence normaux et on met en gras l'élément concerné
            foreach (TreeViewItem item in registreAbre.Items)
            {
                item.FontWeight = FontWeights.Normal;
                string[] headerTitle = item.Header.ToString().Split('.');
                if (NumeroPage == -1)
                {
                    if (headerTitle[0].ToLower() == "PAGE DE GARDE".ToLower())
                    {
                        item.Tag = iconName;
                    }
                }
                else if (NumeroPage == 0)
                {
                    if (headerTitle[0].ToLower() == "PAGE D'OUVERTURE".ToLower())
                    {
                        item.Tag = iconName;
                    }
                }
                else
                {
                    int treeNumber = 0;
                    if (Int32.TryParse(headerTitle[0], out treeNumber) && treeNumber == NumeroPage && headerTitle.Count() > 1)
                    {
                        item.Tag = iconName;
                    }
                }
            }
        }

        private void setActiveAborescenceImage()
        {
            // On rend tout les items de l'aborescence normaux et on met en gras l'élément concerné
            foreach (TreeViewItem item in registreAbre.Items)
            {
                item.FontWeight = FontWeights.Normal;
                int numeroPage = 0;
                if (currentImage == -1)
                {
                    if (item.Header.ToString().Remove(item.Header.ToString().Length - 4).ToLower() == ConfigurationManager.AppSettings["Nom_Page_Garde"].ToLower())
                    {
                        item.FontWeight = FontWeights.Bold;
                    }
                }
                else if (currentImage == 0)
                {
                    if (item.Header.ToString().Remove(item.Header.ToString().Length - 4).ToLower() == ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToLower())
                    {
                        item.FontWeight = FontWeights.Bold;
                    }
                }
                else if (Int32.TryParse(item.Header.ToString().Remove(item.Header.ToString().Length - 4), out numeroPage))
                {
                    if (currentImage == numeroPage)
                    {
                        item.FontWeight = FontWeights.Bold;
                    }
                }
            }
        }

        /// <summary>
        /// Actualise l'aborescence en fonction des changements lors de l'indexation !!!
        /// </summary>
        private void ActualiserArborescence()
        {
            try
            {
                #region RECHARGEMENT DE l'ABORESCENCE
                // Chargement de l'aborescence
                // Récupération de la liste des images contenu dans le dossier du registre
                var files = Directory.GetFiles(System.IO.Path.Combine(DossierRacine, RegistreParent.CheminDossier));
                // Affichage et configuration de la TreeView
                registreAbre.Items.Clear();
                fileInfos.Clear();
                fileInfos.Clear();
                registreAbre.Header = RegistreParent.QrCode;
                registreAbre.Tag = RegistreParent.QrCode;
                registreAbre.FontWeight = FontWeights.Normal;
                registreAbre.Foreground = Brushes.White;

                // On extrait en premier la page de garde 
                foreach (var file in files)
                {
                    if (GetFileFolderName(file).Remove(GetFileFolderName(file).Length - 4).ToLower() == ConfigurationManager.AppSettings["Nom_Page_Garde"].ToLower())
                    {
                        var fileTree = new TreeViewItem();
                        fileTree.Header = GetFileFolderName(file);
                        fileTree.Tag = GetFileFolderName(file);
                        fileTree.FontWeight = FontWeights.Normal;
                        fileTree.Foreground = Brushes.White;
                        fileTree.MouseDoubleClick += FileTree_MouseDoubleClick;
                        fileInfos.Add(new FileInfo(file));
                        registreAbre.Items.Add(fileTree);
                    }
                }

                // Puis la page d'ouverture				
                foreach (var file in files)
                {
                    if (GetFileFolderName(file).Remove(GetFileFolderName(file).Length - 4).ToLower() == ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToLower())
                    {
                        var fileTree = new TreeViewItem();
                        fileTree.Header = GetFileFolderName(file);
                        fileTree.Tag = GetFileFolderName(file);
                        fileTree.FontWeight = FontWeights.Normal;
                        fileTree.Foreground = Brushes.White;
                        fileTree.MouseDoubleClick += FileTree_MouseDoubleClick;
                        fileInfos.Add(new FileInfo(file));
                        registreAbre.Items.Add(fileTree);
                    }
                }

                // Ensuite les autres pages numérotées 
                //Système de trie des images de l'aborescence !!
                Dictionary<int, string> filesInt = new Dictionary<int, string>();
                foreach (var file in files)
                {
                    FileInfo file1 = new FileInfo(file);
                    int numero = 0;
                    if (Int32.TryParse(file1.Name.Substring(0, file1.Name.Length - 4), out numero))
                    {
                        filesInt.Add(numero, file);
                    }
                }
                var fileSorted = filesInt.OrderBy(f => f.Key);

                //var fileSorted = files.OrderBy(f => f.ToLower());
                foreach (var file in fileSorted)
                {
                    if (GetFileFolderName(file.Value).Remove(GetFileFolderName(file.Value).Length - 4).ToLower() != ConfigurationManager.AppSettings["Nom_Page_Garde"].ToLower()
                        && GetFileFolderName(file.Value).Remove(GetFileFolderName(file.Value).Length - 4).ToLower() != ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToLower())
                    {
                        var fileTree = new TreeViewItem();
                        fileTree.Header = GetFileFolderName(file.Value);
                        fileTree.Tag = GetFileFolderName(file.Value);
                        fileTree.FontWeight = FontWeights.Normal;
                        fileTree.Foreground = Brushes.White;
                        fileTree.MouseDoubleClick += FileTree_MouseDoubleClick;
                        fileInfos.Add(new FileInfo(file.Value));
                        registreAbre.Items.Add(fileTree);
                    }
                }
                #endregion

                #region DEFINITION DES ICONS DE L'ABORESCENCE
                // récupération de la liste des Images
                List<Models.Image> imageViews = new List<Models.Image>();

                using (var ct = new DocumatContext())
                {
                    imageViews = ct.Database.SqlQuery<Models.Image>($"SELECT * FROM {DocumatContext.TbImage} WHERE RegistreID = {RegistreParent.RegistreID} ").ToList();
                }

                // Définition des types d'icon dans l'aborescence en fonction des statuts des images
                foreach (TreeViewItem item in registreAbre.Items)
                {
                    int NumeroAbr = 0;
                    if (Int32.TryParse(item.Header.ToString().Remove(item.Header.ToString().Length - 4), out NumeroAbr)
                        || item.Header.ToString().Remove(item.Header.ToString().Length - 4).ToUpper()
                        == ConfigurationManager.AppSettings["Nom_Page_Garde"].ToUpper()
                        || item.Header.ToString().Remove(item.Header.ToString().Length - 4).ToUpper()
                        == ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToUpper())
                    {
                        if (item.Header.ToString().Remove(item.Header.ToString().Length - 4).ToUpper()
                        == ConfigurationManager.AppSettings["Nom_Page_Garde"].ToUpper())
                        {
                            NumeroAbr = -1;
                        }

                        if (item.Header.ToString().Remove(item.Header.ToString().Length - 4).ToUpper()
                        == ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToUpper())
                        {
                            NumeroAbr = 0;
                        }

                        if (imageViews.FirstOrDefault(i => i.NumeroPage == NumeroAbr
                            && i.StatutActuel == (int)Enumeration.Image.INDEXEE) != null)
                        {
                            item.Tag = "valide";
                        }
                        else if (imageViews.FirstOrDefault(i => i.NumeroPage == NumeroAbr && i.StatutActuel == (int)Enumeration.Image.INSTANCE) != null)
                        {
                            if (imageViews.FirstOrDefault(i => i.NumeroPage == NumeroAbr
                                    && i.StatutActuel == (int)Enumeration.Image.INSTANCE
                                    && i.typeInstance.ToLower().Trim() == "image manquante") != null)
                            {
                                item.Tag = "notFound";
                            }
                            else
                            {
                                item.Tag = "instance";
                            }
                        }
                        else
                        {
                            Models.Image image = null;
                            if (imageViews.OrderBy(i => i.NumeroPage).FirstOrDefault(i => i.StatutActuel == (int)Enumeration.Image.CREEE) != null)
                            {
                                image = imageViews.OrderBy(i => i.NumeroPage).FirstOrDefault(i => i.StatutActuel == (int)Enumeration.Image.CREEE);
                            }

                            if (image != null)
                            {
                                if (image.NumeroPage == NumeroAbr)
                                {
                                    item.Tag = "edit";
                                }
                                else
                                {
                                    item.Tag = GetFileFolderName(item.Header.ToString());
                                }
                            }
                            else
                            {
                                item.Tag = GetFileFolderName(item.Header.ToString());
                            }
                        }
                    }
                }

                foreach (var image in imageViews.Where(i => i.StatutActuel == (int)Enumeration.Image.INSTANCE && i.typeInstance.ToLower().Trim() == "image manquante"))
                {
                    if (!registreAbre.Items.Cast<TreeViewItem>().Any(tr => tr.Header.ToString().ToLower() == GetFileFolderName(image.CheminImage).ToLower()))
                    {
                        TreeViewItem item = new TreeViewItem();
                        item.Header = GetFileFolderName(image.CheminImage);
                        item.Tag = "notFound";
                        item.FontWeight = FontWeights.Normal;
                        item.Foreground = Brushes.White;
                        item.MouseDoubleClick += FileTree_MouseDoubleClick;
                        registreAbre.Items.Add(item);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }
        #endregion

        public ImageViewer()
        {
            InitializeComponent();

            // Attribution du context de dgSequence
            ContextMenu Cm = (ContextMenu)FindResource("cmSequence");
            dgSequence.ContextMenu = Cm;

            // Attribution du Context Menu de l'Aborescence 
            FolderView.ContextMenu = (ContextMenu)FindResource("cmArbo");

            //Async Method
            GetAsyncAgentAndRegistreInfos.WorkerSupportsCancellation = true;
            GetAsyncAgentAndRegistreInfos.DoWork += GetAsyncAgentAndRegistreInfos_DoWork;
            GetAsyncAgentAndRegistreInfos.RunWorkerCompleted += GetAsyncAgentAndRegistreInfos_RunWorkerCompleted;
            GetAsyncAgentAndRegistreInfos.Disposed += GetAsyncAgentAndRegistreInfos_Disposed;

            GetAsyncIndexIndicateur.WorkerSupportsCancellation = true;
            GetAsyncIndexIndicateur.DoWork += GetAsyncIndexIndicateur_DoWork;
            GetAsyncIndexIndicateur.RunWorkerCompleted += GetAsyncIndexIndicateur_RunWorkerCompleted;
            GetAsyncIndexIndicateur.Disposed += GetAsyncIndexIndicateur_Disposed;
        }

        private void GetAsyncIndexIndicateur_Disposed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void GetAsyncIndexIndicateur_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                //double[] result = new double[] { perTerminer, EnCours, Instance};
                double[] result = (double[])e.Result;
                //Remplissage des éléments de la vue
                tbxImageTerminer.Text = "Terminé : " + Math.Round(result[0], 1) + " %";
                tbxImageEnCours.Text = "Non Traité : " + result[1] + " %";
                tbxImageInstance.Text = "En Instance :" + result[2] + " %";
                ArcIndicator.EndAngle = (result[0] * 360) / 100;
                TextIndicator.Text = Math.Round(result[0], 1) + "%";
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void GetAsyncIndexIndicateur_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                using (var ct = new DocumatContext())
                {
                    // Récupération des Arguments registres
                    Models.Registre registre = (Models.Registre)e.Argument;

                    //Obtention de la liste des images du registre dans la base de données 
                    List<Models.Image> images = ct.Database.SqlQuery<Models.Image>($"SELECT * FROM Images WHERE RegistreID = {registre.RegistreID}").ToList();

                    //Procédure de modification de l'indicateur de reussite
                    double perTerminer = (((float)images.Where(i => i.StatutActuel == (int)Enumeration.Image.INDEXEE).Count() / (float)images.Count()) * 100);
                    double Instance = Math.Round(((float)images.Where(i => i.StatutActuel == (int)Enumeration.Image.INSTANCE).Count() / (float)images.Count()) * 100, 1);
                    double EnCours = Math.Round(((float)images.Where(i => i.StatutActuel == (int)Enumeration.Image.CREEE).Count() / (float)images.Count()) * 100, 1);

                    // Tableau de Resultats 
                    double[] result = new double[] { perTerminer, EnCours, Instance };
                    e.Result = result; 
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void GetAsyncAgentAndRegistreInfos_Disposed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void GetAsyncAgentAndRegistreInfos_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                //object[] obs = new object[] { registre, versement, livraison, service };
                object[] obs = (object[])e.Result;
                Models.Registre registre = (Models.Registre)obs[0];
                Models.Versement versement = (Models.Versement)obs[1];
                Models.Livraison livraison = (Models.Livraison)obs[2];
                Models.Service service = (Models.Service)obs[3];

                tbxQrCode.Text = "Qrcode : " + registre.QrCode;
                tbxNumVolume.Text = "N° Volume : " + registre.Numero;
                tbxTypeRegistre.Text = "Type : " + registre.Type;
                tbxDepotFin.Text = "N° Dépôt Fin. : " + registre.NumeroDepotFin;
                tbxService.Text = "Service : " + service.NomComplet;
                tbxVersement.Text = "Versement : " + versement.NumeroVers;
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void GetAsyncAgentAndRegistreInfos_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                using (var ct = new DocumatContext())
                {
                    Models.Registre registre = (Models.Registre)e.Argument;
                    // Réactualisation des informations du Registres
                    registre = ct.Registre.FirstOrDefault(r => r.RegistreID == registre.RegistreID);
                    Models.Versement versement = ct.Versement.FirstOrDefault(v => v.VersementID == RegistreParent.VersementID);
                    Models.Livraison livraison = ct.Livraison.FirstOrDefault(l => l.LivraisonID == versement.LivraisonID);
                    Models.Service service = ct.Service.FirstOrDefault(s => s.ServiceID == livraison.ServiceID);

                    // Tableau des Resultats
                    object[] obs = new object[] { registre, versement, livraison, service };
                    e.Result = obs;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        public ImageViewer(Models.Registre registre, Indexation.Indexation indexation) : this()
        {
            MainParent = indexation;
            RegistreParent = registre;
        }

        /// <summary>
        /// Fonction chargée d'afficher l'image à Indexer
        /// </summary>
        /// <param name="currentImage"> Le numéro de l'image à récupérer et à afficher </param>
        public void ChargerImage(int currentImage)
        {
            try
            {
                ImageView imageView1 = new ImageView();
                Models.Image image = null;

                using (var ct = new DocumatContext())
                {
                    image = ct.Database.SqlQuery<Models.Image>($"SELECT TOP 1 * FROM {DocumatContext.TbImage} WHERE RegistreID = {RegistreParent.RegistreID} AND NumeroPage = {currentImage} ORDER BY ImageID ASC").FirstOrDefault(); 
                }
                               
                if (image != null)
                {
                    imageView1.Image = image;

                    #region VERIFICATION DE l'IMAGE EN COURS D'INDEXATION 
                    if (imageView1.Image.StatutActuel == (int)Enumeration.Image.CREEE)
                    {
                        using (var ct = new DocumatContext())
                        {
                            //On vérifie que lindexation de l'image est terminée 
                            int? imageEnCours = ct.Database.SqlQuery<int?>($"SELECT TOP 1 NumeroPage FROM {DocumatContext.TbImage} WHERE RegistreID = {RegistreParent.RegistreID} AND StatutActuel = {(int)Enumeration.Image.CREEE} ORDER BY NumeroPage ASC,ImageID ASC").FirstOrDefault();
                            if (imageEnCours != null)
                            {
                                if (imageView1.Image.NumeroPage != imageEnCours)
                                {
                                    return;
                                }
                            }
                            else
                            {
                                return;
                            } 
                        }
                    } 
                    #endregion

                    #region CHARGEMENT IMAGE SCANNE
                    // Chargement de la visionneuse
                    if (File.Exists(Path.Combine(DossierRacine, imageView1.Image.CheminImage)))
                    {
                        viewImage(Path.Combine(DossierRacine, imageView1.Image.CheminImage));
                    }
                    else
                    {
                        PageImage.Source = null;
                        MessageBox.Show("La page : \"" + imageView1.Image.NomPage + "\" est introuvable !!!", "AVERTISSEMENT", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    #endregion

                    #region RENSEIGNEMENT DE L'ENTETE DU FORMULAIRE D'INDEXATION
                    //On change l'image actuelle
                    CurrentImageView = imageView1;
                    this.currentImage = currentImage;

                    if (imageView1.Image.NumeroPage == -1)
                    {
                        tbxNomPage.Text = ConfigurationManager.AppSettings["Nom_Page_Garde"];
                        tbxNumeroPage.Text = "";
                        tbxDebSeq.Text = "";
                        tbxFinSeq.Text = "";
                        tbxPagePrecName.Text = "";
                        tbPageName.Text = imageView1.Image.NomPage.ToUpper();
                        tbPageName.Visibility = Visibility.Collapsed;
                    }
                    else if (imageView1.Image.NumeroPage == 0)
                    {
                        tbxNomPage.Text = ConfigurationManager.AppSettings["Nom_Page_Ouverture"];
                        tbxNumeroPage.Text = "";
                        tbxDebSeq.Text = "";
                        tbxFinSeq.Text = "";
                        tbxPagePrecName.Text = "";
                        tbPageName.Text = imageView1.Image.NomPage.ToUpper();
                        tbPageName.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        using (var ct = new DocumatContext())
                        {
                            tbxNomPage.Text = "PAGE : " + imageView1.Image.NomPage;
                            tbxNumeroPage.Text = "N° : " + imageView1.Image.NumeroPage.ToString() + " / " + ct.Database.SqlQuery<int?>($"SELECT COUNT(DISTINCT NumeroPage) FROM {DocumatContext.TbImage} WHERE RegistreID = {RegistreParent.RegistreID} AND NumeroPage NOT IN (-1,0)").FirstOrDefault();
                            tbxDebSeq.Text = ((imageView1.Image.DebutSequence == 0) ? "" : "N° Debut : " + imageView1.Image.DebutSequence.ToString());
                            tbxFinSeq.Text = ((imageView1.Image.FinSequence == 0) ? "" : imageView1.Image.FinSequence.ToString());

                            if (imageView1.Image.NumeroPage == 0)
                            {
                                tbxPagePrecName.Text = "P.PREC : ND / P.ACT : ";
                            }
                            else if (imageView1.Image.NumeroPage == -1)
                            {
                                tbxPagePrecName.Text = "P.PREC : PAGE DE GARDE / P.ACT : ";
                            }
                            else if (imageView1.Image.NumeroPage == 1)
                            {
                                tbxPagePrecName.Text = "P.PREC : PAGE D'OUVERTURE / P.ACT : ";
                            }
                            else if (imageView1.Image.NumeroPage > 1)
                            {
                                tbxPagePrecName.Text = "P.PREC : " + (imageView1.Image.NumeroPage - 1) + " / P.ACT : ";
                            }
                            else
                            {
                                tbxPagePrecName.Text = "P.PREC : ND / P.ACT : ";
                            }
                            tbPageName.Text = imageView1.Image.NomPage.ToUpper();
                            tbPageName.Visibility = Visibility.Visible; 
                        }
                    }
                    #endregion

                    #region ADAPTATION DE L'AFFICHAGE EN FONCTION DES INSTANCES DE L'IMAGE
                    // Réinitialisation de l'interface avant Adaptation
                    cbxBisOrdre.IsEnabled = true;
                    cbxDoublonOrdre.IsEnabled = true;
                    cbxSautOrdre.IsEnabled = true;
                    BtnAddSequence.IsEnabled = true;
                    BtnTerminerImage.IsEnabled = true;
                    BtnSupprimerSequence.IsEnabled = true;
                    dgSequence.IsEnabled = true;
                    PanelIndicateur.IsEnabled = true;
                    BtnCancelModifSequence.IsEnabled = false;
                    BtnSaveModifSequence.IsEnabled = false;

                    // Procédure d'affichage lorsque les statuts d'images changes
                    if (imageView1.Image.StatutActuel == (int)Enumeration.Image.INSTANCE)
                    {
                        BtnAddSequence.IsEnabled = false;
                        BtnTerminerImage.IsEnabled = false;
                        BtnSupprimerSequence.IsEnabled = false;
                        tbNumeroOrdreSequence.IsEnabled = false;
                        tbDateSequence.IsEnabled = false;
                        tbReference.IsEnabled = false;
                        cbxBisOrdre.IsEnabled = false;
                        cbxDoublonOrdre.IsEnabled = false;
                        cbxSautOrdre.IsEnabled = false;

                        TbkIndicateurImage.Text = "EN INSTANCE : " + imageView1.Image.typeInstance.ToUpper();
                        cbxMettreInstance.IsEnabled = false;
                        cbxMettreInstance.IsChecked = true;
                        panelChoixInstance.Visibility = Visibility.Collapsed;
                        tbPageName.IsEnabled = false;
                        panelIndexation.IsEnabled = false;
                    }
                    else if (imageView1.Image.StatutActuel == (int)Enumeration.Image.CREEE)
                    {
                        using (var ct = new DocumatContext())
                        {
                            //On vérifie que lindexation de l'image est terminée 
                            int? imageEnCours = ct.Database.SqlQuery<int?>($"SELECT TOP 1 NumeroPage FROM {DocumatContext.TbImage} WHERE RegistreID = {RegistreParent.RegistreID} AND StatutActuel = {(int)Enumeration.Image.CREEE} ORDER BY NumeroPage ASC,ImageID ASC").FirstOrDefault();

                            if (imageEnCours != null)
                            {
                                if (imageView1.Image.NumeroPage == imageEnCours)
                                {
                                    BtnAddSequence.IsEnabled = true;
                                    BtnSupprimerSequence.IsEnabled = true;
                                    tbNumeroOrdreSequence.IsEnabled = true;
                                    tbDateSequence.IsEnabled = true;
                                    tbReference.IsEnabled = true;
                                    cbxBisOrdre.IsEnabled = true;
                                    cbxDoublonOrdre.IsEnabled = true;
                                    cbxSautOrdre.IsEnabled = true;
                                    BtnTerminerImage.IsEnabled = false;

                                    TbkIndicateurImage.Text = "EN COURS";
                                    cbxMettreInstance.IsEnabled = true;
                                    cbxMettreInstance.IsChecked = false;
                                    panelChoixInstance.Visibility = Visibility.Collapsed;
                                    tbPageName.IsEnabled = true;
                                    panelIndexation.IsEnabled = true;
                                    BtnAddSequence.IsEnabled = true;
                                    BtnTerminerImage.IsEnabled = true;
                                    setActiveAboImageIcon((int)imageEnCours, "edit");
                                }
                            } 
                        }
                    }
                    else if (imageView1.Image.StatutActuel == (int)Enumeration.Image.INDEXEE)
                    {
                        BtnAddSequence.IsEnabled = false;
                        BtnTerminerImage.IsEnabled = false;
                        BtnSupprimerSequence.IsEnabled = false;
                        tbNumeroOrdreSequence.IsEnabled = false;
                        tbDateSequence.IsEnabled = false;
                        tbReference.IsEnabled = false;
                        cbxBisOrdre.IsEnabled = false;
                        cbxDoublonOrdre.IsEnabled = false;
                        cbxSautOrdre.IsEnabled = false;

                        TbkIndicateurImage.Text = "TERMINER";
                        cbxMettreInstance.IsEnabled = false;
                        cbxMettreInstance.IsChecked = false;
                        panelChoixInstance.Visibility = Visibility.Collapsed;
                        tbPageName.IsEnabled = false;
                        panelIndexation.IsEnabled = false;
                    }
                    #endregion

                    #region RECUPERATION DES SEQUENCES DE L'IMAGE
                    using (var ct = new DocumatContext())
                    {
                        // On réinitialise l'état des cbx saut,doublon et bis pour le rechargement
                        cbxSautOrdre.IsChecked = false;
                        cbxDoublonOrdre.IsChecked = false;
                        cbxBisOrdre.IsChecked = false;

                        // Récupération des sequences déja renseignées, Différent des images préindexer ayant la référence défaut
                        List<Sequence> sequences = ct.Database.SqlQuery<Models.Sequence>($"SELECT * FROM {DocumatContext.TbSequence} WHERE ImageID = {imageView1.Image.ImageID} ").ToList();
                        dgSequence.ItemsSource = SequenceView.GetViewsList(sequences);
                        if (sequences.Count != 0)
                        {
                            dgSequence.ScrollIntoView(dgSequence.Items.GetItemAt(dgSequence.Items.Count - 1));
                        }

                        #region RECUPERATION DE LA PAGE PECEDENTE INDEXEE
                        // Récupération de la dernière séquence et renseignement des champs d'ajout de séquence
                        //var LastSequence = imageView1.context.Sequence.Where(s => s.ImageID == imageView1.Image.ImageID && s.References.ToLower() != "defaut").OrderByDescending(i => i.NUmeroOdre).FirstOrDefault();
                        var LastSequence = sequences.Where(s => s.ImageID == imageView1.Image.ImageID && s.References.ToLower() != "defaut").OrderByDescending(i => i.NUmeroOdre).FirstOrDefault();

                        if (imageView1.Image.NumeroPage == 1)
                        {
                            tbNumeroOrdreSequence.Text = (LastSequence != null) ? (LastSequence.NUmeroOdre + 1).ToString() : imageView1.Image.DebutSequence.ToString();
                            tbDateSequence.Text = (LastSequence != null) ? LastSequence.DateSequence.ToShortDateString() : imageView1.Image.DateDebutSequence.ToShortDateString();
                            tbReference.Text = "";
                        }
                        else if (imageView1.Image.NumeroPage >= 2)
                        {
                            if (LastSequence == null)
                            {
                                Models.Image imagePreced = ct.Image.FirstOrDefault(im => im.NumeroPage == imageView1.Image.NumeroPage - 1 && im.RegistreID == RegistreParent.RegistreID);
                                Models.Sequence LastSequenceImagePrecede = null;
                                int NumeroImagePreced = imageView1.Image.NumeroPage;
                                if (imagePreced != null)
                                {
                                    NumeroImagePreced = imagePreced.NumeroPage;
                                    LastSequenceImagePrecede = ct.Sequence.Where(s => s.ImageID == imagePreced.ImageID).OrderBy(s => s.NUmeroOdre).ToList().LastOrDefault();
                                }

                                do
                                {
                                    if (imagePreced != null)
                                    {
                                        if (imagePreced.StatutActuel == (int)Enumeration.Image.INSTANCE)
                                        {
                                            tbNumeroOrdreSequence.Text = (string.IsNullOrWhiteSpace(imagePreced.ObservationInstance)) ? "ERREUR" : (Int32.Parse(imagePreced.ObservationInstance) + 1).ToString();
                                            DateTime? lastDateSeq = ct.Database.SqlQuery<DateTime?>($"SELECT TOP 1 DateSequence FROM {DocumatContext.TbSequence} S INNER JOIN {DocumatContext.TbImage} I ON S.ImageID = I.ImageID " +
                                                $"WHERE I.RegistreID = {RegistreParent.RegistreID} ORDER BY I.NumeroPage DESC,S.SequenceID DESC").FirstOrDefault();
                                            if (lastDateSeq != null)
                                            {
                                                tbDateSequence.Text = lastDateSeq.Value.ToShortDateString();
                                            }
                                            else
                                            {
                                                tbDateSequence.Text = RegistreParent.DateDepotDebut.ToShortDateString();
                                            }
                                            tbReference.Text = "";
                                            break;
                                        }
                                        else if (imagePreced.NumeroPage == 1)
                                        {
                                            LastSequenceImagePrecede = ct.Sequence.Where(s => s.ImageID == imagePreced.ImageID).OrderBy(s => s.NUmeroOdre).ToList().LastOrDefault();
                                            tbNumeroOrdreSequence.Text = (LastSequenceImagePrecede != null) ? (LastSequenceImagePrecede.NUmeroOdre + 1).ToString() : imageView1.Image.DebutSequence.ToString();
                                            tbDateSequence.Text = (LastSequenceImagePrecede != null) ? LastSequenceImagePrecede.DateSequence.ToShortDateString() : imageView1.Image.DateDebutSequence.ToShortDateString();
                                            tbReference.Text = "";
                                            break;
                                        }
                                        else
                                        {
                                            LastSequenceImagePrecede = ct.Sequence.Where(s => s.ImageID == imagePreced.ImageID).OrderBy(s => s.NUmeroOdre).ToList().LastOrDefault();
                                            if (LastSequenceImagePrecede != null)
                                            {
                                                tbNumeroOrdreSequence.Text = (LastSequenceImagePrecede.NUmeroOdre + 1).ToString();
                                                tbDateSequence.Text = LastSequenceImagePrecede.DateSequence.ToShortDateString();
                                                tbReference.Text = "";
                                                break;
                                            }
                                        }
                                    }
                                    NumeroImagePreced = NumeroImagePreced - 1;
                                    imagePreced = ct.Image.FirstOrDefault(iv => iv.NumeroPage == NumeroImagePreced && iv.RegistreID == RegistreParent.RegistreID);
                                }
                                while (LastSequenceImagePrecede == null);
                            }
                            else
                            {
                                tbNumeroOrdreSequence.Text = (LastSequence != null) ? (LastSequence.NUmeroOdre + 1).ToString() : imageView1.Image.DebutSequence.ToString();
                                tbDateSequence.Text = (LastSequence != null) ? LastSequence.DateSequence.ToShortDateString() : imageView1.Image.DateDebutSequence.ToShortDateString();
                                tbReference.Text = "";
                            }
                        } 
                    }
                    #endregion

                    // On vide le tableau des références 
                    References.Clear();
                    tbListeReferences.Visibility = Visibility.Collapsed;
                    tbxNbRefsList.Text = "Nb : 0";
                    tbxNbRefsList.Visibility = Visibility.Collapsed;

                    //On donne le focus à la date 
                    tbDateSequence.Focus();
                    #endregion

                    #region Affichage Bouton de Validation
                    using (var ct = new DocumatContext())
                    {
                        //Affichage du bouton de validadtion d'indexation si le 
                        int nbImageNonTraite = ct.Database.SqlQuery<int>($"SELECT COUNT(DISTINCT ImageID) FROM {DocumatContext.TbImage} WHERE " +
                            $"RegistreID = {RegistreParent.RegistreID} AND StatutActuel <= {(int)Enumeration.Image.INSTANCE} ").FirstOrDefault();

                        if (nbImageNonTraite == 0)
                        {
                            btnValideIndexation.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            btnValideIndexation.Visibility = Visibility.Collapsed;
                        } 
                    }
                    #endregion

                    // Actualisation du Selecteur dans l'aborescence
                    setActiveAborescenceImage();
                }
                else
                {
                    PageImage.Source = null;
                    MessageBox.Show("Impossible de charger la Page, Numero : " + currentImage + " , Elle n'est pas enregistrée !!!", "AVERTISSEMENT", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                PageImage.Source = null;
                ex.ExceptionCatcher();
            }
        }

        //Fonction de gestion des click sur les icons de l'aborescence 
        private void FileTree_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                TreeViewItem treeViewItem = (TreeViewItem)sender;
                // Récupération des images du registre
                FileInfo fileInfo = new FileInfo(Path.Combine(DossierRacine, RegistreParent.CheminDossier, treeViewItem.Header.ToString()));

                // Chargement de l'image 
                int numeroPage = 0;
                if (fileInfo.Exists)
                {
                    //Cas d'une page normal (Page numérotée)
                    if (Int32.TryParse(fileInfo.Name.Remove(fileInfo.Name.Length - fileInfo.Extension.Length), out numeroPage))
                    {
                        currentImage = numeroPage;
                        ChargerImage(currentImage);
                    }
                    else if (fileInfo.Name.Remove(fileInfo.Name.Length - fileInfo.Extension.Length).ToLower() == ConfigurationManager.AppSettings["Nom_Page_Garde"].ToLower())
                    {
                        //Affichage de la page de garde
                        currentImage = -1;

                        tbxNomPage.Text = ConfigurationManager.AppSettings["Nom_Page_Garde"];
                        tbxNumeroPage.Text = "";
                        tbNumeroOrdreSequence.Text = "";
                        tbDateSequence.Text = "";
                        tbReference.Text = "";
                        dgSequence.ItemsSource = null;
                        // Chargement de l'image		
                        ChargerImage(currentImage);
                    }
                    else if (fileInfo.Name.Remove(fileInfo.Name.Length - fileInfo.Extension.Length).ToLower() == ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToLower())
                    {
                        //Affichage de la page d'ouverture
                        currentImage = 0;

                        tbxNomPage.Text = ConfigurationManager.AppSettings["Nom_Page_Ouverture"];
                        tbxNumeroPage.Text = "";
                        tbNumeroOrdreSequence.Text = "";
                        tbDateSequence.Text = "";
                        tbReference.Text = "";
                        dgSequence.ItemsSource = null;
                        // Chargement de l'image		
                        ChargerImage(currentImage);
                    }
                }
                else
                {
                    PageImage.Source = null;
                    MessageBox.Show("L'image n'existe pas dans le dossier de scan !", "Image Introuvable", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                PageImage.Source = null;
                ex.ExceptionCatcher();
            }
        }

        private void btnAnnule_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LoadAll()
        {
            try
            {
                #region RECUPERATION ET CREATION DES PAGES SPECIAUX
                // Création des Pages Spéciaux : PAGE DE GARDE : numero : -1 ET PAGE D'OUVERTURE : numero : 0		
                // date du Server

                using (var ct = new DocumatContext())
                {
                    //PAGE DE GARDE N°-1
                    if (!ct.Image.Any(i => i.RegistreID == RegistreParent.RegistreID && i.NumeroPage == -1))
                    {
                        var file = fileInfos.FirstOrDefault(f => f.Name.Remove(f.Name.Length - 4).ToLower() == ConfigurationManager.AppSettings["Nom_Page_Garde"].ToLower());

                        //On vérifie que le fichier existe 
                        if (file == null)
                        {
                            MessageBox.Show("La PAGE DE GARDE est introuvable dans le Dossier !!!", "AVERTISSEMENT", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            // Modification Manuel de l'Image
                            ct.Database.ExecuteSqlCommand($"INSERT INTO {DocumatContext.TbImage} ([NumeroPage],[NomPage],[CheminImage],[Type],[Taille],[DateScan]" +
                            $",[StatutActuel],[RegistreID],[DebutSequence],[FinSequence],[DateDebutSequence],[DateCreation],[DateModif]) OUTPUT Inserted.ImageID " +
                            $"VALUES (-1,@NomPage,@CheminImage,@Type,@Taille,@DateScan," +
                            $"@StatutActuel,@RegistreID,0,0,GETDATE(),GETDATE(),GETDATE());"
                            , new SqlParameter[] { new SqlParameter("@NomPage", ConfigurationManager.AppSettings["Nom_Page_Garde"])
                            , new SqlParameter("@CheminImage", Path.Combine(RegistreParent.CheminDossier, file.Name))
                            , new SqlParameter("@Type", file.Extension.Substring(1).ToUpper())
                            , new SqlParameter("@Taille", file.Length)
                            , new SqlParameter("@DateScan", file.LastWriteTime)
                            , new SqlParameter("@StatutActuel", (int)Enumeration.Image.INDEXEE)
                            , new SqlParameter("@RegistreID", RegistreParent.RegistreID)});

                            int imageID = ct.Database.SqlQuery<int>($"SELECT DISTINCT ImageID FROM {DocumatContext.TbImage} WHERE RegistreID = {RegistreParent.RegistreID} AND NumeroPage = -1 ORDER BY ImageID DESC").FirstOrDefault();

                            DocumatContext.AddTraitement(DocumatContext.TbImage, imageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.CREATION, "#INDEXATION_PAGE_GARDE");
                        }
                        // Enregistrement de l'action effectué par l'agent
                    }

                    //PAGE D'OUVERTURE N°0
                    if (!ct.Image.Any(i => i.RegistreID == RegistreParent.RegistreID && i.NumeroPage == 0))
                    {
                        var file = fileInfos.FirstOrDefault(f => f.Name.Remove(f.Name.Length - 4) == ConfigurationManager.AppSettings["Nom_Page_Ouverture"]);

                        //On vérifie que le fichier existe 
                        if (file == null)
                        {
                            MessageBox.Show("La PAGE D'OUVERTURE est introuvable dans le Dossier !!!", "AVERTISSEMENT", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            // Modification Manuel de l'Image
                            ct.Database.ExecuteSqlCommand($"INSERT INTO {DocumatContext.TbImage} ([NumeroPage],[NomPage],[CheminImage],[Type],[Taille],[DateScan]" +
                            $",[StatutActuel],[RegistreID],[DebutSequence],[FinSequence],[DateDebutSequence],[DateCreation],[DateModif]) OUTPUT Inserted.ImageID " +
                            $"VALUES (0,@NomPage,@CheminImage,@Type,@Taille,@DateScan," +
                            $"@StatutActuel,@RegistreID,0,0,GETDATE(),GETDATE(),GETDATE());"
                            , new SqlParameter[] { new SqlParameter("@NomPage", ConfigurationManager.AppSettings["Nom_Page_Ouverture"])
                            , new SqlParameter("@CheminImage", Path.Combine(RegistreParent.CheminDossier, file.Name))
                            , new SqlParameter("@Type", file.Extension.Substring(1).ToUpper())
                            , new SqlParameter("@Taille", file.Length)
                            , new SqlParameter("@DateScan", file.LastWriteTime)
                            , new SqlParameter("@StatutActuel", (int)Enumeration.Image.INDEXEE)
                            , new SqlParameter("@RegistreID", RegistreParent.RegistreID)});

                            int imageID = ct.Database.SqlQuery<int>($"SELECT DISTINCT ImageID FROM {DocumatContext.TbImage} WHERE RegistreID = {RegistreParent.RegistreID} AND NumeroPage = 0 ORDER BY ImageID DESC").FirstOrDefault();

                            // Enregistrement de l'action effectué par l'agent
                            DocumatContext.AddTraitement(DocumatContext.TbImage, imageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.CREATION, "#INDEXATION_PAGE_OUVERTURE");
                        }
                    }
                }
                #endregion

                #region RECUPERATION OU CREATION DES PAGES NUMEROTEES
                // Modification des Pages Numerotées / Pré-Indexées
                // Les Pages doivent être scannées de manière à respecter la nomenclature de fichier standard
                List<Models.Image> imageViews = new List<Models.Image>();
                string ListPageIntrouvable = "";

                using (var ct = new DocumatContext())
                {
                    imageViews = ct.Database.SqlQuery<Models.Image>($"SELECT * FROM {DocumatContext.TbImage} WHERE RegistreID = {RegistreParent.RegistreID} AND NumeroPage NOT IN (-1,0)").ToList();

                    // Modification Manuel de l'Image
                    ct.Database.ExecuteSqlCommand($"UPDATE {DocumatContext.TbImage} SET  " +
                        $"DateModif = GETDATE(), StatutActuel = {(int)Enumeration.Image.CREEE} " +
                        $"WHERE RegistreID = {RegistreParent.RegistreID} AND StatutActuel = {(int)Enumeration.Image.SCANNEE}");
                } 
                
                foreach (var imageView in imageViews)
                {
                    if (imageView.StatutActuel == (int)Enumeration.Image.SCANNEE)
                    {
                        string nomFile;
                        if (imageView.NumeroPage > 9)
                        {
                            if (imageView.NumeroPage > 99)
                            {
                                nomFile = $"{imageView.NumeroPage}";
                            }
                            else
                            {
                                nomFile = $"0{imageView.NumeroPage}";
                            }
                        }
                        else
                        {
                            nomFile = $"00{imageView.NumeroPage}";
                        }

                        FileInfo file = fileInfos.FirstOrDefault(f => f.Name.Remove(f.Name.Length - 4) == nomFile);
                        if (file == null)
                        {
                            ListPageIntrouvable = ListPageIntrouvable + imageView.NumeroPage + " ,";
                        }
                    }
                }

                if (ListPageIntrouvable != "")
                {
                    //ListPageIntrouvable = ListPageIntrouvable.Remove()
                    MessageBox.Show("Impossible de retrouver La/les Page(s) de numéro : " + ListPageIntrouvable, "AVERTISSEMENT", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                #endregion

                #region VERIFICATION DE L'AGENT 				
                using (var ct = new DocumatContext())
                {
                    if (MainParent.Utilisateur.Affectation != (int)Enumeration.AffectationAgent.ADMINISTRATEUR && MainParent.Utilisateur.Affectation != (int)Enumeration.AffectationAgent.SUPERVISEUR)
                    {
                        Models.Traitement traitementAttr = ct.Traitement.FirstOrDefault(t => t.TableSelect == DocumatContext.TbRegistre && t.TableID == RegistreParent.RegistreID
                                                    && t.TypeTraitement == (int)Enumeration.TypeTraitement.REGISTRE_ATTRIBUE_INDEXATION && t.AgentID == MainParent.Utilisateur.AgentID);
                        if (traitementAttr != null)
                        {
                            Models.Traitement traitementRegistreAgent = ct.Traitement.FirstOrDefault(t => t.TableSelect == DocumatContext.TbRegistre && t.TableID == RegistreParent.RegistreID
                                                    && t.TypeTraitement == (int)Enumeration.TypeTraitement.INDEXATION_REGISTRE_DEBUT && t.AgentID == MainParent.Utilisateur.AgentID);
                            Models.Traitement traitementAutreAgent = ct.Traitement.FirstOrDefault(t => t.TableSelect == DocumatContext.TbRegistre && t.TableID == RegistreParent.RegistreID
                                                    && t.TypeTraitement == (int)Enumeration.TypeTraitement.INDEXATION_REGISTRE_DEBUT && t.AgentID != MainParent.Utilisateur.AgentID);

                            if (traitementRegistreAgent == null)
                            {
                                if (traitementAutreAgent != null)
                                {
                                    Models.Agent agent = ct.Agent.FirstOrDefault(t => t.AgentID == traitementAutreAgent.AgentID);
                                    MessageBox.Show("Ce Registre est a été entamer par l'agent : " + agent.Noms,"Registre réattribué",MessageBoxButton.OK,MessageBoxImage.Information);
                                }

                                if (MessageBox.Show("Voulez vous commencez l'indexation ?", "COMMENCER L'INDEXATION", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                {
                                    DocumatContext.AddTraitement(DocumatContext.TbRegistre, RegistreParent.RegistreID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.INDEXATION_REGISTRE_DEBUT,"DEBUT INDEXATION");
                                }
                                else
                                {
                                    this.Close();
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Ce registre ne Vous est pas attribué !!", "ERREUR", MessageBoxButton.OK, MessageBoxImage.Error);
                            this.Close();
                        }
                    }
                }
                #endregion

                #region CHARGEMENT DE LA PAGE EN COURS D'INDEXATION 
                Models.Image image = imageViews.OrderBy(i => i.NumeroPage).FirstOrDefault(i => i.StatutActuel == (int)Enumeration.Image.CREEE
                && i.RegistreID == RegistreParent.RegistreID);
                currentImage = (image != null) ? image.NumeroPage : -1;
                ChargerImage(currentImage);
                #endregion

                ActualiserArborescence();
                HeaderInfosGetter();
                setActiveAborescenceImage();
                AllisLoad = true;
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                #region RECUPERATION DES INFORMATIONS DE L'AGENT
                try
                {
                    // Chargement des information d'entête
                    HeaderInfosGetter();

                    //Remplissage des éléments de la vue
                    //Information sur l'agent 
                    if (!string.IsNullOrEmpty(MainParent.Utilisateur.CheminPhoto))
                    {
                        AgentImage.Source = new BitmapImage(new Uri(Path.Combine(DossierRacine, MainParent.Utilisateur.CheminPhoto)), new System.Net.Cache.RequestCachePolicy());
                    }
                    tbxAgentLogin.Text = "Login : " + MainParent.Utilisateur.Login;
                    tbxAgentNoms.Text = "Noms : " + MainParent.Utilisateur.Noms;
                    tbxAgentType.Text = "Aff : " + Enum.GetName(typeof(Enumeration.AffectationAgent), MainParent.Utilisateur.Affectation);
                }
                catch (Exception ex)
                {
                    ex.ExceptionCatcher();
                }
                #endregion

                #region INSPECTION DU DOSSIER DE REGISTRE ET CREATION DE L'ABORESCENCE
                // Chargement de l'aborescence
                // Récupération de la liste des images contenu dans le dossier du registre
                var files = Directory.GetFiles(Path.Combine(DossierRacine, RegistreParent.CheminDossier));
                List<FileInfo> fileInfos1 = new List<FileInfo>();

                // Récupération des fichiers dans un fileInfo
                foreach (var file in files)
                {
                    fileInfos1.Add(new FileInfo(file));
                }
                fileInfos1 = fileInfos1.Where(f => f.Extension.ToLower() == ".tif" || f.Extension.ToLower() == ".jpg" || f.Extension.ToLower() == ".png" || f.Extension.ToLower() == ".pdf").ToList();
                int NbImage = fileInfos1.Count;
                int MajNbImage = Int32.Parse(ConfigurationManager.AppSettings["Maj_Scan"]);

                // Si le nombre de Page Scannée est différent du nombre de page attendu, signal
                // Le Plus Trois est composée : de la page de garde + la page d'ouverture 
                if (!((RegistreParent.NombrePage + 2 + MajNbImage) >= NbImage && NbImage >= (RegistreParent.NombrePage + 2 - MajNbImage)))
                {
                    MessageBox.Show("Attention le nombre de Page Scannée est différents du nombre de page attendu : " +
                                    "\n Nombre pages scannées: " + fileInfos1.Count() +
                                    "\n Nombre pages attendues : " + (RegistreParent.NombrePage + 2) +
                                    "\n Contacter le Superviseur Pour régler le Problème", "AVERTISSEMENT", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }

                // Affichage et configuration de la TreeView
                registreAbre.Header = RegistreParent.QrCode;
                registreAbre.Tag = RegistreParent.QrCode;
                registreAbre.FontWeight = FontWeights.Normal;
                registreAbre.Foreground = Brushes.White;

                // Définition de la lettre de référence pour ce registre
                if (RegistreParent.Type == "R4")
                {
                    RefInitiale = "T";
                }

                // On extrait en premier la page de garde 
                foreach (var file in files)
                {
                    if (GetFileFolderName(file).Remove(GetFileFolderName(file).Length - 4).ToLower() == ConfigurationManager.AppSettings["Nom_Page_Garde"].ToLower())
                    {
                        var fileTree = new TreeViewItem();
                        fileTree.Header = GetFileFolderName(file);
                        fileTree.Tag = GetFileFolderName(file);
                        fileTree.FontWeight = FontWeights.Normal;
                        fileTree.Foreground = Brushes.White;
                        fileTree.MouseDoubleClick += FileTree_MouseDoubleClick;
                        fileInfos.Add(new FileInfo(file));
                        registreAbre.Items.Add(fileTree);
                    }
                }

                // Puis la page d'ouverture				
                foreach (var file in files)
                {
                    if (GetFileFolderName(file).Remove(GetFileFolderName(file).Length - 4).ToLower() == ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToLower())
                    {
                        var fileTree = new TreeViewItem();
                        fileTree.Header = GetFileFolderName(file);
                        fileTree.Tag = GetFileFolderName(file);
                        fileTree.FontWeight = FontWeights.Normal;
                        fileTree.Foreground = Brushes.White;
                        fileTree.MouseDoubleClick += FileTree_MouseDoubleClick;
                        fileInfos.Add(new FileInfo(file));
                        registreAbre.Items.Add(fileTree);
                    }
                }

                // Ensuite les autres pages numérotées 
                //Système de trie des images de l'aborescence !!
                Dictionary<int, string> filesInt = new Dictionary<int, string>();
                foreach (var file in files)
                {
                    FileInfo file1 = new FileInfo(file);
                    int numero = 0;
                    if (Int32.TryParse(file1.Name.Substring(0, file1.Name.Length - 4), out numero))
                    {
                        filesInt.Add(numero, file);
                    }
                }
                var fileSorted = filesInt.OrderBy(f => f.Key);

                //var fileSorted = files.OrderBy(f => f.ToLower());
                foreach (var file in fileSorted)
                {
                    if (GetFileFolderName(file.Value).Remove(GetFileFolderName(file.Value).Length - 4).ToLower() != ConfigurationManager.AppSettings["Nom_Page_Garde"].ToLower()
                        && GetFileFolderName(file.Value).Remove(GetFileFolderName(file.Value).Length - 4).ToLower() != ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToLower())
                    {
                        var fileTree = new TreeViewItem();
                        fileTree.Header = GetFileFolderName(file.Value);
                        fileTree.Tag = GetFileFolderName(file.Value);
                        fileTree.FontWeight = FontWeights.Normal;
                        fileTree.Foreground = Brushes.White;
                        fileTree.MouseDoubleClick += FileTree_MouseDoubleClick;
                        fileInfos.Add(new FileInfo(file.Value));
                        registreAbre.Items.Add(fileTree);
                    }
                }
                FolderView.Items.Add(registreAbre);
                registreAbre.Expanded += RegistreAbre_Expanded;
                #endregion
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void RegistreAbre_Expanded(object sender, RoutedEventArgs e)
        {
            if (!AllisLoad)
            {
                LoadAll();
            }
        }

        private void BtnAddSequence_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int numeroOdre = 0;
                if (Int32.TryParse(tbNumeroOrdreSequence.Text, out numeroOdre) && tbDateSequence.Text != "" && CurrentImageView.Image != null
                    && (cbxSautOrdre.IsChecked == true || References.Count > 0))
                {
                    // Définition d'une séquence spéciale
                    string isSpeciale = "";
                    if (cbxSautOrdre.IsChecked == true)
                    {
                        isSpeciale = "saut";
                    }
                    else if (cbxDoublonOrdre.IsChecked == true)
                    {
                        isSpeciale = "doublon";
                    }
                    else if (cbxBisOrdre.IsChecked == true)
                    {
                        isSpeciale = "bis";
                    }

                    Models.Sequence sequence;
                    using (var ct = new DocumatContext())
                    {
                        sequence = ct.Sequence.FirstOrDefault(s => s.ImageID == CurrentImageView.Image.ImageID && s.NUmeroOdre == numeroOdre);
                        //sequence = ct.Database.SqlQuery< Models.Sequence>($"SELECT TOP 1 SE.* FROM SEQUENCES SE INNER JOIN IMAGES IM ON SE.ImageID = IM.ImageID WHERE IM.RegistreID = {RegistreParent.RegistreID} AND SE.NUmeroOdre = {numeroOdre}").FirstOrDefault();
                        if (sequence != null)
                        {
                            if (cbxDoublonOrdre.IsChecked == true || cbxBisOrdre.IsChecked == true)
                            {
                                var localReferences = "";
                                foreach (var refer in References)
                                {
                                    if (References.Last().Key != refer.Key)
                                    {
                                        localReferences = localReferences + refer.Value + ",";
                                    }
                                    else
                                    {
                                        localReferences = localReferences + refer.Value;
                                    }
                                }

                                // Ajout d'un traitement Manuel
                                ct.Database.ExecuteSqlCommand($"INSERT INTO {DocumatContext.TbSequence} ([NUmeroOdre],[DateSequence],[References],[NombreDeReferences],[isSpeciale]," +
                                    $"[ImageID],[PhaseActuelle],[DateCreation],[DateModif]) VALUES (@numeroOdre,@dateDepot,@refs,@nbRefs," +
                                    $"@isSpeciale,@ImageID,0,GETDATE(),GETDATE())"
                                    , new SqlParameter[] { 
                                        new SqlParameter("@numeroOdre", numeroOdre),
                                        new SqlParameter("@dateDepot", DateTime.Parse(tbDateSequence.Text)),
                                        new SqlParameter("@refs", localReferences),
                                        new SqlParameter("@nbRefs", References.Count),
                                        new SqlParameter("@isSpeciale", isSpeciale),
                                        new SqlParameter("@ImageID", CurrentImageView.Image.ImageID)
                                    });
                            }
                            else
                            {
                                throw new Exception("Cette séquence existe déja sur cette image veuillez, préciser si c'est un bis ou un doublon !!");
                            }
                        }
                        else
                        {
                            var localReferences = "";
                            foreach (var refer in References)
                            {
                                if (References.Last().Key != refer.Key)
                                {
                                    localReferences = localReferences + refer.Value + ",";
                                }
                                else
                                {
                                    localReferences = localReferences + refer.Value;

                                }
                            }

                            if (cbxSautOrdre.IsChecked == true)
                            {
                                localReferences = "saut";
                            }

                            // Ajout d'un traitement Manuel
                            ct.Database.ExecuteSqlCommand($"INSERT INTO {DocumatContext.TbSequence} ([NUmeroOdre],[DateSequence],[References],[NombreDeReferences],[isSpeciale]," +
                                $"[ImageID],[PhaseActuelle],[DateCreation],[DateModif]) VALUES (@numeroOdre,@dateDepot,@refs,@nbRefs," +
                                $"@isSpeciale,@ImageID,0,GETDATE(),GETDATE())"
                                , new SqlParameter[] {
                                        new SqlParameter("@numeroOdre", numeroOdre),
                                        new SqlParameter("@dateDepot", DateTime.Parse(tbDateSequence.Text)),
                                        new SqlParameter("@refs", localReferences),
                                        new SqlParameter("@nbRefs", References.Count),
                                        new SqlParameter("@isSpeciale", isSpeciale),
                                        new SqlParameter("@ImageID", CurrentImageView.Image.ImageID)
                                });
                        }

                        #region ACTUALISATION DU TABLEAU DES SEQUENCES
                        dgSequence.ItemsSource = SequenceView.GetViewsList(ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID
                        && s.References.ToLower() != "defaut").OrderBy(s => s.NUmeroOdre).ToList());
                        if (dgSequence.Items.Count != 0)
                        {
                            dgSequence.ScrollIntoView(dgSequence.Items.GetItemAt(dgSequence.Items.Count - 1));
                        }
                        #endregion

                        #region RECUPERATION DU NUMERO D'ORDRE DE LA DERNIERE SEQUENCE INDEXE ET DEFINITION DE LA PROCHAINE SEQUENCE
                        //var LastSequence = ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID
                        //                    && s.References.ToLower() != "defaut").OrderByDescending(i => i.NUmeroOdre).FirstOrDefault();
                        var LastSequence = ct.Database.SqlQuery<Models.Sequence>($"SELECT TOP 1 * FROM SEQUENCES WHERE ImageID = {CurrentImageView.Image.ImageID} ORDER BY NumeroOdre DESC").FirstOrDefault();

                        if (isSpeciale.Contains("bis") || isSpeciale.Contains("doublon"))
                        {
                            tbNumeroOrdreSequence.Text = (LastSequence != null) ? (LastSequence.NUmeroOdre).ToString() : CurrentImageView.Image.DebutSequence.ToString();
                        }
                        else
                        {
                            tbNumeroOrdreSequence.Text = (LastSequence != null) ? (LastSequence.NUmeroOdre + 1).ToString() : CurrentImageView.Image.DebutSequence.ToString();
                        }
                        tbDateSequence.Text = (LastSequence != null) ? LastSequence.DateSequence.ToShortDateString() : CurrentImageView.Image.DateDebutSequence.ToShortDateString();

                        // On vide le tableau des références 
                        tbReference.Text = "";
                        References.Clear();
                        tbListeReferences.Visibility = Visibility.Collapsed;
                        tbxNbRefsList.Text = "Nb : 0";
                        tbxNbRefsList.Visibility = Visibility.Collapsed;

                        cbxSautOrdre.IsChecked = false;
                        cbxBisOrdre.IsChecked = false;
                        cbxDoublonOrdre.IsChecked = false;

                        //On donne le focus à la date 
                        tbDateSequence.Focus();
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void BtnSupprimerSequence_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Voulez vous supprimer la dernière séquence ?", "Confirmer la Suppression", MessageBoxButton.YesNo, MessageBoxImage.Question)
                    == MessageBoxResult.Yes)
                {
                    using (var ct = new DocumatContext())
                    {
                        List<Sequence> sequences = ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID
                                                   && s.References.ToLower() != "defaut").OrderBy(s => s.NUmeroOdre).ToList();
                        if (sequences.Count > 0)
                        {
                            // Ajout d'un traitement Manuel
                            ct.Database.ExecuteSqlCommand($"DELETE FROM {DocumatContext.TbSequence} WHERE SequenceID = {sequences.Last().SequenceID}");

                            dgSequence.ItemsSource = SequenceView.GetViewsList(ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID
                                 && s.References.ToLower() != "defaut").OrderBy(s => s.NUmeroOdre).ToList());
                            Models.Sequence LastSequence = null;
                            if (dgSequence.Items.Count > 1)
                            {
                                dgSequence.ScrollIntoView(dgSequence.Items.GetItemAt(dgSequence.Items.Count - 1));
                                LastSequence = dgSequence.ItemsSource.Cast<SequenceView>().ToList().OrderByDescending(i => i.Sequence.NUmeroOdre).FirstOrDefault().Sequence;
                            }
                            if (CurrentImageView.Image.NumeroPage == 1)
                            {
                                tbNumeroOrdreSequence.Text = (LastSequence != null) ? (LastSequence.NUmeroOdre + 1).ToString() : CurrentImageView.Image.DebutSequence.ToString();
                                tbDateSequence.Text = (LastSequence != null) ? LastSequence.DateSequence.ToShortDateString() : CurrentImageView.Image.DateDebutSequence.ToShortDateString();
                                tbReference.Text = "";
                            }
                            else if (CurrentImageView.Image.NumeroPage >= 2)
                            {
                                if (LastSequence == null)
                                {
                                    Models.Image imagePreced = ct.Image.FirstOrDefault(im => im.RegistreID == CurrentImageView.Image.RegistreID && im.NumeroPage == CurrentImageView.Image.NumeroPage - 1);
                                    var LastSequenceImagePrecede = ct.Sequence.Where(s => s.ImageID == imagePreced.ImageID).OrderBy(s => s.NUmeroOdre).ToList().LastOrDefault();

                                    do
                                    {
                                        if (imagePreced.StatutActuel == (int)Enumeration.Image.INSTANCE)
                                        {
                                            tbNumeroOrdreSequence.Text = (string.IsNullOrWhiteSpace(imagePreced.ObservationInstance)) ? "ERREUR" : (Int32.Parse(imagePreced.ObservationInstance) + 1).ToString();
                                            tbDateSequence.Text = (imagePreced.DateDebutSequence != null) ? imagePreced.DateDebutSequence.ToShortDateString() : imagePreced.DateDebutSequence.ToShortDateString();
                                            tbReference.Text = "";
                                            break;
                                        }
                                        if (imagePreced.NumeroPage == 1)
                                        {
                                            LastSequenceImagePrecede = ct.Sequence.Where(s => s.ImageID == imagePreced.ImageID).OrderBy(s => s.NUmeroOdre).ToList().LastOrDefault();
                                            tbNumeroOrdreSequence.Text = (LastSequenceImagePrecede != null) ? (LastSequenceImagePrecede.NUmeroOdre + 1).ToString() : CurrentImageView.Image.DebutSequence.ToString();
                                            tbDateSequence.Text = (LastSequenceImagePrecede != null) ? LastSequenceImagePrecede.DateSequence.ToShortDateString() : CurrentImageView.Image.DateDebutSequence.ToShortDateString();
                                            tbReference.Text = "";
                                            break;
                                        }
                                        else
                                        {
                                            LastSequenceImagePrecede = ct.Sequence.Where(s => s.ImageID == imagePreced.ImageID).OrderBy(s => s.NUmeroOdre).ToList().LastOrDefault();
                                            if (LastSequenceImagePrecede != null)
                                            {
                                                tbNumeroOrdreSequence.Text = (LastSequenceImagePrecede.NUmeroOdre + 1).ToString();
                                                tbDateSequence.Text = LastSequenceImagePrecede.DateSequence.ToShortDateString();
                                                tbReference.Text = "";
                                                break;
                                            }
                                        }
                                        imagePreced.NumeroPage--;
                                        imagePreced = ct.Image.FirstOrDefault(i => i.RegistreID == CurrentImageView.Image.RegistreID && i.NumeroPage == imagePreced.NumeroPage);
                                    }
                                    while (LastSequenceImagePrecede == null);
                                }
                                else
                                {
                                    tbNumeroOrdreSequence.Text = (LastSequence != null) ? (LastSequence.NUmeroOdre + 1).ToString() : CurrentImageView.Image.DebutSequence.ToString();
                                    tbDateSequence.Text = (LastSequence != null) ? LastSequence.DateSequence.ToShortDateString() : CurrentImageView.Image.DateDebutSequence.ToShortDateString();
                                    tbReference.Text = "";
                                }
                            }

                            // On vide le tableau des références 
                            tbReference.Text = "";
                            References.Clear();
                            tbListeReferences.Visibility = Visibility.Collapsed;

                            // récupération de la séquence préindexer devant être indexer 
                            cbxSautOrdre.IsChecked = false;
                            cbxBisOrdre.IsChecked = false;
                            cbxDoublonOrdre.IsChecked = false;
                            tbxNbRefs.Text = "Refs : ND";

                            //On donne le focus à la date 
                            tbDateSequence.Focus();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void BtnImageSuivante_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var ct = new DocumatContext())
                {
                   ChargerImage(currentImage + 1); 
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void cbxSautOrdre_Checked(object sender, RoutedEventArgs e)
        {
            cbxBisOrdre.IsChecked = false;
            cbxDoublonOrdre.IsChecked = false;
            tbReference.Focus();
        }

        private void cbxDoublonOrdre_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                cbxBisOrdre.IsChecked = false;
                cbxSautOrdre.IsChecked = false;
                BtnAddSequence.IsEnabled = true;
                using (var ct = new DocumatContext())
                {
                    var LastSequence = ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID
                                                         && s.References.ToLower() != "defaut").OrderByDescending(i => i.NUmeroOdre).FirstOrDefault();
                    if (LastSequence != null)
                    {
                        tbNumeroOrdreSequence.Text = LastSequence.NUmeroOdre.ToString();
                    }
                    else
                    {
                        #region RECHERCHER LA DERNIERE SEQUENCE INDEXE DU REGISTRE
                        Models.Sequence LastSequenceImagePrecede = null;
                        int pageAremonte = 1;
                        do
                        {
                            Models.Image imagePreced = ct.Image.FirstOrDefault(im => im.RegistreID == CurrentImageView.Image.RegistreID && im.NumeroPage == CurrentImageView.Image.NumeroPage - pageAremonte);
                            LastSequenceImagePrecede = ct.Sequence.Where(s => s.ImageID == imagePreced.ImageID).OrderBy(s => s.NUmeroOdre).ToList().LastOrDefault();

                            if (imagePreced != null)
                            {
                                if (imagePreced.StatutActuel == (int)Enumeration.Image.INSTANCE)
                                {
                                    tbNumeroOrdreSequence.Text = (string.IsNullOrWhiteSpace(imagePreced.ObservationInstance)) ? "ERREUR" : (Int32.Parse(imagePreced.ObservationInstance)).ToString();
                                    tbDateSequence.Text = (imagePreced.DateDebutSequence != null) ? imagePreced.DateDebutSequence.ToShortDateString() : imagePreced.DateDebutSequence.ToShortDateString();
                                    tbReference.Text = "";
                                    break;
                                }
                                else if (imagePreced.NumeroPage == 1)
                                {
                                    LastSequenceImagePrecede = ct.Sequence.Where(s => s.ImageID == imagePreced.ImageID).OrderBy(s => s.NUmeroOdre).ToList().LastOrDefault();
                                    tbNumeroOrdreSequence.Text = (LastSequenceImagePrecede != null) ? (LastSequenceImagePrecede.NUmeroOdre).ToString() : CurrentImageView.Image.DebutSequence.ToString();
                                    tbDateSequence.Text = (LastSequenceImagePrecede != null) ? LastSequenceImagePrecede.DateSequence.ToShortDateString() : CurrentImageView.Image.DateDebutSequence.ToShortDateString();
                                    tbReference.Text = "";
                                    break;
                                }
                                else
                                {
                                    if (LastSequenceImagePrecede != null)
                                    {
                                        tbNumeroOrdreSequence.Text = (LastSequenceImagePrecede.NUmeroOdre).ToString();
                                        tbDateSequence.Text = LastSequenceImagePrecede.DateSequence.ToShortDateString();
                                        tbReference.Text = "";
                                        break;
                                    }
                                }
                            }
                            pageAremonte++;
                        }
                        while (LastSequenceImagePrecede == null);
                        #endregion
                    }
                }
                tbReference.Focus();
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void cbxBisOrdre_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                cbxDoublonOrdre.IsChecked = false;
                cbxSautOrdre.IsChecked = false;
                BtnAddSequence.IsEnabled = true;
                using (var ct = new DocumatContext())
                {
                    var LastSequence = ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID
                                                         && s.References.ToLower() != "defaut").OrderByDescending(i => i.NUmeroOdre).FirstOrDefault();
                    if (LastSequence != null)
                    {
                        tbNumeroOrdreSequence.Text = LastSequence.NUmeroOdre.ToString();
                    }
                    else
                    {
                        #region RECHERCHER LA DERNIERE SEQUENCE INDEXE DU REGISTRE
                        Models.Sequence LastSequenceImagePrecede = null;
                        int pageAremonte = 1;
                        do
                        {
                            Models.Image imagePreced = ct.Image.FirstOrDefault(im => im.RegistreID == CurrentImageView.Image.RegistreID && im.NumeroPage == CurrentImageView.Image.NumeroPage - pageAremonte);
                            LastSequenceImagePrecede = ct.Sequence.Where(s => s.ImageID == imagePreced.ImageID).OrderBy(s => s.NUmeroOdre).ToList().LastOrDefault();

                            if (imagePreced != null)
                            {
                                if (imagePreced.StatutActuel == (int)Enumeration.Image.INSTANCE)
                                {
                                    tbNumeroOrdreSequence.Text = (string.IsNullOrWhiteSpace(imagePreced.ObservationInstance)) ? "ERREUR" : (Int32.Parse(imagePreced.ObservationInstance)).ToString();
                                    tbDateSequence.Text = (imagePreced.DateDebutSequence != null) ? imagePreced.DateDebutSequence.ToShortDateString() : imagePreced.DateDebutSequence.ToShortDateString();
                                    tbReference.Text = "";
                                    break;
                                }
                                else if (imagePreced.NumeroPage == 1)
                                {
                                    LastSequenceImagePrecede = ct.Sequence.Where(s => s.ImageID == imagePreced.ImageID).OrderBy(s => s.NUmeroOdre).ToList().LastOrDefault();
                                    tbNumeroOrdreSequence.Text = (LastSequenceImagePrecede != null) ? (LastSequenceImagePrecede.NUmeroOdre).ToString() : CurrentImageView.Image.DebutSequence.ToString();
                                    tbDateSequence.Text = (LastSequenceImagePrecede != null) ? LastSequenceImagePrecede.DateSequence.ToShortDateString() : CurrentImageView.Image.DateDebutSequence.ToShortDateString();
                                    tbReference.Text = "";
                                    break;
                                }
                                else
                                {
                                    if (LastSequenceImagePrecede != null)
                                    {
                                        tbNumeroOrdreSequence.Text = (LastSequenceImagePrecede.NUmeroOdre).ToString();
                                        tbDateSequence.Text = LastSequenceImagePrecede.DateSequence.ToShortDateString();
                                        tbReference.Text = "";
                                        break;
                                    }
                                }
                            }
                            pageAremonte++;
                        }
                        while (LastSequenceImagePrecede == null);
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void cbxBisOrdre_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var ct = new DocumatContext())
                {
                    var LastSequence = ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID
                                                            && s.References.ToLower() != "defaut").OrderByDescending(i => i.NUmeroOdre).FirstOrDefault();
                    if (LastSequence != null)
                    {
                        tbNumeroOrdreSequence.Text = (LastSequence.NUmeroOdre + 1).ToString();
                    }
                    else
                    {
                        #region RECHERCHER LA DERNIERE SEQUENCE INDEXE DU REGISTRE
                        Models.Sequence LastSequenceImagePrecede = null;
                        int pageAremonte = 1;
                        do
                        {
                            Models.Image imagePreced = ct.Image.FirstOrDefault(im => im.RegistreID == CurrentImageView.Image.RegistreID && im.NumeroPage == CurrentImageView.Image.NumeroPage - pageAremonte);
                            LastSequenceImagePrecede = ct.Sequence.Where(s => s.ImageID == imagePreced.ImageID).OrderBy(s => s.NUmeroOdre).ToList().LastOrDefault();

                            if (imagePreced != null)
                            {
                                if (imagePreced.StatutActuel == (int)Enumeration.Image.INSTANCE)
                                {
                                    tbNumeroOrdreSequence.Text = (string.IsNullOrWhiteSpace(imagePreced.ObservationInstance)) ? "ERREUR" : (Int32.Parse(imagePreced.ObservationInstance) + 1).ToString();
                                    tbDateSequence.Text = (imagePreced.DateDebutSequence != null) ? imagePreced.DateDebutSequence.ToShortDateString() : imagePreced.DateDebutSequence.ToShortDateString();
                                    tbReference.Text = "";
                                    break;
                                }
                                else if (imagePreced.NumeroPage == 1)
                                {
                                    LastSequenceImagePrecede = ct.Sequence.Where(s => s.ImageID == imagePreced.ImageID).OrderBy(s => s.NUmeroOdre).ToList().LastOrDefault();
                                    tbNumeroOrdreSequence.Text = (LastSequenceImagePrecede != null) ? (LastSequenceImagePrecede.NUmeroOdre + 1).ToString() : CurrentImageView.Image.DebutSequence.ToString();
                                    tbDateSequence.Text = (LastSequenceImagePrecede != null) ? LastSequenceImagePrecede.DateSequence.ToShortDateString() : CurrentImageView.Image.DateDebutSequence.ToShortDateString();
                                    tbReference.Text = "";
                                    break;
                                }
                                else
                                {
                                    if (LastSequenceImagePrecede != null)
                                    {
                                        tbNumeroOrdreSequence.Text = (LastSequenceImagePrecede.NUmeroOdre + 1).ToString();
                                        tbDateSequence.Text = LastSequenceImagePrecede.DateSequence.ToShortDateString();
                                        tbReference.Text = "";
                                        break;
                                    }
                                }
                            }
                            pageAremonte++;
                        }
                        while (LastSequenceImagePrecede == null);
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void cbxDoublonOrdre_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var ct = new DocumatContext())
                {
                    var LastSequence = ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID
                                                            && s.References.ToLower() != "defaut").OrderByDescending(i => i.NUmeroOdre).FirstOrDefault();
                    if (LastSequence != null)
                    {
                        tbNumeroOrdreSequence.Text = (LastSequence.NUmeroOdre + 1).ToString();
                    }
                    else
                    {
                        #region RECHERCHER LA DERNIERE SEQUENCE INDEXE DU REGISTRE
                        Models.Sequence LastSequenceImagePrecede = null;
                        int pageAremonte = 1;
                        do
                        {
                            Models.Image imagePreced = ct.Image.FirstOrDefault(im => im.RegistreID == CurrentImageView.Image.RegistreID && im.NumeroPage == CurrentImageView.Image.NumeroPage - pageAremonte);
                            LastSequenceImagePrecede = ct.Sequence.Where(s => s.ImageID == imagePreced.ImageID).OrderBy(s => s.NUmeroOdre).ToList().LastOrDefault();

                            if (imagePreced != null)
                            {
                                if (imagePreced.StatutActuel == (int)Enumeration.Image.INSTANCE)
                                {
                                    tbNumeroOrdreSequence.Text = (string.IsNullOrWhiteSpace(imagePreced.ObservationInstance)) ? "ERREUR" : (Int32.Parse(imagePreced.ObservationInstance) + 1).ToString();
                                    tbDateSequence.Text = (imagePreced.DateDebutSequence != null) ? imagePreced.DateDebutSequence.ToShortDateString() : imagePreced.DateDebutSequence.ToShortDateString();
                                    tbReference.Text = "";
                                    break;
                                }
                                else if (imagePreced.NumeroPage == 1)
                                {
                                    LastSequenceImagePrecede = ct.Sequence.Where(s => s.ImageID == imagePreced.ImageID).OrderBy(s => s.NUmeroOdre).ToList().LastOrDefault();
                                    tbNumeroOrdreSequence.Text = (LastSequenceImagePrecede != null) ? (LastSequenceImagePrecede.NUmeroOdre + 1).ToString() : CurrentImageView.Image.DebutSequence.ToString();
                                    tbDateSequence.Text = (LastSequenceImagePrecede != null) ? LastSequenceImagePrecede.DateSequence.ToShortDateString() : CurrentImageView.Image.DateDebutSequence.ToShortDateString();
                                    tbReference.Text = "";
                                    break;
                                }
                                else
                                {
                                    if (LastSequenceImagePrecede != null)
                                    {
                                        tbNumeroOrdreSequence.Text = (LastSequenceImagePrecede.NUmeroOdre + 1).ToString();
                                        tbDateSequence.Text = LastSequenceImagePrecede.DateSequence.ToShortDateString();
                                        tbReference.Text = "";
                                        break;
                                    }
                                }
                            }
                            pageAremonte++;
                        }
                        while (LastSequenceImagePrecede == null);
                        #endregion
                    }
                }
                tbReference.Focus();
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void cbxSautOrdre_Unchecked(object sender, RoutedEventArgs e)
        {
            tbReference.Focus();
        }

        private void BtnTerminerImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Si mettre En instance n'est pas Checké
                if (cbxMettreInstance.IsChecked != true)
                {
                    if (MessageBox.Show("Voulez-vous terminer l'indexation de cette image ?", "QUESTION", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        using (var ct = new DocumatContext())
                        {
                            // Récupération des séquences pour vérifier les vides 
                            List<SequenceView> sequenceViews = dgSequence.ItemsSource as List<SequenceView>;
                            if (!sequenceViews.Any(s => s.Sequence.References.Trim().ToUpper().Contains("/ERR"))
                                || !sequenceViews.Any(s => s.Sequence.References.Trim().ToUpper().Contains("00/AA")))
                            {
                                //List<Models.Image> images = ct.Image.Where(i => i.RegistreID == RegistreParent.RegistreID).ToList();
                                Models.Image image = ct.Database.SqlQuery<Models.Image>($"SELECT TOP 1 * FROM {DocumatContext.TbImage} WHERE RegistreID = {RegistreParent.RegistreID} ORDER BY NumeroPage DESC").FirstOrDefault();

                                // CAS DE LA DERNIERE PAGE DE REGISTRE A INDEXER
                                if (image.ImageID == CurrentImageView.Image.ImageID)
                                {
                                    #region CAS DE LA DERNIERE PAGE DU REGISTRE
                                    // VERIFICATION QUE LE NUMERO DE DEPOT FIN EST LE NUMERO DE LA DERNIERE SEQUENCE
                                    var LastSequence = ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID
                                                        && s.References.ToLower() != "defaut").OrderByDescending(i => i.NUmeroOdre).FirstOrDefault();

                                    if (LastSequence != null && LastSequence.NUmeroOdre == RegistreParent.NumeroDepotFin)
                                    {
                                        // Modification Manuel de l'Image
                                        ct.Database.ExecuteSqlCommand($"UPDATE {DocumatContext.TbImage} SET NomPage = @nomPage, " +
                                            $"DateModif = GETDATE(), StatutActuel = @StatutActuel WHERE ImageID = @ImageID"
                                            , new SqlParameter[] { new SqlParameter("@nomPage", tbPageName.Text.Trim().ToUpper()) 
                                            ,new SqlParameter("@StatutActuel", (int)Enumeration.Image.INDEXEE)
                                            ,new SqlParameter("@ImageID", CurrentImageView.Image.ImageID)});

                                        // Enregistrement de l'action effectuée par l'agent
                                        DocumatContext.AddTraitement(DocumatContext.TbImage, image.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION,"#IMAGE_INDEXE");

                                        // Actualisation des Images 
                                        HeaderInfosGetter();
                                        ActualiserArborescence();
                                        ChargerImage(-1); // On recharge la Page de Garde Car le Registre est Terminé
                                    }
                                    // SI LA PAGE N'A PAS DE SEQUENCE, RECUPERATION DE LA DERNIERE SEQUENCE INDEXEE SUR LE REGISTRE
                                    // ET COMPARAISON AVEC LE NUMERO DEPOT FIN
                                    else if (LastSequence == null)
                                    {
                                        // Initialisation de la Variable de drnière séquence
                                        Models.Sequence LastSequenceImagePrecede = null;
                                        Models.Image imagePreced = null;
                                        int pageAremonte = image.NumeroPage;
                                        do
                                        {
                                           imagePreced = ct.Database.SqlQuery<Models.Image>($"SELECT TOP 1 * FROM {DocumatContext.TbImage} WHERE RegistreID = {RegistreParent.RegistreID} AND NumeroPage < {pageAremonte} ORDER BY NumeroPage DESC").FirstOrDefault();                                   

                                            if (imagePreced != null)
                                            {
                                                LastSequenceImagePrecede = ct.Sequence.Where(s => s.ImageID == imagePreced.ImageID).OrderBy(s => s.NUmeroOdre).ToList().LastOrDefault();
                                            }
                                            pageAremonte = imagePreced.NumeroPage;
                                        }
                                        while (LastSequenceImagePrecede == null && imagePreced != null);

                                        // COMPARAISON AVEC LE NUMERO DEPOT FIN
                                        if (LastSequenceImagePrecede != null && LastSequenceImagePrecede.NUmeroOdre == RegistreParent.NumeroDepotFin)
                                        {
                                            // Modification Manuel de l'Image
                                            ct.Database.ExecuteSqlCommand($"UPDATE {DocumatContext.TbImage} SET NomPage = @nomPage, " +
                                                $"DateModif = GETDATE(), StatutActuel = {(int)Enumeration.Image.INDEXEE} WHERE ImageID = {CurrentImageView.Image.ImageID}", new SqlParameter("@nomPage", tbPageName.Text.Trim().ToUpper()));

                                            // Enregistrement de l'action effectuée par l'agent
                                            DocumatContext.AddTraitement(DocumatContext.TbImage,CurrentImageView.Image.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION,"#IMAGE_INDEXE");

                                            // Actualisation des Images 
                                            HeaderInfosGetter();
                                            ActualiserArborescence();
                                            ChargerImage(currentImage);
                                        }
                                        else
                                        {
                                            MessageBox.Show("Le numero d'ordre de La dernière séquence du registre doit correspondre au numéro de dépôt fin", "Erreur Indexation", MessageBoxButton.OK, MessageBoxImage.Warning);
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Le numero d'ordre de La dernière séquence doit correspondre au numéro de Dépôt fin du Registre", "Erreur Indexation", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    }
                                    #endregion
                                }
                                // CAS D'UNE PAGE ORDINAIRE DU REGISTRE A INDEXER
                                else
                                {
                                    #region CAS D'UNE PAGE LAMBDA
                                    // Modification Manuel de l'Image
                                    ct.Database.ExecuteSqlCommand($"UPDATE {DocumatContext.TbImage} SET NomPage = @nomPage, " +
                                        $"DateModif = GETDATE(), StatutActuel = {(int)Enumeration.Image.INDEXEE} WHERE ImageID = {CurrentImageView.Image.ImageID}",new SqlParameter("@nomPage",tbPageName.Text.Trim().ToUpper()));

                                    // Enregistrement de l'action effectuée par l'agent
                                    DocumatContext.AddTraitement(DocumatContext.TbImage, CurrentImageView.Image.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION,"#IMAGE_INDEXE");

                                    // Actualisation des Images 
                                    HeaderInfosGetter();
                                    #region Actualisation instantanée de l'icon de L'Abo                                
                                    setActiveAboImageIcon(CurrentImageView.Image.NumeroPage, "valide");
                                    #endregion

                                    this.BtnImageSuivante_Click(null, new RoutedEventArgs());
                                    #endregion
                                }
                            }
                            else
                            {
                                MessageBox.Show("Des références ou des indices sont erronées, veuillez mettre ce registre en instance : \"Indice ou Reference Vide\""
                                    , "Références Erronées", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                    }
                }
                else
                {
                    // Triger on btnMettreEnInstance Click
                    this.BtnMettreInstance_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void BtnMettreInstance_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Voulez vous vraiment mettre cette image en instance ?", "QUESTION", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    // Vérification du dernier numéro d'ordre
                    int LastOrderNumber = 0;
                    if (Int32.TryParse(tbObservationInstance.Text.Trim(), out LastOrderNumber))
                    {
                        // Mettre l'image en instance
                        string typeInstance = "", tagInstance = "";

                        switch (CbChoixInstance.SelectedIndex)
                        {
                            case 0:
                                typeInstance = "Image Illisible";
                                tagInstance = "IMAGE_ILLISIBLE";
                                break;
                            case 1:
                                typeInstance = "Ecriture Illisible";
                                tagInstance = "ECRITURE_ILLISIBLE";
                                break;
                            case 2:
                                typeInstance = "Mauvaise Image";
                                tagInstance = "MAUVAISE_IMAGE";
                                break;
                            case 3:
                                typeInstance = "Doublon d'Image";
                                tagInstance = "DOUBLON_IMAGE";
                                break;
                            case 4:
                                typeInstance = "Référence ou Indice Vide";
                                tagInstance = "REFERENCE_INDICE_VIDE";
                                break;
                        }

                        using (var ct = new DocumatContext())
                        {
                            // Modification Manuel de l'Image
                            ct.Database.ExecuteSqlCommand($"UPDATE {DocumatContext.TbImage} SET TypeInstance = @typeInstance, DateModif = GETDATE()" +
                                $",ObservationInstance = @Observation, StatutActuel = {(int)Enumeration.Image.INSTANCE} WHERE ImageID = {CurrentImageView.Image.ImageID}"
                                , new SqlParameter[] { new SqlParameter("@typeInstance", typeInstance),
                                    new SqlParameter("@Observation", tbObservationInstance.Text.Trim()) });

                            // Enregistrement de l'action effectuée par l'agent
                            DocumatContext.AddTraitement(DocumatContext.TbImage, CurrentImageView.Image.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION, "#IMAGE_INSTANCE POUR #" + tagInstance);

                            TbkIndicateurImage.Text = "EN INSTANCE";
                            panelChoixInstance.Visibility = Visibility.Collapsed;
                            BtnTerminerImage.IsEnabled = false;
                            cbxMettreInstance.IsEnabled = false;
                        }

                        HeaderInfosGetter();
                        ActualiserArborescence();
                        this.BtnImageSuivante_Click(null, new RoutedEventArgs());
                    }
                    else
                    {
                        MessageBox.Show("Veuillez Renseigner le dernier Numero d'Ordre svp !!!", "Dernier numero d'ordre incorrecte", MessageBoxButton.OK, MessageBoxImage.Question);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void cbxMettreInstance_Checked(object sender, RoutedEventArgs e)
        {
            panelChoixInstance.Visibility = Visibility.Visible;
        }

        private void cbxMettreInstance_Unchecked(object sender, RoutedEventArgs e)
        {
            panelChoixInstance.Visibility = Visibility.Collapsed;
        }

        private void tbDateSequence_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (cbxDateManual.IsChecked == false)
                {
                    // Définition de restriction sur la date  format : 18/03/2020
                    Regex regex = new Regex("^[0-9]{0,2}/[0-9]{2}/[0-9]{4}$");
                    TextBox textBox = ((TextBox)sender);
                    //textBox.CaretIndex = 2;
                    string text = textBox.Text;

                    if (!regex.IsMatch(text))
                    {
                        using (var ct = new DocumatContext())
                        {
                            var LastSequence = ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID
                                               && s.References.ToLower() != "defaut").OrderByDescending(i => i.NUmeroOdre).FirstOrDefault();
                            if (LastSequence != null)
                            {
                                tbDateSequence.Text = LastSequence.DateSequence.ToShortDateString();
                            }
                            else
                            {
                                tbDateSequence.Text = CurrentImageView.Image.DateDebutSequence.ToShortDateString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void tbDateSequence_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == System.Windows.Input.Key.Up && cbxDateManual.IsChecked == false)
                {
                    DateTime date;
                    if (DateTime.TryParse(tbDateSequence.Text, out date))
                    {
                        tbDateSequence.Text = date.AddDays(1).ToShortDateString();
                    }
                }
                else if (e.Key == System.Windows.Input.Key.Down && cbxDateManual.IsChecked == false)
                {
                    DateTime date;
                    if (DateTime.TryParse(tbDateSequence.Text, out date))
                    {
                        tbDateSequence.Text = date.AddDays(-1).ToShortDateString();
                    }
                }
                else if (e.Key == System.Windows.Input.Key.PageUp && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                     && cbxDateManual.IsChecked == false)
                {
                    tbDateSequence.CaretIndex = 0;
                    DateTime date;
                    if (DateTime.TryParse(tbDateSequence.Text, out date))
                    {
                        tbDateSequence.Text = date.AddYears(1).ToShortDateString();
                    }
                }
                else if (e.Key == System.Windows.Input.Key.PageDown && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                      && cbxDateManual.IsChecked == false)
                {
                    tbDateSequence.CaretIndex = 0;
                    DateTime date;
                    if (DateTime.TryParse(tbDateSequence.Text, out date))
                    {
                        tbDateSequence.Text = date.AddYears(-1).ToShortDateString();
                    }
                }
                else if (e.Key == System.Windows.Input.Key.PageUp && cbxDateManual.IsChecked == false)
                {
                    tbDateSequence.CaretIndex = 0;
                    DateTime date;
                    if (DateTime.TryParse(tbDateSequence.Text, out date))
                    {
                        tbDateSequence.Text = date.AddMonths(1).ToShortDateString();
                    }
                }
                else if (e.Key == System.Windows.Input.Key.PageDown && cbxDateManual.IsChecked == false)
                {
                    tbDateSequence.CaretIndex = 0;
                    DateTime date;
                    if (DateTime.TryParse(tbDateSequence.Text, out date))
                    {
                        tbDateSequence.Text = date.AddMonths(-1).ToShortDateString();
                    }
                }
                else if (e.Key == System.Windows.Input.Key.Right && cbxDateManual.IsChecked == false)
                {
                    tbReference.Focus();
                }
                else if (e.Key == System.Windows.Input.Key.Left && cbxDateManual.IsChecked == false)
                {
                    DateTime date;
                    if (DateTime.TryParse(tbDateSequence.Text, out date))
                    {
                        tbDateSequence.Text = date.ToShortDateString().Remove(0, 2);
                        tbDateSequence.CaretIndex = 0;
                    }
                    else
                    {
                        using (var ct = new DocumatContext())
                        {
                            var LastSequence = ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID && s.References.ToLower() != "defaut")
                                                 .OrderByDescending(i => i.NUmeroOdre).FirstOrDefault();
                            if (LastSequence != null)
                            {
                                tbDateSequence.Text = LastSequence.DateSequence.ToShortDateString();
                            }
                            else
                            {
                                tbDateSequence.Text = CurrentImageView.Image.DateDebutSequence.ToShortDateString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void tbDateSequence_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime date;
                if (!DateTime.TryParse(tbDateSequence.Text, out date))
                {
                    using (var ct = new DocumatContext())
                    {
                        var LastSequence = ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID && s.References.ToLower() != "defaut")
                                             .OrderByDescending(i => i.NUmeroOdre).FirstOrDefault();
                        if (LastSequence != null)
                        {
                            tbDateSequence.Text = LastSequence.DateSequence.ToShortDateString();
                        }
                        else
                        {
                            tbDateSequence.Text = CurrentImageView.Image.DateDebutSequence.ToShortDateString();
                        }
                    }
                    tbDateSequence.Focus();
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void tbDateSequence_GotFocus(object sender, RoutedEventArgs e)
        {
            tbDateSequence.CaretIndex = 0;
        }

        private void tbReference_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                Regex regex = new Regex("^[0-9]{1,}/");

                if (e.Key == System.Windows.Input.Key.Right)
                {
                    tbReference.CaretIndex = tbReference.Text.IndexOf('/') + 1;
                }
                else if (e.Key == System.Windows.Input.Key.Left)
                {
                    if (tbReference.Text == "" || tbReference.Text == "/")
                    {
                        tbReference.Text = "";
                        tbDateSequence.Focus();
                    }
                    else if (tbReference.CaretIndex == 0)
                    {
                        tbDateSequence.Focus();
                    }
                }
                else if (e.Key == System.Windows.Input.Key.Up)
                {
                    Regex regC = new Regex("^[0-9]{1,}/[\\w*]{1,}$");
                    Regex regCvide = new Regex("^[0-9]{1,}/$");
                    Regex regSpec = new Regex("^[0-9]{1,}/[\\w*]{1,}[_]([M][T])|([M][B])|([M])$");
                    bool isSpec = false;

                    if (regCvide.IsMatch(tbReference.Text.Trim()))
                    {
                        tbReference.Text = tbReference.Text.Trim() + "VIDE";
                    }

                    if(regSpec.IsMatch(tbReference.Text.Trim()))
                    {
                        isSpec = true;
                    }

                    if(isSpec)
                    {
                        tbListeReferences.Text = "";
                        tbListeReferences.ToolTip = "";
                        tbListeReferences.Visibility = Visibility.Visible;
                        tbxNbRefsList.Visibility = Visibility.Visible;
                        try
                        {
                            string ref1 = tbReference.Text.Trim();
                            var titreIndiceTab = ref1.Split('/');
                            var IndiceTab = titreIndiceTab[1].Split('_');
                            ref1 = titreIndiceTab[0] + IndiceTab[1] + "/" + IndiceTab[0];
                            References.Add(RefInitiale + ref1, RefInitiale + ref1);
                            tbxNbRefsList.Text = "Nb : " + References.Count;
                            tbReference.Text = "";
                        }
                        catch (Exception) { }

                        foreach (var reference in References)
                        {
                            tbListeReferences.Text += reference.Value + " ";
                            tbListeReferences.ToolTip += reference.Value + " ";
                        }
                    }
                    else if (regC.IsMatch(tbReference.Text))
                    {
                        if (!tbReference.Text.Contains("_"))
                        {
                            tbListeReferences.Text = "";
                            tbListeReferences.ToolTip = "";
                            tbListeReferences.Visibility = Visibility.Visible;
                            tbxNbRefsList.Visibility = Visibility.Visible;
                            try
                            {
                                References.Add(RefInitiale + tbReference.Text, RefInitiale + tbReference.Text);
                                tbxNbRefsList.Text = "Nb : " + References.Count;
                                tbReference.Text = "";
                            }
                            catch (Exception) { }

                            foreach (var reference in References)
                            {
                                tbListeReferences.Text += reference.Value + " ";
                                tbListeReferences.ToolTip += reference.Value + " ";
                            } 
                        }
                    }
                }
                else if (e.Key == System.Windows.Input.Key.Down)
                {
                    tbListeReferences.Text = "";
                    tbListeReferences.ToolTip = "";
                    if (References.Count != 0)
                    {
                        References.Remove(References.ElementAt(References.Count - 1).Key);
                        tbxNbRefsList.Text = "Nb : " + References.Count;
                        foreach (var reference in References)
                        {
                            tbListeReferences.Text += reference.Value + "  ";
                            tbListeReferences.ToolTip += reference.Value + " ";
                        }

                        if (References.Count == 0)
                        {
                            tbListeReferences.Visibility = Visibility.Collapsed;
                            tbxNbRefsList.Visibility = Visibility.Collapsed;
                        }
                    }
                }
                else if (e.Key == System.Windows.Input.Key.Enter)
                {
                    if (editSequence == false)
                    {
                        if (References.Count > 0 || cbxSautOrdre.IsChecked == true)
                        {
                            this.BtnAddSequence_Click(null, new RoutedEventArgs());
                        }
                    }
                    else
                    {
                        this.BtnSaveModifSequence_Click(sender, e);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void tbReference_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Regex regex = new Regex("^[0-9]{1,}/");
                if (!regex.IsMatch(tbReference.Text))
                {
                    tbReference.Text = "/";
                    tbReference.CaretIndex = 0;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void tbReference_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (tbReference.Text == "")
                {
                    tbReference.Text = "/";
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void btnValideIndexation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Voulez vous terminer l'indexation du registre ?", "QUESTION", MessageBoxButton.YesNo, MessageBoxImage.Question)
                        == MessageBoxResult.Yes)
                {
                    if (MainParent.Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR || MainParent.Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
                    {
                        if (MessageBox.Show("Attention en terminant ce Registre, Il sera pris en compte que Vous avez Indexé ce Registre, Voulez Vous Terminez ce Registre ?", "AGENT INDEXEUR", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                        {
                            return;
                        }
                    }

                    //Affichage du bouton de validadtion d'indexation si le 
                    using (var ct = new DocumatContext())
                    {
                        // Récupération de la date du serveur 
                        DateTime dateServer = ct.Database.SqlQuery<DateTime>("SELECT GETDATE()").FirstOrDefault();
                        //Récupération de toutes les images du registre 
                        ImageView imageView = new ImageView();
                        List<ImageView> imageViews = imageView.GetSimpleViewsList(RegistreParent);

                        if (imageViews.All(i => i.Image.StatutActuel == (int)Enumeration.Image.INDEXEE))
                        {
                            Models.Registre registre = ct.Registre.FirstOrDefault(r => r.RegistreID == RegistreParent.RegistreID);
                            // Récupération et modification de l'ancien statut du registre
                            Models.StatutRegistre AncienStatut = ct.StatutRegistre.FirstOrDefault(s => s.RegistreID == RegistreParent.RegistreID
                                                                 && s.Code == registre.StatutActuel);
                            AncienStatut.DateFin = AncienStatut.DateModif = dateServer;

                            // Création du nouveau statut de registre
                            Models.StatutRegistre NewStatut = new StatutRegistre();
                            NewStatut.Code = (int)Enumeration.Registre.INDEXE;
                            NewStatut.DateCreation = NewStatut.DateDebut = NewStatut.DateModif = dateServer;
                            NewStatut.RegistreID = RegistreParent.RegistreID;
                            ct.StatutRegistre.Add(NewStatut);

                            //Changement du statut du registre
                            registre.StatutActuel = (int)Enumeration.Registre.INDEXE;
                            ct.SaveChanges();

                            //Enregistrement du traitement fait au registre 						
                            DocumatContext.AddTraitement(DocumatContext.TbRegistre, RegistreParent.RegistreID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.INDEXATION_REGISTRE_TERMINE,"INDEXATION REGISTRE TERMINE");

                            MessageBox.Show("Indexation du Registre Terminé ", "Indexation Terminé", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Operation to remove the current registres from the registre List
                            MainParent.ListRegistreActuel.Remove(MainParent.ListRegistreActuel.FirstOrDefault(r => r.RegistreID == RegistreParent.RegistreID));
                            MainParent.dgRegistre.Items.Refresh();
                            this.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainParent.IsEnabled = true;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Mettre à Jour la taille de la visionneuse
            gripDgSequence.MaxHeight = dgSequence.MaxHeight = BorderControle.ActualHeight - (FormIndexPart4.MinHeight + FormIndexPart3.MaxHeight + FormIndexPart2.MaxHeight + FormIndexPart1.MaxHeight);
        }

        private void BtnGenererAutoRefs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Génération automatique des references sequencées
                Regex regex = new Regex("^[0-9]{1,}/[\\w*-]{1,}$");
                if (regex.IsMatch(tbReference.Text))
                {
                    //Récupération du nombre d'index demande
                    string[] refEtNbRef = tbReference.Text.Split('-');
                    int nbRefs = 0;
                    if (refEtNbRef.Length == 2 && Int32.TryParse(refEtNbRef[1].Trim(), out nbRefs))
                    {
                        if (nbRefs > 1 && nbRefs < 10000)
                        {
                            string refVariantIndice = refEtNbRef[0].Split('/')[0];
                            string refRacine = string.IsNullOrWhiteSpace(refEtNbRef[0].Split('/')[1]) ? "VIDE" : refEtNbRef[0].Split('/')[1];
                            string[] AllRefVariant = new string[nbRefs + 1];
                            int refVariantInt = Int32.Parse(refVariantIndice);

                            for (int i = 0; i <= nbRefs; i++)
                            {
                                AllRefVariant[i] = (refVariantInt + i) + "/" + refRacine;
                                try
                                {
                                    References.Add(RefInitiale + AllRefVariant[i], RefInitiale + AllRefVariant[i]);
                                }
                                catch (Exception) { }
                            }

                            tbListeReferences.Text = "";
                            tbListeReferences.ToolTip = "";
                            tbListeReferences.Visibility = Visibility.Visible;
                            tbxNbRefsList.Visibility = Visibility.Visible;
                            tbxNbRefsList.Text = "Nb : " + References.Count;
                            tbReference.Text = "";

                            foreach (var reference in References)
                            {
                                tbListeReferences.Text += reference.Value + " ";
                                tbListeReferences.ToolTip += reference.Value + " ";
                            }
                        }
                        else
                        {
                            MessageBox.Show("Spécifier le nombre de references doit être 1 et 10000", "Reference Mal spécifiée", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Spécifier le nombre de references exemple(s) : 1/27-2, 1/27-3", "Reference Mal spécifiée", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else if ((string.IsNullOrWhiteSpace(tbReference.Text.Trim()) || tbReference.Text.Trim() == "/") && References.Count > 0)
                {
                    if (MessageBox.Show("Voulez vous videz les références le tableau des références ??", "Vider les Références", MessageBoxButton.YesNo, MessageBoxImage.Question)
                        == MessageBoxResult.Yes)
                    {
                        tbListeReferences.Text = "";
                        tbListeReferences.ToolTip = "";
                        tbListeReferences.Visibility = Visibility.Collapsed;
                        tbxNbRefsList.Visibility = Visibility.Collapsed;
                        References.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void RefrechAll_Click(object sender, RoutedEventArgs e)
        {
            LoadAll();
        }

        private void TermineAnnule_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (TreeViewItem treeViewItem in registreAbre.Items)
                {
                    if (treeViewItem.IsSelected)
                    {
                        if (MessageBox.Show("Voulez vous changer le statut de l'image : " + treeViewItem.Header.ToString() + " ?", "Confirmer ?", MessageBoxButton.YesNo, MessageBoxImage.Question)
                            == MessageBoxResult.Yes)
                        {
                            string FilePath = System.IO.Path.Combine(DossierRacine, RegistreParent.CheminDossier, treeViewItem.Header.ToString());
                            FileInfo fileInfo = new FileInfo(FilePath);
                            int NumeroPage = 0;
                            if (fileInfo.Name.Remove(fileInfo.Name.Length - fileInfo.Extension.Length).ToUpper() == ConfigurationManager.AppSettings["Nom_Page_Garde"].ToUpper())
                            {
                                NumeroPage = -1;
                            }
                            else if (fileInfo.Name.Remove(fileInfo.Name.Length - fileInfo.Extension.Length).ToUpper() == ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToUpper())
                            {
                                NumeroPage = 0;
                            }
                            else if (Int32.TryParse(fileInfo.Name.Remove(fileInfo.Name.Length - fileInfo.Extension.Length), out NumeroPage))
                            {

                            }
                            else
                            {
                                throw new Exception("Impossible de Retrouver la Page demandée !!!");
                            }

                            using (var ct = new DocumatContext())
                            {
                                Models.Image image = ct.Image.FirstOrDefault(i => i.RegistreID == RegistreParent.RegistreID && i.NumeroPage == NumeroPage);
                                if (image != null)
                                {
                                    if (image.StatutActuel != (int)Enumeration.Image.INSTANCE)
                                    {
                                        if (image.StatutActuel == (int)Enumeration.Image.INDEXEE)
                                        {
                                            image.StatutActuel = (int)Enumeration.Image.CREEE;
                                            ct.SaveChanges();

                                            // Enregistrement du traitement
                                            DocumatContext.AddTraitement(DocumatContext.TbImage, image.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION,"#IMAGE_INDEXE_ANNULE");                                            
                                            HeaderInfosGetter();
                                            ChargerImage(NumeroPage);
                                            ActualiserArborescence();

                                            MessageBox.Show("L'indexation de l'image a été annulée", "Indexation Image Annulée", MessageBoxButton.OK, MessageBoxImage.Information);
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Image en Instance, Vous Pouvez qu'annuler les Images Indexées !!!", "Image en Instance", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Veuillez Enregistrer l'Image avant d'effectuer cette opération !!!", "Image Non Enregistrée", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void ModifSequence_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgSequence.SelectedItems.Count == 1)
                {
                    if (cbxBisOrdre.IsChecked == true)
                    {
                        cbxBisOrdre.IsChecked = false;
                    }

                    if (cbxDoublonOrdre.IsChecked == true)
                    {
                        cbxDoublonOrdre.IsChecked = false;
                    }

                    SequenceView sequenceView = (SequenceView)dgSequence.SelectedItem;
                    tbNumeroOrdreSequence.Text = sequenceView.Sequence.NUmeroOdre.ToString();
                    tbDateSequence.Text = sequenceView.Sequence.DateSequence.ToShortDateString().Trim();

                    dgSequence.IsEnabled = false;
                    BtnTerminerImage.IsEnabled = false;
                    BtnSupprimerSequence.IsEnabled = false;
                    BtnAddSequence.IsEnabled = false;
                    PanelIndicateur.IsEnabled = false;
                    cbxBisOrdre.IsEnabled = false;
                    cbxDoublonOrdre.IsEnabled = false;
                    cbxSautOrdre.IsEnabled = false;
                    BtnCancelModifSequence.IsEnabled = true;
                    BtnSaveModifSequence.IsEnabled = true;

                    tbListeReferences.Text = "";
                    tbListeReferences.ToolTip = "";
                    tbxNbRefsList.Text = "Nb : 0";
                    tbxNbRefs.Text = "Nb : ND";
                    References.Clear();

                    foreach (var refer in sequenceView.strReferences.Split(','))
                    {
                        if (!string.IsNullOrWhiteSpace(refer) && refer.Trim().ToLower() != "saut")
                        {
                            tbListeReferences.Text += refer + " ";
                            tbListeReferences.ToolTip += refer + " ";
                            References.Add(refer, refer);
                        }
                    }

                    if (References.Count > 0)
                    {
                        tbxNbRefsList.Text = "Nb : " + References.Count;
                        tbxNbRefs.Text = "Nb : " + References.Count;
                        tbListeReferences.Visibility = Visibility.Visible;
                        tbxNbRefsList.Visibility = Visibility.Visible;
                        tbReference.Text = "";
                    }
                    editSequence = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void BtnSaveModifSequence_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgSequence.SelectedItems.Count == 1)
                {
                    SequenceView sequenceView = (SequenceView)dgSequence.SelectedItem;
                    if (References.Count > 0)
                    {
                        if (MessageBox.Show("Voulez-vous vraiment enregistrer les données ?", "CONFIRMER", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            using (var ct = new DocumatContext())
                            {
                                var LocalReferences = "";
                                foreach (var refer in References)
                                {
                                    if (References.Last().Key != refer.Key)
                                    {
                                        LocalReferences = LocalReferences + refer.Value + ",";
                                    }
                                    else
                                    {
                                        LocalReferences = LocalReferences + refer.Value;
                                    }
                                }

                                // on fait Sauter le saut, au cas où des références sont définie
                                var noSaut = "";
                                if (sequenceView.Sequence.isSpeciale.Trim().ToLower() == "saut")
                                {
                                    noSaut = "saut";
                                }

                                if (string.IsNullOrWhiteSpace(noSaut))
                                {
                                    // Ajout d'une Séquence Manuelle sans saut
                                    ct.Database.ExecuteSqlCommand($"UPDATE {DocumatContext.TbSequence} SET DateSequence = @dateDepot" +
                                        $",DateModif = GETDATE(),[References] = @refs WHERE SequenceID = {sequenceView.Sequence.SequenceID}"
                                        , new SqlParameter[]{new SqlParameter("@dateDepot", DateTime.Parse(tbDateSequence.Text)),new SqlParameter("@refs", LocalReferences)});
                                }
                                else
                                {
                                    // Ajout d'une Séquence Manuelle avec saut
                                    ct.Database.ExecuteSqlCommand($"UPDATE {DocumatContext.TbSequence} SET DateSequence = @dateDepot" +
                                        $",DateModif = GETDATE(),[References] = @refs , IsSpecial = 'saut' WHERE SequenceID = {sequenceView.Sequence.SequenceID}"
                                        , new SqlParameter[] { new SqlParameter("@dateDepot", DateTime.Parse(tbDateSequence.Text)), new SqlParameter("@refs", LocalReferences)});
                                }

                                // Refresh des données
                                List<Sequence> sequences = ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID).OrderBy(s => s.NUmeroOdre).ToList();
                                dgSequence.ItemsSource = SequenceView.GetViewsList(sequences);
                                if (sequences.Count != 0)
                                {
                                    dgSequence.ScrollIntoView(dgSequence.Items.GetItemAt(dgSequence.Items.Count - 1));
                                }

                                tbNumeroOrdreSequence.Text = "";
                                tbDateSequence.Text = "";
                                tbReference.Text = "";
                                dgSequence.IsEnabled = true;

                                dgSequence.IsEnabled = true;
                                BtnTerminerImage.IsEnabled = true;
                                BtnSupprimerSequence.IsEnabled = true;
                                BtnAddSequence.IsEnabled = true;
                                PanelIndicateur.IsEnabled = true;
                                cbxBisOrdre.IsEnabled = true;
                                cbxDoublonOrdre.IsEnabled = true;
                                cbxSautOrdre.IsEnabled = true;
                                BtnCancelModifSequence.IsEnabled = false;
                                BtnSaveModifSequence.IsEnabled = false;

                                tbListeReferences.Visibility = Visibility.Collapsed;
                                tbxNbRefsList.Visibility = Visibility.Collapsed;
                                tbListeReferences.Text = "";
                                tbListeReferences.ToolTip = "";
                                tbxNbRefsList.Text = "Nb : 0";
                                tbxNbRefs.Text = "Nb : ND";
                                References.Clear();
                                editSequence = false;

                                #region RECUPERATION DU NUMERO D'ORDRE DE LA DERNIERE SEQUENCE INDEXE ET DEFINITION DE LA PROCHAINE SEQUENCE
                                var LastSequence = ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID
                                                            && s.References.ToLower() != "defaut").OrderByDescending(i => i.NUmeroOdre).FirstOrDefault();
                                tbNumeroOrdreSequence.Text = (LastSequence != null) ? (LastSequence.NUmeroOdre + 1).ToString() : CurrentImageView.Image.DebutSequence.ToString();
                                tbDateSequence.Text = (LastSequence != null) ? LastSequence.DateSequence.ToShortDateString() : CurrentImageView.Image.DateDebutSequence.ToShortDateString();
                                //On donne le focus à la date 
                                tbDateSequence.Focus();
                                #endregion

                                MessageBox.Show("La séquence : " + sequenceView.Sequence.NUmeroOdre + ", a bien été modifié !", "Modification effectuée", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void BtnCancelModifSequence_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SequenceView sequenceView = (SequenceView)dgSequence.SelectedItem;
                tbNumeroOrdreSequence.Text = sequenceView.Sequence.NUmeroOdre.ToString();
                tbDateSequence.Text = sequenceView.Sequence.DateSequence.ToShortDateString().Trim();

                dgSequence.IsEnabled = true;
                BtnTerminerImage.IsEnabled = true;
                BtnSupprimerSequence.IsEnabled = true;
                BtnAddSequence.IsEnabled = true;
                PanelIndicateur.IsEnabled = true;
                cbxBisOrdre.IsEnabled = true;
                cbxDoublonOrdre.IsEnabled = true;
                cbxSautOrdre.IsEnabled = true;
                BtnCancelModifSequence.IsEnabled = false;
                BtnSaveModifSequence.IsEnabled = false;

                tbListeReferences.Visibility = Visibility.Collapsed;
                tbxNbRefsList.Visibility = Visibility.Collapsed;
                tbListeReferences.Text = "";
                tbListeReferences.ToolTip = "";
                tbxNbRefsList.Text = "Nb : 0";
                tbxNbRefs.Text = "Nb : ND";
                References.Clear();
                editSequence = false;

                #region RECUPERATION DU NUMERO D'ORDRE DE LA DERNIERE SEQUENCE INDEXE ET DEFINITION DE LA PROCHAINE SEQUENCE
                using (var ct = new DocumatContext())
                {
                    var LastSequence = ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID
                                               && s.References.ToLower() != "defaut").OrderByDescending(i => i.NUmeroOdre).FirstOrDefault();
                    tbNumeroOrdreSequence.Text = (LastSequence != null) ? (LastSequence.NUmeroOdre + 1).ToString() : CurrentImageView.Image.DebutSequence.ToString();
                    tbDateSequence.Text = (LastSequence != null) ? LastSequence.DateSequence.ToShortDateString() : CurrentImageView.Image.DateDebutSequence.ToShortDateString();
                }
                //On donne le focus à la date 
                tbDateSequence.Focus();
                #endregion
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void DeclareManquant_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Déclaration d'une page Manquante
                // vérification que la Page est celle en indexation
                // Chargement de la première page/ou de la page en cours
                using (var ct = new DocumatContext())
                {
                    // Récupération de la date du serveur 
                    DateTime dateServer = ct.Database.SqlQuery<DateTime>("SELECT GETDATE()").FirstOrDefault();

                    // Récupération de la page en cours d'indexation
                    Models.Image image = ct.Image.OrderBy(i => i.NumeroPage).FirstOrDefault(i => i.StatutActuel == (int)Enumeration.Image.CREEE
                    && i.RegistreID == RegistreParent.RegistreID);

                    if (currentImage == image.NumeroPage && currentImage > 0)
                    {
                        if (MessageBox.Show($"Voulez vous vraiment déclarer la page {image.NumeroPage} comme manquant ?", "Déclarer le Manquant", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            // Intégration d'un saut de page 
                            // Récupération des image comprise dans la fouchette
                            List<FileInfo> fileInfosToRename = new List<FileInfo>();
                            List<FileInfo> fileInfosNewRename = new List<FileInfo>();

                            // Création de L'image Sauter                        
                            // On Crée L'image Manquante dans la Base de Données
                            Models.Image imageManquant = new Models.Image()
                            {
                                CheminImage = image.CheminImage,
                                DateCreation = dateServer,
                                DateModif = dateServer,
                                DebutSequence = image.DebutSequence,
                                DateDebutSequence = image.DateDebutSequence,
                                FinSequence = image.FinSequence,
                                Taille = image.Taille,
                                NomPage = image.NomPage,
                                NumeroPage = image.NumeroPage,
                                StatutActuel = (int)Enumeration.Image.INSTANCE,
                                Type = image.Type,
                                RegistreID = image.RegistreID,
                                typeInstance = "Image Manquante",
                                DateScan = dateServer
                            };

                            foreach (var file in fileInfos)
                            {
                                int NumeroPageRen = 0;
                                if (Int32.TryParse(file.Name.Replace(file.Extension, "").Trim(), out NumeroPageRen) && NumeroPageRen >= image.NumeroPage)
                                {
                                    // On renomme la page avec un tiret à la fin
                                    string newFile = Path.Combine(file.DirectoryName, file.Name.Remove(file.Name.Length - file.Extension.Length) + "_" + file.Extension);
                                    File.Move(file.FullName, newFile);
                                    fileInfosToRename.Add(new FileInfo(newFile));
                                }
                            }

                            // On Renomme Maintenant le tout 
                            foreach (var file in fileInfosToRename)
                            {
                                int NumeroPageRen = 0;
                                if (Int32.TryParse(file.Name.Remove(file.Name.Length - file.Extension.Length).Replace("_", ""), out NumeroPageRen))
                                {
                                    string newNom;
                                    if ((NumeroPageRen + 1) > 9)
                                    {
                                        if ((NumeroPageRen + 1) > 99)
                                        {
                                            newNom = $"{(NumeroPageRen + 1)}";
                                        }
                                        else
                                        {
                                            newNom = $"0{(NumeroPageRen + 1)}";
                                        }
                                    }
                                    else
                                    {
                                        newNom = $"00{(NumeroPageRen + 1)}";
                                    }
                                    string newFile = Path.Combine(file.DirectoryName, newNom + file.Extension);
                                    File.Move(file.FullName, newFile);
                                }
                            }

                            /// Récuoértion de toutes les images du registre
                            List<Models.Image> images = ct.Image.Where(i => i.RegistreID == RegistreParent.RegistreID).OrderByDescending(i => i.NumeroPage).ToList();

                            foreach (var im in images)
                            {
                                if (im.NumeroPage >= image.NumeroPage)
                                {
                                    // On renomme la page 
                                    string newNom;
                                    if ((im.NumeroPage + 1) > 9)
                                    {
                                        if ((im.NumeroPage + 1) > 99)
                                        {
                                            newNom = $"{(im.NumeroPage + 1)}";
                                        }
                                        else
                                        {
                                            newNom = $"0{(im.NumeroPage + 1)}";
                                        }
                                    }
                                    else
                                    {
                                        newNom = $"00{(im.NumeroPage + 1)}";
                                    }
                                    im.NumeroPage = im.NumeroPage + 1;
                                    im.NomPage = newNom;
                                    im.CheminImage = Path.Combine(RegistreParent.CheminDossier, newNom + "." + im.Type.ToLower());
                                    im.DateModif = dateServer;
                                    ct.SaveChanges();
                                }
                            }

                            #region RECHERCHER LA DERNIERE SEQUENCE INDEXE DU REGISTRE
                            Models.Sequence LastSequenceImagePrecede = null;
                            int pageAremonte = 1, NumeroOdre = 1;
                            do
                            {
                                Models.Image imagePreced = ct.Image.FirstOrDefault(im => im.RegistreID == CurrentImageView.Image.RegistreID && im.NumeroPage == CurrentImageView.Image.NumeroPage - pageAremonte);

                                if (imagePreced != null)
                                {
                                    LastSequenceImagePrecede = ct.Sequence.Where(s => s.ImageID == imagePreced.ImageID).OrderBy(s => s.NUmeroOdre).ToList().LastOrDefault();

                                    if (imagePreced.StatutActuel == (int)Enumeration.Image.INSTANCE)
                                    {
                                        NumeroOdre = (string.IsNullOrWhiteSpace(imagePreced.ObservationInstance)) ? 1 : (Int32.Parse(imagePreced.ObservationInstance));
                                        break;
                                    }
                                    else if (imagePreced.NumeroPage == 1)
                                    {
                                        LastSequenceImagePrecede = ct.Sequence.Where(s => s.ImageID == imagePreced.ImageID).OrderBy(s => s.NUmeroOdre).ToList().LastOrDefault();
                                        NumeroOdre = (LastSequenceImagePrecede != null) ? LastSequenceImagePrecede.NUmeroOdre : CurrentImageView.Image.DebutSequence;
                                        break;
                                    }
                                    else
                                    {
                                        if (LastSequenceImagePrecede != null)
                                        {
                                            NumeroOdre = LastSequenceImagePrecede.NUmeroOdre;
                                            break;
                                        }
                                    }
                                }
                                pageAremonte++;
                            }
                            while (LastSequenceImagePrecede == null);
                            #endregion

                            // Ajout de L'image Manquante à la BD
                            imageManquant.ObservationInstance = NumeroOdre.ToString();
                            ct.Image.Add(imageManquant);
                            ct.SaveChanges();

                            // Enregistrement de l'action effectuée par l'agent
                            DocumatContext.AddTraitement(DocumatContext.TbImage, imageManquant.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION, "#IMAGE_MANQUANTE");

                            // Actualisation du Registre
                            LoadAll();
                            MessageBox.Show($"L'image {imageManquant.NumeroPage} a été déclarée comme Manquant", "Manquant", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Attention pour déclarer un Manquant,  veuillez séctionner la page en cours d'indexation", "Selectionner La Page en Indexation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void cbxDateManual_Click(object sender, RoutedEventArgs e)
        {
            tbDateSequence.Focus();
            if (cbxDateManual.IsChecked == true)
            {
                cbxDateManual.ToolTip = "Manuelle : Activé";
            }
            else
            {
                cbxDateManual.ToolTip = "Manuelle : Désactivé";
            }
        }

        private void SupprimeSequence_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgSequence.SelectedItems.Count == 1)
                {
                    if (MessageBox.Show("Voulez-vous Vraiment supprimer la séquence sélectionnée ?", "CONFIRMER SUPPRESSION", MessageBoxButton.YesNo, MessageBoxImage.Question)
                        == MessageBoxResult.Yes)
                    {
                        SequenceView sequenceView = dgSequence.SelectedItem as SequenceView;

                        using (var ct = new DocumatContext())
                        {
                            ct.Sequence.Remove(ct.Sequence.FirstOrDefault(s => s.SequenceID == sequenceView.Sequence.SequenceID));
                            ct.SaveChanges();

                            // Refresh des données
                            ImageView imageView1 = new ImageView();
                            List<ImageView> imageViews = imageView1.GetSimpleViewsList(RegistreParent);
                            imageView1 = imageViews.FirstOrDefault(i => i.Image.NumeroPage == currentImage);
                            if (imageView1 != null)
                            {
                                List<Sequence> sequences = ct.Sequence.Where(s => s.ImageID == imageView1.Image.ImageID).OrderBy(s => s.NUmeroOdre).ToList();
                                dgSequence.ItemsSource = SequenceView.GetViewsList(sequences);
                                if (sequences.Count != 0)
                                {
                                    dgSequence.ScrollIntoView(dgSequence.Items.GetItemAt(dgSequence.Items.Count - 1));
                                }
                            }

                            MessageBox.Show("La séquence a bien été supprimée !!!", "Séquence Supprimée", MessageBoxButton.OK, MessageBoxImage.Information);
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
