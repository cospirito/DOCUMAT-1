using DOCUMAT.Models;
using DOCUMAT.Pages.Registre;
using DOCUMAT.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
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
		Indexation.Indexation MainParent;

		RegistreView RegistreViewParent;
		ImageView CurrentImageView;
		private int currentImage = 1;
		private int dgSequenceMaxHeight = 400;
		private int dgSequenceMinHeight = 300;

		// Définition de l'aborescence
		TreeViewItem registreAbre = new TreeViewItem();
		//Fichier d'image dans disque dure
		List<FileInfo> fileInfos = new List<FileInfo>();
		// Dossier racine 
		string DossierRacine = ConfigurationManager.AppSettings["CheminDossier_Scan"];
		// Fichier Image upload
		string FichierImporte = "";

		//Initiale des références par type de registre R3 = R / R4 = T
		string RefInitiale = "R";
		//Liste des possibles references		
		Dictionary<String, String> References = new Dictionary<String, String>();

		// Sequence en Mode edition ou ajout
		bool editSequence = false;

        public ImageIndexSuperviseur()
        {
            InitializeComponent();
			ContextMenu Cm = (ContextMenu)FindResource("cmSequence");
			dgSequence.ContextMenu = Cm;

			// Attribution du Context Menu de l'Aborescence 
			FolderView.ContextMenu = (ContextMenu)FindResource("cmAbo");
		}

		public ImageIndexSuperviseur(RegistreView registreview, Indexation.Indexation indexation) : this()
        {
			MainParent = indexation;
			RegistreViewParent = registreview;
		}


		#region FONCTIONS			
		private void ForceNombre(object sender)
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
			System.Windows.Controls.Image Image;
			FixedDocument FixedDocument;
			FixedPage FixedPage;
			PageContent PageContent;

            ImageSource img = BitmapFrame.Create(new Uri(ImagePath), BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad);
            Image = new System.Windows.Controls.Image();
			Image.Source = img;
			FixedPage = new FixedPage();
			FixedPage.Height = img.Height;
			FixedPage.Width = img.Width;
			FixedPage.Children.Add(Image);
			PageContent = new PageContent();
			PageContent.Child = FixedPage;
			FixedDocument = new FixedDocument();
			FixedDocument.Pages.Add(PageContent);
			DocumentViewer.Document = FixedDocument;
		}

		/// <summary>
		///  Permet d'afficher les informations de l'entête de la fenêtre
		/// </summary>
		private void HeaderInfosGetter()
		{
			//Remplissage des éléments de la vue
			//Obtention de la liste des images du registre dans la base de données 
			ImageView imageView1 = new ImageView();
			List<ImageView> imageViews = imageView1.GetSimpleViewsList(RegistreViewParent.Registre);			
			if (!string.IsNullOrEmpty(MainParent.Utilisateur.CheminPhoto))
			{
				AgentImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(DossierRacine, MainParent.Utilisateur.CheminPhoto)), new System.Net.Cache.RequestCachePolicy());
			}
			// Agent Superviseur / Administrateur
			tbxAgentLogin.Text = "Login : " + MainParent.Utilisateur.Login;
			tbxAgentNoms.Text = "Noms : " + MainParent.Utilisateur.Noms;
			tbxAgentType.Text = "Aff : " + Enum.GetName(typeof(Enumeration.AffectationAgent),MainParent.Utilisateur.Affectation);

			// Récupération de l'agent en charge de l'indexation 
			Models.Traitement traitementAttrRegistre = imageView1.context.Traitement.FirstOrDefault(t => t.TableSelect == DocumatContext.TbRegistre && t.TableID == RegistreViewParent.Registre.RegistreID
								&& t.TypeTraitement == (int)Enumeration.TypeTraitement.REGISTRE_ATTRIBUE_INDEXATION);
			if(traitementAttrRegistre != null)
			{
				Models.Agent agentAttrRegistre = imageView1.context.Agent.FirstOrDefault(a => a.AgentID == traitementAttrRegistre.AgentID);
				// Agent Indexeur
				if (!string.IsNullOrEmpty(agentAttrRegistre.CheminPhoto))
				{
					AgentIndexImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(DossierRacine, agentAttrRegistre.CheminPhoto)), new System.Net.Cache.RequestCachePolicy());
				}
				tbxAgentIndexLogin.Text = "Login : " + agentAttrRegistre.Login;
				tbxAgentIndexNoms.Text = "Noms : " + agentAttrRegistre.Noms;
				tbxAgentIndexType.Text = "Aff : " + Enum.GetName(typeof(Enumeration.AffectationAgent), agentAttrRegistre.Affectation);
            }

			tbxQrCode.Text = "Qrcode : " + RegistreViewParent.Registre.QrCode;
			tbxVersement.Text = "N° Versement : " + RegistreViewParent.Versement.NumeroVers.ToString();
			tbxService.Text = "Service : " + RegistreViewParent.ServiceVersant.Nom;
			//tbAgent.Text = 
			//Procédure de modification de l'indicateur de reussite
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

		/// <summary>
		/// Actualise l'aborescence en fonction des changements lors de l'indexation !!!
		/// </summary>
		private void ActualiserArborescence()
		{
			#region RECHARGEMENT DE l'ABORESCENCE
			// Chargement de l'aborescence
			// Récupération de la liste des images contenu dans le dossier du registre
			var files = Directory.GetFiles(System.IO.Path.Combine(DossierRacine, RegistreViewParent.Registre.CheminDossier));
			// Affichage et configuration de la TreeView
			registreAbre.Items.Clear();
			registreAbre.Header = RegistreViewParent.Registre.QrCode;
			registreAbre.Tag = RegistreViewParent.Registre.QrCode;
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

            // Définition des types d'icon dans l'aborescence en fonction des statuts des images
            foreach (TreeViewItem item in registreAbre.Items)
			{
				using (var ct = new DocumatContext())
				{
					if (ct.Image.FirstOrDefault(i => i.NomPage.ToLower() == item.Header.ToString().Remove(item.Header.ToString().Length - 4).ToLower()
						&& i.StatutActuel == (int)Enumeration.Image.INDEXEE && i.RegistreID == RegistreViewParent.Registre.RegistreID) != null)
					{
						item.Tag = "valide";
					}
					else if (ct.Image.FirstOrDefault(i => i.NomPage.ToLower() == item.Header.ToString().Remove(item.Header.ToString().Length - 4).ToLower()
							&& i.StatutActuel == (int)Enumeration.Image.INSTANCE && i.RegistreID == RegistreViewParent.Registre.RegistreID) != null)
					{
						item.Tag = "instance";
					}
					else
					{
						Models.Image image = ct.Image.OrderBy(i => i.NumeroPage).FirstOrDefault(i => i.StatutActuel == (int)Enumeration.Image.CREEE
						 && i.RegistreID == RegistreViewParent.Registre.RegistreID);
						if (image != null)
						{
							if (image.NomPage == item.Header.ToString().Remove(item.Header.ToString().Length - 4).ToLower())
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
		#endregion

		/// <summary>
		/// Fonction chargée d'afficher l'image à Indexer
		/// </summary>
		/// <param name="currentImage"> Le numéro de l'image à récupérer et à afficher </param>
		public void ChargerImage(int currentImage)
		{
            try
            {
                #region RECUPERATION ET AFFICHAGE DE L'IMAGE SELECTIONNEE
                ImageView imageView1 = new ImageView();
				List<ImageView> imageViews = imageView1.GetSimpleViewsList(RegistreViewParent.Registre);
				imageView1 = imageViews.FirstOrDefault(i => i.Image.NumeroPage == currentImage);
				if (imageView1 != null)
				{
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

					//On change l'image actuelle
					CurrentImageView = imageView1;
					this.currentImage = currentImage;

					if (imageView1.Image.NumeroPage == -1)
					{
						tbxNomPage.Text = ConfigurationManager.AppSettings["Nom_Page_Garde"];
						tbxNumeroPage.Text = "";
						tbxDebSeq.Text = "";
						tbxFinSeq.Text = "";
					}
					else if (imageView1.Image.NumeroPage == 0)
					{
						tbxNomPage.Text = ConfigurationManager.AppSettings["Nom_Page_Ouverture"];
						tbxNumeroPage.Text = "";
						tbxDebSeq.Text = "";
						tbxFinSeq.Text = "";
					}
					else
					{
						tbxNomPage.Text = "PAGE : " + imageView1.Image.NomPage;
						tbxNumeroPage.Text = "N° " + imageView1.Image.NumeroPage.ToString() + " / " + RegistreViewParent.Registre.NombrePage;
						tbxDebSeq.Text = "N° Debut : " + imageView1.Image.DebutSequence;
						tbxFinSeq.Text = "N° Fin : " + imageView1.Image.FinSequence;
					}
					#endregion

					#region RECUPERATION DES SEQUENCES DE L'IMAGE
					// Récupération des sequences déja renseignées, Différent des images préindexer ayant la référence défaut
					List<Sequence> sequences = imageView1.context.Sequence.Where(s => s.ImageID == imageView1.Image.ImageID).OrderBy(s => s.NUmeroOdre).ToList();
					dgSequence.ItemsSource = SequenceView.GetViewsList(sequences);
					if (sequences.Count != 0)
					{
						dgSequence.ScrollIntoView(dgSequence.Items.GetItemAt(dgSequence.Items.Count - 1));				
					}

					//Vide les champs du formulaire de séquence 
					ChampsSequence.Visibility = Visibility.Collapsed;
					tbNumeroOrdreSequence.Text = "";
					tbIsSpecial.Text = "";
					tbDateSequence.Text = "";
					tbReference.Text = "";
					tbNbRefPreindex.Text = "";
					dgSequence.Height = dgSequenceMaxHeight;
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

					// Actualisation de l'aborescence
					ActualiserArborescence();
				}
				else
				{
					MessageBox.Show("Impossible de charger la Page, Numero : " + currentImage + " , Elle n'est pas enregistrée !!!", "AVERTISSEMENT", MessageBoxButton.OK, MessageBoxImage.Warning);
					if(MessageBox.Show("Voulez - Vous, enregistrer cette image et commencer l'indexation ?","ENREGISTRER L'IMAGE ?",MessageBoxButton.YesNo,MessageBoxImage.Question)
						== MessageBoxResult.Yes)
                    {
						if (currentImage == -1)
                        {
							throw new Exception("Si l'Image de la PAGE DE GARDE est bien scannée, fermez et reouvrez la fenêtre afin de pouvoir enregistrer la page !!!");
                        }
					
						if(currentImage == 0 )
                        {
							throw new Exception("Si l'Image de la PAGE D'OUVERTURE est bien scannée, fermez et reouvrez la fenêtre afin de pouvoir enregistrer la page !!!");
                        }

						string NomPage = (currentImage < 10) ? $"0{currentImage}" : currentImage.ToString();
						FileInfo file = fileInfos.FirstOrDefault(f => f.Name.Remove(f.Name.Length - 4) == NomPage);
						if (file != null)
						{
							using (var ct = new DocumatContext())
							{
								Models.Image image = new Models.Image()
								{
									CheminImage = System.IO.Path.Combine(RegistreViewParent.Registre.CheminDossier, file.Name),
									Type = file.Extension.Substring(1).ToUpper(),
									Taille = file.Length,
									DateScan = file.LastWriteTime,
									DateModif = DateTime.Now,
									DateCreation = DateTime.Now,
									StatutActuel = (int)Enumeration.Image.CREEE,
									DateDebutSequence = DateTime.Now,
									DebutSequence = 0,
									FinSequence = 0,
									NumeroPage = currentImage,
									NomPage = currentImage.ToString(),
									RegistreID = RegistreViewParent.Registre.RegistreID
								};
								ct.Image.Add(image);
								ct.SaveChanges();

								// Création du nouveau Statut 
								Models.StatutImage statutImage = new StatutImage();
								statutImage.ImageID = image.ImageID;
								statutImage.Code = (int)Enumeration.Image.CREEE;
								statutImage.DateCreation = statutImage.DateDebut = statutImage.DateModif = DateTime.Now;
								ct.StatutImage.Add(statutImage);
								ct.SaveChanges();

								DocumatContext.AddTraitement(DocumatContext.TbImage, image.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.CREATION);
								MessageBox.Show("Page Ajoutée !!!", "AJOUT EFFECTUE", MessageBoxButton.OK, MessageBoxImage.Information);
							}
						}
						else
						{
							MessageBox.Show("La page est introuvable dans le Dossier de Scan !!!", "ATTENTION", MessageBoxButton.OK, MessageBoxImage.Warning);
						}
					}
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
			TreeViewItem treeViewItem = (TreeViewItem)sender;
			// Récupération des images du registre
			ImageView imageView1 = new ImageView();
			List<ImageView> imageViews = imageView1.GetSimpleViewsList(RegistreViewParent.Registre);

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

		private void btnAnnule_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			//Image.RefreshImages();	
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
            try
            {
                #region INSPECTION DU DOSSIER DE REGISTRE ET CREATION DE L'ABORESCENCE
                // Chargement de l'aborescence
                // Récupération de la liste des images contenu dans le dossier du registre
                var files = Directory.GetFiles(System.IO.Path.Combine(DossierRacine, RegistreViewParent.Registre.CheminDossier));
				List<FileInfo> fileInfos1 = new List<FileInfo>();

				// Récupération des fichiers dans un fileInfo
				foreach (var file in files)
				{
					fileInfos1.Add(new FileInfo(file));
				}
				fileInfos1 = fileInfos1.Where(f => f.Extension.ToLower() == ".tif" || f.Extension.ToLower() == ".jpg" || f.Extension.ToLower() == ".png").ToList();

				// Si le nombre de Page Scannée est différent du nombre de page attendu, signal
				// Le Plus Trois est composée : de la page de garde + la page d'ouverture 
				if ((RegistreViewParent.Registre.NombrePage + 2) != fileInfos1.Count())
				{
					MessageBox.Show("Attention le nombre de Page Scannée est différents du nombre de page attendu : " +
									"\n Nombre pages scannées: " + fileInfos1.Count() +
									"\n Nombre pages attendues : " + (RegistreViewParent.Registre.NombrePage + 2), "AVERTISSEMENT", MessageBoxButton.OK, MessageBoxImage.Asterisk);
				}

				// Affichage et configuration de la TreeView
				registreAbre.Header = RegistreViewParent.Registre.QrCode;
				registreAbre.Tag = RegistreViewParent.Registre.QrCode;
				registreAbre.FontWeight = FontWeights.Normal;
				registreAbre.Foreground = Brushes.White;

				// Définition de la lettre de référence pour ce registre
				if (RegistreViewParent.Registre.Type == "R4")
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
				#endregion

				#region RECUPERATION ET CREATION DES PAGES SPECIAUX
				// Création des Pages Spéciaux : PAGE DE GARDE : numero : -1 ET PAGE D'OUVERTURE : numero : 0			
				using (var ct = new DocumatContext())
				{
					//PAGE DE GARDE N°-1
					if (ct.Image.FirstOrDefault(i => i.RegistreID == RegistreViewParent.Registre.RegistreID && i.NumeroPage == -1) == null)
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
							if(MessageBox.Show("La PAGE DE GARDE n'a pas encore été Indexée(enregistrée), Voulez vous le faire maintenant ?", "INDEXER PAGE DE GARDE",
								MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
								pageGarde.CheminImage = System.IO.Path.Combine(RegistreViewParent.Registre.CheminDossier, file.Name);
								pageGarde.Type = file.Extension.Substring(1).ToUpper();
								pageGarde.Taille = file.Length;
								pageGarde.DateCreation = pageGarde.DateDebutSequence = pageGarde.DateModif = pageGarde.DateScan
								= pageGarde.DateScan = DateTime.Now;
								pageGarde.RegistreID = RegistreViewParent.Registre.RegistreID;
								pageGarde.StatutActuel = (int)Enumeration.Image.INDEXEE;
								ct.Image.Add(pageGarde);
								ct.SaveChanges();

								// Création du nouveau Statut 
								Models.StatutImage statutImage = new StatutImage();
								statutImage.ImageID = pageGarde.ImageID;
								statutImage.Code = (int)Enumeration.Image.INDEXEE;
								statutImage.DateCreation = statutImage.DateDebut = statutImage.DateModif = DateTime.Now;
								ct.StatutImage.Add(statutImage);
								ct.SaveChanges();

								//Enregistrement du traitement 
								DocumatContext.AddTraitement(DocumatContext.TbImage, pageGarde.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.CREATION);
							}
						}
					}

					//PAGE D'OUVERTURE N°0
					if (ct.Image.FirstOrDefault(i => i.RegistreID == RegistreViewParent.Registre.RegistreID && i.NumeroPage == 0) == null)
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
							if (MessageBox.Show("La PAGE D'OUVERTURE n'a pas encore été Indexée(enregistrée), Voulez vous le faire maintenant ?", "INDEXER PAGE DE GARDE",
								MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
							{
								pageOuverture.CheminImage = System.IO.Path.Combine(RegistreViewParent.Registre.CheminDossier, file.Name);
								pageOuverture.Type = file.Extension.Substring(1).ToUpper();
								pageOuverture.Taille = file.Length;
								pageOuverture.DateCreation = pageOuverture.DateDebutSequence = pageOuverture.DateModif = pageOuverture.DateScan
								= pageOuverture.DateScan = DateTime.Now;
								pageOuverture.RegistreID = RegistreViewParent.Registre.RegistreID;
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
								DocumatContext.AddTraitement(DocumatContext.TbImage, pageOuverture.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.CREATION);
							}
						}
					}
				}
				#endregion

				// Chargement de la première page/ou de la page en cours
				using (var ct = new DocumatContext())
				{
					Models.Image image = ct.Image.OrderBy(i => i.NumeroPage).FirstOrDefault(i => i.StatutActuel == (int)Enumeration.Image.CREEE
					&& i.RegistreID == RegistreViewParent.Registre.RegistreID);
					currentImage = (image != null) ? image.NumeroPage : -1;
					ChargerImage(currentImage);
				}

				// Modification des information d'entête
				HeaderInfosGetter();
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void SupprimeSequence_Click(object sender, RoutedEventArgs e)
		{
			if (dgSequence.SelectedItems.Count == 1)
			{
				if (MessageBox.Show("Voulez-vous Vraiment supprimer cette séquence ?","CONFIRMER SUPPRESSION",MessageBoxButton.YesNo,MessageBoxImage.Question)
					== MessageBoxResult.Yes)
                {
					SequenceView sequenceView = (SequenceView)dgSequence.SelectedItem;
					using (var ct = new DocumatContext())
                    {
						ct.Sequence.Remove(ct.Sequence.FirstOrDefault(s=>s.SequenceID == sequenceView.Sequence.SequenceID));
						ct.SaveChanges();
						DocumatContext.AddTraitement(DocumatContext.TbSequence, sequenceView.Sequence.SequenceID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.SUPPRESSION);

						// Refresh des données
						ImageView imageView1 = new ImageView();
						List<ImageView> imageViews = imageView1.GetSimpleViewsList(RegistreViewParent.Registre);
						imageView1 = imageViews.FirstOrDefault(i => i.Image.NumeroPage == currentImage);
						if (imageView1 != null)
						{
							List<Sequence> sequences = imageView1.context.Sequence.Where(s => s.ImageID == imageView1.Image.ImageID).OrderBy(s => s.NUmeroOdre).ToList();
							dgSequence.ItemsSource = SequenceView.GetViewsList(sequences);
							if (sequences.Count != 0)
								dgSequence.ScrollIntoView(dgSequence.Items.GetItemAt(dgSequence.Items.Count - 1));
						}
					}
                }
			}
		}

		private void AddSequence_Click(object sender, RoutedEventArgs e)
		{
			ChampsSequence.Visibility = Visibility.Visible;
			dgSequence.Height = dgSequenceMinHeight;
			editSequence = false;
			dgSequence.IsEnabled = false;
		}

		private void dgSequence_LoadingRow(object sender, DataGridRowEventArgs e)
		{

		}

		private void dgSequence_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}

		private void dgSequence_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
		{

		}

		private void btnNext_Click(object sender, RoutedEventArgs e)
		{

		}

		private void BtnSave_Click_1(object sender, RoutedEventArgs e)
		{

		}

		private void btnSave_Click_2(object sender, RoutedEventArgs e)
		{

		}

		private void BtnEnd_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btnSave_Click_3(object sender, RoutedEventArgs e)
		{

		}

		private void BtnCancel_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btnViderSequenceForm_Click(object sender, RoutedEventArgs e)
		{

		}

		private void BtnEditSequence_Click(object sender, RoutedEventArgs e)
		{

		}

		private void ModifSequence_Click(object sender, RoutedEventArgs e)
		{
			if(dgSequence.SelectedItems.Count == 1)
            {
				SequenceView sequenceView = (SequenceView)dgSequence.SelectedItem;
				ChampsSequence.Visibility = Visibility.Visible;
				dgSequence.Height = dgSequenceMinHeight; 
				tbNumeroOrdreSequence.Text = sequenceView.Sequence.NUmeroOdre.ToString();
				tbIsSpecial.Text = sequenceView.Sequence.isSpeciale.Trim();
				tbDateSequence.Text = sequenceView.Sequence.DateSequence.ToShortDateString().Trim();
				tbReference.Text = sequenceView.strReferences.Trim();
				tbNbRefPreindex.Text = sequenceView.Sequence.NombreDeReferences.ToString().Trim();

				dgSequence.IsEnabled = false;
				editSequence = true;
			}
		}

		private void BtnSupprimerSequence_Click(object sender, RoutedEventArgs e)
		{
			using (var ct = new DocumatContext())
			{
				List<Sequence> sequences = ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID
										   && s.References.ToLower() != "defaut").OrderBy(s => s.NUmeroOdre).ToList();
				if (sequences.Count > 0)
				{
					if (sequences.Last().NombreDeReferences >= 1)
					{
						int idseq = sequences.Last().SequenceID;
						Models.Sequence sq = ct.Sequence.FirstOrDefault(s => s.SequenceID == idseq);
						sq.References = "Defaut";
						sq.DateCreation = sq.DateSequence = sq.DateModif = DateTime.Now;
					}
					else
					{
						ct.Sequence.Remove(sequences.Last());
					}
					ct.SaveChanges();

					dgSequence.ItemsSource = SequenceView.GetViewsList(ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID
						 && s.References.ToLower() != "defaut").OrderBy(s => s.NUmeroOdre).ToList());
					if (dgSequence.Items.Count != 0)
						dgSequence.ScrollIntoView(dgSequence.Items.GetItemAt(dgSequence.Items.Count - 1));

					var LastSequence = ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID
									   && s.References.ToLower() != "defaut").OrderByDescending(i => i.NUmeroOdre).FirstOrDefault();
					tbNumeroOrdreSequence.Text = (LastSequence != null) ? (LastSequence.NUmeroOdre + 1).ToString() : CurrentImageView.Image.DebutSequence.ToString();
					tbDateSequence.Text = (LastSequence != null) ? LastSequence.DateSequence.ToShortDateString() : CurrentImageView.Image.DateDebutSequence.ToShortDateString();
					// On vide le tableau des références 
					tbReference.Text = "";
					References.Clear();
				}
			}
		}

		private void BtnImageSuivante_Click(object sender, RoutedEventArgs e)
		{
		}

		private void cbxSautOrdre_Checked(object sender, RoutedEventArgs e)
		{
		
		}

		private void cbxDoublonOrdre_Checked(object sender, RoutedEventArgs e)
		{
		}

		private void cbxBisOrdre_Checked(object sender, RoutedEventArgs e)
		{
		}

		private void cbxBisOrdre_Unchecked(object sender, RoutedEventArgs e)
		{

		}

		private void cbxDoublonOrdre_Unchecked(object sender, RoutedEventArgs e)
		{
		}

		private void cbxSautOrdre_Unchecked(object sender, RoutedEventArgs e)
		{
			
		}

		private void BtnTerminerImage_Click(object sender, RoutedEventArgs e)
		{
		}

		private void BtnMettreInstance_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Voulez vous vraiment mettre cette image en instance ?", "QUESTION", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
			{
				using (var ct = new DocumatContext())
				{
					Models.Image image = ct.Image.FirstOrDefault(i => i.ImageID == CurrentImageView.Image.ImageID);
					image.ObservationInstance = tbObservationInstance.Text;
					image.StatutActuel = (int)Enumeration.Image.INSTANCE;

					// Création de l'instance ou modification de l'instance
					StatutImage newstatut = ct.StatutImage.FirstOrDefault(s => s.ImageID == CurrentImageView.Image.ImageID && s.Code == (int)Enumeration.Image.INSTANCE);
					if (newstatut != null)
					{
						newstatut.DateDebut = newstatut.DateModif = DateTime.Now;
						newstatut.DateFin = DateTime.Now;
					}
					else
					{
						newstatut = new StatutImage();
						newstatut.ImageID = CurrentImageView.Image.ImageID;
						newstatut.Code = (int)Enumeration.Image.INSTANCE;
						newstatut.DateCreation = newstatut.DateDebut = newstatut.DateModif = DateTime.Now;
						ct.StatutImage.Add(newstatut);
					}

					ct.SaveChanges();
					TbkIndicateurImage.Text = "EN INSTANCE";
					panelChoixInstance.Visibility = Visibility.Collapsed;
					cbxRetirerInstance.IsEnabled = false;
				}

				HeaderInfosGetter();

				this.BtnImageSuivante_Click(null, new RoutedEventArgs());
				//Changement de l'aborescence
				// Définition des types d'icon dans l'aborescence en fonction des statuts des images
				foreach (TreeViewItem item in registreAbre.Items)
				{
					using (var ct = new DocumatContext())
					{
						if (ct.Image.FirstOrDefault(i => i.NomPage.ToLower() == item.Header.ToString().Remove(item.Header.ToString().Length - 4).ToLower()
							&& i.StatutActuel == (int)Enumeration.Image.INDEXEE && i.RegistreID == RegistreViewParent.Registre.RegistreID) != null)
							item.Tag = "valide";
						else if (ct.Image.FirstOrDefault(i => i.NomPage.ToLower() == item.Header.ToString().Remove(item.Header.ToString().Length - 4).ToLower()
							 && i.StatutActuel == (int)Enumeration.Image.INSTANCE && i.RegistreID == RegistreViewParent.Registre.RegistreID) != null)
							item.Tag = "instance";
						else
							item.Tag = GetFileFolderName(item.Header.ToString());
					}
				}
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

		private void tbDateSequence_TextChanged(object sender, TextChangedEventArgs e)
		{
		}

		private void tbDateSequence_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
		}

		private void tbDateSequence_LostFocus(object sender, RoutedEventArgs e)
		{
		}

		private void tbDateSequence_GotFocus(object sender, RoutedEventArgs e)
		{
			tbDateSequence.CaretIndex = 0;
		}

		private void tbReference_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
		}

		private void tbReference_LostFocus(object sender, RoutedEventArgs e)
		{
		}

		private void tbReference_TextChanged(object sender, TextChangedEventArgs e)
		{
		} 

		private void tbReference_GotFocus(object sender, RoutedEventArgs e)
		{
		}

		private void Window_GotFocus(object sender, RoutedEventArgs e)
		{
		}

		private void btnValideIndexation_Click(object sender, RoutedEventArgs e)
		{
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			MainParent.IsEnabled = true;
			MainParent.RefreshRegistre();
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			double MyWindowWidth = 1600;
			double MyWindowHeight = 900;
			double dgSMaxHeight = 400;

			// Gestion de l'IHM
			if (this.ActualHeight < MyWindowHeight)
			{
				dgSequence.MaxHeight = dgSMaxHeight - (MyWindowHeight - this.ActualHeight);
			}
			else
			{
				dgSequence.MaxHeight = dgSMaxHeight + (MyWindowHeight - this.ActualHeight);
			}
		}

        private void BtnSaveSequence_Click(object sender, RoutedEventArgs e)
        {
			var tbSequences = tbReference.Text.Trim().ToLower().Split(',');
			if(tbSequences.Distinct().Count() == tbSequences.Count())
            {
				if(editSequence && dgSequence.IsEnabled == false)
				{
					int NbRefsPreIndex = 0,NumeroOrdre = 0;
					DateTime DateSequence;
					Regex RefsReg = new Regex("^([0-9]{1,}/[\\w*]{1,},){1,}$");
					if (Int32.TryParse(tbNbRefPreindex.Text, out NbRefsPreIndex)
						&& Int32.TryParse(tbNumeroOrdreSequence.Text, out NumeroOrdre) && DateTime.TryParse(tbDateSequence.Text, out DateSequence)
						&& !string.IsNullOrWhiteSpace(tbReference.Text))
					{
						if(MessageBox.Show("Voulez-vous vraiment enregistrer les données ?","CONFIRMER",MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes)
						{
							SequenceView sequenceView = (SequenceView)dgSequence.SelectedItem;			
							using(var ct = new DocumatContext())
							{
								Models.Sequence sequence = ct.Sequence.FirstOrDefault(s => s.SequenceID == sequenceView.Sequence.SequenceID);
								sequence.DateModif = DateTime.Now;
								sequence.DateSequence = DateSequence;
								sequence.isSpeciale = tbIsSpecial.Text;
								sequence.NombreDeReferences = NbRefsPreIndex;
								sequence.NUmeroOdre = NumeroOrdre;
								sequence.References = tbReference.Text;
								ct.SaveChanges();
								DocumatContext.AddTraitement(DocumatContext.TbSequence, sequence.SequenceID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION);

								// Refresh des données
								ImageView imageView1 = new ImageView();
								List<ImageView> imageViews = imageView1.GetSimpleViewsList(RegistreViewParent.Registre);
								imageView1 = imageViews.FirstOrDefault(i => i.Image.NumeroPage == currentImage);
								if(imageView1 != null)
								{
									List<Sequence> sequences = imageView1.context.Sequence.Where(s => s.ImageID == imageView1.Image.ImageID).OrderBy(s => s.NUmeroOdre).ToList();
									dgSequence.ItemsSource = SequenceView.GetViewsList(sequences);
									if (sequences.Count != 0)
										dgSequence.ScrollIntoView(dgSequence.Items.GetItemAt(dgSequence.Items.Count - 1));
								}

								ChampsSequence.Visibility = Visibility.Collapsed;
								dgSequence.Height = dgSequenceMaxHeight;
								tbNumeroOrdreSequence.Text = "";
								tbIsSpecial.Text = "";
								tbDateSequence.Text = "";
								tbReference.Text = "";
								tbNbRefPreindex.Text = "";
								dgSequence.IsEnabled = true;
								editSequence = false;
								MessageBox.Show("La séquence : " + NumeroOrdre + " " + sequence.isSpeciale + ", a bien été modifié !", "Modification effectuée", MessageBoxButton.OK, MessageBoxImage.Information);
							}
						}
					}
				}
				else if(editSequence == false)
				{
					int NbRefsPreIndex = 0, NumeroOrdre = 0;
					DateTime DateSequence;
					if (Int32.TryParse(tbNbRefPreindex.Text, out NbRefsPreIndex)
						&& Int32.TryParse(tbNumeroOrdreSequence.Text, out NumeroOrdre) && DateTime.TryParse(tbDateSequence.Text, out DateSequence)
						&& !string.IsNullOrWhiteSpace(tbReference.Text))
					{
						if(MessageBox.Show("Voulez-vous vraiment enregistrer les données ?", "CONFIRMER", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
						{ 
							using (var ct = new DocumatContext())
							{
								Models.Sequence sequence = new Sequence() {
									DateCreation = DateTime.Now,
									DateModif = DateTime.Now,
									DateSequence = DateSequence,
									isSpeciale = tbIsSpecial.Text,
									NombreDeReferences = NbRefsPreIndex,
									NUmeroOdre = NumeroOrdre,
									PhaseActuelle = 0,
									References = tbReference.Text,
									ImageID = CurrentImageView.Image.ImageID,
								};
								sequence = ct.Sequence.Add(sequence);
								ct.SaveChanges();
								DocumatContext.AddTraitement(DocumatContext.TbSequence, sequence.SequenceID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.CREATION);

								// Refresh des données
								ImageView imageView1 = new ImageView();
								List<ImageView> imageViews = imageView1.GetSimpleViewsList(RegistreViewParent.Registre);
								imageView1 = imageViews.FirstOrDefault(i => i.Image.NumeroPage == currentImage);
								if (imageView1 != null)
								{
									List<Sequence> sequences = imageView1.context.Sequence.Where(s => s.ImageID == imageView1.Image.ImageID).OrderBy(s => s.NUmeroOdre).ToList();
									dgSequence.ItemsSource = SequenceView.GetViewsList(sequences);
									if (sequences.Count != 0)
										dgSequence.ScrollIntoView(dgSequence.Items.GetItemAt(dgSequence.Items.Count - 1));
								}

								ChampsSequence.Visibility = Visibility.Collapsed;
								dgSequence.Height = dgSequenceMaxHeight;
								tbNumeroOrdreSequence.Text = "";
								tbIsSpecial.Text = "";
								tbDateSequence.Text = "";
								tbReference.Text = "";
								tbNbRefPreindex.Text = "";
								dgSequence.IsEnabled = true;						
								MessageBox.Show("La séquence : " + sequence.NUmeroOdre + ", a bien été ajoutée !", "Ajout effectuée", MessageBoxButton.OK, MessageBoxImage.Information);
							}
						}
					}
				}
            }
			else
            {
				MessageBox.Show("Attention la liste des références contient des doublons, veuillez corriger ce problème avant d'enregistrer", "DOUBLON TROUVE", MessageBoxButton.OK, MessageBoxImage.Warning);
            }			
        }

        private void BtnAnnuleSequence_Click(object sender, RoutedEventArgs e)
        {
			ChampsSequence.Visibility = Visibility.Collapsed;
			dgSequence.Height = dgSequenceMaxHeight;
			tbNumeroOrdreSequence.Text = "";
			tbIsSpecial.Text = "";
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
			if (MessageBox.Show("Voulez vous vraiment levez l'instance de cet image ?", "QUESTION", MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
				using(var ct = new DocumatContext())
                {
					Models.Image image = ct.Image.FirstOrDefault(i => i.ImageID == CurrentImageView.Image.ImageID);
					if(image != null)
                    {
						string typeInstance = image.typeInstance;
						image.typeInstance = "";
						image.ObservationInstance = "";
						image.DateModif = DateTime.Now;
						image.StatutActuel = (int)Enumeration.Image.CREEE;

						// Création de l'instance ou modification de l'instance
						StatutImage newstatut = ct.StatutImage.FirstOrDefault(s => s.ImageID == image.ImageID && s.Code == (int)Enumeration.Image.CREEE);
						if (newstatut != null)
						{
							newstatut.DateDebut = newstatut.DateModif = DateTime.Now;
							newstatut.DateFin = DateTime.Now;
						}
						else
						{
							newstatut = new StatutImage();
							newstatut.ImageID = image.ImageID;
							newstatut.Code = (int)Enumeration.Image.CREEE;
							newstatut.DateCreation = newstatut.DateDebut = newstatut.DateModif = DateTime.Now;
							ct.StatutImage.Add(newstatut);
						}
						ct.SaveChanges();
						DocumatContext.AddTraitement(DocumatContext.TbImage, image.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION, "RETRAIT IMAGE D'INSTANCE : " + typeInstance);
						ChargerImage(image.NumeroPage);
					}
				}
            }
        }

        private void BtnImporterImage_Click(object sender, RoutedEventArgs e)
        {
			// Importation de l'image 
			try
			{
				OpenFileDialog open = new OpenFileDialog();
				open.Filter = "Fichier image |*.jpg;*.png;*.tif";
				open.Multiselect = false;
				open.FilterIndex = 3;

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
					|| CurrentImageView.Image.DateDebutSequence.ToShortDateString() != DateTime.Parse(tbDateDebSequence.Text).ToShortDateString())
                {
                    if (MessageBox.Show("Voulez-vous vraiment modifier les données ?", "MODIFIER LES DONNEES", MessageBoxButton.YesNo, MessageBoxImage.Question)
                        == MessageBoxResult.Yes)
                    {
                        int NumeroPage = CurrentImageView.Image.NumeroPage;
                        string NomPage = CurrentImageView.Image.NomPage, CheminFichier = CurrentImageView.Image.CheminImage,TypeFichier = CurrentImageView.Image.Type;
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
								NomPage = (NumeroPage > 9) ? NumeroPage.ToString() : $"0{NumeroPage}";
							}

                            // Modification du fichier image dans le dossier de scan 						
                            string nomFichierOld = CurrentImageView.Image.NomPage + "." + CurrentImageView.Image.Type.ToLower();
                            string nomFichier = NomPage + "." + CurrentImageView.Image.Type.ToLower();
                            CheminFichier = System.IO.Path.Combine(RegistreViewParent.Registre.CheminDossier, nomFichier);
                            if (!File.Exists(System.IO.Path.Combine(DossierRacine, CheminFichier)))
                            {
                                File.Move(System.IO.Path.Combine(DossierRacine, RegistreViewParent.Registre.CheminDossier, nomFichierOld)
                                    , System.IO.Path.Combine(DossierRacine, CheminFichier));								
                                viewImage(System.IO.Path.Combine(DossierRacine, CheminFichier));
                                //if (File.Exists(System.IO.Path.Combine(DossierRacine, CheminFichier)))
                                //{

                                //}
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
                            CheminFichier = System.IO.Path.Combine(RegistreViewParent.Registre.CheminDossier, nomFichier);
                            if (File.Exists(System.IO.Path.Combine(DossierRacine, CheminFichier)))
                            {
                                if (MessageBox.Show("Une Image portant le même nom existe dans le Dossier voulez vous la remplacer ?", "REMPLACER ?", MessageBoxButton.YesNo, MessageBoxImage.Question)
                                    == MessageBoxResult.Yes)
                                {
									File.Copy(FichierImporte, System.IO.Path.Combine(DossierRacine, CheminFichier),true);
                                }
                            }
                            else
                            {
								if(!string.IsNullOrEmpty(CurrentImageView.Image.CheminImage) &&
									File.Exists(Path.Combine(DossierRacine,CurrentImageView.Image.CheminImage)))
                                {
									File.Delete(Path.Combine(DossierRacine, CurrentImageView.Image.CheminImage));
                                }
                                File.Copy(FichierImporte, System.IO.Path.Combine(DossierRacine, CheminFichier));
                            }
                        }

                        using (var ct = new DocumatContext())
                        {
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
                                image.DateModif = DateTime.Now;
                                ct.SaveChanges();

								string Observation = "";
								if (FichierImporte != null)
									Observation = "REMPLACEMENT DE L'IMAGE source fichier : " + FichierImporte;

                                DocumatContext.AddTraitement(DocumatContext.TbImage, image.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION,Observation);
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
			if (string.IsNullOrWhiteSpace(tbNumeroPage.Text))
            {
				tbNumeroPage.Text = CurrentImageView.Image.NumeroPage.ToString();
            }
			else
            {
				if(CurrentImageView.Image.NumeroPage.ToString() != tbNumeroPage.Text.Trim())
                {
					BtnValiderModifImage.Foreground = Brushes.LightGreen;
				}
				else
                {
					if(string.IsNullOrWhiteSpace(FichierImporte) && CurrentImageView.Image.DebutSequence.ToString() == tbNumeroDebutSequence.Text.Trim()
						&& CurrentImageView.Image.FinSequence.ToString() == tbFinSequence.Text.Trim()
						&& CurrentImageView.Image.DateDebutSequence.ToShortDateString() == DateTime.Parse(tbDateDebSequence.Text).ToShortDateString())
                    {
						BtnValiderModifImage.Foreground = Brushes.White;
                    }
				}
			}
        }

        private void tbDateDebSequence_LostFocus(object sender, RoutedEventArgs e)
        {
			DateTime dateDebSeq;
			if (!string.IsNullOrWhiteSpace(tbDateDebSequence.Text) && DateTime.TryParse(tbDateDebSequence.Text.Trim(),out dateDebSeq))
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

        private void tbFinSequence_LostFocus(object sender, RoutedEventArgs e)
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

        private void tbNumeroDebutSequence_LostFocus(object sender, RoutedEventArgs e)
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

        private void ImporteImage_Click(object sender, RoutedEventArgs e)
        {
			// Importation de l'image 
			try
			{
				OpenFileDialog open = new OpenFileDialog();
				open.Filter = "Fichier image |*.jpg;*.png;*.tif";
				open.Multiselect = true;
				open.FilterIndex = 3;

				if (open.ShowDialog() == true)
				{
					List<FileInfo> filesInfos = new List<FileInfo>();
					foreach(var filePath in open.FileNames)
                    {
						FileInfo fileInfo = new FileInfo(filePath);
						if(File.Exists(System.IO.Path.Combine(DossierRacine, RegistreViewParent.Registre.CheminDossier, fileInfo.Name)))
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
					foreach(var fileInfo in filesInfos)
                    {
						File.Copy(fileInfo.FullName, System.IO.Path.Combine(DossierRacine, RegistreViewParent.Registre.CheminDossier, fileInfo.Name));
						ListeFileImport = ListeFileImport + fileInfo.Name + ",";
                    }
					ListeFileImport = ListeFileImport.Remove(ListeFileImport.Length - 1);

					// enregistrement du traitement 
					DocumatContext.AddTraitement(DocumatContext.TbRegistre, RegistreViewParent.Registre.RegistreID,MainParent.Utilisateur.AgentID,(int)Enumeration.TypeTraitement.MODIFICATION,"IMPORTATION IMAGE : " + ListeFileImport);
					ActualiserArborescence();
					MessageBox.Show(filesInfos.Count + " Image(s) : " + ListeFileImport + " ont/a été importée(s) avec success !!!","IMPORTATION EFFECTUEE",MessageBoxButton.OK,MessageBoxImage.Information);
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
                            string FilePath = System.IO.Path.Combine(DossierRacine, RegistreViewParent.Registre.CheminDossier, treeViewItem.Header.ToString());
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
                                    File.Delete(fileInfo.FullName);
									Models.Image image = ct.Image.FirstOrDefault(i => i.RegistreID == RegistreViewParent.Registre.RegistreID && i.NumeroPage == NumeroPage);
									if(image != null)
                                    {
										ct.Image.Remove(image);
										ct.SaveChanges();
										// Enregistrement du traitement de suppression
										DocumatContext.AddTraitement(DocumatContext.TbImage, image.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.SUPPRESSION,"SUPPRESSION IMAGE SEQUENCES : " + fileInfo.Name.ToUpper() + " DU REGISTRE ID : " + RegistreViewParent.Registre.RegistreID);
										ChargerImage(currentImage);
										MessageBox.Show("L'image : " + fileInfo.Name.ToUpper() + " ainsi que toutes ses séquences ont bien été supprimés !!!", "IMAGE SUPPRIMEE", MessageBoxButton.OK, MessageBoxImage.Information);								
                                    }
									else
                                    {
										// Enregistrement du traitement de suppression
										DocumatContext.AddTraitement(DocumatContext.TbImage, 0, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.SUPPRESSION, "SUPPRESSION IMAGE SANS SEQUENCES : " + fileInfo.Name.ToUpper() + " DU REGISTRE ID : " + RegistreViewParent.Registre.RegistreID);
										ActualiserArborescence();
										MessageBox.Show("L'image : " + fileInfo.Name.ToUpper() + ", n'avait pas de séquence et a été supprimé !!!", "IMAGE SUPPRIMEE", MessageBoxButton.OK, MessageBoxImage.Information);
									}
								}
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

        private void ModifierRegistre_Click(object sender, RoutedEventArgs e)
        {
			FormRegistre registre = new FormRegistre(RegistreViewParent, MainParent.Utilisateur);
			registre.Show();
            registre.Closed += Registre_Closed;
        }

        private void Registre_Closed(object sender, EventArgs e)
        {
			RegistreViewParent.Registre = RegistreViewParent.context.Registre.FirstOrDefault(r => r.RegistreID == RegistreViewParent.Registre.RegistreID);
			HeaderInfosGetter();
			ChargerImage(currentImage);
			MainParent.RefreshRegistre();
		}

        private void ChampsSequence_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
			if(ChampsSequence.IsVisible)
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
    }
}
