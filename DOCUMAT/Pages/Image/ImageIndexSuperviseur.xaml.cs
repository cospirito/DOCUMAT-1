using DOCUMAT.Models;
using DOCUMAT.Pages.Registre;
using DOCUMAT.ViewModels;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brushes = System.Windows.Media.Brushes;

namespace DOCUMAT.Pages.Image
{
    /// <summary>
    /// Logique d'interaction pour ImageIndexSuperviseur.xaml
    /// </summary>
    public partial class ImageIndexSuperviseur : Window
    {

        #region  ATTRIBUTES
        Indexation.Indexation MainParent;
        Models.Registre RegistreParent;
        ImageView CurrentImageView;
        private int currentImage = 1;
#pragma warning disable CS0414 // Le champ 'ImageIndexSuperviseur.dgSequenceMaxHeight' est assigné, mais sa valeur n'est jamais utilisée
        private int dgSequenceMaxHeight = 400;
#pragma warning restore CS0414 // Le champ 'ImageIndexSuperviseur.dgSequenceMaxHeight' est assigné, mais sa valeur n'est jamais utilisée
#pragma warning disable CS0414 // Le champ 'ImageIndexSuperviseur.dgSequenceMinHeight' est assigné, mais sa valeur n'est jamais utilisée
        private int dgSequenceMinHeight = 300;
#pragma warning restore CS0414 // Le champ 'ImageIndexSuperviseur.dgSequenceMinHeight' est assigné, mais sa valeur n'est jamais utilisée

        // Définition de l'aborescence
        TreeViewItem registreAbre = new TreeViewItem();
        //Fichier d'image dans disque dure
        List<FileInfo> fileInfos = new List<FileInfo>();
        // Dossier racine 
        string DossierRacine = ConfigurationManager.AppSettings["CheminDossier_Scan"];
        // Fichier Image upload
        string FichierImporte = "";
        //Initiale des références par type de registre R3 = R / R4 = T
#pragma warning disable CS0414 // Le champ 'ImageIndexSuperviseur.RefInitiale' est assigné, mais sa valeur n'est jamais utilisée
        public string RefInitiale = "R";
#pragma warning restore CS0414 // Le champ 'ImageIndexSuperviseur.RefInitiale' est assigné, mais sa valeur n'est jamais utilisée
        //Liste des possibles references		
        Dictionary<String, String> References = new Dictionary<String, String>();
        // Sequence en Mode edition ou ajout
        bool editSequence = false;
        public bool AllisLoad = false;
        #endregion

        #region FONCTIONS			
        private void ForceNombre(object sender)
        {
            Regex regex = new Regex("^[0-9-]{1,}$");
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

        public static string GetFileFolderName(string path)
        {
            // If we have no path, return empty
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            // Make all slashes back slashes
            var normalizedPath = path.Replace('/', '\\');

            // Find the last backslash in the path
            var lastIndex = normalizedPath.LastIndexOf('\\');

            // If we don't find a backslash, return the path itself
            if (lastIndex <= 0)
                return path;

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

        private void setActiveAborescenceImage()
        {
            try
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
                //Remplissage des éléments de la vue
                //Obtention de la liste des images du registre dans la base de données 
                ImageView imageView1 = new ImageView();
                List<ImageView> imageViews = imageView1.GetSimpleViewsList(RegistreParent);
                if (!string.IsNullOrEmpty(MainParent.Utilisateur.CheminPhoto))
                {
                    AgentImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(DossierRacine, MainParent.Utilisateur.CheminPhoto)), new System.Net.Cache.RequestCachePolicy());
                }
                // Agent Superviseur / Administrateur
                tbxAgentLogin.Text = "Login : " + MainParent.Utilisateur.Login;
                tbxAgentNoms.Text = "Noms : " + MainParent.Utilisateur.Noms;
                tbxAgentType.Text = "Aff : " + Enum.GetName(typeof(Enumeration.AffectationAgent), MainParent.Utilisateur.Affectation);

                using (var ct = new DocumatContext())
                {
                    // Récupération de l'agent en charge de l'indexation 
                    Models.Traitement traitementAttrRegistre = ct.Traitement.FirstOrDefault(t => t.TableSelect == DocumatContext.TbRegistre && t.TableID == RegistreParent.RegistreID
                                        && t.TypeTraitement == (int)Enumeration.TypeTraitement.REGISTRE_ATTRIBUE_INDEXATION);
                    if (traitementAttrRegistre != null)
                    {
                        Models.Agent agentAttrRegistre = ct.Agent.FirstOrDefault(a => a.AgentID == traitementAttrRegistre.AgentID);
                        // Agent Indexeur
                        if (!string.IsNullOrEmpty(agentAttrRegistre.CheminPhoto))
                        {
                            AgentIndexImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(DossierRacine, agentAttrRegistre.CheminPhoto)), new System.Net.Cache.RequestCachePolicy());
                        }
                        tbxAgentIndexLogin.Text = "Login : " + agentAttrRegistre.Login;
                        tbxAgentIndexNoms.Text = "Noms : " + agentAttrRegistre.Noms;
                        tbxAgentIndexType.Text = "Aff : " + Enum.GetName(typeof(Enumeration.AffectationAgent), agentAttrRegistre.Affectation);
                    } 
                }

                tbxQrCode.Text = "Qrcode : " + RegistreParent.QrCode; using (var ct = new DocumatContext())
                {
                    Models.Versement versement = ct.Versement.FirstOrDefault(v => v.VersementID == RegistreParent.VersementID);
                    Livraison livraison = ct.Livraison.FirstOrDefault(l => l.LivraisonID == versement.LivraisonID);
                    tbxService.Text = "Service : " + ct.Service.FirstOrDefault(s => s.ServiceID == livraison.ServiceID).NomComplet;
                    tbxVersement.Text = "N° Versement : " + versement.NumeroVers.ToString();
                }

                tbxNumVolume.Text = "N° Volume : " + RegistreParent.Numero;
                tbxTypeRegistre.Text = "Type : " + RegistreParent.Type;
                tbxDepotFin.Text = "N° Dépôt Fin. : " + RegistreParent.NumeroDepotFin;

                double perTerminer = (((float)imageViews.Where(i => i.Image.StatutActuel == (int)Enumeration.Image.INDEXEE).Count() / (float)imageViews.Count()) * 100);
                string Instance = (Math.Round(((float)imageViews.Where(i => i.Image.StatutActuel == (int)Enumeration.Image.INSTANCE).Count() / (float)imageViews.Count()) * 100, 1)).ToString() + " %";
                string EnCours = (Math.Round(((float)imageViews.Where(i => i.Image.StatutActuel == (int)Enumeration.Image.CREEE).Count() / (float)imageViews.Count()) * 100, 1)).ToString() + " %";
                string Scanne = (Math.Round(((float)imageViews.Where(i => i.Image.StatutActuel == (int)Enumeration.Image.SCANNEE).Count() / (float)imageViews.Count()) * 100, 1)).ToString() + " %";
                tbxImageTerminer.Text = "Terminé : " + Math.Round(perTerminer, 1) + " %";
                tbxImageInstance.Text = "En Instance :" + Instance;
                tbxImageEnCours.Text = "Non Traité : " + EnCours;
                ArcIndicator.EndAngle = (perTerminer * 360) / 100;
                TextIndicator.Text = Math.Round(perTerminer, 1) + "%";
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
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
                ImageView imageView1 = new ImageView();
                List<ImageView> imageViews = imageView1.GetSimpleViewsList(RegistreParent);

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

