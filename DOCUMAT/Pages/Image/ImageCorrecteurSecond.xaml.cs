using DOCUMAT.Models;
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

namespace DOCUMAT.Pages.Image
{
	/// <summary>
	/// Logique d'interaction pour ImageCorrecteurSecond.xaml
	/// </summary>
	public partial class ImageCorrecteurSecond : Window
    {
		Correction.Correction MainParent;

		RegistreView RegistreViewParent;
		ImageView CurrentImageView;
		private int currentImage = 1;

		// Définition de l'aborescence
		TreeViewItem registreAbre = new TreeViewItem();
		//Fichier d'image dans disque dure
		List<FileInfo> fileInfos = new List<FileInfo>();

		string DossierRacine = ConfigurationManager.AppSettings["CheminDossier_Scan"];

		//Liste des sequences indexés comme faux dans l'image courant du registre
		//soit dont le numéro est currentImage
		Dictionary<int, SequenceView> ListeSequences = new Dictionary<int, SequenceView>();

		//Initiale des références par type de registre R3 = R / R4 = T
		string RefInitiale = "R";
		//Liste des possibles references		
		Dictionary<String, String> References = new Dictionary<String, String>();

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
			//Information sur l'agent 
			if (!string.IsNullOrEmpty(MainParent.Utilisateur.CheminPhoto))
			{
				AgentImage.Source = new BitmapImage(new Uri(Path.Combine(DossierRacine, MainParent.Utilisateur.CheminPhoto)), new System.Net.Cache.RequestCachePolicy());
			}

			tbxAgentLogin.Text = "Login : " + MainParent.Utilisateur.Login;
			tbxAgentNoms.Text = "Noms : " + MainParent.Utilisateur.Noms;
			tbxAgentType.Text = "Aff : " + Enum.GetName(typeof(Enumeration.AffectationAgent), MainParent.Utilisateur.Affectation);

			//Remplissage des éléments de la vue
			//Obtention de la liste des images du registre dans la base de données 
			ImageView imageView1 = new ImageView();
			List<ImageView> imageViews = imageView1.GetSimpleViewsList(RegistreViewParent.Registre);
			tbxQrCode.Text = "QRCODE : " + RegistreViewParent.Registre.QrCode;
			tbxVersement.Text = "Versement N° : " + RegistreViewParent.Versement.NumeroVers.ToString();
			tbxService.Text = "Service : " + RegistreViewParent.ServiceVersant.Nom;

			// Récupération des images en correction
			List<Models.Image> imagesEnCorrection = new List<Models.Image>();
			List<Models.Image> imagesValide = new List<Models.Image>();
			foreach (var image in imageViews)
			{
				using (var ct = new DocumatContext())
				{
					if (ct.Correction.Any(c => c.RegistreId == RegistreViewParent.Registre.RegistreID && c.ImageID == image.Image.ImageID
						 && c.SequenceID == null && c.StatutCorrection == 1 && c.PhaseCorrection == 3)
						 &&
						 !ct.Correction.Any(c => c.RegistreId == RegistreViewParent.Registre.RegistreID && c.ImageID == image.Image.ImageID
						 && c.SequenceID == null && c.StatutCorrection == 0 && c.PhaseCorrection == 3))
					{
						imagesEnCorrection.Add(image.Image);
					}
					else 
					{
						imagesValide.Add(image.Image);
					}
				}
			}

			//Procédure de modification de l'indicateur de reussite
			double perTerminer = (Math.Round(((float)imagesValide.Count() / (float)imageViews.Count()) * 100, 1));
			string EnCorrection = (Math.Round(((float)imagesEnCorrection.Count() / (float)imageViews.Count()) * 100, 1)).ToString() + " %";
			string Valide = (Math.Round(((float)imagesValide.Count() / (float)imageViews.Count()) * 100, 1)).ToString() + " %"; ;

