using DOCUMAT.Models;
using DOCUMAT.ViewModels;
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

namespace DOCUMAT.Pages.Image
{
    /// <summary>
    /// Logique d'interaction pour ImageViewer.xaml
    /// </summary>
    public partial class ImageViewer : Window
	{
		Indexation.Indexation MainParent;

		RegistreView RegistreViewParent;		
		ImageView CurrentImageView;		
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
		Dictionary<String, String> References = new Dictionary<String,String>();

		#region FONCTIONS			
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
					AgentImage.Source = new BitmapImage(new Uri(Path.Combine(DossierRacine, MainParent.Utilisateur.CheminPhoto)), new System.Net.Cache.RequestCachePolicy());
				}
				tbxAgentLogin.Text = "Login : " +  MainParent.Utilisateur.Login;
				tbxAgentNoms.Text = "Noms : " +  MainParent.Utilisateur.Noms;
				tbxAgentType.Text = "Aff : " + Enum.GetName(typeof(Enumeration.AffectationAgent),MainParent.Utilisateur.Affectation);

				tbxQrCode.Text = "Qrcode : " + RegistreViewParent.Registre.QrCode;
				tbxVersement.Text = "N° Versement : " + RegistreViewParent.Versement.NumeroVers.ToString();
				tbxService.Text =  "Service : " + RegistreViewParent.ServiceVersant.Nom;
				//tbAgent.Text = 
				//Procédure de modification de l'indicateur de reussite
				double perTerminer = (((float)imageViews.Where(i => i.Image.StatutActuel == (int)Enumeration.Image.INDEXEE).Count() / (float)imageViews.Count()) * 100);
				string Instance =  (Math.Round(((float)imageViews.Where(i => i.Image.StatutActuel == (int)Enumeration.Image.INSTANCE).Count() / (float)imageViews.Count()) * 100,1)).ToString() + " %";
				string EnCours =  (Math.Round(((float)imageViews.Where(i => i.Image.StatutActuel == (int)Enumeration.Image.CREEE).Count() / (float)imageViews.Count()) * 100,1)).ToString() + " %";
				string Scanne =  (Math.Round(((float)imageViews.Where(i => i.Image.StatutActuel == (int)Enumeration.Image.SCANNEE).Count() / (float)imageViews.Count()) * 100,1)).ToString() + " %";
				tbxImageTerminer.Text = "Terminé : " + Math.Round(perTerminer,1) + " %" ;
				tbxImageInstance.Text = "En Instance :" + Instance;
				tbxImageEnCours.Text =  "Non Traité : " + EnCours;
				ArcIndicator.EndAngle = (perTerminer * 360) / 100;
				TextIndicator.Text = Math.Round(perTerminer, 1) + "%";
			}

			/// <summary>
			/// Actualise l'aborescence en fonction des changements lors de l'indexation !!!
			/// </summary>
			private void ActualiserArborescence()
			{
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
								if(image.NomPage == item.Header.ToString().Remove(item.Header.ToString().Length - 4).ToLower())
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
						if (item.Header.ToString().Remove(item.Header.ToString().Length - 4).ToLower() == "PAGE DE GARDE".ToLower())
						{
							item.FontWeight = FontWeights.Bold;
						}
					}
					else if (currentImage == 0)
					{						
						if (item.Header.ToString().Remove(item.Header.ToString().Length - 4).ToLower() == "PAGE D'OUVERTURE".ToLower())
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

		public ImageViewer()
		{
			InitializeComponent();
			//Grid.Children.Add(sfCircularProgressBar);
		}

		public ImageViewer(RegistreView registreview,Indexation.Indexation indexation) : this()
		{
			MainParent = indexation;
			RegistreViewParent = registreview;
        }

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
				if(imageView1 != null)
                {
					// Chargement de la visionneuse
					if (File.Exists(Path.Combine(DossierRacine, imageView1.Image.CheminImage)))
					{
						viewImage(Path.Combine(DossierRacine, imageView1.Image.CheminImage));
					}
					else
					{
						MessageBox.Show("La page : \"" + imageView1.Image.NomPage + "\" est introuvable !!!", "AVERTISSEMENT", MessageBoxButton.OK, MessageBoxImage.Warning);
						return;
					}

					//On change l'image actuelle
					CurrentImageView = imageView1;
					this.currentImage = currentImage;

					if(imageView1.Image.NumeroPage == -1)
					{
						tbxNomPage.Text = "PAGE DE GARDE";
						tbxNumeroPage.Text = "";
						tbxDebSeq.Text = "";
						tbxFinSeq.Text = "";
					}
					else if(imageView1.Image.NumeroPage == 0)
					{
						tbxNomPage.Text = "PAGE D'OUVERTURE";
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
					// On réinitialise l'état des cbx saut,doublon et bis pour le rechargement
					cbxSautOrdre.IsChecked = false;
					cbxDoublonOrdre.IsChecked = false;
					cbxBisOrdre.IsChecked = false;

					// Récupération des sequences déja renseignées, Différent des images préindexer ayant la référence défaut
					List<Sequence> sequences = imageView1.context.Sequence.Where(s => s.ImageID == imageView1.Image.ImageID
					&& s.References.ToLower() != "defaut").OrderBy(s => s.NUmeroOdre).ToList();
					dgSequence.ItemsSource = SequenceView.GetViewsList(sequences);
					if (sequences.Count != 0)
						dgSequence.ScrollIntoView(dgSequence.Items.GetItemAt(dgSequence.Items.Count - 1));

					// Récupération de la dernière séquence et renseignement des champs d'ajout de séquence
					var LastSequence = imageView1.context.Sequence.Where(s => s.ImageID == imageView1.Image.ImageID && s.References.ToLower() != "defaut").OrderByDescending(i => i.NUmeroOdre).FirstOrDefault();
					tbNumeroOrdreSequence.Text = (LastSequence != null) ? (LastSequence.NUmeroOdre + 1).ToString() : imageView1.Image.DebutSequence.ToString();
					tbDateSequence.Text = (LastSequence != null) ? LastSequence.DateSequence.ToShortDateString() : imageView1.Image.DateDebutSequence.ToShortDateString();
					tbReference.Text = "";

					// récupération de la séquence préindexer devant être indexer 
					int numeroOrdreSequence = (LastSequence != null) ? LastSequence.NUmeroOdre : imageView1.Image.DebutSequence;
					Sequence sequenceActu = imageView1.context.Sequence.Where(s => s.ImageID == imageView1.Image.ImageID && s.References.ToLower() == "defaut" && s.NUmeroOdre == (numeroOrdreSequence)).FirstOrDefault();
					if (sequenceActu != null)
					{
						tbxNbRefs.Text = "Refs : " + sequenceActu.NombreDeReferences;
						cbxBisOrdre.IsChecked = (sequenceActu.isSpeciale.ToLower().Contains("bis")) ? true : false;
						cbxDoublonOrdre.IsChecked = (sequenceActu.isSpeciale.ToLower().Contains("doublon")) ? true : false;
					}
					else
                    {
						sequenceActu = imageView1.context.Sequence.Where(s => s.ImageID == imageView1.Image.ImageID && s.References.ToLower() == "defaut" && s.NUmeroOdre == (numeroOrdreSequence + 1)).FirstOrDefault();
						if(sequenceActu != null)
                        {
							tbxNbRefs.Text = "Refs : " + sequenceActu.NombreDeReferences;
                        }
						else
                        {
							tbxNbRefs.Text = "Refs : ND";
                        }
					}

					// On vide le tableau des références 
					References.Clear();
					tbListeReferences.Visibility = Visibility.Collapsed;
					tbxNbRefsList.Text = "Nb : 0";
					tbxNbRefsList.Visibility = Visibility.Collapsed;

					//On donne le focus à la date 
					tbDateSequence.Focus();
					#endregion

					#region ADAPTATION DE L'AFFICHAGE EN FONCTION DES INSTANCES DE L'IMAGE
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
					}
					else if (imageView1.Image.StatutActuel == (int)Enumeration.Image.CREEE)
					{
						//On vérifie que lindexation de l'image est terminée ou en instance
						imageView1 = imageViews.OrderBy(i=>i.Image.NumeroPage).FirstOrDefault(i=>i.Image.StatutActuel == (int)Enumeration.Image.CREEE);					
						if (imageView1 != null)
						{
							if(imageView1.Image.NumeroPage == currentImage)
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

								// Vérifie si l'image en cours n'a pas toutes les séquences renseignées
								if (LastSequence != null)
								{
									if (imageView1.Image.FinSequence == LastSequence.NUmeroOdre)
									{
										if (cbxDoublonOrdre.IsChecked == true || cbxBisOrdre.IsChecked == true)
										{
											BtnAddSequence.IsEnabled = true;
											BtnTerminerImage.IsEnabled = false;
										}
										else
										{
											BtnAddSequence.IsEnabled = false;
											BtnTerminerImage.IsEnabled = true;
										}
									}
								}
							}
							else
							{
								currentImage = -1;
								ChargerImage(currentImage);
							}
						}
						else
						{
							currentImage = -1;
							ChargerImage(currentImage);
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
					}
					//Affichage du bouton de validadtion d'indexation si le 
					if (imageViews.All(i => i.Image.StatutActuel == (int)Enumeration.Image.INDEXEE))
					{
						btnValideIndexation.Visibility = Visibility.Visible;
					}
					else
					{
						btnValideIndexation.Visibility = Visibility.Collapsed;
					}
					#endregion

					// Actualisation de l'aborescence
					ActualiserArborescence();
                }
				else
                {
					MessageBox.Show("Impossible de charger la Page, Numero : " + currentImage + " , Elle n'est pas enregistrée !!!","AVERTISSEMENT", MessageBoxButton.OK, MessageBoxImage.Warning);
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
			FileInfo fileInfo = new FileInfo(Path.Combine(DossierRacine, RegistreViewParent.CheminDossier,treeViewItem.Header.ToString()));

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
					imageView1 = imageViews.FirstOrDefault(i => i.Image.NumeroPage == -1);

					tbxNomPage.Text = "PAGE DE GARDE";
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
					imageView1 = imageViews.FirstOrDefault(i => i.Image.NumeroPage == 0);				

					tbxNomPage.Text = "PAGE D'OUVERTURE";
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
				MessageBox.Show("L'image n'existe pas dans le dossier de scan !", "Image Introuvable", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                var files = Directory.GetFiles(Path.Combine(DossierRacine, RegistreViewParent.Registre.CheminDossier));
				List<FileInfo> fileInfos1 = new List<FileInfo>();

				// Récupération des fichiers dans un fileInfo
				foreach(var file in files)
                {
					fileInfos1.Add(new FileInfo(file));
                }
				fileInfos1 = fileInfos1.Where(f => f.Extension.ToLower() == ".tif" || f.Extension.ToLower() == ".jpg" || f.Extension.ToLower() == ".png").ToList();

				// Si le nombre de Page Scannée est différent du nombre de page attendu, signal
				// Le Plus Trois est composée : de la page de garde + la page d'ouverture 
				if ((RegistreViewParent.Registre.NombrePage + 2 ) != fileInfos1.Count())
                {
					MessageBox.Show("Attention le nombre de Page Scannée est différents du nombre de page attendu : " +
									"\n Nombre pages scannées: " + fileInfos1.Count() +
									"\n Nombre pages attendues : " + (RegistreViewParent.Registre.NombrePage + 2),"AVERTISSEMENT",MessageBoxButton.OK,MessageBoxImage.Asterisk) ;
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
						pageGarde.NomPage = "PAGE DE GARDE";
						var file = fileInfos.FirstOrDefault(f => f.Name.Remove(f.Name.Length - 4) == pageGarde.NomPage);

						//On vérifie que le fichier existe 
						if (file == null)
                        {
							MessageBox.Show("La PAGE DE GARDE est introuvable dans le Dossier !!!", "AVERTISSEMENT", MessageBoxButton.OK,MessageBoxImage.Warning);
                        }
						else
                        {
							pageGarde.CheminImage = Path.Combine(RegistreViewParent.Registre.CheminDossier,file.Name);
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

							// Enregistrement de l'action effectué par l'agent
							DocumatContext.AddTraitement(DocumatContext.TbImage, pageGarde.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.CREATION, "INDEXATION DE LA PAGE DE GARDE DU REGISTRE ID N° " + pageGarde.RegistreID);
						}
					}

					//PAGE D'OUVERTURE N°0
					if (ct.Image.FirstOrDefault(i => i.RegistreID == RegistreViewParent.Registre.RegistreID && i.NumeroPage == 0) == null)
					{
						Models.Image pageOuverture = new Models.Image();
						pageOuverture.NumeroPage = 0;
						pageOuverture.NomPage = "PAGE D'OUVERTURE";
						var file = fileInfos.FirstOrDefault(f => f.Name.Remove(f.Name.Length - 4) == pageOuverture.NomPage);

						//On vérifie que le fichier existe 
						if (file == null)
                        {
							MessageBox.Show("La PAGE D'OUVERTURE est introuvable dans le Dossier !!!", "AVERTISSEMENT",MessageBoxButton.OK,MessageBoxImage.Warning);
                        }
						else
                        {
							pageOuverture.CheminImage = Path.Combine(RegistreViewParent.Registre.CheminDossier, file.Name);
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

							// Enregistrement de l'action effectué par l'agent
							DocumatContext.AddTraitement(DocumatContext.TbImage, pageOuverture.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.CREATION, "INDEXATION DE LA PAGE D'OUVERTURE DU REGISTRE ID N° " + pageOuverture.RegistreID);
						}
					}
				}
				#endregion

				#region RECUPERATION OU CREATION DES PAGES NUMEROTEES
				// Modification des Pages Numerotées / Pré-Indexées
				// Les Pages doivent être scannées de manière à respecter la nomenclature de fichier standard 
				ImageView imageView1 = new ImageView();
				string ListPageIntrouvable = "";
				List<ImageView> imageViews = imageView1.GetSimpleViewsList(RegistreViewParent.Registre);
				foreach (var imageView in imageViews)
				{
					if (imageView.Image.StatutActuel == (int)Enumeration.Image.SCANNEE)
					{
						string nomFile = (imageView.Image.NumeroPage > 9) ? imageView.Image.NumeroPage.ToString() : $"0{imageView.Image.NumeroPage}";
						FileInfo file = fileInfos.FirstOrDefault(f => f.Name.Remove(f.Name.Length - 4) == nomFile);
						if (file != null)
						{
							using (var ct = new DocumatContext())
							{
								var image = ct.Image.FirstOrDefault(i => i.ImageID == imageView.Image.ImageID);
								image.CheminImage = Path.Combine(RegistreViewParent.Registre.CheminDossier, file.Name);
								image.Type = file.Extension.Substring(1).ToUpper();
								image.Taille = file.Length;
								image.DateScan = file.LastWriteTime;
								image.DateModif = DateTime.Now;
								image.DateCreation = DateTime.Now;
								image.StatutActuel = (int)Enumeration.Image.CREEE;
								image.NomPage = nomFile	;
								ct.SaveChanges();

								// Création du nouveau Statut 
								Models.StatutImage statutImage = new StatutImage();
								statutImage.ImageID = image.ImageID;
								statutImage.Code = (int)Enumeration.Image.CREEE;
								statutImage.DateCreation = statutImage.DateDebut = statutImage.DateModif = DateTime.Now;
								ct.StatutImage.Add(statutImage);
								ct.SaveChanges();

								// Enregistrement de l'action effectuée par l'agent
								DocumatContext.AddTraitement(DocumatContext.TbImage, image.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION, "INDEXATION VERIFICATION DU SCAN DE LA PAGE N° : "+ image.NumeroPage+" DU REGISTRE ID N° " + image.RegistreID);
							}
						}
						else
						{
							ListPageIntrouvable = ListPageIntrouvable + imageView.Image.NumeroPage + " ,";
						}
					}
				}

				if(ListPageIntrouvable != "")
                {
					//ListPageIntrouvable = ListPageIntrouvable.Remove()
					MessageBox.Show("Impossible de retrouver La/les Page(s) de numéro : " + ListPageIntrouvable,"AVERTISSEMENT", MessageBoxButton.OK,MessageBoxImage.Warning);
                }
				#endregion

				// Chargement de la première page/ou de la page en cours
				using(var ct = new DocumatContext())
                {
					Models.Image image = ct.Image.OrderBy(i => i.NumeroPage).FirstOrDefault(i => i.StatutActuel == (int)Enumeration.Image.CREEE
					&& i.RegistreID == RegistreViewParent.Registre.RegistreID);
					currentImage = (image != null) ? image.NumeroPage : -1;
					ChargerImage(currentImage);
                }

				// Modification des information d'entête
				HeaderInfosGetter();

				#region VERIFICATION DE L'AGENT 				
				using (var ct = new DocumatContext())
				{
					if (MainParent.Utilisateur.Affectation != (int)Enumeration.AffectationAgent.ADMINISTRATEUR && MainParent.Utilisateur.Affectation != (int)Enumeration.AffectationAgent.SUPERVISEUR)
					{
						Models.Traitement traitementAttr = ct.Traitement.FirstOrDefault(t => t.TableSelect == DocumatContext.TbRegistre && t.TableID == RegistreViewParent.Registre.RegistreID
													&& t.TypeTraitement == (int)Enumeration.TypeTraitement.REGISTRE_ATTRIBUE_INDEXATION && t.AgentID == MainParent.Utilisateur.AgentID);
						if (traitementAttr != null)
						{

							Models.Traitement traitementRegistreAgent = ct.Traitement.FirstOrDefault(t => t.TableSelect == DocumatContext.TbRegistre && t.TableID == RegistreViewParent.Registre.RegistreID
													&& t.TypeTraitement == (int)Enumeration.TypeTraitement.INDEXATION_REGISTRE_DEBUT && t.AgentID == MainParent.Utilisateur.AgentID);
							Models.Traitement traitementAutreAgent = ct.Traitement.FirstOrDefault(t => t.TableSelect == DocumatContext.TbRegistre && t.TableID == RegistreViewParent.Registre.RegistreID
													&& t.TypeTraitement == (int)Enumeration.TypeTraitement.INDEXATION_REGISTRE_DEBUT && t.AgentID != MainParent.Utilisateur.AgentID);

							if (traitementAutreAgent != null)
							{
								Models.Agent agent = ct.Agent.FirstOrDefault(t => t.AgentID == traitementAutreAgent.AgentID);
								MessageBox.Show("Ce Registre est en Cours d'Indexation par l'agent : " + agent.Noms);
								this.Close();
							}
							else
							{
								if (traitementRegistreAgent == null)
								{
									if (MessageBox.Show("Voulez vous commencez l'indexation ?", "COMMNCER L'INDEXATION", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
									{
										DocumatContext.AddTraitement(DocumatContext.TbRegistre, RegistreViewParent.Registre.RegistreID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.INDEXATION_REGISTRE_DEBUT);
									}
									else
									{
										this.Close();
									}
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
			}
			catch (Exception ex)
			{
				ex.ExceptionCatcher();
			}
		}

		private void SupprimeSequence_Click(object sender, RoutedEventArgs e)
		{

		}

		private void AddSequence_Click(object sender, RoutedEventArgs e)
		{

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

		}

		private void BtnAddSequence_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				int numeroOdre = 0;
				if (Int32.TryParse(tbNumeroOrdreSequence.Text, out numeroOdre) && tbDateSequence.Text != "" && CurrentImageView.Image != null 
					&& (cbxSautOrdre.IsChecked == true || References.Count > 0))
				{
					var LastSequence = CurrentImageView.context.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID
									   && s.References.ToLower() != "defaut").OrderByDescending(i => i.NUmeroOdre).FirstOrDefault();
					if (LastSequence == null || LastSequence.NUmeroOdre < CurrentImageView.Image.FinSequence ||
						(LastSequence.NUmeroOdre == CurrentImageView.Image.FinSequence && (cbxDoublonOrdre.IsChecked == true || cbxBisOrdre.IsChecked == true)))
					{
						// Définition d'une séquence spéciale
						string isSpeciale = "";
						if (cbxSautOrdre.IsChecked == true)
							isSpeciale = "saut";
						else if (cbxDoublonOrdre.IsChecked == true)
							isSpeciale = "doublon";
						else if (cbxBisOrdre.IsChecked == true)
							isSpeciale = "bis";
						Models.Sequence sequence;
						using (var ct = new DocumatContext())
						{
							sequence = ct.Sequence.FirstOrDefault(s => s.ImageID == CurrentImageView.Image.ImageID && s.NUmeroOdre == numeroOdre);
							if (sequence != null)
							{
								if (cbxDoublonOrdre.IsChecked == true || cbxBisOrdre.IsChecked == true)
								{								
									var sequenceSpecial = ct.Sequence.FirstOrDefault(s => s.ImageID == CurrentImageView.Image.ImageID && s.NUmeroOdre == numeroOdre
										&& s.isSpeciale.ToLower().Contains(isSpeciale) && s.References.ToLower() == "defaut");
									if(sequenceSpecial != null)
                                    {
										
										if (References.Count == sequenceSpecial.NombreDeReferences)
										{
											// Un contrôle de référence/indexée doit être fait avant l'update
											sequenceSpecial.DateSequence = DateTime.Parse(tbDateSequence.Text);
											sequenceSpecial.DateCreation = DateTime.Now;
											sequenceSpecial.DateModif = DateTime.Now;
											sequenceSpecial.References = "";
											foreach (var refer in References)
											{
												if(References.Last().Key != refer.Key)
													sequenceSpecial.References = sequenceSpecial.References + refer.Value + ",";
												else
													sequenceSpecial.References = sequenceSpecial.References + refer.Value;
											}
										}
										else
										{
											MessageBox.Show("Le Nombre de Références que Vous avez saisi est différent du Nombre de Référence Préindexé " +
												"\n Nombre Références Saisi : " + References.Count +
												"\n Nombre Références Préindexé : " + sequenceSpecial.NombreDeReferences, "NOMBRE DE REFERENCE INCORRECTE", MessageBoxButton.OK, MessageBoxImage.Warning);
											return;
										}
                                    }
									else
                                    {
										// Ajout des doublons et des bis
										sequence = new Sequence();
										sequence.DateCreation = DateTime.Now;
										sequence.DateModif = DateTime.Now;
										sequence.DateSequence = DateTime.Parse(tbDateSequence.Text);
										sequence.NUmeroOdre = numeroOdre;
										sequence.isSpeciale = isSpeciale;
										sequence.ImageID = CurrentImageView.Image.ImageID;
										sequence.References = "";
										foreach (var refer in References)
										{
											if (References.Last().Key != refer.Key)
												sequence.References = sequence.References + refer.Value + ",";
											else
												sequence.References = sequence.References + refer.Value;
										}
										ct.Sequence.Add(sequence);
                                    }
								}
								else
								{
									if (cbxSautOrdre.IsChecked != true)
									{
										if (References.Count == sequence.NombreDeReferences)
										{
											// Un contrôle de référence/indexée doit être fait avant l'update
											sequence.DateSequence = DateTime.Parse(tbDateSequence.Text);
											sequence.DateCreation = DateTime.Now;
											sequence.DateModif = DateTime.Now;
											sequence.isSpeciale = isSpeciale;
											sequence.References = "";
											foreach (var refer in References)
											{
												if (References.Last().Key != refer.Key)
													sequence.References = sequence.References + refer.Value + ",";
												else
													sequence.References = sequence.References + refer.Value;
											}
										}
										else
                                        {
											MessageBox.Show("Le Nombre de Références que Vous avez saisi est différent du Nombre de Référence Préindexé " +
															"\n Nombre Références Saisi : " + References.Count +
															"\n Nombre Références Préindexé : " + sequence.NombreDeReferences, "NOMBRE DE REFERENCE INCORRECTE", MessageBoxButton.OK, MessageBoxImage.Warning);
											return;
										}
									}
									else
									{
										throw new Exception("Impossible de mettre un saut de séquence/ligne à une ligne/séquence déja préindexer");
									}
								}

								ct.SaveChanges();
								// Enregistrement de l'action effectuée par l'agent
								DocumatContext.AddTraitement(DocumatContext.TbSequence, sequence.SequenceID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION, "INDEXATION  DE LA SEQUENCE N° " + sequence.NUmeroOdre + "DE L'IMAGE N° " + CurrentImageView.Image.NumeroPage + " DU REGISTRE ID N° " + CurrentImageView.Image.RegistreID);
							}
							else
							{
								//Ajout d'un nouvel element normal
								sequence = new Sequence();
								sequence.References = "";
								foreach (var refer in References)
								{
									if (References.Last().Key != refer.Key)
										sequence.References = sequence.References + refer.Value + ",";
									else
										sequence.References = sequence.References + refer.Value;
								}
								sequence.DateCreation = DateTime.Now;
								sequence.DateModif = DateTime.Now;
								sequence.DateSequence = DateTime.Parse(tbDateSequence.Text);
								sequence.NUmeroOdre = numeroOdre;
								sequence.isSpeciale = isSpeciale;
								sequence.ImageID = CurrentImageView.Image.ImageID;
								if (cbxSautOrdre.IsChecked == true)
								{
									sequence.References = "saut";
								}
								ct.Sequence.Add(sequence);
								ct.SaveChanges();	

								// Enregistrement de l'action effectuée par l'agent
								DocumatContext.AddTraitement(DocumatContext.TbSequence, sequence.SequenceID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.CREATION, "INDEXATION (AJOUT/INDEX) DE LA SEQUENCE N° " + sequence.NUmeroOdre + "DE L'IMAGE N° " + CurrentImageView.Image.NumeroPage + " DU REGISTRE ID N° " + CurrentImageView.Image.RegistreID);
							}

							dgSequence.ItemsSource = SequenceView.GetViewsList(ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID
								 && s.References.ToLower() != "defaut").OrderBy(s => s.NUmeroOdre).ToList());
							if (dgSequence.Items.Count != 0)
								dgSequence.ScrollIntoView(dgSequence.Items.GetItemAt(dgSequence.Items.Count - 1));

							LastSequence = ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID
											   && s.References.ToLower() != "defaut").OrderByDescending(i => i.NUmeroOdre).FirstOrDefault();
							tbNumeroOrdreSequence.Text = (LastSequence != null) ? (LastSequence.NUmeroOdre + 1).ToString() : CurrentImageView.Image.DebutSequence.ToString();
							tbDateSequence.Text = (LastSequence != null) ? LastSequence.DateSequence.ToShortDateString() : CurrentImageView.Image.DateDebutSequence.ToShortDateString();
							// On vide le tableau des références 
							tbReference.Text = "";
							References.Clear();
							tbListeReferences.Visibility = Visibility.Collapsed;
							tbxNbRefsList.Text = "Nb : 0";
							tbxNbRefsList.Visibility = Visibility.Collapsed;

							//On donne le focus à la date 
							tbDateSequence.Focus();

							cbxSautOrdre.IsChecked = false;
							cbxBisOrdre.IsChecked = false;
							cbxDoublonOrdre.IsChecked = false;

							// Récupération du Nombre de référence et du Bis/Doublon éventuel du prochain
							// récupération de la séquence préindexer devant être indexer 
							int numeroOrdreSequence = (LastSequence != null) ? LastSequence.NUmeroOdre : CurrentImageView.Image.DebutSequence;
							Sequence sequenceActu = CurrentImageView.context.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID && s.References.ToLower() == "defaut" && s.NUmeroOdre == (numeroOrdreSequence)).FirstOrDefault();
							if (sequenceActu != null)
							{
								tbxNbRefs.Text = "Refs : " + sequenceActu.NombreDeReferences;
								cbxBisOrdre.IsChecked = (sequenceActu.isSpeciale.ToLower().Contains("bis")) ? true : false;
								cbxDoublonOrdre.IsChecked = (sequenceActu.isSpeciale.ToLower().Contains("doublon")) ? true : false;
							}
							else
							{
								sequenceActu = CurrentImageView.context.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID && s.References.ToLower() == "defaut" && s.NUmeroOdre == (numeroOrdreSequence + 1)).FirstOrDefault();
								if (sequenceActu != null)
								{
									tbxNbRefs.Text = "Refs : " + sequenceActu.NombreDeReferences;
								}
								else
								{
									tbxNbRefs.Text = "Refs : ND";
								}
							}						

							// permet de vérouiller l'ajout et de dévérouiller le btn Terminer image 
							if (dgSequence.Items.Count > 0)
                            {								
								if (((List<SequenceView>)dgSequence.ItemsSource).OrderByDescending(s => s.Sequence.NUmeroOdre).FirstOrDefault().Sequence.NUmeroOdre == CurrentImageView.Image.FinSequence)
								{
									if(cbxDoublonOrdre.IsChecked == true || cbxBisOrdre.IsChecked == true)
                                    {
										BtnAddSequence.IsEnabled = true;
										BtnTerminerImage.IsEnabled = false;
									}
									else
                                    {
										BtnAddSequence.IsEnabled = false;
										BtnTerminerImage.IsEnabled = true;
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

		private void BtnSupprimerSequence_Click(object sender, RoutedEventArgs e)
		{
			using (var ct = new DocumatContext())
			{
				List<Sequence> sequences = ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID 
										   && s.References.ToLower() != "defaut").OrderBy(s => s.NUmeroOdre).ToList();
				if (sequences.Count > 0)
				{
					Sequence sequence = sequences.Last();
					if(sequences.Last().NombreDeReferences >= 1)
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

					// Enregistrement de l'action effectuée par l'agent
					DocumatContext.AddTraitement(DocumatContext.TbSequence, sequence.SequenceID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.SUPPRESSION, "INDEXATION (SUPPRESSION) DE LA SEQUENCE N° " + sequence.NUmeroOdre + " DE L'IMAGE N° " + CurrentImageView.Image.NumeroPage + " DU REGISTRE ID N° " + CurrentImageView.Image.RegistreID);

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
					tbListeReferences.Visibility = Visibility.Collapsed;

					// récupération de la séquence préindexer devant être indexer 
					cbxSautOrdre.IsChecked = false;
					cbxBisOrdre.IsChecked = false;
					cbxDoublonOrdre.IsChecked = false;

					// récupération de la séquence préindexer devant être indexer 
					int numeroOrdreSequence = (LastSequence != null) ? LastSequence.NUmeroOdre : CurrentImageView.Image.DebutSequence;
					Sequence sequenceActu = CurrentImageView.context.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID && s.References.ToLower() == "defaut" && s.NUmeroOdre == (numeroOrdreSequence)).FirstOrDefault();
					if (sequenceActu != null)
					{
						tbxNbRefs.Text = "Refs : " + sequenceActu.NombreDeReferences;
						cbxBisOrdre.IsChecked = (sequenceActu.isSpeciale.ToLower().Contains("bis")) ? true : false;
						cbxDoublonOrdre.IsChecked = (sequenceActu.isSpeciale.ToLower().Contains("doublon")) ? true : false;
					}
					else
					{
						sequenceActu = CurrentImageView.context.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID && s.References.ToLower() == "defaut" && s.NUmeroOdre == (numeroOrdreSequence + 1)).FirstOrDefault();
						if (sequenceActu != null)
						{
							tbxNbRefs.Text = "Refs : " + sequenceActu.NombreDeReferences;
						}
						else
						{
							tbxNbRefs.Text = "Refs : ND";
						}
					}

					//On donne le focus à la date 
					tbDateSequence.Focus();

					if (LastSequence != null)
					{
						if (LastSequence.NUmeroOdre == CurrentImageView.Image.FinSequence)
						{
							if (cbxDoublonOrdre.IsChecked == true || cbxBisOrdre.IsChecked == true)
							{
								BtnAddSequence.IsEnabled = true;
								BtnTerminerImage.IsEnabled = false;
							}
							else
							{
								BtnAddSequence.IsEnabled = false;
								BtnTerminerImage.IsEnabled = true;
							}
						}
						else
						{
							BtnAddSequence.IsEnabled = true;
							BtnTerminerImage.IsEnabled = false;
						}
					}
					else
					{
						BtnAddSequence.IsEnabled = true;
						BtnTerminerImage.IsEnabled = false;
					}
				}
			}
		}

		private void BtnImageSuivante_Click(object sender, RoutedEventArgs e)
		{
			//on vérifie que tous t'es ajouté dans la BD;
			List<ImageView> images = CurrentImageView.GetSimpleViewsList(RegistreViewParent.Registre);

			//MessageBox.Show("Images Disk " + fileInfos.Count + " / images Bd : " + images.Count + " / image actuelle : " + currentImage.ToString());
			if (fileInfos.Count == images.Count)
			{
				if ((currentImage + 2) == images.Count)
				{
					//MessageBox.Show("Réinitialisation !!!");
					currentImage = 1;
				}
				else if ((currentImage + 2) < images.Count)
				{
					currentImage++;
				}

				// On rend tout les items de l'aborescence normaux et on met en gras l'élément concerné
				foreach (TreeViewItem item in registreAbre.Items)
				{
					item.FontWeight = FontWeights.Normal;
					int numeroPage = 0;
					if (Int32.TryParse(item.Header.ToString().Remove(item.Header.ToString().Length - 4), out numeroPage))
					{
						if (currentImage == numeroPage)
						{
							item.FontWeight = FontWeights.Bold;
						}
					}
				}

				ChargerImage(currentImage);
			}
			else
			{
				MessageBox.Show("Le nombre d'images dans le dossier n'est pas identique à celui attendu !!!" +
					"\n Nombre d'image dans le dossier : " + fileInfos.Count + " / Nombre d'image dans préindexer : " + images.Count, "AVERTISSEMENT",
					MessageBoxButton.OK,MessageBoxImage.Warning);
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
			cbxBisOrdre.IsChecked = false;
			cbxSautOrdre.IsChecked = false;
			BtnAddSequence.IsEnabled = true;
			using (var ct = new DocumatContext())
			{
				var LastSequence = ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID 
													 && s.References.ToLower() != "defaut").OrderByDescending(i => i.NUmeroOdre).FirstOrDefault();
				if (LastSequence != null)
				{
					tbNumeroOrdreSequence.Text = (Int32.Parse(tbNumeroOrdreSequence.Text) - 1).ToString();
				}
				else
				{
					tbNumeroOrdreSequence.Text = CurrentImageView.Image.DebutSequence + "";
				}
			}
			tbReference.Focus();		
		}

		private void cbxBisOrdre_Checked(object sender, RoutedEventArgs e)
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
					tbNumeroOrdreSequence.Text = (Int32.Parse(tbNumeroOrdreSequence.Text) - 1).ToString();
				}
				else
				{
					tbNumeroOrdreSequence.Text = CurrentImageView.Image.DebutSequence + "";
				}
			}
			tbReference.Focus();
		}

		private void cbxBisOrdre_Unchecked(object sender, RoutedEventArgs e)
		{
			using (var ct = new DocumatContext())
			{
				var LastSequence = ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID 
													 && s.References.ToLower() != "defaut").OrderByDescending(i => i.NUmeroOdre).FirstOrDefault();
				if (LastSequence != null)
				{
					if (LastSequence.NUmeroOdre == CurrentImageView.Image.FinSequence)
					{
						tbNumeroOrdreSequence.Text = (LastSequence.NUmeroOdre + 1).ToString();
						BtnAddSequence.IsEnabled = false;
					}
					else
					{
						tbNumeroOrdreSequence.Text = (LastSequence.NUmeroOdre + 1).ToString();						
					}
				}
			}
			tbReference.Focus();
		}

		private void cbxDoublonOrdre_Unchecked(object sender, RoutedEventArgs e)
		{
			using (var ct = new DocumatContext())
			{
				var LastSequence = ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID 
								   && s.References.ToLower() != "defaut").OrderByDescending(i => i.NUmeroOdre).FirstOrDefault();
				if (LastSequence != null)
				{
					if (LastSequence.NUmeroOdre == CurrentImageView.Image.FinSequence)
					{
						tbNumeroOrdreSequence.Text = (LastSequence.NUmeroOdre + 1).ToString();
						BtnAddSequence.IsEnabled = false;
					}
					else
					{
						tbNumeroOrdreSequence.Text = (LastSequence.NUmeroOdre + 1).ToString();
					}
				}
			}
			tbReference.Focus();
		}

		private void cbxSautOrdre_Unchecked(object sender, RoutedEventArgs e)
		{
			tbReference.Focus();
		}

		private void BtnTerminerImage_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Voulez-vous terminer l'indexation de cette image ?", "QUESTION", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
			{
				var LastSequence = CurrentImageView.context.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID 
								   && s.References.ToLower() != "defaut").OrderByDescending(i => i.NUmeroOdre).FirstOrDefault();
				if (LastSequence.NUmeroOdre == CurrentImageView.Image.FinSequence)
				{
					using (var ct = new DocumatContext())
					{
						Models.Image image = ct.Image.FirstOrDefault(i => i.ImageID == CurrentImageView.Image.ImageID);
						image.DateModif = DateTime.Now;
						image.StatutActuel = (int)Enumeration.Image.INDEXEE;

						// Récupération du statut scanné de l'image
						StatutImage AncienStatut = ct.StatutImage.FirstOrDefault(s => s.ImageID == image.ImageID && s.Code == (int)Enumeration.Image.CREEE);
						AncienStatut.DateFin = AncienStatut.DateModif = DateTime.Now;

						StatutImage NewStatut = ct.StatutImage.FirstOrDefault(s => s.ImageID == image.ImageID && s.Code == (int)Enumeration.Image.INDEXEE);
						if(NewStatut == null)
						{
							// Création du nouvea staut indexé
							NewStatut = new StatutImage();
							NewStatut.ImageID = image.ImageID;
							NewStatut.Code = (int)Enumeration.Image.INDEXEE;
							NewStatut.DateCreation = NewStatut.DateDebut = NewStatut.DateModif = DateTime.Now;
							ct.StatutImage.Add(NewStatut);
						}
						else
						{
							NewStatut.DateDebut = NewStatut.DateModif = DateTime.Now;
							NewStatut.DateFin = DateTime.Now;
						}

						ct.SaveChanges();
						// Enregistrement de l'action effectuée par l'agent
						DocumatContext.AddTraitement(DocumatContext.TbImage, image.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION, "INDEXATION TERMINER DE L'IMAGE N° " + image.NumeroPage + " DU REGISTRE ID N° " + image.RegistreID);
					}

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

					HeaderInfosGetter();
					this.BtnImageSuivante_Click(null, new RoutedEventArgs());
				}
			}
		}

		private void BtnMettreInstance_Click(object sender, RoutedEventArgs e)
		{
			if(MessageBox.Show("Voulez vous vraiment mettre cette image en instance ?","QUESTION",MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes)
			{
				// Mettre l'image en instance
				string typeInstance = "";
				switch (CbChoixInstance.SelectedIndex)
				{
					case 0 :
						typeInstance = "Image Illisible";
						break;
					case 1 :
						typeInstance = "Ecriture Illisible";
						break;
					default:
						typeInstance = "Autre";
						break;
				}

				using(var ct = new DocumatContext())
				{
					Models.Image image = ct.Image.FirstOrDefault(i => i.ImageID == CurrentImageView.Image.ImageID);
					image.typeInstance = typeInstance;
					image.ObservationInstance = tbObservationInstance.Text;
					image.StatutActuel = (int)Enumeration.Image.INSTANCE;

					// Création de l'instance ou modification de l'instance
					StatutImage newstatut = ct.StatutImage.FirstOrDefault(s => s.ImageID == CurrentImageView.Image.ImageID && s.Code == (int)Enumeration.Image.INSTANCE);
					if(newstatut != null)
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
					// Enregistrement de l'action effectuée par l'agent
					DocumatContext.AddTraitement(DocumatContext.TbImage, image.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION, "INDEXATION MISE EN INSTANCE DE L'IMAGE N°" + image.NumeroPage + " DU REGISTRE ID N° " + image.RegistreID);

					TbkIndicateurImage.Text = "EN INSTANCE";
					panelChoixInstance.Visibility = Visibility.Collapsed;
					BtnTerminerImage.IsEnabled = false;
					cbxMettreInstance.IsEnabled = false;
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
			// Définition de restriction sur la date  format : 18/03/2020
			Regex regex = new Regex("^[0-9]{0,2}/[0-9]{2}/[0-9]{4}$");
			TextBox textBox = ((TextBox)sender);
			//textBox.CaretIndex = 2;
			string text = textBox.Text;

			if(!regex.IsMatch(text))
			{
				using (var ct = new DocumatContext())
				{
					var LastSequence = ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID 
									   && s.References.ToLower() != "defaut")
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

		private void tbDateSequence_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Up || e.Key == System.Windows.Input.Key.Add)
			{
				DateTime date;
				if (DateTime.TryParse(tbDateSequence.Text, out date))
				{
					tbDateSequence.Text = date.AddDays(1).ToShortDateString();
				}
			}
			else if (e.Key == System.Windows.Input.Key.Down || e.Key == System.Windows.Input.Key.Subtract)
			{				
				DateTime date;
				if (DateTime.TryParse(tbDateSequence.Text, out date))
				{
					tbDateSequence.Text = date.AddDays(-1).ToShortDateString();
				}
			}
			else if (e.Key == System.Windows.Input.Key.PageUp && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
			{
				tbDateSequence.CaretIndex = 0;
				DateTime date;
				if (DateTime.TryParse(tbDateSequence.Text, out date))
				{
					tbDateSequence.Text = date.AddYears(1).ToShortDateString();
				}
			}
			else if (e.Key == System.Windows.Input.Key.PageDown && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
			{
				tbDateSequence.CaretIndex = 0;
				DateTime date;
				if (DateTime.TryParse(tbDateSequence.Text, out date))
				{
					tbDateSequence.Text = date.AddYears(-1).ToShortDateString();
				}
			}
			else if (e.Key == System.Windows.Input.Key.PageUp)
			{
				tbDateSequence.CaretIndex = 0;
				DateTime date;
				if (DateTime.TryParse(tbDateSequence.Text, out date))
				{
					tbDateSequence.Text = date.AddMonths(1).ToShortDateString();
				}
			}
			else if (e.Key == System.Windows.Input.Key.PageDown)
			{
				tbDateSequence.CaretIndex = 0;
				DateTime date;
				if (DateTime.TryParse(tbDateSequence.Text, out date))
				{
					tbDateSequence.Text = date.AddMonths(-1).ToShortDateString();
				}
			}
			else if (e.Key == System.Windows.Input.Key.Right)
			{
				tbReference.Focus();
			}
			else if (e.Key == System.Windows.Input.Key.Left)
			{
				DateTime date;
				if (DateTime.TryParse(tbDateSequence.Text, out date))
				{
					tbDateSequence.Text = date.ToShortDateString().Remove(0,2);
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

		private void tbDateSequence_LostFocus(object sender, RoutedEventArgs e)
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

		private void tbDateSequence_GotFocus(object sender, RoutedEventArgs e)
		{
			tbDateSequence.CaretIndex = 0;
		}

		private void tbReference_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			Regex regex = new Regex("^[0-9]{1,}/");

			if(e.Key == System.Windows.Input.Key.Right)
			{
				tbReference.CaretIndex = tbReference.Text.IndexOf('/') + 1;
			}
			else if (e.Key == System.Windows.Input.Key.Left)
			{
				if(tbReference.Text == "" || tbReference.Text == "/")
				{
					tbReference.Text = "";
					tbDateSequence.Focus();
				}
				else if(tbReference.CaretIndex == 0)
				{
					tbDateSequence.Focus();
				}
			}
			else if(e.Key == System.Windows.Input.Key.Up || e.Key == System.Windows.Input.Key.Add)
			{
				Regex regC = new Regex("^[0-9]{1,}/[\\w*]{1,}$");			
				if (regC.IsMatch(tbReference.Text))
				{
					tbListeReferences.Text = "";
					tbListeReferences.ToolTip = "";
					tbListeReferences.Visibility = Visibility.Visible;
					tbxNbRefsList.Visibility = Visibility.Visible;
					try
					{
						References.Add(RefInitiale + tbReference.Text,RefInitiale + tbReference.Text);
						tbxNbRefsList.Text = "Nb : " + References.Count;
						tbReference.Text = "";
                    }
					catch (Exception){}

					foreach (var reference in References)
					{
						tbListeReferences.Text += reference.Value + " ";	
						tbListeReferences.ToolTip += reference.Value + " ";
					}
				}
			}
			else if(e.Key == System.Windows.Input.Key.Down || e.Key == System.Windows.Input.Key.Subtract)
			{
				tbListeReferences.Text = "";
				tbListeReferences.ToolTip =  "";
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
			else if(e.Key == System.Windows.Input.Key.Enter)
			{
				if (References.Count > 0 || cbxSautOrdre.IsChecked == true)
				{
					this.BtnAddSequence_Click(null, new RoutedEventArgs());
				}
			}
        }

		private void tbReference_LostFocus(object sender, RoutedEventArgs e)
		{
			Regex regex = new Regex("^[0-9]{1,}/");
			if (tbReference.Text == "/")
			{
				tbReference.Text = "";
			}
            else if (!regex.IsMatch(tbReference.Text))
            {
                tbReference.Text = "";
                tbReference.CaretIndex = 0;
            }
        }

		private void tbReference_TextChanged(object sender, TextChangedEventArgs e)
		{
            Regex regex = new Regex("^[0-9]{1,}/");
            if (!regex.IsMatch(tbReference.Text))
            {
                tbReference.Text = "/";
                tbReference.CaretIndex = 0;
            }
        }

		private void tbReference_GotFocus(object sender, RoutedEventArgs e)
		{
			if(tbReference.Text == "")
			{
				tbReference.Text = "/";
			}
		}

		private void Window_GotFocus(object sender, RoutedEventArgs e)
		{			

		}

		private void btnValideIndexation_Click(object sender, RoutedEventArgs e)
		{
			if(MessageBox.Show("Voulez vous terminer l'indexation du registre ?","QUESTION",MessageBoxButton.YesNo,MessageBoxImage.Question)
				== MessageBoxResult.Yes)
			{
				if(MainParent.Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR || MainParent.Utilisateur.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
                {
					if(MessageBox.Show("Attention en terminant ce Registre, Il sera pris en compte que Vous avez Indexé ce Registre, Voulez Vous Terminez ce Registre ?","AGENT INDEXEUR",MessageBoxButton.YesNo,MessageBoxImage.Question) ==  MessageBoxResult.No)
                    {
						return;
                    }
                }
				//Affichage du bouton de validadtion d'indexation si le 
				using (var ct = new DocumatContext())
				{
					//Récupération de toutes les images du registre 
					ImageView imageView = new ImageView();
					List<ImageView> imageViews = imageView.GetSimpleViewsList(RegistreViewParent.Registre);

					if(imageViews.All(i=>i.Image.StatutActuel == (int)Enumeration.Image.INDEXEE))
					{					
						Models.Registre registre = ct.Registre.FirstOrDefault(r => r.RegistreID == RegistreViewParent.Registre.RegistreID);
						// Récupération et modification de l'ancien statut du registre
						Models.StatutRegistre AncienStatut = ct.StatutRegistre.FirstOrDefault(s => s.RegistreID == RegistreViewParent.Registre.RegistreID
															 && s.Code == registre.StatutActuel);
						AncienStatut.DateFin = AncienStatut.DateModif = DateTime.Now;

						// Création du nouveau statut de registre
						Models.StatutRegistre NewStatut = new StatutRegistre();
						NewStatut.Code = (int)Enumeration.Registre.INDEXE;
						NewStatut.DateCreation = NewStatut.DateDebut = NewStatut.DateModif = DateTime.Now;
						NewStatut.RegistreID = RegistreViewParent.Registre.RegistreID;
						ct.StatutRegistre.Add(NewStatut);

						//Changement du statut du registre
						registre.StatutActuel = (int)Enumeration.Registre.INDEXE;						
						ct.SaveChanges();

						//Enregistrement du traitement fait au registre 						
						DocumatContext.AddTraitement(DocumatContext.TbRegistre, RegistreViewParent.Registre.RegistreID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.INDEXATION_REGISTRE_TERMINE);

						MainParent.RefreshRegistre();
						this.Close();						
					}
				}
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			MainParent.IsEnabled = true;
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

        private void BtnGenererAutoRefs_Click(object sender, RoutedEventArgs e)
        {
			// Génération automatique des references sequencées
			Regex regex = new Regex("^[0-9]{1,}/[\\w*-]{1,}$");
			if (regex.IsMatch(tbReference.Text))
			{
				//Récupération du nombre d'index demande
				string[] refEtNbRef = tbReference.Text.Split('-');
				int nbRefs = 0;
				if(refEtNbRef.Length == 2 && Int32.TryParse(refEtNbRef[1].Trim(),out nbRefs))
                {
					if(nbRefs > 1 && nbRefs < 100)
                    {
						string refVariantIndice = refEtNbRef[0].Split('/')[0];
						string refRacine = refEtNbRef[0].Split('/')[1];
						string[] AllRefVariant = new string[nbRefs];
						int refVariantInt = Int32.Parse(refVariantIndice);

						for (int i = 0; i < nbRefs; i++)
                        {
							AllRefVariant[i] = (refVariantInt + i) + "/" + refRacine;
							try
							{
								References.Add(RefInitiale + AllRefVariant[i], RefInitiale + AllRefVariant[i]);
							}
							catch (Exception) {}
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
                }
				else
                {
					MessageBox.Show("Spécifier le nombre de references exemple(s) : 1/27-2, 1/27-3", "Reference Mal spécifiée", MessageBoxButton.OK, MessageBoxImage.Information);
                }
			}
		}
    }
}