                        if (imageViews.FirstOrDefault(i => i.Image.NumeroPage == NumeroAbr
                            && i.Image.StatutActuel == (int)Enumeration.Image.INDEXEE) != null)
                        {
                            item.Tag = "valide";
                        }
                        else if (imageViews.FirstOrDefault(i => i.Image.NumeroPage == NumeroAbr && i.Image.StatutActuel == (int)Enumeration.Image.INSTANCE) != null)
                        {
                            if (imageViews.FirstOrDefault(i => i.Image.NumeroPage == NumeroAbr
                                    && i.Image.StatutActuel == (int)Enumeration.Image.INSTANCE
                                    && i.Image.typeInstance.ToLower().Trim() == "image manquante") != null)
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
                            if (imageViews.OrderBy(i => i.Image.NumeroPage).FirstOrDefault(i => i.Image.StatutActuel == (int)Enumeration.Image.CREEE) != null)
                            {
                                image = imageViews.OrderBy(i => i.Image.NumeroPage).FirstOrDefault(i => i.Image.StatutActuel == (int)Enumeration.Image.CREEE).Image;
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

                foreach (var image in imageViews.Where(i => i.Image.StatutActuel == (int)Enumeration.Image.INSTANCE && i.Image.typeInstance.ToLower().Trim() == "image manquante"))
                {
                    if (!registreAbre.Items.Cast<TreeViewItem>().Any(tr => tr.Header.ToString().ToLower() == GetFileFolderName(image.Image.CheminImage).ToLower()))
                    {
                        TreeViewItem item = new TreeViewItem();
                        item.Header = GetFileFolderName(image.Image.CheminImage);
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

        public ImageIndexSuperviseur()
        {
            InitializeComponent();

            // Attribution du context de dgSequence
            ContextMenu Cm = (ContextMenu)FindResource("cmSequence");
            dgSequence.ContextMenu = Cm;

            // Attribution du Context Menu de l'Aborescence 
            FolderView.ContextMenu = (ContextMenu)FindResource("cmAbo");
        }

        public ImageIndexSuperviseur(Models.Registre registre, Indexation.Indexation indexation) : this()
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
                List<ImageView> imageViews = imageView1.GetSimpleViewsList(RegistreParent);
                imageView1 = imageViews.FirstOrDefault(i => i.Image.NumeroPage == currentImage);
                if (imageView1 != null)
                {
                    #region CHARGEMENT DE L'IMAGE SCANNEE
                    // Chargement de la visionneuse
                    if (!string.IsNullOrWhiteSpace(imageView1.Image.CheminImage))
                    {
                        if (File.Exists(Path.Combine(DossierRacine, imageView1.Image.CheminImage)))
                        {
                            viewImage(Path.Combine(DossierRacine, imageView1.Image.CheminImage));
                        }
                        else
                        {
                            MessageBox.Show("La page : \"" + imageView1.Image.NomPage + "\" est introuvable dans le Dossier de Scan !!!", "AVERTISSEMENT", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Le chemin de l'image de la page : " + imageView1.Image.NomPage + " est null", "Chemin image Introuvable", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                        tbPageName.Visibility = Visibility.Collapsed;
                    }
                    else if (imageView1.Image.NumeroPage == 0)
                    {
                        tbxNomPage.Text = ConfigurationManager.AppSettings["Nom_Page_Ouverture"];
                        tbxNumeroPage.Text = "";
                        tbxDebSeq.Text = "";
                        tbxFinSeq.Text = "";
                        tbxPagePrecName.Text = "";
                        tbPageName.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        tbxNomPage.Text = "PAGE : " + imageView1.Image.NomPage;
                        tbxNumeroPage.Text = "N° " + imageView1.Image.NumeroPage.ToString() + " / " + imageViews.Where(iv => iv.Image.NumeroPage != -1 && iv.Image.NumeroPage != 0).Count();
                        tbxDebSeq.Text = ((imageView1.Image.DebutSequence == 0) ? "" : "N° Debut : " + imageView1.Image.DebutSequence.ToString());
                        tbxFinSeq.Text = ((imageView1.Image.FinSequence == 0) ? "" : imageView1.Image.FinSequence.ToString());
                        if (imageViews.FirstOrDefault(iv => iv.Image.NumeroPage == imageView1.Image.NumeroPage - 1) != null)
                        {
                            tbxPagePrecName.Text = "P.PREC : " + imageViews.FirstOrDefault(iv => iv.Image.NumeroPage == imageView1.Image.NumeroPage - 1).Image.NomPage + " / P.ACT : ";
                        }
                        tbPageName.Text = imageView1.Image.NomPage;
                        tbPageName.Visibility = Visibility.Visible;
                    }
                    #endregion

                    #region RECUPERATION DES SEQUENCES DE L'IMAGE
                    using (var ct = new DocumatContext())
                    {
                        // Récupération des sequences déja renseignées, Différent des images préindexer ayant la référence défaut
                        List<Sequence> sequences = ct.Sequence.Where(s => s.ImageID == imageView1.Image.ImageID).OrderBy(s => s.NUmeroOdre).ToList();
                        dgSequence.ItemsSource = SequenceView.GetViewsList(sequences);
                        if (sequences.Count != 0)
                        {
                            dgSequence.ScrollIntoView(dgSequence.Items.GetItemAt(dgSequence.Items.Count - 1));
                        }

                        //Vide les champs du formulaire de séquence 
                        ChampsSequence.Visibility = Visibility.Collapsed;
                        tbNumeroOrdreSequence.Text = "";
                        tbIsSpecial.SelectedIndex = 0;
                        tbDateSequence.Text = "";
                        tbReference.Text = "";
                        tbNbRefPreindex.Text = "";
                        dgSequence.IsEnabled = true;
                        editSequence = false;

                        // Définition des champs de modification de l'image 
                        tbNumeroPage.Text = CurrentImageView.Image.NumeroPage.ToString();
                        tbNumeroDebutSequence.Text = CurrentImageView.Image.DebutSequence.ToString();
                        tbFinSequence.Text = CurrentImageView.Image.FinSequence.ToString();
                        tbDateDebSequence.Text = CurrentImageView.Image.DateDebutSequence.ToShortDateString();
                        FichierImporte = "";
                        BtnImporterImage.Foreground = Brushes.White;
                        BtnValiderModifImage.Foreground = Brushes.White; 
                    }
                    #endregion

                    #region ADAPTATION DE L'AFFICHAGE EN FONCTION DES INSTANCES DE L'IMAGE
                    // Procédure d'affichage lorsque les statuts d'images changes
                    if (imageView1.Image.StatutActuel == (int)Enumeration.Image.INSTANCE)
                    {
                        TbkIndicateurImage.Text = "EN INSTANCE : " + imageView1.Image.typeInstance.ToUpper();
                        tbObservationInstance.Text = imageView1.Image.ObservationInstance;
                        cbxRetirerInstance.IsEnabled = true;
                        cbxRetirerInstance.IsChecked = false;
                        panelChoixInstance.Visibility = Visibility.Collapsed;
                    }
                    else if (imageView1.Image.StatutActuel == (int)Enumeration.Image.CREEE)
                    {
                        TbkIndicateurImage.Text = "EN COURS";
                        cbxRetirerInstance.IsEnabled = false;
                        cbxRetirerInstance.IsChecked = false;
                        panelChoixInstance.Visibility = Visibility.Collapsed;
                    }
                    else if (imageView1.Image.StatutActuel == (int)Enumeration.Image.INDEXEE)
                    {

                        TbkIndicateurImage.Text = "TERMINER";
                        cbxRetirerInstance.IsEnabled = false;
                        cbxRetirerInstance.IsChecked = false;
                        panelChoixInstance.Visibility = Visibility.Collapsed;
                    }
                    #endregion

                    // Actualisation du Selecteur dans l'aborescence
                    setActiveAborescenceImage();
                }
                else
                {
                    #region ENREGISTREMENT D'UNE IMAGE SCANNEE NON REPERTORIE DANS LA BASE DE DONNEES
                    MessageBox.Show("Impossible de charger la Page, Numero : " + currentImage + " , Elle n'est pas enregistrée !!!", "AVERTISSEMENT", MessageBoxButton.OK, MessageBoxImage.Warning);
                    if (MessageBox.Show("Voulez - Vous, enregistrer cette image pour commencer l'indexation ?", "ENREGISTRER L'IMAGE ?", MessageBoxButton.YesNo, MessageBoxImage.Question)
                        == MessageBoxResult.Yes)
                    {
                        if (currentImage == -1)
                        {
                            throw new Exception("Fermez et reouvrez la fenêtre afin de pouvoir enregistrer la page DE GARDE!!!");
                        }

                        if (currentImage == 0)
                        {
                            throw new Exception("Fermez et reouvrez la fenêtre afin de pouvoir enregistrer LA PAGE D'OUVERTURE!!!");
                        }

                        string NomPage = currentImage.ToString();
                        if (currentImage < 10)
                        {
                            NomPage = $"00{currentImage}";
                        }
                        else if (currentImage < 100)
                        {
                            NomPage = $"0{currentImage}";
                        }

                        FileInfo file = fileInfos.FirstOrDefault(f => f.Name.Remove(f.Name.Length - 4) == NomPage);
                        if (file != null)
                        {
                            using (var ct = new DocumatContext())
                            {
                                // Récupération de la date du serveur 
                                DateTime dateServer = ct.Database.SqlQuery<DateTime>("SELECT GETDATE()").FirstOrDefault();

                                Models.Image image = new Models.Image()
                                {
                                    CheminImage = System.IO.Path.Combine(RegistreParent.CheminDossier, file.Name),
                                    Type = file.Extension.Substring(1).ToUpper(),
                                    Taille = file.Length,
                                    DateScan = file.LastWriteTime,
                                    DateModif = dateServer,
                                    DateCreation = dateServer,
                                    StatutActuel = (int)Enumeration.Image.CREEE,
                                    DateDebutSequence = dateServer,
                                    DebutSequence = 0,
                                    FinSequence = 0,
                                    NumeroPage = currentImage,
                                    NomPage = file.Name.Remove(file.Name.Length - file.Extension.Length),
                                    RegistreID = RegistreParent.RegistreID
                                };
                                ct.Image.Add(image);
                                ct.SaveChanges();

                                // Création du nouveau Statut 
                                Models.StatutImage statutImage = new StatutImage();
                                statutImage.ImageID = image.ImageID;
                                statutImage.Code = (int)Enumeration.Image.CREEE;
                                statutImage.DateCreation = statutImage.DateDebut = statutImage.DateModif = dateServer;
                                ct.StatutImage.Add(statutImage);
                                ct.SaveChanges();

                                DocumatContext.AddTraitement(DocumatContext.TbImage, image.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.CREATION,"AJOUT IMAGE");
                                MessageBox.Show("Page Ajoutée !!!", "AJOUT EFFECTUE", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        else
                        {
                            PageImage.Source = null;
                            MessageBox.Show("La page est introuvable dans le Dossier de Scan !!!", "ATTENTION", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
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
                ImageView imageView1 = new ImageView();
                List<ImageView> imageViews = imageView1.GetSimpleViewsList(RegistreParent);

                //Chargement de l'image 
                int numeroPage = 0;

                //Cas d'une page normal (Page numérotée)
                if (Int32.TryParse(treeViewItem.Header.ToString().Remove(treeViewItem.Header.ToString().Length - 4), out numeroPage))
                {
                    currentImage = numeroPage;
                    ChargerImage(currentImage);
                }
                else if (treeViewItem.Header.ToString().Remove(treeViewItem.Header.ToString().Length - 4).ToLower() == ConfigurationManager.AppSettings["Nom_Page_Garde"].ToLower())
                {
                    //Affichage de la page de garde
                    currentImage = -1;
                    imageView1 = imageViews.FirstOrDefault(i => i.Image.NumeroPage == -1);

                    tbxNomPage.Text = ConfigurationManager.AppSettings["Nom_Page_Garde"].ToUpper();
                    tbxNumeroPage.Text = "";
                    tbNumeroOrdreSequence.Text = "";
                    tbDateSequence.Text = "";
                    tbReference.Text = "";
                    dgSequence.ItemsSource = null;
                    // Chargement de l'image		
                    ChargerImage(currentImage);
                }
                else if (treeViewItem.Header.ToString().Remove(treeViewItem.Header.ToString().Length - 4).ToLower() == ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToLower())
                {
                    //Affichage de la page d'ouverture
                    currentImage = 0;
                    imageView1 = imageViews.FirstOrDefault(i => i.Image.NumeroPage == 0);

                    tbxNomPage.Text = ConfigurationManager.AppSettings["Nom_Page_Ouverture"].ToUpper();
                    tbxNumeroPage.Text = "";
                    tbNumeroOrdreSequence.Text = "";
                    tbDateSequence.Text = "";
                    tbReference.Text = "";
                    dgSequence.ItemsSource = null;
                    // Chargement de l'image		
                    ChargerImage(currentImage);
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

        public void LoadAll()
        {
            try
            {
                #region RECUPERATION ET CREATION DES PAGES SPECIAUX
                // Création des Pages Spéciaux : PAGE DE GARDE : numero : -1 ET PAGE D'OUVERTURE : numero : 0			
                using (var ct = new DocumatContext())
                {
                    // Récupération de la date du serveur 
                    DateTime dateServer = ct.Database.SqlQuery<DateTime>("SELECT GETDATE()").FirstOrDefault();

                    //PAGE DE GARDE N°-1
                    if (ct.Image.FirstOrDefault(i => i.RegistreID == RegistreParent.RegistreID && i.NumeroPage == -1) == null)
                    {
                        Models.Image pageGarde = new Models.Image();
                        pageGarde.NumeroPage = -1;
                        pageGarde.NomPage = ConfigurationManager.AppSettings["Nom_Page_Garde"];
                        var file = fileInfos.FirstOrDefault(f => f.Name.Remove(f.Name.Length - 4) == pageGarde.NomPage);

                        //On vérifie que le fichier existe 
                        if (file == null)
                        {
                            MessageBox.Show("La PAGE DE GARDE n'a pas été indexée, et est introuvable dans le Dossier !!!", "AVERTISSEMENT", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            if (MessageBox.Show("La PAGE DE GARDE n'a pas encore été Indexée(enregistrée), Voulez vous le faire maintenant ?", "INDEXER PAGE DE GARDE",
                                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                pageGarde.CheminImage = System.IO.Path.Combine(RegistreParent.CheminDossier, file.Name);
                                pageGarde.Type = file.Extension.Substring(1).ToUpper();
                                pageGarde.Taille = file.Length;
                                pageGarde.DateCreation = pageGarde.DateDebutSequence = pageGarde.DateModif = pageGarde.DateScan
                                = pageGarde.DateScan = dateServer;
                                pageGarde.RegistreID = RegistreParent.RegistreID;
                                pageGarde.StatutActuel = (int)Enumeration.Image.INDEXEE;
                                ct.Image.Add(pageGarde);
                                ct.SaveChanges();

                                // Création du nouveau Statut 
                                Models.StatutImage statutImage = new StatutImage();
                                statutImage.ImageID = pageGarde.ImageID;
                                statutImage.Code = (int)Enumeration.Image.INDEXEE;
                                statutImage.DateCreation = statutImage.DateDebut = statutImage.DateModif = dateServer;
                                ct.StatutImage.Add(statutImage);
                                ct.SaveChanges();

                                //Enregistrement du traitement 
                                DocumatContext.AddTraitement(DocumatContext.TbImage, pageGarde.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.CREATION,"INDEXATION PAGE GARDE");
                            }
                        }
                    }

                    //PAGE D'OUVERTURE N°0
                    if (ct.Image.FirstOrDefault(i => i.RegistreID == RegistreParent.RegistreID && i.NumeroPage == 0) == null)
                    {
                        Models.Image pageOuverture = new Models.Image();
                        pageOuverture.NumeroPage = 0;
                        pageOuverture.NomPage = ConfigurationManager.AppSettings["Nom_Page_Ouverture"];
                        var file = fileInfos.FirstOrDefault(f => f.Name.Remove(f.Name.Length - 4) == pageOuverture.NomPage);

                        //On vérifie que le fichier existe 
                        if (file == null)
                        {
                            MessageBox.Show("La PAGE D'OUVERTURE n'a pas été indexée, et est introuvable dans le Dossier !!!", "AVERTISSEMENT", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            if (MessageBox.Show("La PAGE D'OUVERTURE n'a pas encore été Indexée(enregistrée), Voulez vous le faire maintenant ?", "INDEXER PAGE D'OUVERTURE",
                                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                pageOuverture.CheminImage = System.IO.Path.Combine(RegistreParent.CheminDossier, file.Name);
                                pageOuverture.Type = file.Extension.Substring(1).ToUpper();
                                pageOuverture.Taille = file.Length;
                                pageOuverture.DateCreation = pageOuverture.DateDebutSequence = pageOuverture.DateModif = pageOuverture.DateScan
                                = pageOuverture.DateScan = DateTime.Now;
                                pageOuverture.RegistreID = RegistreParent.RegistreID;
                                pageOuverture.StatutActuel = (int)Enumeration.Image.INDEXEE;
                                ct.Image.Add(pageOuverture);
                                ct.SaveChanges();

                                // Création du nouveau Statut 
                                Models.StatutImage statutImage = new StatutImage();
                                statutImage.ImageID = pageOuverture.ImageID;
                                statutImage.Code = (int)Enumeration.Image.INDEXEE;
                                statutImage.DateCreation = statutImage.DateDebut = statutImage.DateModif = DateTime.Now;
                                ct.StatutImage.Add(statutImage);
                                ct.SaveChanges();

                                //Enregistrement du traitement 
                                DocumatContext.AddTraitement(DocumatContext.TbImage, pageOuverture.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.CREATION, "PAGE OUVERTURE");
                            }
                        }
                    }
                }
                #endregion

                #region CHARGEMENT DE LA PAGE EN COURS D'INDEXATION
                // Chargement de la première page/ou de la page en cours
                using (var ct = new DocumatContext())
                {
                    Models.Image image = ct.Image.Where(i => i.StatutActuel == (int)Enumeration.Image.CREEE
                    && i.RegistreID == RegistreParent.RegistreID).OrderBy(i => i.NumeroPage).FirstOrDefault();
                    currentImage = (image != null) ? image.NumeroPage : -1;
                    ChargerImage(currentImage);
                }
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
                // Chargement des information d'entête
                HeaderInfosGetter();

                #region INSPECTION DU DOSSIER DE REGISTRE ET CREATION DE L'ABORESCENCE
                // Chargement de l'aborescence
                // Récupération de la liste des images contenu dans le dossier du registre
                var files = Directory.GetFiles(System.IO.Path.Combine(DossierRacine, RegistreParent.CheminDossier));
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

        private void SupprimeSequence_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgSequence.SelectedItems.Count >= 1)
                {
                    if (MessageBox.Show("Voulez-vous Vraiment supprimer ces séquences ?", "CONFIRMER SUPPRESSION", MessageBoxButton.YesNo, MessageBoxImage.Question)
                        == MessageBoxResult.Yes)
                    {
                        List<SequenceView> sequenceViews = dgSequence.SelectedItems.Cast<SequenceView>().ToList();

                        using (var ct = new DocumatContext())
                        {
                            foreach (var seq in sequenceViews)
                            {
                                ct.Sequence.Remove(ct.Sequence.FirstOrDefault(s => s.SequenceID == seq.Sequence.SequenceID));
                                ct.SaveChanges();
                                //DocumatContext.AddTraitement(DocumatContext.TbSequence, seq.Sequence.SequenceID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.SUPPRESSION);
                            }

                            // Refresh des données
                            List<Sequence> sequences = ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID).OrderBy(s => s.NUmeroOdre).ToList();
                            dgSequence.ItemsSource = SequenceView.GetViewsList(sequences);
                            if (sequences.Count != 0)
                            {
                                dgSequence.ScrollIntoView(dgSequence.Items.GetItemAt(dgSequence.Items.Count - 1));
                            }

                            MessageBox.Show("Les séquences ont bien été supprimées !!!", "Séquences Supprimées", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void AddSequence_Click(object sender, RoutedEventArgs e)
        {
            ChampsSequence.Visibility = Visibility.Visible;
            editSequence = false;
            dgSequence.IsEnabled = false;
            tbDateSequence.Text = RegistreParent.DateDepotDebut.ToShortDateString();
        }

        private void ModifSequence_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgSequence.SelectedItems.Count == 1)
                {
                    SequenceView sequenceView = (SequenceView)dgSequence.SelectedItem;
                    ChampsSequence.Visibility = Visibility.Visible;
                    tbNumeroOrdreSequence.Text = sequenceView.Sequence.NUmeroOdre.ToString();
                    if(sequenceView.Sequence.isSpeciale.Trim().ToLower() == "bis")
                    {
                        tbIsSpecial.SelectedIndex = 1;                       
                    }
                    else if (sequenceView.Sequence.isSpeciale.Trim().ToLower() == "doublon")
                    {
                        tbIsSpecial.SelectedIndex = 2;
                    }
                    else
                    {
                        tbIsSpecial.SelectedIndex = 0;
                    }
                    tbDateSequence.Text = sequenceView.Sequence.DateSequence.ToShortDateString().Trim();
                    tbReference.Text = sequenceView.strReferences.Trim();
                    tbNbRefPreindex.Text = sequenceView.Sequence.NombreDeReferences.ToString().Trim();

                    dgSequence.IsEnabled = false;
                    editSequence = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void cbxRetirerInstance_Checked(object sender, RoutedEventArgs e)
        {
            panelChoixInstance.Visibility = Visibility.Visible;
        }

        private void cbxRetirerInstance_Unchecked(object sender, RoutedEventArgs e)
        {
            panelChoixInstance.Visibility = Visibility.Collapsed;
        }

        private void tbDateSequence_GotFocus(object sender, RoutedEventArgs e)
        {
            tbDateSequence.CaretIndex = 0;
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

        private void BtnSaveSequence_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Regex RefsReg = new Regex("^(["+RefInitiale+"][0-9]{1,}/[\\w*]{1,}[,]){1,}([TR][0-9]{1,}/[\\w*]{1,})$");
                Regex RefsReg1 = new Regex("^["+RefInitiale+"][0-9]{1,}/[\\w*]{1,}$");
                if (editSequence && dgSequence.IsEnabled == false)
                {
                    int NbRefsPreIndex = 0, NumeroOrdre = 0;
                    DateTime DateSequence;
                    if (Int32.TryParse(tbNbRefPreindex.Text, out NbRefsPreIndex)
                        && Int32.TryParse(tbNumeroOrdreSequence.Text, out NumeroOrdre) && DateTime.TryParse(tbDateSequence.Text, out DateSequence)
                        && (string.IsNullOrWhiteSpace(tbReference.Text) || RefsReg.IsMatch(tbReference.Text.Trim()) 
                        || RefsReg1.IsMatch(tbReference.Text.Trim())))
                    {
                        if (MessageBox.Show("Voulez-vous vraiment enregistrer les données ?", "CONFIRMER", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            SequenceView sequenceView = (SequenceView)dgSequence.SelectedItem;
                            using (var ct = new DocumatContext())
                            {
                                // Récupération de la date du serveur 
                                DateTime dateServer = ct.Database.SqlQuery<DateTime>("SELECT GETDATE()").FirstOrDefault();

                                Models.Sequence sequence = ct.Sequence.FirstOrDefault(s => s.SequenceID == sequenceView.Sequence.SequenceID);
                                sequence.DateModif = dateServer;
                                sequence.DateSequence = DateSequence;
                                sequence.isSpeciale = tbIsSpecial.Text.Trim().ToLower();
                                sequence.NombreDeReferences = NbRefsPreIndex;
                                sequence.NUmeroOdre = NumeroOrdre;
                                sequence.References = tbReference.Text;
                                ct.SaveChanges();
                                //DocumatContext.AddTraitement(DocumatContext.TbSequence, sequence.SequenceID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION,"");

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

                                ChampsSequence.Visibility = Visibility.Collapsed;
                                //dgSequence.Height = dgSequenceMaxHeight;
                                tbNumeroOrdreSequence.Text = "";
                                tbIsSpecial.SelectedIndex = 0;
                                tbDateSequence.Text = "";
                                tbReference.Text = "";
                                tbNbRefPreindex.Text = "";
                                dgSequence.IsEnabled = true;
                                editSequence = false;
                                MessageBox.Show("La séquence : " + NumeroOrdre + " " + sequence.isSpeciale + ", a bien été modifié !", "Modification effectuée", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Verifier Svp, les champs que vous avez saisie !!", "Erreur de Saisie", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else if (editSequence == false)
                {
                    int NbRefsPreIndex = 0, NumeroOrdre = 0;
                    DateTime DateSequence;
                    if (Int32.TryParse(tbNumeroOrdreSequence.Text, out NumeroOrdre) && DateTime.TryParse(tbDateSequence.Text, out DateSequence)
                        && (string.IsNullOrWhiteSpace(tbReference.Text) || RefsReg.IsMatch(tbReference.Text.Trim())
                        || RefsReg1.IsMatch(tbReference.Text.Trim())))
                    {
                        if (MessageBox.Show("Voulez-vous vraiment enregistrer les données ?", "CONFIRMER", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            using (var ct = new DocumatContext())
                            {
                                // Récupération de la date du serveur 
                                DateTime dateServer = ct.Database.SqlQuery<DateTime>("SELECT GETDATE()").FirstOrDefault();

                                Models.Sequence sequence = new Sequence()
                                {
                                    DateCreation = dateServer,
                                    DateModif = dateServer,
                                    DateSequence = DateSequence,
                                    isSpeciale = tbIsSpecial.Text.Trim().ToLower(),
                                    NombreDeReferences = NbRefsPreIndex,
                                    NUmeroOdre = NumeroOrdre,
                                    PhaseActuelle = 0,
                                    References = tbReference.Text,
                                    ImageID = CurrentImageView.Image.ImageID,
                                };
                                sequence = ct.Sequence.Add(sequence);
                                ct.SaveChanges();
                                //DocumatContext.AddTraitement(DocumatContext.TbSequence, sequence.SequenceID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.CREATION,"");

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

                                ChampsSequence.Visibility = Visibility.Collapsed;
                                //dgSequence.Height = dgSequenceMaxHeight;
                                tbNumeroOrdreSequence.Text = "";
                                tbIsSpecial.SelectedIndex = 0;
                                tbDateSequence.Text = "";
                                tbReference.Text = "";
                                tbNbRefPreindex.Text = "";
                                dgSequence.IsEnabled = true;
                                MessageBox.Show("La séquence : " + sequence.NUmeroOdre + ", a bien été ajoutée !", "Ajout effectuée", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Verifier Svp, les champs que vous avez saisie !!", "Erreur de Saisie", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void BtnAnnuleSequence_Click(object sender, RoutedEventArgs e)
        {
            ChampsSequence.Visibility = Visibility.Collapsed;
            tbNumeroOrdreSequence.Text = "";
            tbIsSpecial.SelectedIndex = 0;
            tbDateSequence.Text = "";
            tbReference.Text = "";
            tbNbRefPreindex.Text = "";
            dgSequence.IsEnabled = true;
            editSequence = false;
        }

        private void tbNumeroOrdreSequence_TextChanged(object sender, TextChangedEventArgs e)
        {
            ForceNombre(sender);
        }

        private void BtnRetirerInstance_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Voulez vous vraiment levez l'instance de cet image ?", "QUESTION", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    using (var ct = new DocumatContext())
                    {
                        // Récupération de la date du serveur 
                        DateTime dateServer = ct.Database.SqlQuery<DateTime>("SELECT GETDATE()").FirstOrDefault();

                        Models.Image image = ct.Image.FirstOrDefault(i => i.ImageID == CurrentImageView.Image.ImageID);
                        if (image != null)
                        {
                            string typeInstance = image.typeInstance;
                            image.typeInstance = "";
                            image.ObservationInstance = "";
                            image.DateModif = dateServer;
                            image.StatutActuel = (int)Enumeration.Image.CREEE;

                            // Création de l'instance ou modification de l'instance
                            StatutImage newstatut = ct.StatutImage.FirstOrDefault(s => s.ImageID == image.ImageID && s.Code == (int)Enumeration.Image.CREEE);
                            if (newstatut != null)
                            {
                                newstatut.DateDebut = newstatut.DateModif = dateServer;
                                newstatut.DateFin = dateServer;
                            }
                            else
                            {
                                newstatut = new StatutImage();
                                newstatut.ImageID = image.ImageID;
                                newstatut.Code = (int)Enumeration.Image.CREEE;
                                newstatut.DateCreation = newstatut.DateDebut = newstatut.DateModif = dateServer;
                                ct.StatutImage.Add(newstatut);
                            }
                            ct.SaveChanges();
                            DocumatContext.AddTraitement(DocumatContext.TbImage, image.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION, "RETRAIT IMAGE INSTANCE : " + typeInstance);
                            ChargerImage(image.NumeroPage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void BtnImporterImage_Click(object sender, RoutedEventArgs e)
        {
            // Importation de l'image 
            try
            {
                OpenFileDialog open = new OpenFileDialog();
                open.Filter = "Fichiers PDF | *.pdf; | Fichiers Images (TIF, PNG, JPG) |*.jpg;*.png;*.tif;";
                open.Multiselect = false;
                open.FilterIndex = 1;

                if (open.ShowDialog() == true)
                {
                    FichierImporte = open.FileName;
                    BtnImporterImage.Foreground = Brushes.LightGreen;
                    BtnValiderModifImage.Foreground = Brushes.LightGreen;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERREUR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnValiderModifImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentImageView.Image.NumeroPage.ToString() != tbNumeroPage.Text.Trim() || !string.IsNullOrWhiteSpace(FichierImporte)
                    || CurrentImageView.Image.DebutSequence.ToString() != tbNumeroDebutSequence.Text.Trim()
                    || CurrentImageView.Image.FinSequence.ToString() != tbFinSequence.Text.Trim()
                    || CurrentImageView.Image.DateDebutSequence.ToShortDateString() != DateTime.Parse(tbDateDebSequence.Text).ToShortDateString()
                    || !string.IsNullOrEmpty(tbxNomPage.Text.Trim().ToLower()))
                {
                    if (MessageBox.Show("Voulez-vous vraiment modifier les données ?", "MODIFIER LES DONNEES", MessageBoxButton.YesNo, MessageBoxImage.Question)
                        == MessageBoxResult.Yes)
                    {
                        int NumeroPage = CurrentImageView.Image.NumeroPage;
                        string NomPage = CurrentImageView.Image.NomPage, CheminFichier = CurrentImageView.Image.CheminImage, TypeFichier = CurrentImageView.Image.Type;
                        float TaillePage = CurrentImageView.Image.Taille;
                        DateTime DateScan = CurrentImageView.Image.DateScan;

                        // si le numero de l'image change 
                        if (CurrentImageView.Image.NumeroPage.ToString() != tbNumeroPage.Text.Trim())
                        {
                            NumeroPage = Int32.Parse(tbNumeroPage.Text.Trim());
                            if (NumeroPage == -1)
                            {
                                NomPage = ConfigurationManager.AppSettings["Nom_Page_Garde"];
                            }
                            else if (NumeroPage == 0)
                            {
                                NomPage = ConfigurationManager.AppSettings["Nom_Page_Ouverture"];
                            }
                            else
                            {
                                if (NumeroPage > 9)
                                {
                                    if (NumeroPage > 99)
                                    {
                                        NomPage = $"{NumeroPage}";
                                    }
                                    else
                                    {
                                        NomPage = $"0{NumeroPage}";
                                    }
                                }
                                else
                                {
                                    NomPage = $"00{NumeroPage}";
                                }
                            }

                            // Modification du fichier image dans le dossier de scan 						
                            string nomFichierOld = CurrentImageView.Image.NomPage + "." + CurrentImageView.Image.Type.ToLower();
                            string nomFichier = NomPage + "." + CurrentImageView.Image.Type.ToLower();
                            CheminFichier = System.IO.Path.Combine(RegistreParent.CheminDossier, nomFichier);
                            if (!File.Exists(System.IO.Path.Combine(DossierRacine, CheminFichier)))
                            {
                                File.Move(System.IO.Path.Combine(DossierRacine, RegistreParent.CheminDossier, nomFichierOld)
                                    , System.IO.Path.Combine(DossierRacine, CheminFichier));
                                viewImage(System.IO.Path.Combine(DossierRacine, CheminFichier));
                            }
                            else
                            {
                                if (MessageBox.Show("Une image du même nom existe dans le Dossier de scan, Voulez vous garder le même nom dans la base de données ?",
                                                "GARDER LE NOM", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                                {
                                    MessageBox.Show("Modification Annulée !!!", "Modification Annulée", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                    return;
                                }
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(FichierImporte))
                        {
                            FileInfo file = new FileInfo(FichierImporte);
                            TypeFichier = file.Extension.Remove(0, 1).ToUpper();
                            TaillePage = file.Length;
                            DateScan = file.CreationTime;
                            string nomFichier = NomPage + file.Extension;
                            CheminFichier = System.IO.Path.Combine(RegistreParent.CheminDossier, nomFichier);
                            if (File.Exists(System.IO.Path.Combine(DossierRacine, CheminFichier)))
                            {
                                if (MessageBox.Show("Une Image portant le même nom existe dans le Dossier voulez vous la remplacer ?", "REMPLACER ?", MessageBoxButton.YesNo, MessageBoxImage.Question)
                                    == MessageBoxResult.Yes)
                                {
                                    File.Copy(FichierImporte, System.IO.Path.Combine(DossierRacine, CheminFichier), true);
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(CurrentImageView.Image.CheminImage) &&
                                    File.Exists(Path.Combine(DossierRacine, CurrentImageView.Image.CheminImage)))
                                {
                                    File.Delete(Path.Combine(DossierRacine, CurrentImageView.Image.CheminImage));
                                }
                                File.Copy(FichierImporte, System.IO.Path.Combine(DossierRacine, CheminFichier));
                            }
                        }

                        if (!string.IsNullOrEmpty(tbPageName.Text.Trim().ToLower()))
                        {
                            NomPage = tbPageName.Text.Trim().ToLower();
                        }

                        using (var ct = new DocumatContext())
                        {
                            // Récupération de la date du serveur 
                            DateTime dateServer = ct.Database.SqlQuery<DateTime>("SELECT GETDATE()").FirstOrDefault();

                            Models.Image image = ct.Image.FirstOrDefault(i => i.ImageID == CurrentImageView.Image.ImageID);
                            if (image != null)
                            {
                                image.NomPage = NomPage;
                                image.NumeroPage = NumeroPage;
                                image.CheminImage = CheminFichier;
                                image.Type = TypeFichier;
                                image.Taille = TaillePage;
                                image.DateScan = DateScan;
                                image.DebutSequence = Int32.Parse(tbNumeroDebutSequence.Text);
                                image.FinSequence = Int32.Parse(tbFinSequence.Text);
                                image.DateDebutSequence = DateTime.Parse(tbDateDebSequence.Text);
                                image.DateModif = dateServer;
                                ct.SaveChanges();

                                string Observation = "";
                                if (FichierImporte != null)
                                {
                                    Observation = "REMPLACEMENT IMAGE source fichier : " + FichierImporte;
                                }

                                DocumatContext.AddTraitement(DocumatContext.TbImage, image.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION, Observation);
                                ChargerImage(image.NumeroPage);
                                MessageBox.Show("Image Modifiée !!", "Modifié", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void tbNumeroPage_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tbNumeroPage.Text))
                {
                    tbNumeroPage.Text = CurrentImageView.Image.NumeroPage.ToString();
                }
                else
                {
                    if (CurrentImageView.Image.NumeroPage.ToString() != tbNumeroPage.Text.Trim())
                    {
                        BtnValiderModifImage.Foreground = Brushes.LightGreen;
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(FichierImporte) && CurrentImageView.Image.DebutSequence.ToString() == tbNumeroDebutSequence.Text.Trim()
                            && CurrentImageView.Image.FinSequence.ToString() == tbFinSequence.Text.Trim()
                            && CurrentImageView.Image.DateDebutSequence.ToShortDateString() == DateTime.Parse(tbDateDebSequence.Text).ToShortDateString())
                        {
                            BtnValiderModifImage.Foreground = Brushes.White;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void tbDateDebSequence_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime dateDebSeq;
                if (!string.IsNullOrWhiteSpace(tbDateDebSequence.Text) && DateTime.TryParse(tbDateDebSequence.Text.Trim(), out dateDebSeq))
                {
                    if (CurrentImageView.Image.DateDebutSequence.ToShortDateString() != dateDebSeq.ToShortDateString())
                    {
                        BtnValiderModifImage.Foreground = Brushes.LightGreen;
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(FichierImporte) && CurrentImageView.Image.NumeroPage.ToString() == tbNumeroPage.Text.Trim()
                            && CurrentImageView.Image.DebutSequence.ToString() == tbNumeroDebutSequence.Text.Trim()
                            && CurrentImageView.Image.FinSequence.ToString() == tbFinSequence.Text.Trim())
                        {
                            BtnValiderModifImage.Foreground = Brushes.White;
                        }
                    }
                }
                else
                {
                    tbDateDebSequence.Text = CurrentImageView.Image.NumeroPage.ToString();
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void tbFinSequence_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tbFinSequence.Text))
                {
                    tbFinSequence.Text = CurrentImageView.Image.FinSequence.ToString();
                }
                else
                {
                    if (CurrentImageView.Image.FinSequence.ToString() != tbFinSequence.Text.Trim())
                    {
                        BtnValiderModifImage.Foreground = Brushes.LightGreen;
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(FichierImporte) && CurrentImageView.Image.DebutSequence.ToString() == tbNumeroDebutSequence.Text.Trim()
                            && CurrentImageView.Image.NumeroPage.ToString() == tbNumeroPage.Text.Trim()
                            && CurrentImageView.Image.DateDebutSequence.ToShortDateString() == DateTime.Parse(tbDateDebSequence.Text).ToShortDateString())
                        {
                            BtnValiderModifImage.Foreground = Brushes.White;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void tbNumeroDebutSequence_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tbNumeroDebutSequence.Text))
                {
                    tbNumeroDebutSequence.Text = CurrentImageView.Image.DebutSequence.ToString();
                }
                else
                {
                    if (CurrentImageView.Image.DebutSequence.ToString() != tbNumeroDebutSequence.Text.Trim())
                    {
                        BtnValiderModifImage.Foreground = Brushes.LightGreen;
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(FichierImporte) && CurrentImageView.Image.FinSequence.ToString() == tbFinSequence.Text.Trim()
                            && CurrentImageView.Image.NumeroPage.ToString() == tbNumeroPage.Text.Trim()
                            && CurrentImageView.Image.DateDebutSequence.ToShortDateString() == DateTime.Parse(tbDateDebSequence.Text).ToShortDateString())
                        {
                            BtnValiderModifImage.Foreground = Brushes.White;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        //Remplacement d'une image
        private void ImporteImage_Click(object sender, RoutedEventArgs e)
        {
            // Importation de l'image 
            try
            {
                OpenFileDialog open = new OpenFileDialog();
                open.Filter = "Fichiers PDF | *.pdf; | Fichiers Images (TIF, PNG, JPG) |*.jpg;*.png;*.tif;";
                open.Multiselect = true;
                open.FilterIndex = 1;

                if (open.ShowDialog() == true)
                {
                    List<FileInfo> filesInfos = new List<FileInfo>();
                    foreach (var filePath in open.FileNames)
                    {
                        FileInfo fileInfo = new FileInfo(filePath);
                        if (File.Exists(System.IO.Path.Combine(DossierRacine, RegistreParent.CheminDossier, fileInfo.Name)))
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
                        File.Copy(fileInfo.FullName, System.IO.Path.Combine(DossierRacine, RegistreParent.CheminDossier, fileInfo.Name));
                        ListeFileImport = ListeFileImport + fileInfo.Name + ",";
                    }
                    ListeFileImport = ListeFileImport.Remove(ListeFileImport.Length - 1);

                    // enregistrement du traitement 
                    DocumatContext.AddTraitement(DocumatContext.TbRegistre, RegistreParent.RegistreID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION, "IMPORTATION IMAGE : " + ListeFileImport);
                    ActualiserArborescence();
                    MessageBox.Show(filesInfos.Count + " Image(s) : " + ListeFileImport + " ont/a été importée(s) avec success !!!", "IMPORTATION EFFECTUEE", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERREUR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SupprimerImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (TreeViewItem treeViewItem in registreAbre.Items)
                {
                    if (treeViewItem.IsSelected)
                    {
                        if (MessageBox.Show("Voulez Vous vraiment Supprimer l'élément " + treeViewItem.Header + ", Avec ses index et ses séquences ?", "SUPPRIMER " + treeViewItem.Header.ToString().ToUpper(),
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            string FilePath = System.IO.Path.Combine(DossierRacine, RegistreParent.CheminDossier, treeViewItem.Header.ToString());
                            FileInfo fileInfo = new FileInfo(FilePath);
                            // Suppression de l'image 
                            if (fileInfo.Exists)
                            {
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
                                    throw new Exception("Impossible de Retrouver la Page a supprimer !!!");
                                }

                                using (var ct = new DocumatContext())
                                {
                                    // Récupération de la date du serveur 
                                    DateTime dateServer = ct.Database.SqlQuery<DateTime>("SELECT GETDATE()").FirstOrDefault();

                                    File.Delete(fileInfo.FullName);
                                    Models.Image image = ct.Image.FirstOrDefault(i => i.RegistreID == RegistreParent.RegistreID && i.NumeroPage == NumeroPage);
                                    if (image != null)
                                    {
                                        ct.Image.Remove(image);
                                        ct.SaveChanges();
                                        // Enregistrement du traitement de suppression
                                        DocumatContext.AddTraitement(DocumatContext.TbImage, image.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.SUPPRESSION, "SUPPRESSION IMAGE SEQUENCES : " + fileInfo.Name.ToUpper() + " DU REGISTRE ID : " + RegistreParent.RegistreID);
                                        ChargerImage(currentImage);
                                        MessageBox.Show("L'image : " + fileInfo.Name.ToUpper() + " ainsi que toutes ses séquences ont bien été supprimés !!!", "IMAGE SUPPRIMEE", MessageBoxButton.OK, MessageBoxImage.Information);
                                    }
                                    else
                                    {
                                        // Enregistrement du traitement de suppression
                                        DocumatContext.AddTraitement(DocumatContext.TbImage, 0, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.SUPPRESSION, "SUPPRESSION IMAGE SANS SEQUENCES : " + fileInfo.Name.ToUpper() + " DU REGISTRE ID : " + RegistreParent.RegistreID);
                                        ActualiserArborescence();
                                        MessageBox.Show("L'image : " + fileInfo.Name.ToUpper() + ", n'avait pas de séquence et a été supprimé !!!", "IMAGE SUPPRIMEE", MessageBoxButton.OK, MessageBoxImage.Information);
                                    }
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

        private void ModifierRegistre_Click(object sender, RoutedEventArgs e)
        {
            FormRegistre registre = new FormRegistre(RegistreParent, MainParent.Utilisateur);
            registre.Show();
            registre.Closed += Registre_Closed;
        }

        private void Registre_Closed(object sender, EventArgs e)
        {
            try
            {
                using (var ct = new DocumatContext())
                {
                    RegistreParent = ct.Registre.FirstOrDefault(r => r.RegistreID == RegistreParent.RegistreID);
                    HeaderInfosGetter();
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void ChampsSequence_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ChampsSequence.IsVisible)
            {
                ChampsImage.Visibility = Visibility.Collapsed;
            }
            else
            {
                ChampsImage.Visibility = Visibility.Visible;
            }
        }

        private void panelChoixInstance_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (panelChoixInstance.IsVisible)
            {
                ChampsImage.Visibility = Visibility.Collapsed;
            }
            else
            {
                ChampsImage.Visibility = Visibility.Visible;
            }
        }

        private void OuvrirDossierScan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DirectoryInfo RegistreDossier = new DirectoryInfo(Path.Combine(DossierRacine, RegistreParent.CheminDossier));
                OpenFileDialog openFileDialog = new OpenFileDialog();
                var dlg = new CommonOpenFileDialog();
                dlg.Title = "Dossier de Scan du Registre : " + RegistreParent.QrCode;
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

                if (dlg.ShowDialog() == CommonFileDialogResult.Ok) { }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void TermineAnnule_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<TreeViewItem> treeViewItems = registreAbre.Items.Cast<TreeViewItem>().ToList();

                TreeViewItem selectedItem = treeViewItems.FirstOrDefault(tr => tr.IsSelected == true);

                if (selectedItem != null)
                {
                    if (MessageBox.Show("Voulez vous changer le statut de l'image : " + selectedItem.Header.ToString() + " ?", "Confirmer ?", MessageBoxButton.YesNo, MessageBoxImage.Question)
                        == MessageBoxResult.Yes)
                    {
                        string FilePath = System.IO.Path.Combine(DossierRacine, RegistreParent.CheminDossier, selectedItem.Header.ToString());
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
                            // Récupération de la date du serveur 
                            DateTime dateServer = ct.Database.SqlQuery<DateTime>("SELECT GETDATE()").FirstOrDefault();

                            Models.Image image = ct.Image.FirstOrDefault(i => i.RegistreID == RegistreParent.RegistreID && i.NumeroPage == NumeroPage);
                            if (image != null)
                            {
                                if (image.StatutActuel != (int)Enumeration.Image.INSTANCE)
                                {
                                    if (image.StatutActuel == (int)Enumeration.Image.CREEE)
                                    {
                                        image.StatutActuel = (int)Enumeration.Image.INDEXEE;
                                        ct.SaveChanges();
                                        // Enregistrement du traitement
                                        DocumatContext.AddTraitement(DocumatContext.TbImage, image.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION, "MODIFICATION DU STATUT IMAGE A : A #TERMINER, REGISTRE ID : " + RegistreParent.RegistreID);
                                        ActualiserArborescence();
                                        HeaderInfosGetter();
                                        if (currentImage == NumeroPage)
                                        {
                                            ChargerImage(NumeroPage);
                                        }
                                        MessageBox.Show("L'image a été Marquée comme Indexée", "Image Terminée", MessageBoxButton.OK, MessageBoxImage.Information);
                                    }
                                    else
                                    {
                                        image.StatutActuel = (int)Enumeration.Image.CREEE;
                                        ct.SaveChanges();
                                        // Enregistrement du traitement
                                        DocumatContext.AddTraitement(DocumatContext.TbImage, image.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION, "MODIFICATION DU STATUT IMAGE A : A #CREEE, DU REGISTRE ID : " + RegistreParent.RegistreID);
                                        ActualiserArborescence();
                                        HeaderInfosGetter();
                                        if (currentImage == NumeroPage)
                                        {
                                            ChargerImage(NumeroPage);
                                        }
                                        MessageBox.Show("L'indexation de l'image a été annulée", "Indexation Image Annulée", MessageBoxButton.OK, MessageBoxImage.Information);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Veuillez retirer d'abord l'Instance de l'Image avant d'effectuer cette opération !!!", "Image en Instance", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Veuillez retirer d'abord Enregistrer l'Image avant d'effectuer cette opération !!!", "Image Non Enregistrée", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private void RefrechAll_Click(object sender, RoutedEventArgs e)
        {
            LoadAll();
        }

        private void RenommerForm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FormRenommage formRenommage = new FormRenommage(RegistreParent);
                formRenommage.ShowDialog();
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void GenererDepot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (currentImage != 0 && currentImage != -1)
                {
                    using (var ct = new DocumatContext())
                    {
                        Models.Image image = ct.Image.FirstOrDefault(i => i.RegistreID == RegistreParent.RegistreID && i.NumeroPage == currentImage);
                        if (image != null)
                        {
                            GenererSequenceAuto genererSequenceAuto = new GenererSequenceAuto(image, MainParent.Utilisateur, this);
                            genererSequenceAuto.ShowDialog();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("La page que vous avez sélectionnée ne peut pas avoir de références ", "Page Inadaptée", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void TransfererDepot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgSequence.SelectedItems.Count >= 1)
                {
                    // Chargement du Formulaire de transfert
                    // Récupération des références
                    List<SequenceView> sequenceViews = dgSequence.SelectedItems.Cast<SequenceView>().ToList();

                    FormTransfertSequence formTransfertSequence = new FormTransfertSequence(sequenceViews, CurrentImageView, RegistreParent, this);
                    formTransfertSequence.ShowDialog();
                }
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
                            DocumatContext.AddTraitement(DocumatContext.TbImage, imageManquant.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION, "INSTANCE DE IMAGE N°" + imageManquant.NumeroPage + " DU REGISTRE ID N°  " + imageManquant.RegistreID + " POUR #IMAGE_MANQUANTE");

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
    }
}