			tbxImageTerminer.Text = "Terminé : " + Math.Round(perTerminer, 1) + " %";
			tbxImageRejete.Text = "En Correction : " + EnCorrection + " / " + imagesEnCorrection.Count();
			tbxImageValide.Text = "Validé : " + Valide + " / " + imagesValide.Count();
			tbxImageEnCours.Text = "Non Traité : " + EnCorrection;
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
					Models.Image image1 = ct.Image.FirstOrDefault(i => i.NomPage.ToLower() == item.Header.ToString().Remove(item.Header.ToString().Length - 4).ToLower()
																&& i.RegistreID == RegistreViewParent.Registre.RegistreID);
					if (image1 != null)
					{
						if (image1.StatutActuel == (int)Enumeration.Image.PHASE1 || image1.StatutActuel == (int)Enumeration.Image.PHASE2)
						{
							item.Tag = "valide";
						}
						else if (image1.StatutActuel == (int)Enumeration.Image.PHASE3)
						{ 
							if (ct.Controle.FirstOrDefault(c => c.ImageID == image1.ImageID && c.SequenceID == null
								&& c.PhaseControle == 3 && c.StatutControle == 0) != null)
							{
								item.Tag = "valide";
							}
							else
							{
								if (ct.Controle.FirstOrDefault(c => c.ImageID == image1.ImageID && c.SequenceID == null
										&& c.PhaseControle == 3 && c.StatutControle == 1 && c.ASupprimer == 1) != null)
								{
									item.Tag = "fileDelele";
								}
								else
								{
									item.Tag = "instance";
								}
							}
						}
					}
					else
					{
						item.Tag = "image.png";
					}					
				}
			}

			// On rend tout les items de l'aborescence normaux et on met en gras l'élément concerné
			foreach (TreeViewItem item in registreAbre.Items)
			{
				item.FontWeight = FontWeights.Normal;
				int numeroPage = 0;
				Models.ManquantImage manquantImage = null;
				using (var ct = new DocumatContext())
				{
					manquantImage = ct.ManquantImage.FirstOrDefault(m => m.IdRegistre == RegistreViewParent.Registre.RegistreID
											&& m.NumeroPage.ToString() == item.Header.ToString());
				}

				if (manquantImage != null)
				{
					//Doit obtenir le manque concernée
					//if (currentImage == numeroPage)
					//{
					//	item.FontWeight = FontWeights.Bold;
					//}
				}
				else if (currentImage == -1)
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

		private void FileTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			TreeViewItem treeViewItem = (TreeViewItem)sender;
			// Récupération des images du registre
			ImageView imageView1 = new ImageView();
			List<ImageView> imageViews = imageView1.GetSimpleViewsList(RegistreViewParent.Registre);

			//Chargement de l'image 
			int numeroPage = 0;
			Models.ManquantImage manquantImage = null;

			using (var ct = new DocumatContext())
			{
				manquantImage = ct.ManquantImage.FirstOrDefault(m => m.IdRegistre == RegistreViewParent.Registre.RegistreID
											&& m.NumeroPage.ToString() == treeViewItem.Header.ToString());
			}

			if (manquantImage != null)
			{
				if (MessageBox.Show("Voulez vous importer l'image ?", "QUESTION", MessageBoxButton.YesNo, MessageBoxImage.Question)
					== MessageBoxResult.Yes)
				{
					OpenFileDialog open = new OpenFileDialog();
					open.Filter = "JPEG|*.jpg|TIFF|*.tif";
					open.Multiselect = false;
					string NomFichier = "";

					if (open.ShowDialog() == true)
					{
						NomFichier = open.FileNames[0];
						viewImage(NomFichier);
						PanelCorrectionEnCours.Visibility = Visibility.Visible;
						PanelCorrectionEffectue.Visibility = Visibility.Collapsed;
						tbxNomPage.Text = "PAGE : " + manquantImage.NumeroPage;
						tbxNumeroPage.Text = "N° : " + manquantImage.NumeroPage;
						dgSequence.ItemsSource = null;
						tbNumeroOrdreSequence.Text = manquantImage.DebutSequence.ToString();
						//tbDateSequence.Text = manquantImage.DateSequenceDebut.Value.ToString();
						tbNumeroOrdreSequence.IsEnabled = false;
						tbDateSequence.IsEnabled = true;
						tbReference.IsEnabled = true;
						cbxBisOrdre.IsEnabled = true;
						cbxDoublonOrdre.IsEnabled = true;
						BtnModifierSequence.IsEnabled = true;
						BtnSupprimerSequence.IsEnabled = true;
						if (MessageBox.Show("Voulez-vous valider l'image et commencer l'indexation ?", "QUESTION", MessageBoxButton.YesNo, MessageBoxImage.Question)
											== MessageBoxResult.Yes)
						{
							//Création de l'image
							ImageView imageView = new ImageView();
							//Définition du nomo du fichier
							FileInfo fichierSource = new FileInfo(NomFichier);
							string newfichier = manquantImage.NumeroPage.ToString();
							if (manquantImage.NumeroPage == -1)
							{
								newfichier = "PAGE DE GARDE";
							}
							else if (manquantImage.NumeroPage == 0)
							{
								newfichier = "PAGE D'OUVERTURE";
							}

							//Préparation du chemin de l'image et déplacement de l'image dans le dossier parent
							if (imageView.context.Image.FirstOrDefault(i => i.NumeroPage == manquantImage.NumeroPage) != null)
							{
								using (var ct = new DocumatContext())
								{
									string Destination = System.IO.Path.Combine(RegistreViewParent.Registre.CheminDossier
														, newfichier + fichierSource.Extension);
									//Copy du fichier dans le dossier du registre
									File.Copy(fichierSource.FullName, Destination, true);
									//Ajout de l'image avec le statut scanné
									Models.Image image = new Models.Image()
									{
										RegistreID = RegistreViewParent.Registre.RegistreID,
										NomPage = newfichier,
										NumeroPage = manquantImage.NumeroPage,
										CheminImage = Destination,
										Taille = fichierSource.Length,
										Type = fichierSource.Extension.Substring(1).ToUpper(),
										DateScan = DateTime.Now,
										StatutActuel = (int)Enumeration.Image.CREEE,
										DebutSequence = manquantImage.DebutSequence,
										FinSequence = manquantImage.FinSequence,
										DateDebutSequence = DateTime.Parse(manquantImage.DateSequenceDebut.Value.ToString()),
										DateCreation = DateTime.Now,
										DateModif = DateTime.Now,
									};
									ct.Image.Add(image);
									ct.SaveChanges();

									//Création de statut image scanné
									Models.StatutImage statutImageScan = new StatutImage()
									{
										ImageID = image.ImageID,
										Code = (int)Enumeration.Image.SCANNEE,
										DateCreation = DateTime.Now,
										DateModif = DateTime.Now,
										DateFin = DateTime.Now,
										DateDebut = DateTime.Now,
									};
									ct.StatutImage.Add(statutImageScan);
									ct.SaveChanges();

									//Création du statut crée
									Models.StatutImage statutImageCre = new StatutImage()
									{
										ImageID = image.ImageID,
										Code = (int)Enumeration.Image.CREEE,
										DateCreation = DateTime.Now,
										DateModif = DateTime.Now,
										DateFin = DateTime.Now,
										DateDebut = DateTime.Now,
									};
									ct.StatutImage.Add(statutImageCre);
									ct.SaveChanges();

									//Modification du manquant
									ManquantImage UpManquant = ct.ManquantImage.FirstOrDefault(m => m.ManquantImageID == manquantImage.ManquantImageID);
									UpManquant.IdImage = image.ImageID;
									UpManquant.DateModif = DateTime.Now;
									UpManquant.DateCorrectionManquant = DateTime.Now;
									ct.SaveChanges();

									//Chargement de la nouvelle image ajouté
									currentImage = manquantImage.NumeroPage;
									ChargerImage(currentImage);
								}
							}
							else
							{
								throw new Exception("Une image ayant le même numéro de page existe déja !!!");
							}
						}
						else
						{
							ChargerImage(currentImage);
						}
					}
				}
			}
			//Cas d'une page normal (Page numérotée)
			else if (Int32.TryParse(treeViewItem.Header.ToString().Remove(treeViewItem.Header.ToString().Length - 4), out numeroPage))
			{
				currentImage = numeroPage;
				// Chargement de l'image		
				ChargerImage(currentImage);
			}
			else if (treeViewItem.Header.ToString().Remove(treeViewItem.Header.ToString().Length - 4).ToLower() == "PAGE DE GARDE".ToLower())
			{
				//Affichage de la page de garde
				currentImage = -1;
				// Chargement de l'image		
				ChargerImage(currentImage);
			}
			else if (treeViewItem.Header.ToString().Remove(treeViewItem.Header.ToString().Length - 4).ToLower() == "PAGE D'OUVERTURE".ToLower())
			{
				//Affichage de la page d'ouverture
				currentImage = 0;
				// Chargement de l'image		
				ChargerImage(currentImage);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void ActualiseDataIndexer()
		{
			#region ACTUALISATION DES SEQUENCES
			using (var ct = new DocumatContext())
			{
				//Actualisation des séquences				
				List<Sequence> NewSequences = ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID).OrderBy(s => s.NUmeroOdre).ToList();
				List<SequenceView> NewSequenceViews = SequenceView.GetViewsList(NewSequences).ToList();
				dgSequence.Visibility = Visibility.Collapsed;
				dgSequenceIndex.Visibility = Visibility.Visible;
				dgSequenceIndex.ItemsSource = NewSequenceViews;

				if (NewSequences.Count != 0)
				{
					tbNumeroOrdreSequence.Text = (NewSequences.Last().NUmeroOdre + 1).ToString();
					tbDateSequence.Text = NewSequences.Last().DateSequence.ToShortDateString();
					tbReference.Text = "";

					// Pour savoir toutes séquences sont indexées 
					if (NewSequences.Last().NUmeroOdre == CurrentImageView.Image.FinSequence)
					{
						BtnTerminerImage.IsEnabled = true;
						BtnModifierSequence.IsEnabled = false;						
					}
					else
					{
						BtnTerminerImage.IsEnabled = false;
						BtnModifierSequence.IsEnabled = true;						
					}
				}
				else
				{
					tbNumeroOrdreSequence.Text = CurrentImageView.Image.DebutSequence.ToString();
					tbDateSequence.Text = CurrentImageView.Image.DateDebutSequence.ToShortDateString();
					tbListeReferences.Text = "";
				}

			}
			#endregion
		}

		public void ActualiseDataCorriger()
		{
			#region ACTUALISATION DES SEQUENCES
			//Actualisation des séquences	
			using (var ct = new DocumatContext())
			{
				List<Sequence> sequences = ct.Sequence.Where(s => s.ImageID == CurrentImageView.Image.ImageID).OrderBy(s => s.NUmeroOdre).ToList();
				List<SequenceView> sequenceViews = SequenceView.GetViewsList(sequences).Where(s => s.En_Correction == true).ToList();
				List<SequenceView> manquantSequences = SequenceView.GetManquants(CurrentImageView.Image).Where(s => s.En_Correction == true).ToList();
				sequenceViews.AddRange(manquantSequences);
				dgSequence.ItemsSource = sequenceViews;

				if (sequenceViews.Count == 0)
				{
					BtnTerminerImage.IsEnabled = true;
				}
				else
				{
					BtnTerminerImage.IsEnabled = false;
				}
			}
			#endregion
		}
		#endregion

		public ImageCorrecteurSecond()
		{
			InitializeComponent();
		}

		public ImageCorrecteurSecond(RegistreView registreview, Correction.Correction correction) : this()
		{
			MainParent = correction;
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

				if (imageView1 != null)
				{
					//On change l'image actuelle
					CurrentImageView = imageView1;
					this.currentImage = currentImage;

					if (imageView1.Image.NumeroPage == -1)
					{
						tbxNomPage.Text = "PAGE DE GARDE";
						tbxNumeroPage.Text = "";
						tbxDebSeq.Text = "";
						tbxFinSeq.Text = "";
					}
					else if (imageView1.Image.NumeroPage == 0)
					{
						tbxNomPage.Text = "PAGE D'OUVERTURE";
						tbxNumeroPage.Text = "";
						tbxDebSeq.Text = "";
						tbxFinSeq.Text = "";
					}
					else
					{
						tbxNomPage.Text = "PAGE : " + imageView1.Image.NomPage;
						tbxNumeroPage.Text = "N° " + imageView1.Image.NumeroPage.ToString() + "/ " + (imageViews.Count() - 2);
						tbxDebSeq.Text = "N° Debut : " + imageView1.Image.DebutSequence;
						tbxFinSeq.Text = "N° Fin : " + imageView1.Image.FinSequence;
					}

					// Chargement de la visionneuse
					if (File.Exists(Path.Combine(DossierRacine, imageView1.Image.CheminImage)))
                    {
						viewImage(Path.Combine(DossierRacine, imageView1.Image.CheminImage));
                    }
					else
                    {
						throw new Exception("La page : \"" + imageView1.Image.NumeroPage + "\" est introuvable !!!");
                    }
					#endregion

					#region RECUPERATION DES SEQUENCES DE L'IMAGE

					// On vide le Dictionary des séquences de l'image précédente
					ListeSequences.Clear();
					// Récupération des sequences déja renseignées, Différent des images préindexer ayant la référence défaut
					List<Sequence> sequences = imageView1.context.Sequence.Where(s => s.ImageID == imageView1.Image.ImageID).OrderBy(s => s.NUmeroOdre).ToList();

					//Récupération des contrôles de séquences rejetés
					List<SequenceView> sequenceViews = SequenceView.GetViewsList(sequences).Where(s => s.En_Correction == true).ToList();

					dgSequence.Visibility = Visibility.Visible;
					dgSequenceIndex.Visibility = Visibility.Collapsed;
					if (sequenceViews.Count() > 0)
					{
						dgSequence.ItemsSource = sequenceViews;
						dgSequence.ScrollIntoView(dgSequence.Items.GetItemAt(dgSequence.Items.Count - 1));
						tbNumeroOrdreSequence.IsEnabled = false;
						tbDateSequence.IsEnabled = false;
						tbReference.IsEnabled = false;
						cbxBisOrdre.IsEnabled = false;
						cbxDoublonOrdre.IsEnabled = false;
						BtnModifierSequence.IsEnabled = false;
						BtnSupprimerSequence.IsEnabled = false;
						BtnTerminerImage.IsEnabled = false;
					}
					else
					{
						dgSequence.ItemsSource = null;
						tbNumeroOrdreSequence.IsEnabled = false;
						tbDateSequence.IsEnabled = false;
						tbReference.IsEnabled = false;
						cbxBisOrdre.IsEnabled = false;
						cbxDoublonOrdre.IsEnabled = false;
						BtnModifierSequence.IsEnabled = false;
						BtnSupprimerSequence.IsEnabled = false;
						BtnTerminerImage.IsEnabled = true;
					}
					#endregion

					#region ADAPTATION DE L'AFFICHAGE EN FONCTION DES INSTANCES DE L'IMAGE
					btnValiderImageImporte.Foreground = Brushes.White;
					btnImporterImage.Tag = "";
					//On vide les références
					References.Clear();
					tbListeReferences.Visibility = Visibility.Collapsed;
					tbListeReferences.Text = "";

					// Procédure d'affichage lorsque les statuts d'images changes
					if (imageView1.Image.StatutActuel == (int)Enumeration.Image.PHASE1)
					{
						using (var ct = new DocumatContext())
						{
							if (ct.Controle.FirstOrDefault(c => c.ImageID == imageView1.Image.ImageID && c.SequenceID == null
								&& c.PhaseControle == 1 && c.StatutControle == 0) != null)
							{
								//Affichage des images validées 
								PanelCorrectionEnCours.Visibility = Visibility.Collapsed;
								PanelCorrectionEffectue.Visibility = Visibility.Visible;
								ImgCorrecte.Visibility = Visibility.Visible;
								ImgEdit.Visibility = Visibility.Collapsed;
								PanelRejetImage.Visibility = Visibility.Collapsed;
								tbxControleStatut.Text = "VALIDE";
							}
						}
					}
					else if (imageView1.Image.StatutActuel == (int)Enumeration.Image.PHASE3)
					{
						using (var ct = new DocumatContext())
						{
							if (ct.Controle.Any(c => c.ImageID == imageView1.Image.ImageID && c.SequenceID == null
								&& c.PhaseControle == 3 && c.StatutControle == 1))
							{
								// Image à Supprimer
								if (ct.Controle.FirstOrDefault(c => c.ImageID == imageView1.Image.ImageID && c.SequenceID == null
											 && c.PhaseControle == 3 && c.StatutControle == 1 && c.ASupprimer == 1) != null)
								{
									PanelRejetImage.Visibility = Visibility.Collapsed;
									BtnSupprimerImage.Visibility = Visibility.Visible;
									BtnTerminerImage.Visibility = Visibility.Collapsed;
								}
								else
								{
									List<Models.Correction> correctionRejetImage = ct.Correction.Where(c => c.ImageID == imageView1.Image.ImageID && c.SequenceID == null
																	&& c.PhaseCorrection == 3 && c.RejetImage_idx == 1).ToList();
									if (correctionRejetImage.Count > 0)
									{
										if(correctionRejetImage.All(c=>c.StatutCorrection == 1))
                                        {
											PanelRejetImage.Visibility = Visibility.Visible;
											tbxRejetImage.Text = "Rejet Image : " + correctionRejetImage[0].MotifRejetImage_idx.ToUpper(); 
                                        }
										else
                                        {
											PanelRejetImage.Visibility = Visibility.Collapsed;
										}
									}
									else
									{
										PanelRejetImage.Visibility = Visibility.Collapsed;
									}
									BtnSupprimerImage.Visibility = Visibility.Collapsed;
									BtnTerminerImage.Visibility = Visibility.Visible;
								}
								//Affichage des images en cours de traitement
								PanelCorrectionEnCours.Visibility = Visibility.Visible;
								PanelCorrectionEffectue.Visibility = Visibility.Collapsed;
							}
							else
							{
								//Affichage des images validées 
								PanelCorrectionEnCours.Visibility = Visibility.Collapsed;
								PanelCorrectionEffectue.Visibility = Visibility.Visible;
								ImgCorrecte.Visibility = Visibility.Visible;
								ImgEdit.Visibility = Visibility.Collapsed;
								PanelRejetImage.Visibility = Visibility.Collapsed;
								tbxControleStatut.Text = "VALIDE";
							}
						}
					}
					#endregion

					// Actualisation de l'aborescence
					ActualiserArborescence();

					//Affichage du bouton de validadtion du contrôle si toute les images sont marqué en phase 1 
					bool valide = true;
					foreach (TreeViewItem item in registreAbre.Items)
					{
						if (!item.Tag.ToString().Equals("valide"))
                        {
							valide = false;
                        }
					}

					if (valide)
					{
						btnValideCorrection.Visibility = Visibility.Visible;
					}
					else
					{
						btnValideCorrection.Visibility = Visibility.Collapsed;
					}
				}
				else
				{
					MessageBox.Show("L'image N° : " + currentImage + " est Introuvable dans la Base de Données !!!", "IMAGE INTROUVABLE", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
			catch (Exception ex)
			{
				ex.ExceptionCatcher();
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

				// Récupération des images de registre ayant des erreurs
				// Les Pages doivent être scannées de manière à respecter la nomenclature de fichier standard 
				ImageView imageView1 = new ImageView();
				List<ImageView> imageViews = imageView1.GetSimpleViewsList(RegistreViewParent.Registre);

				// Chargement de l'aborescence
				// Récupération de la liste des images contenu dans le dossier du registre
				var files = Directory.GetFiles(Path.Combine(DossierRacine, RegistreViewParent.Registre.CheminDossier));

				// Affichage et configuration de la TreeView
				registreAbre.Header = RegistreViewParent.Registre.QrCode;
				registreAbre.Tag = RegistreViewParent.Registre.QrCode;
				registreAbre.FontWeight = FontWeights.Normal;
				registreAbre.Foreground = Brushes.White;

				// Définition de la lettre de référence pour ce registre
				if (RegistreViewParent.Registre.Type == "R4")
					RefInitiale = "T";

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

				#region VERIFICATION DES PAGES SPECIALES
				// Récupération des Pages Spéciaux : PAGE DE GARDE : numero : -1 ET PAGE D'OUVERTURE : numero : 0			
				using (var ct = new DocumatContext())
				{
					//PAGE DE GARDE N°-1
					if (ct.Image.FirstOrDefault(i => i.RegistreID == RegistreViewParent.Registre.RegistreID && i.NumeroPage == -1) == null)
					{
						throw new Exception("La PAGE DE GARDE est manquante !!!");
					}

					//PAGE D'OUVERTURE N°0
					if (ct.Image.FirstOrDefault(i => i.RegistreID == RegistreViewParent.Registre.RegistreID && i.NumeroPage == 0) == null)
					{
						throw new Exception("La PAGE D'OUVERTURE est manquante !!!");
					}
				}
				#endregion

				#region VERIFICTION DES PAGES NUMEROTEES
				// Modification des Pages Numerotées / Pré-Indexées
				foreach (var imageView in imageViews)
				{
					if (imageView.Image.StatutActuel == (int)Enumeration.Image.SCANNEE)
					{
						FileInfo file = fileInfos.FirstOrDefault(f => f.Name.Remove(f.Name.Length - 4) == imageView.Image.NumeroPage.ToString());
						if (file == null)
						{
							throw new Exception("La Page N°" + imageView.Image.NumeroPage + " est introuvable ");
						}
					}
				}
				#endregion

				using (var ct = new DocumatContext())
				{
					if (ct.Image.All(i => i.RegistreID == RegistreViewParent.Registre.RegistreID
										&& i.StatutActuel == (int)Enumeration.Image.PHASE2))
					{
						btnValideCorrection.Visibility = Visibility.Visible;
					}
					else
					{
						btnValideCorrection.Visibility = Visibility.Collapsed;
					}
				}

				// Chargement de la première page
				currentImage = -1;
				ChargerImage(currentImage);

				// Modification des information d'entête
				HeaderInfosGetter();

				#region VERIFICATION DE L'AGENT 				
				using (var ct = new DocumatContext())
				{
					if (MainParent.Utilisateur.Affectation != (int)Enumeration.AffectationAgent.ADMINISTRATEUR && MainParent.Utilisateur.Affectation != (int)Enumeration.AffectationAgent.SUPERVISEUR)
					{
						Models.Traitement traitementAttr = ct.Traitement.FirstOrDefault(t => t.TableSelect == DocumatContext.TbRegistre && t.TableID == RegistreViewParent.Registre.RegistreID
															&& t.TypeTraitement == (int)Enumeration.TypeTraitement.CORRECTION_PH3_DEBUT);
						if (traitementAttr != null)
						{
							Models.Traitement traitementRegistreAgent = ct.Traitement.FirstOrDefault(t => t.TableSelect == DocumatContext.TbRegistre && t.TableID == RegistreViewParent.Registre.RegistreID
													&& t.TypeTraitement == (int)Enumeration.TypeTraitement.CORRECTION_PH3_DEBUT && t.AgentID == MainParent.Utilisateur.AgentID);
							Models.Traitement traitementAutreAgent = ct.Traitement.FirstOrDefault(t => t.TableSelect == DocumatContext.TbRegistre && t.TableID == RegistreViewParent.Registre.RegistreID
													&& t.TypeTraitement == (int)Enumeration.TypeTraitement.CORRECTION_PH3_DEBUT && t.AgentID != MainParent.Utilisateur.AgentID);

							if (traitementAutreAgent != null)
							{
								Models.Agent agent = ct.Agent.FirstOrDefault(t => t.AgentID == traitementAutreAgent.AgentID);
								MessageBox.Show("Ce Registre est en Cours traitement par l'agent : " + agent.Noms, "REGISTRE EN CORRECTION PH3", MessageBoxButton.OK, MessageBoxImage.Information);
								this.Close();
							}
						}
						else
						{
							if (MessageBox.Show("Voulez vous commencez la correction ?", "COMMNCER LE CORRECTION PH3", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
							{
								DocumatContext.AddTraitement(DocumatContext.TbRegistre, RegistreViewParent.Registre.RegistreID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.CORRECTION_PH3_DEBUT, "CORRECTION PH3 COMMENCER");
							}
							else
							{
								this.Close();
							}
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
			if (dgSequence.SelectedItems.Count == 1)
			{
				if(MessageBox.Show("Confirmez la suppression ?","Confirmer Suppression",MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
					if (CurrentImageView.Image.StatutActuel == (int)Enumeration.Image.PHASE3)
					{
						SequenceView sequenceView = (SequenceView)dgSequence.SelectedItem;
						if (sequenceView.ASupprimer)
						{
							using (var ct = new DocumatContext())
							{
								//Suppression de la séquence
								ct.Sequence.Remove(ct.Sequence.FirstOrDefault(s => s.SequenceID == sequenceView.Sequence.SequenceID));

								// Création de la correction
								Models.Correction correction = new Models.Correction()
								{
									RegistreId = RegistreViewParent.Registre.RegistreID,
									ImageID = sequenceView.Sequence.ImageID,
									SequenceID = sequenceView.Sequence.SequenceID,

									//Indexes Image mis à null
									RejetImage_idx = null,
									MotifRejetImage_idx = null,

									//Indexes de la séquence de l'image
									OrdreSequence_idx = null,
									DateSequence_idx = null,
									RefSequence_idx = null,
									RefRejetees_idx = null,
									ASupprimer = 1,

									DateCorrection = DateTime.Now,
									DateCreation = DateTime.Now,
									DateModif = DateTime.Now,
									PhaseCorrection = 3,
									StatutCorrection = 0,
								};
								ct.Correction.Add(correction);
								ct.SaveChanges();
							}
						}

						ActualiseDataCorriger();
					}
                }
			}
		}

		private void dgSequence_LoadingRow(object sender, DataGridRowEventArgs e)
		{

		}

		private void dgSequence_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (dgSequence.SelectedItems.Count > 0)
			{
				SequenceView sequenceView = (SequenceView)dgSequence.SelectedItem;
				
				#region CAS DES SEQUENCES EXISTANTES A CORRIGER
				tbNumeroOrdreSequence.Text = sequenceView.strOrdre;
				tbDateSequence.Text = sequenceView.strDate;
				tbReference.Text = "";

				if (sequenceView.ASupprimer)
				{
					tbNumeroOrdreSequence.IsEnabled = false;
					tbDateSequence.IsEnabled = false;
					tbReference.IsEnabled = false;
					BtnModifierSequence.IsEnabled = false;
					BtnSupprimerSequence.IsEnabled = true;					
					cbxBisOrdre.IsEnabled = false;
					cbxDoublonOrdre.IsEnabled = false;
				}
				else if (!sequenceView.OrdreFaux && !sequenceView.DateFausse
						&& string.IsNullOrEmpty(sequenceView.ListReferenceFausse))
				{
					BtnModifierSequence.IsEnabled = false;
					BtnSupprimerSequence.IsEnabled = false;
					tbNumeroOrdreSequence.IsEnabled = false;
					tbDateSequence.IsEnabled = false;
					tbReference.IsEnabled = false;
					cbxBisOrdre.IsEnabled = false;
					cbxDoublonOrdre.IsEnabled = false;
				}
				else
				{
					if (sequenceView.OrdreFaux)
					{
						tbNumeroOrdreSequence.IsEnabled = true;
						cbxDoublonOrdre.IsEnabled = true;
						cbxBisOrdre.IsEnabled = true;
					}
					else
					{
						tbNumeroOrdreSequence.IsEnabled = false;
						cbxDoublonOrdre.IsEnabled = false;
						cbxBisOrdre.IsEnabled = false;
					}

					if (sequenceView.DateFausse)
						tbDateSequence.IsEnabled = true;
					else
						tbDateSequence.IsEnabled = false;

					if (!string.IsNullOrEmpty(sequenceView.ListReferenceFausse))
						tbReference.IsEnabled = true;
					else
						tbReference.IsEnabled = false;

					BtnModifierSequence.IsEnabled = true;
					BtnSupprimerSequence.IsEnabled = false;
				}
				#endregion				
			}
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

		private void BtnModifierSequence_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (dgSequence.SelectedItems.Count == 1)
				{
					if (CurrentImageView.Image.StatutActuel == (int)Enumeration.Image.PHASE3)
					{
						SequenceView sequenceView = (SequenceView)dgSequence.SelectedItem;

						#region CAS DE MODIFICATION DES INDEX D'UNE SEQUENCE EXISTANTE
						using (var ct = new DocumatContext())
						{
							int NewOrdre = sequenceView.Sequence.NUmeroOdre;
							string NewDate = sequenceView.Sequence.DateSequence.ToShortDateString()
								, NewRefs = sequenceView.Sequence.References
								, isSpecial = sequenceView.Sequence.isSpeciale;

							if (sequenceView.OrdreFaux)
							{
								if (!string.IsNullOrWhiteSpace(tbNumeroOrdreSequence.Text))
								{
									if (cbxBisOrdre.IsChecked == true)
									{
										Models.Sequence bisseq = ct.Sequence.Where(s => s.NUmeroOdre == NewOrdre && s.isSpeciale.ToLower().Contains("bis") && s.ImageID == CurrentImageView.Image.ImageID).OrderByDescending(s => s.SequenceID).FirstOrDefault();
										if (bisseq != null)
										{
											string isBisNum = bisseq.isSpeciale;
											isBisNum = isBisNum.Split('_')[1];
											int bisNum = Int32.Parse(isBisNum);
											isSpecial = "bis_" + (bisNum + 1);
										}
										else
										{
											isSpecial = "bis_1";
										}
									}
									else if (cbxDoublonOrdre.IsChecked == true)
									{
										Models.Sequence doubseq = ct.Sequence.Where(s => s.NUmeroOdre == NewOrdre && s.isSpeciale.ToLower().Contains("doublon") && s.ImageID == CurrentImageView.Image.ImageID).OrderByDescending(s => s.SequenceID).FirstOrDefault();
										if (doubseq != null)
										{
											string isDoubNum = doubseq.isSpeciale;
											isDoubNum = isDoubNum.Split('_')[1];
											int DoubNum = Int32.Parse(isDoubNum);
											isSpecial = "doublon_" + (DoubNum + 1);
										}
										else
										{
											isSpecial = "doublon_1";
										}
									}
								}
								else
								{
									throw new Exception("Le numéro d'ordre est incorrecte !!!");
								}
							}

							if (sequenceView.DateFausse)
							{
								if (!string.IsNullOrWhiteSpace(tbDateSequence.Text))
                                {
									NewDate = tbDateSequence.Text;
                                }
								else
                                {
									throw new Exception("La date de référence est incorrecte !!!");
                                }
							}

							if (!string.IsNullOrEmpty(sequenceView.ListReferenceFausse))
							{
								NewRefs = "";
								string[] refsfausse = sequenceView.ListReferenceFausse.Split(',');
								int nbrefat = refsfausse.Where(r => !string.IsNullOrWhiteSpace(r)).Count();
								if (nbrefat != References.Count)
								{
									throw new Exception("Le nombre de référence à corriger est de : " + nbrefat);
								}

								foreach (var ref1 in sequenceView.Sequence.References.Split(','))
								{
									if (!refsfausse.Any(rf => rf.Equals(ref1)) && string.IsNullOrWhiteSpace(ref1))
                                    {
										NewRefs += ref1 + ",";
                                    }
								}

								foreach (var ref1 in References)
								{
									if (!NewRefs.Split(',').Any(rf => rf.Equals(ref1.Value)) && !string.IsNullOrWhiteSpace(ref1.Value))
									{
										NewRefs += ref1.Value + ",";
									}
									else
									{
										throw new Exception("La référence : " + ref1.Value + ", existe déja !!!");
									}
								}
								NewRefs = NewRefs.Remove(NewRefs.Length - 1);
							}

							//Modification de la ligne de séquence
							Models.Sequence UpSequence = ct.Sequence.FirstOrDefault(s => s.SequenceID == sequenceView.Sequence.SequenceID);
							UpSequence.NUmeroOdre = NewOrdre;
							UpSequence.DateSequence = DateTime.Parse(NewDate);
							UpSequence.References = NewRefs;
							UpSequence.DateModif = DateTime.Now;
							UpSequence.isSpeciale = isSpecial;
							UpSequence.PhaseActuelle = 2;

							//Création du correction Phase 1
							Models.Correction correction = new Models.Correction()
							{
								RegistreId = RegistreViewParent.Registre.RegistreID,
								ImageID = sequenceView.Sequence.ImageID,
								SequenceID = sequenceView.Sequence.SequenceID,

								//Indexes Image mis à null
								RejetImage_idx = null,
								MotifRejetImage_idx = null,

								//Indexes de la séquence de l'image
								OrdreSequence_idx = sequenceView.Demande_Correction.OrdreSequence_idx,
								DateSequence_idx = sequenceView.Demande_Correction.DateSequence_idx,
								RefSequence_idx = sequenceView.Demande_Correction.RefSequence_idx,
								RefRejetees_idx = sequenceView.Demande_Correction.RefRejetees_idx,
								ASupprimer = null,

								DateCorrection = DateTime.Now,
								DateCreation = DateTime.Now,
								DateModif = DateTime.Now,
								PhaseCorrection = 3,
								StatutCorrection = 0,
							};
							ct.Correction.Add(correction);
							ct.SaveChanges();

							//On vide les références
							References.Clear();
							tbListeReferences.Visibility = Visibility.Collapsed;
							tbListeReferences.Text = "";

							// Enregistrement de L'action Agent
							DocumatContext.AddTraitement(DocumatContext.TbSequence, UpSequence.SequenceID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION, "CORRECTION PH2 : MODIFICATION DE LA SEQUENCE ID : " + UpSequence.SequenceID + " DE L'IMAGE ID : " + UpSequence.ImageID);

							ActualiseDataCorriger();
						}
						#endregion
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
			//on Récupère les images de Base de Données;
			List<ImageView> images = CurrentImageView.GetSimpleViewsList(RegistreViewParent.Registre);

			if ((currentImage + 2) == images.Count)
			{
				currentImage = -1;
			}
			else if ((currentImage + 2) < images.Count)
			{
				currentImage++;
			}

			ChargerImage(currentImage);
		}

		private void cbxSautOrdre_Checked(object sender, RoutedEventArgs e)
		{
			cbxBisOrdre.IsChecked = false;
			cbxDoublonOrdre.IsChecked = false;
		}

		private void cbxDoublonOrdre_Checked(object sender, RoutedEventArgs e)
		{
			cbxBisOrdre.IsChecked = false;
			BtnModifierSequence.IsEnabled = true;

			if (dgSequence.SelectedItems.Count != 0)
			{
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
			}
		}

		private void cbxBisOrdre_Checked(object sender, RoutedEventArgs e)
		{
			cbxDoublonOrdre.IsChecked = false;			

			if (dgSequence.SelectedItems.Count != 0)
			{
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
			}
		}

		private void cbxBisOrdre_Unchecked(object sender, RoutedEventArgs e)
		{
			if (dgSequence.SelectedItems.Count != 0)
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
						}
						else
						{
							tbNumeroOrdreSequence.Text = (LastSequence.NUmeroOdre + 1).ToString();
						}
					}
				}
			}
		}

		private void cbxDoublonOrdre_Unchecked(object sender, RoutedEventArgs e)
		{
			if (dgSequence.SelectedItems.Count != 0)
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
							BtnModifierSequence.IsEnabled = false;
						}
						else
						{
							tbNumeroOrdreSequence.Text = (LastSequence.NUmeroOdre + 1).ToString();
						}
					}
				}
			}
		}

		private void cbxSautOrdre_Unchecked(object sender, RoutedEventArgs e)
		{

		}

		private void BtnTerminerImage_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Voulez vous terminer cette image ?", "TERMINER IMAGE", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
			{
				if (CurrentImageView.Image.StatutActuel == (int)Enumeration.Image.PHASE3)
				{
					#region FINALISATION DES IMAGES EN CORRECTION
					//Vérification que l'image est totalement corrigée
					//On vérifie que le tableau des séquences erroné est vide cela évite de faire des réquêtes supplementaires
					//On vérifie aussi que le panel rejet image est fermé !!!
					if (dgSequence.Items.Count == 0 && PanelRejetImage.Visibility == Visibility.Collapsed)
					{
						using (var ct = new DocumatContext())
						{
							//Récupération de la demande de correction 
							Models.Correction CorrectionOld = ct.Correction.FirstOrDefault(c => c.RegistreId == RegistreViewParent.Registre.RegistreID
																&& c.SequenceID == null && c.ImageID == CurrentImageView.Image.ImageID);

							// Création d'une correction pour de l'image 
							Models.Correction correction = new Models.Correction()
							{
								RegistreId = RegistreViewParent.Registre.RegistreID,
								ImageID = CurrentImageView.Image.ImageID,
								SequenceID = null,

								//Indexes Image mis à null
								RejetImage_idx = CorrectionOld.RejetImage_idx,
								MotifRejetImage_idx = CorrectionOld.MotifRejetImage_idx,
								ASupprimer = CorrectionOld.ASupprimer,

								//Indexes de la séquence de l'image
								OrdreSequence_idx = null,
								DateSequence_idx = null,
								RefSequence_idx = null,
								RefRejetees_idx = null,

								DateCorrection = DateTime.Now,
								DateCreation = DateTime.Now,
								DateModif = DateTime.Now,
								PhaseCorrection = 3,
								StatutCorrection = 0,
							};
							ct.Correction.Add(correction);
							ct.SaveChanges();

							//Récupération du statut pour le Modifier
							Models.StatutImage statutOld = ct.StatutImage.FirstOrDefault(s => s.ImageID == CurrentImageView.Image.ImageID
																						&& s.Code == CurrentImageView.Image.StatutActuel);
							statutOld.DateFin = DateTime.Now;
							statutOld.DateModif = DateTime.Now;
							ct.SaveChanges();

							//Création du nouveau statut en PHASE 2
							Models.StatutImage statutNew = new StatutImage()
							{
								Code = (int)Enumeration.Image.PHASE2,
								ImageID = CurrentImageView.Image.ImageID,
								DateModif = DateTime.Now,
								DateDebut = DateTime.Now,
								DateCreation = DateTime.Now,
							};
							ct.StatutImage.Add(statutNew);
							ct.SaveChanges();

							Models.Image image = ct.Image.FirstOrDefault(i => i.ImageID == CurrentImageView.Image.ImageID);
							image.StatutActuel = (int)Enumeration.Image.PHASE2;
							image.DateModif = DateTime.Now;
							ct.SaveChanges();

							this.BtnImageSuivante_Click(sender, e);
							ActualiserArborescence();
						}
					}
					#endregion
				}
			}
		}

		private void tbDateSequence_TextChanged(object sender, TextChangedEventArgs e)
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
			else if (e.Key == System.Windows.Input.Key.Up || e.Key == Key.Add)
			{
				Regex regC = new Regex("^[0-9]{1,}/[\\w*]{1,}$");
				if (regC.IsMatch(tbReference.Text))
				{
					tbListeReferences.Text = "";
					tbListeReferences.Visibility = Visibility.Visible;
					try
					{
						References.Add(RefInitiale + tbReference.Text, RefInitiale + tbReference.Text);
						tbReference.Text = "";
					}
					catch (Exception) { }

					foreach (var reference in References)
					{
						tbListeReferences.Text += reference.Value + " ";
					}
				}
			}
			else if (e.Key == System.Windows.Input.Key.Down || e.Key == Key.Subtract)
			{
				tbListeReferences.Text = "";
				if (References.Count != 0)
				{
					References.Remove(References.ElementAt(References.Count - 1).Key);
					foreach (var reference in References)
					{
						tbListeReferences.Text += reference.Value + " ";
					}

					if (References.Count == 0)
					{
						tbListeReferences.Visibility = Visibility.Collapsed;
					}
				}
			}
			else if (e.Key == System.Windows.Input.Key.Enter)
			{
				Regex regC = new Regex("^[0-9]{1,}/");
				if (regC.IsMatch(tbReference.Text))
				{
					try
					{
						References.Add(RefInitiale + tbReference.Text, RefInitiale + tbReference.Text);
					}
					catch (Exception) { }

					if (References.Count != 0)
					{
						if (dgSequence.SelectedItems.Count == 1)
						{
							this.BtnModifierSequence_Click(null, new RoutedEventArgs());
						}
					}
				}
			}
			else
			{
				if (!regex.IsMatch(tbReference.Text))
				{
					tbReference.Text = "/";
					tbReference.CaretIndex = 0;
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
			if (tbReference.Text == "")
			{
				tbReference.Text = "/";
			}
		}

		private void Window_GotFocus(object sender, RoutedEventArgs e){}

		private void btnValideCorrection_Click(object sender, RoutedEventArgs e)
		{
            try
            {
                if (MessageBox.Show("Clôturer la correction du registre ?", "QUESTION", MessageBoxButton.YesNo, MessageBoxImage.Question)
						== MessageBoxResult.Yes)
                {
                    using (var ct = new DocumatContext())
                    {
                        //Récupération de toutes les images du registre 
                        ImageView imageView = new ImageView();
                        List<ImageView> imageViews = imageView.GetSimpleViewsList(RegistreViewParent.Registre);

                        Models.Registre registre = ct.Registre.FirstOrDefault(r => r.RegistreID == RegistreViewParent.Registre.RegistreID);
                        //Ajout de la correction avec le statut corriger
                        Models.Correction NewCorrection = new Models.Correction()
                        {
                            RegistreId = RegistreViewParent.Registre.RegistreID,
                            ImageID = null,
                            SequenceID = null,

                            //Indexes Image mis à null
                            RejetImage_idx = null,
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
                            PhaseCorrection = 3,
                            StatutCorrection = 0,
                        };
                        ct.Correction.Add(NewCorrection);
                        ct.SaveChanges();

                        // Récupération et modification de l'ancien statut du registre
                        Models.StatutRegistre AncienStatut = ct.StatutRegistre.FirstOrDefault(s => s.RegistreID == RegistreViewParent.Registre.RegistreID
                                                                && s.Code == registre.StatutActuel);
                        AncienStatut.DateFin = AncienStatut.DateModif = DateTime.Now;

                        // Création du nouveau statut de registre
                        Models.StatutRegistre NewStatut = new StatutRegistre();
                        NewStatut.Code = (int)Enumeration.Registre.PHASE2;
                        NewStatut.DateCreation = NewStatut.DateDebut = NewStatut.DateModif = DateTime.Now;
                        NewStatut.RegistreID = RegistreViewParent.Registre.RegistreID;
                        ct.StatutRegistre.Add(NewStatut);

                        //Changement du statut du registre
                        registre.StatutActuel = (int)Enumeration.Registre.PHASE2;
                        registre.DateModif = DateTime.Now;
                        ct.SaveChanges();

						//Enregistrement de la tâche
						DocumatContext.AddTraitement(DocumatContext.TbRegistre, RegistreViewParent.Registre.RegistreID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.CORRECTION_PH3_TERMINE, "FIN CORRECTION PH3 DU REGISTRE ID N° " + RegistreViewParent.Registre.RegistreID);

						MainParent.RefreshRegistrePhase2();
                        this.Close();
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

		private void cbxReadOnly_Click(object sender, RoutedEventArgs e)
		{
			if (((CheckBox)sender).IsChecked == true)
			{
				((CheckBox)sender).IsChecked = false;
			}
			else
			{
				((CheckBox)sender).IsChecked = true;
			}
		}

		private void btnSupprimerImage_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btnImporterImage_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog open = new OpenFileDialog();
			open.Filter = "Fichier image |*.jpg;*.png;*.tif";
			open.Multiselect = false;
			string NomFichier = "";

			if (open.ShowDialog() == true)
			{
				NomFichier = open.FileNames[0];
				btnValiderImageImporte.Foreground = Brushes.GreenYellow;
				btnImporterImage.Tag = NomFichier;
				viewImage(NomFichier);
			}
		}

		private void btnValiderImageImporte_Click(object sender, RoutedEventArgs e)
		{
            try
            {
                if(!string.IsNullOrWhiteSpace(btnImporterImage.Tag.ToString()))
                {
                    using (var ct = new DocumatContext())
                    {
                        // Recherche de l'image à remplacer puis remplacer
                        string ImageOld = CurrentImageView.Image.CheminImage;
                        File.Delete(Path.Combine(DossierRacine, ImageOld));
                        File.Copy(btnImporterImage.Tag.ToString(), Path.Combine(DossierRacine, ImageOld));
                        PanelRejetImage.Visibility = Visibility.Collapsed;

						// Récupératiion du contrpôle
						Models.Controle controleRejetImage = ct.Controle.FirstOrDefault(c => c.ImageID == CurrentImageView.Image.ImageID && c.SequenceID == null
														&& c.StatutControle == 1 && c.PhaseControle == 3 && c.RejetImage_idx == 1);

						//Ajout d'une correction image
						// Création d'un controle d'image avec un statut validé
						Models.Correction correction = new Models.Correction()
                        {
                            RegistreId = RegistreViewParent.Registre.RegistreID,
                            ImageID = CurrentImageView.Image.ImageID,
                            SequenceID = null,

                            //Indexes Image mis à null
                            RejetImage_idx = 1,
                            MotifRejetImage_idx = controleRejetImage.MotifRejetImage_idx,
                            ASupprimer = 0,

                            //Indexes de la séquence de l'image
                            OrdreSequence_idx = null,
                            DateSequence_idx = null,
                            RefSequence_idx = null,
                            RefRejetees_idx = null,

                            DateCorrection = DateTime.Now,
                            DateCreation = DateTime.Now,
                            DateModif = DateTime.Now,
                            PhaseCorrection = 3,
                            StatutCorrection = 0,
                        };
                        ct.Correction.Add(correction);
                        ct.SaveChanges();
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
