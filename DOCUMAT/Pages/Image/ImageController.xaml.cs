using DOCUMAT.Models;
using DOCUMAT.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
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
    /// Logique d'interaction pour ImageController.xaml
    /// </summary>
    public partial class ImageController : Window
    {
        #region ATTRIBUTS
        // Controle Parent par lequel la vue ImageControlle à été lancé
        public Controle.Controle MainParent;
        //Registre parent des images à controler
        public Models.Registre RegistreParent;
        // Numéro de l'image en cours
        private int currentImage = 1;
        // Current Image
        private Models.Image CurrentImage;

        string DossierRacine = ConfigurationManager.AppSettings["CheminDossier_Scan"];
        // Définition de l'aborescence
        TreeViewItem registreAbre = new TreeViewItem();
        //Fichier d'image sur le disque dure
        List<FileInfo> fileInfos = new List<FileInfo>();
        //Liste des sequences indexés comme faux dans l'image courant du registre
        //soit dont le numéro est currentImage
        Dictionary<int, SequenceView> ListeSequences = new Dictionary<int, SequenceView>();
        //Initiale des références par type de registre R3 = R / R4 = T
        string RefInitiale = "R";
        //Liste des possibles references pour la séquence en cours de l'image en cours
        Dictionary<String, String> References = new Dictionary<String, String>();
        // Controle des chargements de l'aborescence
        public bool AllisLoad = false;

        //Backround Workers
        private readonly BackgroundWorker GetAsyncControleIndicateur = new BackgroundWorker();
        private readonly BackgroundWorker GetAsyncRejetIndicateur = new BackgroundWorker();
        private readonly BackgroundWorker GetAsyncAgentAndRegistreInfos = new BackgroundWorker();
        private readonly BackgroundWorker RefreshAsyncAbo = new BackgroundWorker();

        #endregion

        #region FONCTIONS			

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
                    image = ct.Image.FirstOrDefault(i => i.NumeroPage == currentImage && i.RegistreID == RegistreParent.RegistreID);
                }

                if (image != null)
                {
                    #region CHARGEMENT DE L'IMAGE DE SCAN 
                    // Chargement de la visionneuse
                    if (File.Exists(Path.Combine(DossierRacine, image.CheminImage)))
                    {
                        viewImage(Path.Combine(DossierRacine, image.CheminImage));
                    }
                    else
                    {
                        dgSequence.ItemsSource = null;
                        PageImage.Source = null;
                        throw new Exception("La page : \"" + image.NumeroPage + "\" est introuvable !!!");
                    }
                    #endregion

                    #region CHARGEMENT DE L'ENTETE DU FORMULAIRE
                    //On charge l'image actuelle
                    CurrentImage = image;
                    this.currentImage = currentImage;

                    if (image.NumeroPage == -1)
                    {
                        tbxNomPage.Text = "PAGE DE GARDE";
                        tbxNumeroPage.Text = "";
                    }
                    else if (image.NumeroPage == 0)
                    {
                        tbxNomPage.Text = "PAGE D'OUVERTURE";
                        tbxNumeroPage.Text = "";
                    }
                    else
                    {
                        tbxNomPage.Text = "PAGE : " + image.NomPage;

                        int NumeroPageLast = RegistreParent.NombrePage;

                        using (var ct = new DocumatContext())
                        {
                            NumeroPageLast = ct.Database.SqlQuery<int>($"SELECT TOP 1 NumeroPage FROM Images WHERE RegistreID = {RegistreParent.RegistreID} ORDER BY NumeroPage DESC").FirstOrDefault();
                        }

                        tbxNumeroPage.Text = "N° " + image.NumeroPage.ToString() + "/ " + NumeroPageLast;
                    }

                    //Reinitialisation de l'affichage
                    SequenceSide.IsExpanded = false;
                    cbxManqueSequences.IsChecked = false;
                    ImageSide.IsExpanded = false;
                    cbxRejetImage.IsChecked = false;
                    cbxSupprimerImage.IsChecked = false;
                    cbxRejetScan.IsChecked = false;
                    #endregion

                    #region RECUPERATION DES SEQUENCES DE L'IMAGE
                    using (var ct = new DocumatContext())
                    {
                        //On réinitialise les Manquants
                        tbManqueSequences.Text = "";
                        // On vide le Dictionary des séquences de l'image précédente
                        ListeSequences.Clear();
                        // Récupération des sequences déja renseignées, Différent des images préindexer ayant la référence défaut
                        List<Sequence> sequences = ct.Sequence.Where(s => s.ImageID == image.ImageID
                        && s.References.ToLower() != "defaut").OrderBy(s => s.NUmeroOdre).ToList();
                        if (sequences.Count != 0)
                        {
                            //dgSequence.ScrollIntoView(dgSequence.Items.GetItemAt(dgSequence.Items.Count - 1));
                            //Ajout des séquences dans le Dictionary					
                            foreach (var seq in SequenceView.GetViewsList(sequences))
                            {
                                ListeSequences.Add(seq.Sequence.SequenceID, seq);
                            }

                            dgSequence.ItemsSource = ListeSequences.Values.ToList();
                        }
                        else
                        {
                            dgSequence.ItemsSource = null;
                            //throw new Exception("Cette image n'as pas de séquences !!!");
                        } 
                    }
                    #endregion

                    #region ADAPTATION DE L'AFFICHAGE EN FONCTION DES INSTANCES DE L'IMAGE
                    // Procédure d'affichage lorsque les statuts d'images changes
                    if (image.StatutActuel == (int)Enumeration.Image.PHASE1)
                    {
                        using (var ct = new DocumatContext())
                        {
                            if (ct.Controle.FirstOrDefault(c => c.ImageID == image.ImageID && c.SequenceID == null
                                && c.PhaseControle == 1 && c.StatutControle == 0) != null)
                            {
                                if (image.NumeroPage != -1)
                                {
                                    PanelControleEnCours.Visibility = Visibility.Collapsed;
                                    PanelControleEffectue.Visibility = Visibility.Visible;
                                    PanelIndexRegistre.Visibility = Visibility.Collapsed;
                                    BtnTerminerImage.Visibility = Visibility.Collapsed;
                                    ImgCorrecte.Visibility = Visibility.Visible;
                                    ImgEdit.Visibility = Visibility.Collapsed;
                                    tbxControleStatut.Text = "VALIDE";
                                }
                                else
                                {
                                    if (ct.Controle.Any(c => c.RegistreId == RegistreParent.RegistreID && c.StatutControle == 1 && c.PhaseControle == 1
                                                           && c.ImageID == null && c.SequenceID == null))
                                    {
                                        PanelControleEnCours.Visibility = Visibility.Collapsed;
                                        PanelControleEffectue.Visibility = Visibility.Visible;
                                        PanelIndexRegistre.Visibility = Visibility.Collapsed;
                                        BtnTerminerImage.Visibility = Visibility.Collapsed;
                                        ImgCorrecte.Visibility = Visibility.Collapsed;
                                        ImgEdit.Visibility = Visibility.Visible;
                                        tbxControleStatut.Text = "INDEX REGISTRE EN CORRECTION";
                                    }
                                    else
                                    {
                                        PanelControleEnCours.Visibility = Visibility.Collapsed;
                                        PanelControleEffectue.Visibility = Visibility.Visible;
                                        PanelIndexRegistre.Visibility = Visibility.Collapsed;
                                        BtnTerminerImage.Visibility = Visibility.Collapsed;
                                        ImgCorrecte.Visibility = Visibility.Visible;
                                        ImgEdit.Visibility = Visibility.Collapsed;
                                        tbxControleStatut.Text = "VALIDE";
                                    }
                                }
                            }
                            else
                            {
                                PanelControleEnCours.Visibility = Visibility.Collapsed;
                                PanelControleEffectue.Visibility = Visibility.Visible;
                                PanelIndexRegistre.Visibility = Visibility.Collapsed;
                                BtnTerminerImage.Visibility = Visibility.Collapsed;
                                ImgCorrecte.Visibility = Visibility.Collapsed;
                                ImgEdit.Visibility = Visibility.Visible;
                                tbxControleStatut.Text = "EN CORRECTION";
                            }
                        }
                    }
                    else if (image.StatutActuel == (int)Enumeration.Image.INDEXEE)
                    {
                        if (image.NumeroPage == -1 || image.NumeroPage == 0)
                        {
                            SequenceSide.Visibility = Visibility.Collapsed;
                            if (image.NumeroPage == -1)
                            {
                                tbxVolume.Text = ": " + RegistreParent.Numero;
                                tbxNumDepotDebut.Text = ": " + RegistreParent.NumeroDepotDebut.ToString();
                                tbxDateDepotDebut.Text = ": " + RegistreParent.DateDepotDebut.ToShortDateString();
                                tbxNumDepotFin.Text = ": " + RegistreParent.NumeroDepotFin.ToString();
                                tbxDateDepotFin.Text = ": " + RegistreParent.DateDepotFin.ToShortDateString();
                                tbxNbPage.Text = ": " + RegistreParent.NombrePage.ToString();
                                PanelIndexRegistre.Visibility = Visibility.Visible;
                                BtnTerminerImage.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                PanelIndexRegistre.Visibility = Visibility.Collapsed;
                                BtnTerminerImage.Visibility = Visibility.Visible;
                            }
                        }
                        else
                        {
                            SequenceSide.Visibility = Visibility.Visible;
                            PanelIndexRegistre.Visibility = Visibility.Collapsed;
                            BtnTerminerImage.Visibility = Visibility.Visible;
                        }

                        PanelControleEnCours.Visibility = Visibility.Visible;
                        PanelControleEffectue.Visibility = Visibility.Collapsed;
                    }
                    BorderControle.IsEnabled = true;
                    #endregion

                    // Actualisation de l'aborescence
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
                ex.ExceptionCatcher();
            }
        }
        
        public void checksControleEnd()
        {
            using (var ct = new DocumatContext())
            {
                #region CHARGEMENT DU BOUTON DE VALIDATION
                //Affichage du bouton de validadtion du contrôle si toute les images sont marqué en phase 1 
                if (!ct.Image.Any(i => i.StatutActuel == (int)Enumeration.Image.INDEXEE && i.RegistreID == RegistreParent.RegistreID))
                {
                    btnValideControle.Visibility = Visibility.Visible;
                }
                else
                {
                    btnValideControle.Visibility = Visibility.Collapsed;
                }
                #endregion
            }
        }

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
                if (!GetAsyncControleIndicateur.IsBusy)
                {
                    GetAsyncControleIndicateur.RunWorkerAsync(RegistreParent);
                }

                if (!GetAsyncRejetIndicateur.IsBusy)
                {
                    GetAsyncRejetIndicateur.RunWorkerAsync(RegistreParent);
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

        /// <summary>
        /// Chargement des éléments de l'Aborescence
        /// </summary>
        public void LoadAborescence()
        {
            #region INSPECTION DU DOSSIER DE REGISTRE ET CREATION DE L'ABORESCENCE
            // Chargement de l'aborescence
            // Récupération de la liste des images contenu dans le dossier du registre
            var files = Directory.GetFiles(Path.Combine(DossierRacine, RegistreParent.CheminDossier));

            // Affichage et configuration de la TreeView
            registreAbre.Items.Clear();
            registreAbre.Header = RegistreParent.QrCode;
            registreAbre.Tag = RegistreParent.QrCode;
            registreAbre.FontWeight = FontWeights.Normal;
            registreAbre.Foreground = Brushes.White;

            // Définition de la lettre de référence pour ce registre
            if (RegistreParent.Type == "R4")
                RefInitiale = "T";

            // Ajout de la page de garde comme entête de l'aborescence
            foreach (var file in files)
            {
                if (GetFileFolderName(file).Remove(GetFileFolderName(file).Length - 4).ToLower() == "PAGE DE GARDE".ToLower())
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

            // Ajout de la page d'ouverture
            foreach (var file in files)
            {
                if (GetFileFolderName(file).Remove(GetFileFolderName(file).Length - 4).ToLower() == "PAGE D'OUVERTURE".ToLower())
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
                if (GetFileFolderName(file.Value).Remove(GetFileFolderName(file.Value).Length - 4).ToLower() != "PAGE DE GARDE".ToLower()
                    && GetFileFolderName(file.Value).Remove(GetFileFolderName(file.Value).Length - 4).ToLower() != "PAGE D'OUVERTURE".ToLower())
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

            //Ajout des pages manquantes
            using (var ct = new DocumatContext())
            {
                // Images Manquantes
                List<Models.ManquantImage> manquantImages = ct.ManquantImage.Where(m => m.IdRegistre == RegistreParent.RegistreID).OrderBy(m => m.NumeroPage).ToList();
                foreach (var mqImage in manquantImages)
                {
                    var fileTree = new TreeViewItem();
                    fileTree.Header = mqImage.NumeroPage.ToString();
                    fileTree.Tag = "manquant";
                    fileTree.FontWeight = FontWeights.Normal;
                    fileTree.Foreground = Brushes.White;
                    fileTree.MouseDoubleClick += FileTree_MouseDoubleClick;
                    registreAbre.Items.Add(fileTree);
                }
            }

            FolderView.Items.Clear();
            FolderView.Items.Add(registreAbre);
            #endregion
        }

        private void setActiveAborescenceImage()
        {
            // On rend tout les items de l'aborescence normaux et on met en gras l'élément concerné
            foreach (TreeViewItem item in registreAbre.Items)
            {
                item.FontWeight = FontWeights.Normal;
                int numeroPage = 0;
                string[] headerTitle = item.Header.ToString().Split('.');
                if (currentImage == -1)
                {
                    if (headerTitle[0].ToLower() == "PAGE DE GARDE".ToLower())
                    {
                        item.FontWeight = FontWeights.Bold;
                    }
                }
                else if (currentImage == 0)
                {
                    if (headerTitle[0].ToLower() == "PAGE D'OUVERTURE".ToLower())
                    {
                        item.FontWeight = FontWeights.Bold;
                    }
                }
                else if (Int32.TryParse(headerTitle[0], out numeroPage))
                {
                    if (currentImage == numeroPage)
                    {
                        item.FontWeight = FontWeights.Bold;
                    }
                }
            }
        }

        private void setActiveAboImageIcon(int NumeroPage,string iconName)
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

        /// <summary>
        /// Actualise l'aborescence les étiquettes de l'aborescence!!!
        /// </summary>
        public void ActualiserArborescence()
        {
            try
            {
                RefreshAsyncAbo.RunWorkerAsync(RegistreParent);
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }
        #endregion

        public ImageController()
        {
            InitializeComponent();
            FolderView.ContextMenu = (ContextMenu)this.FindResource("cmManquantImage");

            //Async Method
            GetAsyncAgentAndRegistreInfos.WorkerSupportsCancellation = true;
            GetAsyncAgentAndRegistreInfos.DoWork += GetAsyncAgentAndRegistreInfos_DoWork;
            GetAsyncAgentAndRegistreInfos.RunWorkerCompleted += GetAsyncAgentAndRegistreInfos_RunWorkerCompleted;
            GetAsyncAgentAndRegistreInfos.Disposed += GetAsyncAgentAndRegistreInfos_Disposed;

            GetAsyncControleIndicateur.WorkerSupportsCancellation = true;
            GetAsyncControleIndicateur.DoWork += GetAsyncControleIndicateur_DoWork;
            GetAsyncControleIndicateur.RunWorkerCompleted += GetAsyncControleIndicateur_RunWorkerCompleted;
            GetAsyncControleIndicateur.Disposed += GetAsyncControleIndicateur_Disposed;

            GetAsyncRejetIndicateur.WorkerSupportsCancellation = true;
            GetAsyncRejetIndicateur.DoWork += GetAsyncRejetIndicateur_DoWork;
            GetAsyncRejetIndicateur.RunWorkerCompleted += GetAsyncRejetIndicateur_RunWorkerCompleted;
            GetAsyncRejetIndicateur.Disposed += GetAsyncRejetIndicateur_Disposed;

            RefreshAsyncAbo.WorkerSupportsCancellation = true;
            RefreshAsyncAbo.DoWork += RefreshAsyncAbo_DoWork;
            RefreshAsyncAbo.RunWorkerCompleted += RefreshAsyncAbo_RunWorkerCompleted;
            RefreshAsyncAbo.Disposed += RefreshAsyncAbo_Disposed; 
        }

        private void RefreshAsyncAbo_Disposed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void RefreshAsyncAbo_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                // tableau des Resultats 
                //object[] obs = new object[] { manquantImages, images, controles };
                object[] obs = (object[])e.Result;
                List<Models.ManquantImage> manquantImages = (List<Models.ManquantImage>)obs[0];
                List<Models.Image> images = (List<Models.Image>)obs[1];
                List<Models.Controle> controles = (List<Models.Controle>)obs[2];

                foreach (TreeViewItem item in registreAbre.Items)
                {
                        int NumeroAbr = 0;
                        string[] headerTitle = item.Header.ToString().Split('.');

                        if (Int32.TryParse(headerTitle[0], out NumeroAbr)
                            || headerTitle[0].ToUpper()
                            == ConfigurationManager.AppSettings["Nom_Page_Garde"].ToUpper()
                            || headerTitle[0].ToUpper()
                            == ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToUpper())
                        {
                            if (headerTitle[0].ToUpper()
                            == ConfigurationManager.AppSettings["Nom_Page_Garde"].ToUpper())
                            {
                                NumeroAbr = -1;
                            }

                            if (headerTitle[0].ToUpper()
                            == ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToUpper())
                            {
                                NumeroAbr = 0;
                            }

                            Models.ManquantImage manquantImage = manquantImages.FirstOrDefault(m => m.NumeroPage == NumeroAbr);
                            if (manquantImage != null)
                            {
                                item.Tag = "notFound";
                            }
                            else
                            {
                                Models.Image image1 = images.FirstOrDefault(i => i.NumeroPage == NumeroAbr);
                                if (image1 != null)
                                {
                                    if (controles.FirstOrDefault(c => c.ImageID == image1.ImageID && c.SequenceID == null
                                       && c.PhaseControle == 1 && c.StatutControle == 0) != null)
                                    {
                                        if (image1.NumeroPage != -1)
                                        {
                                            item.Tag = "valide";
                                        }
                                        else
                                        {
                                            if (controles.Any(c => c.RegistreId == RegistreParent.RegistreID && c.StatutControle == 1 && c.PhaseControle == 1
                                                                   && c.ImageID == null && c.SequenceID == null))
                                            {
                                                item.Tag = "instance";
                                            }
                                            else
                                            {
                                                item.Tag = "valide";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        item.Tag = "instance";
                                    }
                                }
                                else
                                {
                                    item.Tag = "image.png";
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

        private void RefreshAsyncAbo_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                using (var ct = new DocumatContext())
                {
                    Models.Registre registre = (Models.Registre)e.Argument;
                    // Définition des types d'icon dans l'aborescence en fonction des statuts des images
                    // récupération de la liste des Images
                    ImageView imageView1 = new ImageView();
                    // Récupération de la liste des manquants pour ce registre
                    List<Models.ManquantImage> manquantImages = ct.ManquantImage.Where(m => m.IdRegistre == registre.RegistreID).ToList();
                    // Liste des Images de ce registre
                    List<Models.Image> images = ct.Image.Where(i => i.StatutActuel == (int)Enumeration.Image.PHASE1 && i.RegistreID == registre.RegistreID).ToList();
                    // Liste des Controles pour ce registres 
                    List<Models.Controle> controles = ct.Controle.Where(c => c.RegistreId == registre.RegistreID && c.PhaseControle == 1).ToList();

                    // tableau des Resultats 
                    object[] obs = new object[] { manquantImages, images, controles };
                    e.Result = obs; 
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
                    Models.Service service =  ct.Service.FirstOrDefault(s => s.ServiceID == livraison.ServiceID);

                    // Tableau des Resultats
                    object[] obs = new object[] { registre, versement, livraison,service};
                    e.Result = obs;
                }              
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void GetAsyncRejetIndicateur_Disposed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void GetAsyncRejetIndicateur_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                int?[] result = (int?[])e.Result;
                    //new int?[] { NbIndex, NbIndexRejet, NbRejetImage, NbImageASupp, imageViews.Count }                   
                ArcIndicatorThird.EndAngle = ((((float)result[1] / (float)result[0]) * 100) * 360) / 100;
                TextIndicatorThird.Text = Math.Round((((float)result[1] / (float)result[0]) * 100), 2) + "%";

                tbxNbImageASupp.Text = $"Nb A Suppimer : {result[3]} ~ {Math.Round((((float)result[3] / (float)result[4]) * 100), 2)} %";
                tbxNbRejetImage.Text = $"Nb Rejet Image : {result[2]} ~ {Math.Round((((float)result[2] / (float)result[4]) * 100), 2)} %";
                tbxNbIndexRejet.Text = $"Nb Index Rejeté : {result[1]} ~ {Math.Round((((float)result[1] / (float)result[0]) * 100), 2)} %";
                tbxNbIndex.Text = "Nb Index : " + result[0];
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void GetAsyncRejetIndicateur_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                // Récupération des Arguments registres
                Models.Registre registre = (Models.Registre)e.Argument;

                //Calcule des Index
                using (var ct = new DocumatContext())
                {
                    int? NbImageRegistre = ct.Database.SqlQuery<int?>($"SELECT  COUNT(DISTINCT ImageID) FROM Images WHERE RegistreID = {RegistreParent.RegistreID} ").FirstOrDefault();

                    // Nombre d'index du Registres 
                    //Récupération des index de toutes les séquences (References compté 1 fois )  
                    string reqAllIndex = "SELECT SUM(val) FROM( SELECT " +
                                         "SUM(data1.Som) AS val FROM " +
                                         "(SELECT (SELECT SUM( CASE WHEN split.a.value('.', 'VARCHAR(100)') = '' THEN 0 ELSE 1 END) " +
                                         "FROM(SELECT Cast('<M>' + Replace(sq.[References], ',', '</M><M>') + '</M>' AS XML) AS Data) AS A CROSS apply data.nodes('/M') AS Split(a)) AS Som " +
                                         "FROM Sequences sq INNER JOIN Images im ON sq.ImageID = im.ImageID " +
                                         "INNER JOIN Registres rg ON im.RegistreID = rg.RegistreID " +
                                         $"WHERE rg.RegistreID = {registre.RegistreID}) AS data1 " +

                                         //Récupération des index de toutes les séquence (NumeroDepot et DateDepot)  
                                         "UNION " +
                                         "SELECT(COUNT(sq.SequenceID) * 2) AS val " +
                                         "FROM Sequences sq " +
                                         "INNER JOIN Images im ON sq.ImageID = im.ImageID " +
                                         "INNER JOIN Registres rg ON im.RegistreID = rg.RegistreID " +
                                         $"WHERE rg.RegistreID = {registre.RegistreID} " +

                                         //Récupération de toutes les  images * 10 index = 5 index Image + 5 Index Registres Parents
                                         "UNION " +
                                         $"SELECT (COUNT(ImageID) * 10) AS val FROM Images WHERE RegistreID = {registre.RegistreID}" +
                                         ") AS Nb_Index ";
                                         
                    int? NbIndex = ct.Database.SqlQuery<int?>(reqAllIndex).FirstOrDefault();


                    string reqAllIndexRejete = "SELECT SUM(Nb_All_Index.val) " +
                                                "FROM " +
                                                "( " +
                                                "SELECT " +
                                                "SUM(data1.Som) AS val " +
                                                "FROM " +
                                                "(SELECT " +
                                                "(SELECT SUM( " +
                                                "CASE WHEN split.a.value('.', 'VARCHAR(100)') = '' THEN 0 ELSE 1 END ) " +
                                                "FROM(SELECT Cast('<M>' + Replace(ct.RefRejetees_idx, ',', '</M><M>') + '</M>' AS XML) AS Data) AS A CROSS apply data.nodes('/M') AS Split(a)) AS Som " +
                                                "FROM Controles ct " +
                                                "INNER JOIN Registres rg ON ct.RegistreId = rg.RegistreID " +
                                                "WHERE PhaseControle = 1 AND ct.RefSequence_idx = 1 " +
                                                $"AND StatutControle = 1 AND rg.RegistreID = {registre.RegistreID}) AS data1 " +
                                                "UNION " +
                                                "SELECT SUM(" +
                                                "(CASE  WHEN OrdreSequence_idx IS NULL THEN 0 ELSE OrdreSequence_idx END) + " +
                                                "(CASE  WHEN DateSequence_idx IS NULL THEN 0 ELSE DateSequence_idx END) + " +
                                                "(CASE  WHEN Numero_idx IS NULL THEN 0 ELSE Numero_idx END) + " +
                                                "(CASE  WHEN DateDepotDebut_idx IS NULL THEN 0 ELSE DateDepotDebut_idx END) + " +
                                                "(CASE  WHEN DateDepotFin_idx IS NULL THEN 0 ELSE DateDepotFin_idx END) + " +
                                                "(CASE  WHEN NumeroDebut_idx IS NULL THEN 0 ELSE NumeroDebut_idx END) + " +
                                                "(CASE  WHEN NombrePage_idx IS NULL THEN 0 ELSE NombrePage_idx END) + " +
                                                "(CASE  WHEN NumeroDepotFin_idx IS NULL THEN 0 ELSE NumeroDepotFin_idx END) " +
                                                ") AS val " +
                                                "FROM Controles ct " +
                                                "INNER JOIN Registres rg ON ct.RegistreId = rg.RegistreID " +
                                                $"WHERE PhaseControle = 1 AND StatutControle = 1 AND rg.RegistreID = {registre.RegistreID}) AS Nb_All_Index";
                    int? NbIndexRejet = ct.Database.SqlQuery<int?>(reqAllIndexRejete).FirstOrDefault();


                    string reqRejetImage = "SELECT COUNT(DISTINCT im.ImageID) " +
                                           "FROM Images im " +
                                           "INNER JOIN Controles ct ON ct.ImageID = im.ImageID " +
                                           $"WHERE im.RegistreID = {registre.RegistreID} " +
                                           $"AND ct.PhaseControle = 1 AND ct.StatutControle = 1 AND ct.RejetImage_idx = 1";
                    int? NbRejetImage = ct.Database.SqlQuery<int?>(reqRejetImage).FirstOrDefault();


                    string reqImageASupp = "SELECT COUNT(DISTINCT im.ImageID) " +
                                           "FROM Images im " +
                                           "INNER JOIN Controles ct ON ct.ImageID = im.ImageID " +
                                           $"WHERE im.RegistreID = {registre.RegistreID} " +
                                           $"AND ct.PhaseControle = 1 AND ct.StatutControle = 1 " +
                                           $"AND ct.SequenceID is Null AND ct.ASupprimer = 1";
                    int? NbImageASupp = ct.Database.SqlQuery<int?>(reqImageASupp).FirstOrDefault();

                    // Nombtre d'index Rejeté
                    NbIndex = (NbIndex == null) ? 0 : NbIndex;
                    NbIndexRejet = (NbIndexRejet == null) ? 0 : NbIndexRejet;
                    NbRejetImage = (NbRejetImage == null) ? 0 : NbRejetImage;
                    NbImageASupp = (NbImageASupp == null) ? 0 : NbImageASupp;
                    NbImageRegistre = (NbImageRegistre == null) ? 0 : NbImageRegistre;
                    // Tableau des Resultats
                    int?[] result = new int?[] { NbIndex, NbIndexRejet, NbRejetImage, NbImageASupp, NbImageRegistre};
                    e.Result = result;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void GetAsyncControleIndicateur_Disposed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void GetAsyncControleIndicateur_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                double[] result = (double[])e.Result;
                tbxImageTerminer.Text = "Terminé : " + result[0] + " % ";
                tbxImageEnCours.Text = "Non Traité : " + result[1] + " % ";
                tbxImageRejete.Text = "En Correction : " + result[2] + " % ";
                tbxImageValide.Text = "Validé : " + result[3] + " % ";
                ArcIndicator.EndAngle = (result[0] * 360) / 100;
                TextIndicator.Text = Math.Round(result[0], 1) + "%";
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void GetAsyncControleIndicateur_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                // Récupération des Arguments registres
                Models.Registre registre = (Models.Registre)e.Argument;

                //Obtention de la liste des images du registre dans la base de données 
                ImageView imageView1 = new ImageView();
                List<ImageView> imageViews = imageView1.GetSimpleViewsList(registre);

                //Procédure de modification de l'indicateur de reussite
                double perTerminer = Math.Round((((float)imageViews.Where(i => i.Image.StatutActuel == (int)Enumeration.Image.PHASE1).Count() / (float)imageViews.Count()) * 100),1);
                double EnCours = Math.Round(((float)imageViews.Where(i => i.Image.StatutActuel == (int)Enumeration.Image.INDEXEE).Count() / (float)imageViews.Count()) * 100, 1);

                // Récupération des images en correction
                List<Models.Image> imagesEnCorrection = new List<Models.Image>();
                List<Models.Image> imagesValide = new List<Models.Image>();
                foreach (var image in imageViews)
                {
                    using (var ct = new DocumatContext())
                    {
                        if (ct.Correction.Any(c => c.RegistreId == registre.RegistreID && c.ImageID == image.Image.ImageID
                                && c.SequenceID == null && c.StatutCorrection == 1 && c.PhaseCorrection == 1))
                        {
                            imagesEnCorrection.Add(image.Image);
                        }
                        else if (ct.Controle.Any(c => c.RegistreId == registre.RegistreID && c.ImageID == image.Image.ImageID
                                && c.SequenceID == null && c.StatutControle == 0 && c.PhaseControle == 1))
                        {
                            imagesValide.Add(image.Image);
                        }
                    }
                }
                double EnCorrection = (Math.Round(((float)imagesEnCorrection.Count() / (float)imageViews.Count()) * 100, 1));
                double Valide = (Math.Round(((float)imagesValide.Count() / (float)imageViews.Count()) * 100, 1));

                // Tableau de Resultats 
                double[] result = new double[] { perTerminer, EnCours, EnCorrection, Valide };

                e.Result = result;
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        public ImageController(Models.Registre registre, Controle.Controle controle) : this()
        {
            MainParent = controle;
            RegistreParent = registre;
        }

        private void FileTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = (TreeViewItem)sender;
            // Récupération des images du registre
            ImageView imageView1 = new ImageView();
            List<ImageView> imageViews = imageView1.GetSimpleViewsList(RegistreParent);

            //Chargement de l'image 
            int numeroPage = 0;
            string[] headerTitle = treeViewItem.Header.ToString().Split('.');

            // cas des Manquants
            if (headerTitle.Count() == 1)
            {
                Models.ManquantImage manquantImage = null;
                using (var ct = new DocumatContext())
                {
                    manquantImage = ct.ManquantImage.FirstOrDefault(m => m.IdRegistre == RegistreParent.RegistreID
                                                && m.NumeroPage.ToString() == treeViewItem.Header.ToString());
                }

                if (manquantImage != null)
                {
                    if (MessageBox.Show("Voulez vous retirer ce manquant ?", "QUESTION", MessageBoxButton.YesNo, MessageBoxImage.Question)
                        == MessageBoxResult.Yes)
                    {
                        using (var ct = new DocumatContext())
                        {
                            ct.ManquantImage.Remove(ct.ManquantImage.FirstOrDefault(m => m.IdRegistre == RegistreParent.RegistreID
                                                        && m.NumeroPage.ToString() == treeViewItem.Header.ToString()));
                            ct.SaveChanges();
                        }
                        registreAbre.Items.Remove(treeViewItem);
                    }
                } 
            }
            //Cas d'une page normal (Page numérotée)
            else if (Int32.TryParse(headerTitle[0], out numeroPage))
            {
                ChargerImage(numeroPage);
            }
            else if (headerTitle[0].ToLower() == "PAGE DE GARDE".ToLower())
            {
                //Affichage de la page de garde
                tbxNomPage.Text = "PAGE DE GARDE";
                tbxNumeroPage.Text = "";
                dgSequence.ItemsSource = null;
                // Chargement de l'image		
                ChargerImage(-1);
            }
            else if (headerTitle[0].ToLower() == "PAGE D'OUVERTURE".ToLower())
            {
                //Affichage de la page d'ouverture
                tbxNomPage.Text = "PAGE D'OUVERTURE";
                tbxNumeroPage.Text = "";
                dgSequence.ItemsSource = null;
                // Chargement de l'image		
                ChargerImage(0);
            }
        }

        public void LoadAll()
        {
            try
            {
                #region CHARGEMENT DE LA PAGE EN COURS DE CONTROLE 
                // Chargement de la première page/ou de la page en cours
                using (var ct = new DocumatContext())
                {
                    Models.Image image = ct.Image.OrderBy(i => i.NumeroPage).FirstOrDefault(i => i.StatutActuel == (int)Enumeration.Image.INDEXEE
                    && i.RegistreID == RegistreParent.RegistreID);
                    currentImage = (image != null) ? image.NumeroPage : -1;
                    ChargerImage(currentImage); 
                }
                #endregion

                #region VERIFICATION DE L'AGENT 				
                using (var ct = new DocumatContext())
                {
                    if (MainParent.Utilisateur.Affectation != (int)Enumeration.AffectationAgent.ADMINISTRATEUR && MainParent.Utilisateur.Affectation != (int)Enumeration.AffectationAgent.SUPERVISEUR)
                    {
                        Models.Traitement traitementAttr = ct.Traitement.FirstOrDefault(t => t.TableSelect == DocumatContext.TbRegistre && t.TableID == RegistreParent.RegistreID
                                                            && t.TypeTraitement == (int)Enumeration.TypeTraitement.CONTROLE_PH1_DEBUT);
                        if (traitementAttr != null)
                        {
                            Models.Traitement traitementRegistreAgent = ct.Traitement.FirstOrDefault(t => t.TableSelect == DocumatContext.TbRegistre && t.TableID == RegistreParent.RegistreID
                                                    && t.TypeTraitement == (int)Enumeration.TypeTraitement.CONTROLE_PH1_DEBUT && t.AgentID == MainParent.Utilisateur.AgentID);
                            Models.Traitement traitementAutreAgent = ct.Traitement.FirstOrDefault(t => t.TableSelect == DocumatContext.TbRegistre && t.TableID == RegistreParent.RegistreID
                                                    && t.TypeTraitement == (int)Enumeration.TypeTraitement.CONTROLE_PH1_DEBUT && t.AgentID != MainParent.Utilisateur.AgentID);

                            if (traitementAutreAgent != null)
                            {
                                Models.Agent agent = ct.Agent.FirstOrDefault(t => t.AgentID == traitementAutreAgent.AgentID);
                                MessageBox.Show("Ce Registre est en Cours traitement par l'agent : " + agent.Noms, "REGISTRE EN CONTROLE PH1", MessageBoxButton.OK, MessageBoxImage.Information);
                                this.Close();
                            }
                        }
                        else
                        {
                            if (MessageBox.Show("Voulez vous commencez le contrôle ?", "COMMNCER LE CONTROLE PH1", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                DocumatContext.AddTraitement(DocumatContext.TbRegistre, RegistreParent.RegistreID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.CONTROLE_PH1_DEBUT, "CONTROLE PH1 COMMENCER");
                            }
                            else
                            {
                                this.Close();
                            }
                        }
                    }
                }
                #endregion

                // Chargement Async de l'aborescence
                ActualiserArborescence();
                // Chargement Async des infos du header
                HeaderInfosGetter();
                AllisLoad = true;
                checksControleEnd();
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
                    //Remplissage des éléments de la vue
                    //Information sur l'agent 
                    if (!string.IsNullOrEmpty(MainParent.Utilisateur.CheminPhoto))
                    {
                        AgentImage.Source = new BitmapImage(new Uri(Path.Combine(DossierRacine, MainParent.Utilisateur.CheminPhoto)), new System.Net.Cache.RequestCachePolicy());
                    }
                    tbxAgentLogin.Text = "Login : " + MainParent.Utilisateur.Login;
                    tbxAgentNoms.Text = "Noms : " + MainParent.Utilisateur.Noms;
                    tbxAgentType.Text = "Aff : " + Enum.GetName(typeof(Enumeration.AffectationAgent), MainParent.Utilisateur.Affectation);

                    // Chargement des information d'entête
                    HeaderInfosGetter();
                }
                catch (Exception ex)
                {
                    ex.ExceptionCatcher();
                }
                #endregion

                #region RECUPERATION DE L'ABORESCENCE ET CHAREMENT DU HEADERS
                // Chargement de l'aborescence
                LoadAborescence();
                // Définition de l'évènement de déroulement de l'aborescence
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

        private void RefrechAll_Click(object sender, RoutedEventArgs e)
        {
            LoadAll();
        }

        private void cbxRejetImage_Checked(object sender, RoutedEventArgs e)
        {
            cbxSupprimerImage.IsChecked = false;
            cbxRejetScan.IsChecked = false;
            //cbxManqueImages.IsChecked = false;
            PanelRejetImage.Visibility = Visibility.Visible;
        }

        private void cbxRejetImage_Unchecked(object sender, RoutedEventArgs e)
        {
            PanelRejetImage.Visibility = Visibility.Collapsed;
        }

        private void cbxSupprimerImage_Checked(object sender, RoutedEventArgs e)
        {
            //cbxManqueImages.IsChecked = false;
            cbxRejetImage.IsChecked = false;
            cbxRejetScan.IsChecked = false;
        }

        private void cbxManqueSequences_Checked(object sender, RoutedEventArgs e)
        {
            //cbxSuppimerSequences.IsChecked = false;
            PanelManqueSequences.Visibility = Visibility.Visible;
        }

        private void cbxManqueSequences_Unchecked(object sender, RoutedEventArgs e)
        {
            PanelManqueSequences.Visibility = Visibility.Collapsed;
        }

        private void cbxSuppimerSequences_Checked(object sender, RoutedEventArgs e)
        {
            cbxManqueSequences.IsChecked = false;
        }

        private void BtnTerminerImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Voulez vous terminer le contrôle de cette Image ?", "Terminer Le Contrôle ?", MessageBoxButton.YesNo, MessageBoxImage.Question)
                        == MessageBoxResult.Yes)
                {
                    //Booléen d'identification de rejet sur un index
                    bool Image_Index_is_reject = false;

                    // Vérification des Index des Séquences de l'Images courante
                    #region VERIFICATION ET GESTION DES INDEXES DE SEQUENCES
                    foreach (var seq in ListeSequences)
                    {
                        #region VERIFICATION DES INDEXES DE LA SEQUENCE EN COURS
                        int ch_Ordre = 0, ch_Date = 0, ch_Reference = 0, ch_A_Supprimer = 0, Statut_sequence = 0;
                        string listRef = null;
                        bool Index_is_reject = false;

                        // Cas où le Numéro d'ordre est rejeté
                        if (seq.Value.Ordre_Is_Check)
                        {
                            ch_Ordre = 1;
                            Index_is_reject = true;
                            Image_Index_is_reject = true;
                        }

                        // Cas où la date est rejeté
                        if (seq.Value.Date_Is_Check)
                        {
                            ch_Date = 1;
                            Index_is_reject = true;
                            Image_Index_is_reject = true;
                        }

                        // Cas où la référence est rejeté
                        if (seq.Value.References_Is_Check)
                        {
                            foreach (var refEl in seq.Value.ListeRefecrences_check)
                            {
                                listRef += refEl.Value + ", ";
                            }
                            ch_Reference = 1;
                            Index_is_reject = true;
                            Image_Index_is_reject = true;
                        }

                        // Cas où, il y'a des Réferences Manquantes
                        int NbRefsManquant = 0;
                        if (Int32.TryParse(seq.Value.NbRefsManquant, out NbRefsManquant) && NbRefsManquant > 0)
                        {
                            // Ajout des Manquants sur un Format de Références  :
                            // 1/manq, 2/manq, 3/manq ...

                            for (int i = 1; i < (NbRefsManquant + 1); i++)
                            {
                                listRef += $"{i}/manq, ";
                            }
                            ch_Reference = 1;
                            Index_is_reject = true;
                            Image_Index_is_reject = true;
                        }

                        // Cas où la séquence est à Supprimer
                        if (seq.Value.ASupprimer_Is_Check)
                        {
                            ch_A_Supprimer = 1;
                            Index_is_reject = true;
                            Image_Index_is_reject = true;
                        }

                        // Détermination du Statut de la séquence 
                        if (Index_is_reject)
                        {
                            Statut_sequence = 1;
                        }
                        #endregion

                        using (var ct = new DocumatContext())
                        {
                            // On crée les controles de séquence rejété 
                            #region AJOUT D'UN CONTROLE DE SEQUENCE
                            Models.Controle controle = new Models.Controle()
                            {
                                RegistreId = RegistreParent.RegistreID,
                                ImageID = seq.Value.Sequence.ImageID,
                                SequenceID = seq.Value.Sequence.SequenceID,

                                //Indexes Image mis à null
                                NumeroPageImage_idx = null,
                                NomPageImage_idx = null,
                                RejetImage_idx = null,
                                MotifRejetImage_idx = null,

                                //Indexes de la séquence de l'image
                                OrdreSequence_idx = ch_Ordre,
                                DateSequence_idx = ch_Date,
                                RefSequence_idx = ch_Reference,
                                ASupprimer = ch_A_Supprimer,
                                RefRejetees_idx = listRef,

                                DateControle = DateTime.Now,
                                DateCreation = DateTime.Now,
                                DateModif = DateTime.Now,
                                PhaseControle = 1,
                                StatutControle = Statut_sequence,
                            };
                            ct.Controle.Add(controle);
                            #endregion

                            //On crée la correction de controle                         
                            #region AJOUT D'UNE CORRECTION AFIN QUE SEQUENCE PARTE EN CORRECTION 1
                            if (Index_is_reject)
                            {
                                Models.Correction correction = new Models.Correction()
                                {
                                    RegistreId = RegistreParent.RegistreID,
                                    ImageID = seq.Value.Sequence.ImageID,
                                    SequenceID = seq.Value.Sequence.SequenceID,

                                    //Indexes Image mis à null
                                    NumeroPageImage_idx = null,
                                    NomPageImage_idx = null,
                                    RejetImage_idx = null,
                                    MotifRejetImage_idx = null,

                                    //Indexes de la séquence de l'image
                                    OrdreSequence_idx = ch_Ordre,
                                    DateSequence_idx = ch_Date,
                                    RefSequence_idx = ch_Reference,
                                    ASupprimer = ch_A_Supprimer,
                                    RefRejetees_idx = listRef,

                                    DateCorrection = DateTime.Now,
                                    DateCreation = DateTime.Now,
                                    DateModif = DateTime.Now,
                                    PhaseCorrection = 1,
                                    StatutCorrection = Statut_sequence,
                                };
                                ct.Correction.Add(correction);
                            }
                            #endregion

                            //Changement de la phase de la séquence
                            Models.Sequence sequence1 = ct.Sequence.FirstOrDefault(s => s.SequenceID == seq.Value.Sequence.SequenceID);
                            sequence1.PhaseActuelle = 1;
                            // Ajout dans la base de données
                            ct.SaveChanges();
                        }
                    }
                    #endregion

                    #region VERIFICATION DES MANQUANTS DE l'IMAGE
                    // Déclaration et Gestion des Manques de Séquence
                    // Création des séquences Manquantes
                    string[] strManquants = tbManqueSequences.Text.Split(',');
                    foreach (var mq in strManquants)
                    {
                        int numOrdre = 0;
                        if (Int32.TryParse(mq, out numOrdre))
                        {
                            using (var ct = new DocumatContext())
                            {
                                Models.ManquantSequence manquantSequence = new ManquantSequence()
                                {
                                    IdImage = CurrentImage.ImageID,
                                    IdSequence = null,
                                    NumeroOrdre = numOrdre,
                                    statutManquant = 1,
                                    DateDeclareManquant = DateTime.Now,
                                    DateCreation = DateTime.Now,
                                    DateModif = DateTime.Now,
                                };
                                ct.ManquantSequences.Add(manquantSequence);
                                ct.SaveChanges();
                                Image_Index_is_reject = true;
                            }
                        }
                    }
                    #endregion

                    #region VERFIFICATION DE L'IMAGE 
                    // Enregistrement de l'image !!! 
                    int RejetImage = 0, ASupprimer = 0, StatutImage = 0, RejetScan = 0;
                    string MotifRejet = "";
                    if (cbxRejetImage.IsChecked == true)
                    {
                        RejetImage = 1;
                        MotifRejet = cbRejetImage.Text;
                        Image_Index_is_reject = true;
                    }

                    if (cbxRejetScan.IsChecked == true)
                    {
                        RejetScan = 1;
                        Image_Index_is_reject = true;
                    }

                    if (cbxSupprimerImage.IsChecked == true)
                    {
                        ASupprimer = 1;
                        Image_Index_is_reject = true;
                    }

                    if (Image_Index_is_reject)
                    {
                        StatutImage = 1;
                    }

                    using (var ct = new DocumatContext())
                    {
                        // Création d'un controle d'image avec un statut StatutImage
                        Models.Controle controle = new Models.Controle()
                        {
                            RegistreId = RegistreParent.RegistreID,
                            ImageID = CurrentImage.ImageID,
                            SequenceID = null,

                            //Indexes Image mis à null
                            RejetImage_idx = RejetImage,
                            RejetImage_scan = RejetScan,
                            MotifRejetImage_idx = MotifRejet,
                            ASupprimer = ASupprimer,

                            //Indexes de la séquence de l'image
                            OrdreSequence_idx = null,
                            DateSequence_idx = null,
                            RefSequence_idx = null,
                            RefRejetees_idx = null,

                            DateControle = DateTime.Now,
                            DateCreation = DateTime.Now,
                            DateModif = DateTime.Now,
                            PhaseControle = 1,
                            StatutControle = StatutImage,
                        };
                        ct.Controle.Add(controle);

                        if (Image_Index_is_reject)
                        {
                            // Création d'une Correction pour l'image rejetée
                            Models.Correction correction = new Models.Correction()
                            {
                                RegistreId = RegistreParent.RegistreID,
                                ImageID = CurrentImage.ImageID,
                                SequenceID = null,

                                //Indexes Image mis à null
                                RejetImage_idx = RejetImage,
                                RejetImage_scan = RejetScan,
                                MotifRejetImage_idx = MotifRejet,
                                ASupprimer = ASupprimer,

                                //Indexes de la séquence de l'image
                                OrdreSequence_idx = null,
                                DateSequence_idx = null,
                                RefSequence_idx = null,
                                RefRejetees_idx = null,

                                DateCorrection = DateTime.Now,
                                DateCreation = DateTime.Now,
                                DateModif = DateTime.Now,
                                PhaseCorrection = 1,
                                StatutCorrection = StatutImage,
                            };
                            ct.Correction.Add(correction);
                        }

                        // Changement du Statut du de L'image
                        Models.StatutImage statutActuel = ct.StatutImage.FirstOrDefault(s => s.ImageID == CurrentImage.ImageID && s.Code == (int)Enumeration.Image.INDEXEE);
                        if(statutActuel != null)
                        {
                            statutActuel.DateModif = DateTime.Now;
                            statutActuel.DateFin = DateTime.Now;
                        }

                        Models.StatutImage NouvauStatut = new StatutImage()
                        {
                            ImageID = CurrentImage.ImageID,
                            Code = (int)Enumeration.Image.PHASE1,
                            DateDebut = DateTime.Now,
                            DateCreation = DateTime.Now,
                            DateModif = DateTime.Now,
                        };
                        ct.StatutImage.Add(NouvauStatut);

                        // Changement du statut actuel de l'image
                        Models.Image image = ct.Image.FirstOrDefault(i => i.ImageID == CurrentImage.ImageID);
                        if(image != null)
                        {
                            image.StatutActuel = (int)Enumeration.Image.PHASE1;
                        }
                        ct.SaveChanges();

                        //Enregistrement de la tâche
                        DocumatContext.AddTraitement(DocumatContext.TbImage, image.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION, "CONTROLE PH1 : CONTROLE IMAGE : " + image.NumeroPage + " DU REGISTRE ID N° " + RegistreParent.RegistreID);

                        #region Actualisation instantanée de l'icon de L'Abo
                        //Actualisation de l'icon dans l'aborescence
                        if (StatutImage == 1)
                        {
                            setActiveAboImageIcon(CurrentImage.NumeroPage, "instance");
                        }
                        else
                        {
                            setActiveAboImageIcon(CurrentImage.NumeroPage, "valide");
                        }
                        #endregion

                        // Chanrgement des indicateurs du header
                        HeaderInfosGetter();

                        checksControleEnd();

                        // Charger l'élément suivant 
                        this.btnSuivant_Click(sender, e);
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void CheckBoxReferences_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cbxReference = (CheckBox)sender;
                SequenceView sequenceView = (SequenceView)dgSequence.SelectedItem;
                if (dgSequence.SelectedItems.Count == 1)
                {
                    if (cbxReference.IsChecked == true)
                    {
                        SequenceView newseq = new SequenceView();
                        ListeSequences.TryGetValue(sequenceView.Sequence.SequenceID, out newseq);
                        ListeSequences.Remove(sequenceView.Sequence.SequenceID);
                        newseq.References_Is_Check = true;
                        newseq.ListeRefecrences_check.Add(cbxReference.Content.ToString(), cbxReference.Content.ToString());
                        ListeSequences.Add(sequenceView.Sequence.SequenceID, newseq);
                    }
                    else
                    {
                        SequenceView newseq = new SequenceView();
                        ListeSequences.TryGetValue(sequenceView.Sequence.SequenceID, out newseq);
                        ListeSequences.Remove(sequenceView.Sequence.SequenceID);
                        newseq.References_Is_Check = false;
                        newseq.ListeRefecrences_check.Remove(cbxReference.Content.ToString());
                        ListeSequences.Add(sequenceView.Sequence.SequenceID, newseq);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void cbxNumOrdre_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cbxOrdre = (CheckBox)sender;
            SequenceView sequenceView = (SequenceView)dgSequence.SelectedItem;
            if (dgSequence.SelectedItems.Count == 1)
            {
                if (cbxOrdre.IsChecked == true)
                {
                    SequenceView newseq = new SequenceView();
                    ListeSequences.TryGetValue(sequenceView.Sequence.SequenceID, out newseq);
                    ListeSequences.Remove(sequenceView.Sequence.SequenceID);
                    newseq.Ordre_Is_Check = true;
                    ListeSequences.Add(sequenceView.Sequence.SequenceID, newseq);
                }
                else
                {
                    SequenceView newseq = new SequenceView();
                    ListeSequences.TryGetValue(sequenceView.Sequence.SequenceID, out newseq);
                    ListeSequences.Remove(sequenceView.Sequence.SequenceID);
                    newseq.Ordre_Is_Check = false;
                    ListeSequences.Add(sequenceView.Sequence.SequenceID, newseq);
                }
            }
        }

        private void cbxDate_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cbxOrdre = (CheckBox)sender;
            SequenceView sequenceView = (SequenceView)dgSequence.SelectedItem;
            if (dgSequence.SelectedItems.Count == 1)
            {
                if (cbxOrdre.IsChecked == true)
                {
                    SequenceView newseq = new SequenceView();
                    ListeSequences.TryGetValue(sequenceView.Sequence.SequenceID, out newseq);
                    ListeSequences.Remove(sequenceView.Sequence.SequenceID);
                    newseq.Date_Is_Check = true;
                    ListeSequences.Add(sequenceView.Sequence.SequenceID, newseq);
                }
                else
                {
                    SequenceView newseq = new SequenceView();
                    ListeSequences.TryGetValue(sequenceView.Sequence.SequenceID, out newseq);
                    ListeSequences.Remove(sequenceView.Sequence.SequenceID);
                    newseq.Date_Is_Check = false;
                    ListeSequences.Add(sequenceView.Sequence.SequenceID, newseq);
                }
            }
        }

        private void BtnAnnuleManqueSequences_Click(object sender, RoutedEventArgs e)
        {
            tbManqueSequences.Text = "";
        }

        private void SuppSequence_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cbxOrdre = (CheckBox)sender;
            SequenceView sequenceView = (SequenceView)dgSequence.SelectedItem;
            if (dgSequence.SelectedItems.Count == 1)
            {
                if (cbxOrdre.IsChecked == true)
                {
                    SequenceView newseq = new SequenceView();
                    ListeSequences.TryGetValue(sequenceView.Sequence.SequenceID, out newseq);
                    ListeSequences.Remove(sequenceView.Sequence.SequenceID);
                    newseq.ASupprimer_Is_Check = true;
                    ListeSequences.Add(sequenceView.Sequence.SequenceID, newseq);
                }
                else
                {
                    SequenceView newseq = new SequenceView();
                    ListeSequences.TryGetValue(sequenceView.Sequence.SequenceID, out newseq);
                    ListeSequences.Remove(sequenceView.Sequence.SequenceID);
                    newseq.ASupprimer_Is_Check = false;
                    ListeSequences.Add(sequenceView.Sequence.SequenceID, newseq);
                }
            }
        }

        private void btnValideControle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var ct = new DocumatContext())
                {
                    ImageView imageView1 = new ImageView();
                    //Si toute les images sont marqué en phase 1 
                    if (!ct.Image.Any(i => i.StatutActuel == (int)Enumeration.Image.INDEXEE && i.RegistreID == RegistreParent.RegistreID))
                    {
                        //On demande une autorisation
                        if (MessageBox.Show("Voulez vous terminer le contrôle de ce registre ?", "QUESTION", MessageBoxButton.YesNo, MessageBoxImage.Question)
                            == MessageBoxResult.Yes)
                        {
                            int Registre_is_check = 0;
                            // Cas où un index au moins à été réjété
                            if (ct.Controle.Any(c => c.RegistreId == RegistreParent.RegistreID && c.PhaseControle == 1 && c.StatutControle == 1))
                            {
                                Registre_is_check = 1;
                            }

                            //Cas où il y'a un manquant
                            if (ct.ManquantImage.Any(m => m.IdRegistre == RegistreParent.RegistreID && m.statutManquant == 1))
                            {
                                Registre_is_check = 1;
                            }

                            // Récupération du controle registre
                            if (Registre_is_check == 1)
                            {
                                Models.Controle controleRegistre = ct.Controle.FirstOrDefault(c => c.RegistreId == RegistreParent.RegistreID
                                                                                            && c.ImageID == null && c.SequenceID == null && c.PhaseControle == 1 && c.StatutControle == 1);
                                if (controleRegistre == null)
                                {
                                    controleRegistre = ct.Controle.FirstOrDefault(c => c.RegistreId == RegistreParent.RegistreID
                                                                                            && c.ImageID == null && c.SequenceID == null && c.PhaseControle == 1 && c.StatutControle == 0);
                                    if (controleRegistre != null)
                                    {
                                        controleRegistre.StatutControle = 1;
                                        controleRegistre.DateModif = DateTime.Now;

                                        // Création de la correction 
                                        Models.Correction correction = new Models.Correction()
                                        {
                                            RegistreId = RegistreParent.RegistreID,
                                            ImageID = null,
                                            SequenceID = null,

                                            // index Registre mis à null
                                            Numero_idx = null,
                                            DateDepotDebut_idx = null,
                                            DateDepotFin_idx = null,
                                            NumeroDebut_idx = null,
                                            NumeroDepotFin_idx = null,
                                            NombrePage_idx = null,

                                            //Indexes Image mis à null
                                            RejetImage_idx = null,
                                            RejetImage_scan = null,
                                            MotifRejetImage_idx = null,
                                            ASupprimer = null,

                                            //Indexes de la séquence de l'image
                                            OrdreSequence_idx = null,
                                            DateSequence_idx = null,
                                            RefSequence_idx = null,
                                            RefRejetees_idx = null,

                                            DateCorrection = DateTime.Now,
                                            DateCreation = DateTime.Now,
                                            DateModif = DateTime.Now,
                                            PhaseCorrection = 1,
                                            StatutCorrection = Registre_is_check,
                                        };
                                        ct.Correction.Add(correction);
                                        ct.SaveChanges();
                                    }
                                }
                            }

                            // Changement du statut du registre 
                            // Récupération de l'ancien statut 
                            Models.StatutRegistre StatutActu = ct.StatutRegistre.FirstOrDefault(s => s.RegistreID == RegistreParent.RegistreID
                                                                                                && s.Code == (int)Enumeration.Registre.INDEXE);
                            if (StatutActu != null)
                            {
                                StatutActu.DateFin = DateTime.Now;
                                StatutActu.DateModif = DateTime.Now;
                            }

                            Models.StatutRegistre NewStatut = new StatutRegistre()
                            {
                                RegistreID = RegistreParent.RegistreID,
                                Code = (int)Enumeration.Registre.PHASE1,
                                DateDebut = DateTime.Now,
                                DateCreation = DateTime.Now,
                                DateModif = DateTime.Now,
                            };
                            ct.StatutRegistre.Add(NewStatut);

                            //On peut Récupérer et changer le statutActuel du registre 
                            Models.Registre registre = ct.Registre.FirstOrDefault(r => r.RegistreID == RegistreParent.RegistreID);
                            registre.StatutActuel = (int)Enumeration.Registre.PHASE1;
                            registre.DateModif = DateTime.Now;
                            ct.SaveChanges();

                            //Enregistrement de la tâche
                            DocumatContext.AddTraitement(DocumatContext.TbRegistre, RegistreParent.RegistreID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.CONTROLE_PH1_TERMINE, "FIN CONTROLE PH1 DU REGISTRE ID N° " + RegistreParent.RegistreID);

                            // Faire une Action de rafraichissement du  tableau de registre avant la Sortie
                            this.Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Une page semble ne pas avoir subit de Contrôle, Veuillez contrôler cette page avant de valider !!","Image(s) non Contrôlée(s)", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        btnValideControle.Visibility = Visibility.Collapsed;
                    } 
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void btnSuivant_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var ct = new DocumatContext())
                {
                    //on vérifie que tous t'es ajouté dans la BD;
                    int nbImage = ct.Image.Count(i=>i.RegistreID == RegistreParent.RegistreID);

                    if ((currentImage + 2) == nbImage)
                    {
                        currentImage = 1;
                    }
                    else if ((currentImage + 2) < nbImage)
                    {
                        currentImage++;
                    }

                    ChargerImage(currentImage);
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void cmBtnDeclareManquant_Click(object sender, RoutedEventArgs e)
        {
            FormManquantImage formManquantImage = new FormManquantImage(this);
            formManquantImage.Show();
        }

        private void tbNumeroImageManquant_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex regex = new Regex("^[0-9]{1,}$");
            TextBox textBox = ((TextBox)sender);
            string text = textBox.Text;
            if (text.Length > 0)
            {
                if (!regex.IsMatch(text.Substring(text.Length - 1)))
                {
                    textBox.Text = textBox.Text.Remove(text.Length - 1);
                    textBox.CaretIndex = text.Length;
                }
            }
        }

        private void tbDebutSequenceManquant_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex regex = new Regex("^[0-9]{1,}$");
            TextBox textBox = ((TextBox)sender);
            string text = textBox.Text;
            if (text.Length > 0)
            {
                if (!regex.IsMatch(text.Substring(text.Length - 1)))
                {
                    textBox.Text = textBox.Text.Remove(text.Length - 1);
                    textBox.CaretIndex = text.Length;
                }
            }
        }

        private void tbFinSequenceManquant_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex regex = new Regex("^[0-9]{1,}$");
            TextBox textBox = ((TextBox)sender);
            string text = textBox.Text;
            if (text.Length > 0)
            {
                if (!regex.IsMatch(text.Substring(text.Length - 1)))
                {
                    textBox.Text = textBox.Text.Remove(text.Length - 1);
                    textBox.CaretIndex = text.Length;
                }
            }
        }

        private void BtnTerminerControlePageDeGarde_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Voulez vous terminer le contrôle de Page de Garde et des Index du Registres ?", "Terminer Le Contrôle ?", MessageBoxButton.YesNo, MessageBoxImage.Question)
                    == MessageBoxResult.Yes)
                {
                    //Booléen d'identification de rejet sur un index
                    bool Image_Index_is_reject = false;

                    #region VERFIFICATION DE L'IMAGE 
                    // Enregistrement de l'image !!! 
                    int RejetImage = 0, ASupprimer = 0, StatutImage = 0, RejetScan = 0;
                    string MotifRejet = "";
                    if (cbxRejetImage.IsChecked == true)
                    {
                        RejetImage = 1;
                        MotifRejet = cbRejetImage.Text;
                        Image_Index_is_reject = true;
                    }

                    if (cbxRejetScan.IsChecked == true)
                    {
                        RejetScan = 1;
                        Image_Index_is_reject = true;
                    }

                    if (cbxSupprimerImage.IsChecked == true)
                    {
                        ASupprimer = 1;
                        Image_Index_is_reject = true;
                    }

                    if (Image_Index_is_reject)
                    {
                        StatutImage = 1;
                    }

                    using (var ct = new DocumatContext())
                    {
                        // Création d'un controle d'image avec un statut StatutImage
                        Models.Controle controle = new Models.Controle()
                        {
                            RegistreId = RegistreParent.RegistreID,
                            ImageID = CurrentImage.ImageID,
                            SequenceID = null,

                            //Indexes Image mis à null
                            RejetImage_idx = RejetImage,
                            RejetImage_scan = RejetScan,
                            MotifRejetImage_idx = MotifRejet,
                            ASupprimer = ASupprimer,

                            //Indexes de la séquence de l'image
                            OrdreSequence_idx = null,
                            DateSequence_idx = null,
                            RefSequence_idx = null,
                            RefRejetees_idx = null,

                            DateControle = DateTime.Now,
                            DateCreation = DateTime.Now,
                            DateModif = DateTime.Now,
                            PhaseControle = 1,
                            StatutControle = StatutImage,
                        };
                        ct.Controle.Add(controle);

                        if (Image_Index_is_reject)
                        {
                            // Création d'une Correction pour l'image rejetée
                            Models.Correction correction = new Models.Correction()
                            {
                                RegistreId = RegistreParent.RegistreID,
                                ImageID = CurrentImage.ImageID,
                                SequenceID = null,

                                //Indexes Image mis à null
                                RejetImage_idx = RejetImage,
                                RejetImage_scan = RejetScan,
                                MotifRejetImage_idx = MotifRejet,
                                ASupprimer = ASupprimer,

                                //Indexes de la séquence de l'image
                                OrdreSequence_idx = null,
                                DateSequence_idx = null,
                                RefSequence_idx = null,
                                RefRejetees_idx = null,

                                DateCorrection = DateTime.Now,
                                DateCreation = DateTime.Now,
                                DateModif = DateTime.Now,
                                PhaseCorrection = 1,
                                StatutCorrection = StatutImage,
                            };
                            ct.Correction.Add(correction);
                        }

                        // Changement du Statut du de L'image
                        Models.StatutImage statutActuel = ct.StatutImage.FirstOrDefault(s => s.ImageID == CurrentImage.ImageID && s.Code == (int)Enumeration.Image.INDEXEE);
                        if(statutActuel != null)
                        {
                            statutActuel.DateModif = DateTime.Now;
                            statutActuel.DateFin = DateTime.Now;
                        }

                        Models.StatutImage NouvauStatut = new StatutImage()
                        {
                            ImageID = CurrentImage.ImageID,
                            Code = (int)Enumeration.Image.PHASE1,
                            DateDebut = DateTime.Now,
                            DateCreation = DateTime.Now,
                            DateModif = DateTime.Now,
                        };
                        ct.StatutImage.Add(NouvauStatut);

                        // Changement du statut actuel de l'image
                        Models.Image image = ct.Image.FirstOrDefault(i => i.ImageID == CurrentImage.ImageID);
                        if(image != null)
                        {
                            image.StatutActuel = (int)Enumeration.Image.PHASE1;
                        }
                        ct.SaveChanges();

                        //Enregistrement de la tâche
                        DocumatContext.AddTraitement(DocumatContext.TbImage, image.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION, "#IMAGE_CONTROLE");

                        //Contrôle des rejets index du Registre 
                        int StatutRegistre = 0, NumVolume = 0, NumDepotDebut = 0, DateDepotDebut = 0,
                            NumDepotFin = 0, DateDepotFin = 0, NbPage = 0;

                        if (cbVolume.IsChecked == true)
                        {
                            NumVolume = 1;
                            StatutRegistre = 1;
                        }
                        if (cbNumDepotDebut.IsChecked == true)
                        {
                            NumDepotDebut = 1;
                            StatutRegistre = 1;
                        }
                        if (cbDateDepotDebut.IsChecked == true)
                        {
                            DateDepotDebut = 1;
                            StatutRegistre = 1;
                        }
                        if (cbNumDepotFin.IsChecked == true)
                        {
                            NumDepotFin = 1;
                            StatutRegistre = 1;
                        }
                        if (cbDateDepotFin.IsChecked == true)
                        {
                            DateDepotFin = 1;
                            StatutRegistre = 1;
                        }
                        if (cbNbPage.IsChecked == true)
                        {
                            NbPage = 1;
                            StatutRegistre = 1;
                        }

                        //Création du controle de registre 
                        Models.Controle controleRegistre = new Models.Controle()
                        {
                            RegistreId = RegistreParent.RegistreID,
                            ImageID = null,
                            SequenceID = null,

                            // Index du registre 
                            Numero_idx = NumVolume,
                            DateDepotDebut_idx = DateDepotDebut,
                            DateDepotFin_idx = DateDepotFin,
                            NumeroDebut_idx = NumDepotDebut,
                            NumeroDepotFin_idx = NumDepotFin,
                            NombrePage_idx = NbPage,

                            //Indexes Image mis à null
                            RejetImage_idx = null,
                            RejetImage_scan = null,
                            MotifRejetImage_idx = null,
                            ASupprimer = null,

                            //Indexes de la séquence de l'image
                            OrdreSequence_idx = null,
                            DateSequence_idx = null,
                            RefSequence_idx = null,
                            RefRejetees_idx = null,

                            DateControle = DateTime.Now,
                            DateCreation = DateTime.Now,
                            DateModif = DateTime.Now,
                            PhaseControle = 1,
                            StatutControle = StatutRegistre,
                        };
                        ct.Controle.Add(controleRegistre);

                        //Création de la correction au cas où le registre est réjété
                        if (StatutRegistre == 1)
                        {
                            Models.Correction correctionRegistre = new Models.Correction()
                            {
                                RegistreId = RegistreParent.RegistreID,
                                ImageID = null,
                                SequenceID = null,

                                // Index du registre 
                                Numero_idx = NumVolume,
                                DateDepotDebut_idx = DateDepotDebut,
                                DateDepotFin_idx = DateDepotFin,
                                NumeroDebut_idx = NumDepotDebut,
                                NumeroDepotFin_idx = NumDepotFin,
                                NombrePage_idx = NbPage,

                                //Indexes Image mis à null
                                RejetImage_idx = null,
                                RejetImage_scan = null,
                                MotifRejetImage_idx = null,
                                ASupprimer = null,

                                //Indexes de la séquence de l'image
                                OrdreSequence_idx = null,
                                DateSequence_idx = null,
                                RefSequence_idx = null,
                                RefRejetees_idx = null,

                                DateCorrection = DateTime.Now,
                                DateCreation = DateTime.Now,
                                DateModif = DateTime.Now,
                                PhaseCorrection = 1,
                                StatutCorrection = StatutRegistre,
                            };
                            ct.Correction.Add(correctionRegistre);
                        }
                        ct.SaveChanges();

                        //Enregistrement de la tâche
                        DocumatContext.AddTraitement(DocumatContext.TbRegistre, RegistreParent.RegistreID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION, "#INDEX_REGISTRE_CONTROLE");
                        
                        #region Actualisation instantanée de l'icon de L'Abo
                        //Actualisation de l'icon dans l'aborescence
                        if (StatutImage == 1)
                        {
                            setActiveAboImageIcon(CurrentImage.NumeroPage, "instance");
                        }
                        else
                        {
                            setActiveAboImageIcon(CurrentImage.NumeroPage, "valide");
                        }
                        #endregion

                        // Chanrgement des indicateurs du header
                        HeaderInfosGetter();

                        checksControleEnd();

                        // Charger l'élément suivant 
                        this.btnSuivant_Click(sender, e);
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Mettre à Jour la taille de la visionneuse
            gripDgSequence.MaxHeight = dgSequence.MaxHeight = BorderControle.ActualHeight - 200;
        }

        private void cbxRejetScan_Checked(object sender, RoutedEventArgs e)
        {
            cbxRejetImage.IsChecked = false;
            cbxSupprimerImage.IsChecked = false;
        }
    }
}
