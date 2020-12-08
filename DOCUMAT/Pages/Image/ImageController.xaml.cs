﻿using DOCUMAT.Models;
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
	/// Logique d'interaction pour ImageController.xaml
	/// </summary>
	public partial class ImageController : Window
    {
        #region ATTRIBUTS
        // Controle Parent par lequel la vue ImageControlle à été lancé
        public Controle.Controle MainParent;

		//Registre parent des images à controler
        public RegistreView RegistreViewParent;
		// ImageView de l'image en cours 
        ImageView CurrentImageView;
		// Numéro de l'image en cours
        private int currentImage = 1;

		string DossierRacine = ConfigurationManager.AppSettings["CheminDossier_Scan"];

		// Définition de l'aborescence
		TreeViewItem registreAbre = new TreeViewItem();
        //Fichier d'image sur le disque dure
        List<FileInfo> fileInfos = new List<FileInfo>();

		//Liste des sequences indexés comme faux dans l'image courant du registre
		//soit dont le numéro est currentImage
		Dictionary<int,SequenceView> ListeSequences = new Dictionary<int, SequenceView>();

        //Initiale des références par type de registre R3 = R / R4 = T
        string RefInitiale = "R";
        //Liste des possibles references pour la séquence en cours de l'image en cours
        Dictionary<String, String> References = new Dictionary<String, String>();
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
                #region RECUPERATION ET AFFICHAGE DE L'IMAGE SELECTIONNEE
                ImageView imageView1 = new ImageView();
				List<ImageView> imageViews = imageView1.GetSimpleViewsList(RegistreViewParent.Registre);
				imageView1 = imageViews.FirstOrDefault(i => i.Image.NumeroPage == currentImage);

                if (imageView1 != null)
                {
                    // Chargement de la visionneuse
                    if (File.Exists(Path.Combine(DossierRacine, imageView1.Image.CheminImage)))
                        viewImage(Path.Combine(DossierRacine, imageView1.Image.CheminImage));
                    else
                        throw new Exception("La page : \"" + imageView1.Image.NumeroPage + "\" est introuvable !!!");

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

                    //Reinitialisation de l'affichage
                    SequenceSide.IsExpanded = false;
                    cbxManqueSequences.IsChecked = false;
                    ImageSide.IsExpanded = false;
                    cbxRejetImage.IsChecked = false;
                    cbxSupprimerImage.IsChecked = false;

                    #endregion

                    #region RECUPERATION DES SEQUENCES DE L'IMAGE

                    //On réinitialise les Manquants
                    tbManqueSequences.Text = "";
                    // On vide le Dictionary des séquences de l'image précédente
                    ListeSequences.Clear();
                    // Récupération des sequences déja renseignées, Différent des images préindexer ayant la référence défaut
                    List<Sequence> sequences = imageView1.context.Sequence.Where(s => s.ImageID == imageView1.Image.ImageID
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
                        //throw new Exception("Cette image n'as pas de séquences !!!");
                    }
                    #endregion

                    #region ADAPTATION DE L'AFFICHAGE EN FONCTION DES INSTANCES DE L'IMAGE
                    // Procédure d'affichage lorsque les statuts d'images changes
                    if (imageView1.Image.StatutActuel == (int)Enumeration.Image.PHASE1)
                    {
                        using (var ct = new DocumatContext())
                        {
                            if (ct.Controle.FirstOrDefault(c => c.ImageID == imageView1.Image.ImageID && c.SequenceID == null
                                && c.PhaseControle == 1 && c.StatutControle == 0) != null)
                            {
								if(imageView1.Image.NumeroPage != -1)
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
									if (ct.Controle.Any(c => c.RegistreId == RegistreViewParent.Registre.RegistreID && c.StatutControle == 1 && c.PhaseControle == 1
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
                    else if (imageView1.Image.StatutActuel == (int)Enumeration.Image.INDEXEE)
                    {
                        if (imageView1.Image.NumeroPage == -1 || imageView1.Image.NumeroPage == 0)
                        {
                            SequenceSide.Visibility = Visibility.Collapsed;
							if(imageView1.Image.NumeroPage == -1)
                            {
								tbxVolume.Text = ": " + RegistreViewParent.Registre.Numero;
								tbxNumDepotDebut.Text = ": " + RegistreViewParent.Registre.NumeroDepotDebut.ToString();
								tbxDateDepotDebut.Text = ": " + RegistreViewParent.Registre.DateDepotDebut.ToShortDateString();
								tbxNumDepotFin.Text = ": " + RegistreViewParent.Registre.NumeroDepotFin.ToString();
								tbxDateDepotFin.Text = ": " + RegistreViewParent.Registre.DateDepotFin.ToShortDateString();
								tbxNbPage.Text = ": " + RegistreViewParent.Registre.NombrePage.ToString();
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

					//Affichage du bouton de validadtion du contrôle si toute les images sont marqué en phase 1 
					if (imageViews.All(i => i.Image.StatutActuel == (int)Enumeration.Image.PHASE1))
                    {
                        btnValideControle.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        btnValideControle.Visibility = Visibility.Collapsed;
                    }
                    #endregion

                    // Actualisation de l'aborescence
                    ActualiserArborescence();
				}
				else
				{
					MessageBox.Show("Impossible de charger la Page, Numero : " + currentImage + " , Elle n'est pas enregistrée !!!", "AVERTISSEMENT", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			}
			catch (Exception ex)
			{
				ex.ExceptionCatcher();
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
			//Information sur l'agent 
			if (!string.IsNullOrEmpty(MainParent.Utilisateur.CheminPhoto))
			{
				AgentImage.Source = new BitmapImage(new Uri(Path.Combine(DossierRacine, MainParent.Utilisateur.CheminPhoto)), new System.Net.Cache.RequestCachePolicy());
			}

			tbxAgentLogin.Text = "Login : " + MainParent.Utilisateur.Login;
			tbxAgentNoms.Text = "Noms : " + MainParent.Utilisateur.Noms;
			tbxAgentType.Text = "Aff : " + Enum.GetName(typeof(Enumeration.AffectationAgent), MainParent.Utilisateur.Affectation);

			//Obtention de la liste des images du registre dans la base de données 
			ImageView imageView1 = new ImageView();
			List<ImageView> imageViews = imageView1.GetSimpleViewsList(RegistreViewParent.Registre);
			tbxQrCode.Text = "QRCODE : " + RegistreViewParent.Registre.QrCode;
			tbxVersement.Text = "Versement N° : " + RegistreViewParent.Versement.NumeroVers.ToString();
			tbxService.Text = "Service : " + RegistreViewParent.ServiceVersant.Nom;

			//Procédure de modification de l'indicateur de reussite
			double perTerminer = (((float)imageViews.Where(i => i.Image.StatutActuel == (int)Enumeration.Image.PHASE1).Count() / (float)imageViews.Count()) * 100);
			string EnCours = (Math.Round(((float)imageViews.Where(i => i.Image.StatutActuel == (int)Enumeration.Image.INDEXEE).Count() / (float)imageViews.Count()) * 100, 1)).ToString() + " %";

			// Récupération des images en correction
			List<Models.Image> imagesEnCorrection = new List<Models.Image>();
			List<Models.Image> imagesValide = new List<Models.Image>();
			foreach(var image in imageViews)
            {
				using(var ct = new DocumatContext())
                {
					if(ct.Correction.Any(c=>c.RegistreId == RegistreViewParent.Registre.RegistreID && c.ImageID == image.Image.ImageID 
						&& c.SequenceID == null && c.StatutCorrection == 1 && c.PhaseCorrection == 1))
                    {
						imagesEnCorrection.Add(image.Image);
					}
					else if(ct.Controle.Any(c => c.RegistreId == RegistreViewParent.Registre.RegistreID && c.ImageID == image.Image.ImageID
						&& c.SequenceID == null && c.StatutControle == 0 && c.PhaseControle == 1))
                    {
						imagesValide.Add(image.Image);

					}
                }
            }
			string EnCorrection = (Math.Round(((float)imagesEnCorrection.Count() / (float)imageViews.Count()) * 100, 1)).ToString() + " %";;
			string Valide = (Math.Round(((float)imagesValide.Count() / (float)imageViews.Count()) * 100, 1)).ToString() + " %";;

			tbxImageTerminer.Text = "Terminé : " + Math.Round(perTerminer, 1) + " % ";
			tbxImageRejete.Text = "En Correction : " + EnCorrection;
			tbxImageValide.Text = "Validé : " + Valide;
			tbxImageEnCours.Text = "Non Traité : " + EnCours;
			ArcIndicator.EndAngle = (perTerminer * 360) / 100;
			TextIndicator.Text = Math.Round(perTerminer, 1) + "%";
		}

		/// <summary>
		/// Chargement des éléments de l'Aborescence
		/// </summary>
		public void LoadAborescence()
		{
			#region INSPECTION DU DOSSIER DE REGISTRE ET CREATION DE L'ABORESCENCE
			// Chargement de l'aborescence
			// Récupération de la liste des images contenu dans le dossier du registre
			var files = Directory.GetFiles(Path.Combine(DossierRacine, RegistreViewParent.Registre.CheminDossier));

			// Affichage et configuration de la TreeView
			registreAbre.Items.Clear();
			registreAbre.Header = RegistreViewParent.Registre.QrCode;
			registreAbre.Tag = RegistreViewParent.Registre.QrCode;
			registreAbre.FontWeight = FontWeights.Normal;
			registreAbre.Foreground = Brushes.White;

			// Définition de la lettre de référence pour ce registre
			if (RegistreViewParent.Registre.Type == "R4")
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
				List<Models.ManquantImage> manquantImages = ct.ManquantImage.Where(m => m.IdRegistre == RegistreViewParent.Registre.RegistreID).OrderBy(m => m.NumeroPage).ToList();
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
			ActualiserArborescence();
			#endregion
		}

		/// <summary>
		/// Actualise l'aborescence les étiquettes de l'aborescence!!!
		/// </summary>
		public void ActualiserArborescence()
		{
			// Définition des types d'icon dans l'aborescence en fonction des statuts des images
			foreach (TreeViewItem item in registreAbre.Items)
			{
				using (var ct = new DocumatContext())
				{
					Models.ManquantImage manquantImage = ct.ManquantImage.FirstOrDefault(m => m.IdRegistre == RegistreViewParent.Registre.RegistreID
															&& m.NumeroPage.ToString() == item.Header.ToString());
					if(manquantImage != null)
					{
						item.Tag = "notFound";
					}
					else 
					{
						Models.Image image1 = ct.Image.FirstOrDefault(i => i.NomPage.ToLower() == item.Header.ToString().Remove(item.Header.ToString().Length - 4).ToLower()
							&& i.StatutActuel == (int)Enumeration.Image.PHASE1 && i.RegistreID == RegistreViewParent.Registre.RegistreID);
						if (image1 != null)
						{
							if (ct.Controle.FirstOrDefault(c=>c.ImageID == image1.ImageID && c.SequenceID == null 
							   && c.PhaseControle == 1 && c.StatutControle == 0) != null)
                            {
								if(image1.NumeroPage != -1)
                                {
									item.Tag = "valide";
                                }
								else
                                {
									if(ct.Controle.Any(c=>c.RegistreId == RegistreViewParent.Registre.RegistreID && c.StatutControle == 1 && c.PhaseControle == 1
														  && c.ImageID == null  && c.SequenceID == null))
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

			// On rend tout les items de l'aborescence normaux et on met en gras l'élément concerné
			foreach (TreeViewItem item in registreAbre.Items)
			{
				item.FontWeight = FontWeights.Normal;
				int numeroPage = 0;
				Models.ManquantImage manquantImage = null;
				using(var ct = new DocumatContext())
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
		#endregion

		public ImageController()
        {
            InitializeComponent();
			FolderView.ContextMenu = (ContextMenu)this.FindResource("cmManquantImage");
		}

		public ImageController(RegistreView registreview, Controle.Controle controle) :this()
        {
			MainParent = controle;
			RegistreViewParent = registreview;
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

			if(manquantImage != null)
			{
				if (MessageBox.Show("Voulez vous retirer ce manquant ?","QUESTION",MessageBoxButton.YesNo,MessageBoxImage.Question)
					== MessageBoxResult.Yes)
				{
					using (var ct = new DocumatContext())
					{
						ct.ManquantImage.Remove(ct.ManquantImage.FirstOrDefault(m => m.IdRegistre == RegistreViewParent.Registre.RegistreID
													&& m.NumeroPage.ToString() == treeViewItem.Header.ToString()));
						ct.SaveChanges();
					}
					registreAbre.Items.Remove(treeViewItem);
				}
			}
			//Cas d'une page normal (Page numérotée)
			else if (Int32.TryParse(treeViewItem.Header.ToString().Remove(treeViewItem.Header.ToString().Length - 4), out numeroPage))
			{
				// Chargement de la première page/ou de la page en cours
				using (var ct = new DocumatContext())
				{
					Models.Image image = ct.Image.OrderBy(i => i.NumeroPage).FirstOrDefault(i => i.StatutActuel == (int)Enumeration.Image.INDEXEE
					&& i.RegistreID == RegistreViewParent.Registre.RegistreID);
					currentImage = (image != null) ? image.NumeroPage : -1;
					ChargerImage(currentImage);
					currentImage = numeroPage;
				}
			}
			else if (treeViewItem.Header.ToString().Remove(treeViewItem.Header.ToString().Length - 4).ToLower() == "PAGE DE GARDE".ToLower())
			{
				//Affichage de la page de garde
				currentImage = -1;
				imageView1 = imageViews.FirstOrDefault(i => i.Image.NumeroPage == -1);

				tbxNomPage.Text = "PAGE DE GARDE";
				tbxNumeroPage.Text = "";
				dgSequence.ItemsSource = null;
			}
			else if (treeViewItem.Header.ToString().Remove(treeViewItem.Header.ToString().Length - 4).ToLower() == "PAGE D'OUVERTURE".ToLower())
			{
				//Affichage de la page d'ouverture
				currentImage = 0;
				imageView1 = imageViews.FirstOrDefault(i => i.Image.NumeroPage == 0);

				tbxNomPage.Text = "PAGE D'OUVERTURE";
				tbxNumeroPage.Text = "";
				dgSequence.ItemsSource = null;
			}

			// Chargement de l'image		
			ChargerImage(currentImage);
		}

		private void dgSequence_LoadingRow(object sender, DataGridRowEventArgs e)
        {
		}

        private void dgSequence_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
				// Chargement de l'aborescence
				LoadAborescence();

				#region RECUPERATION DES PAGES SPECIAUX
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

				#region RECUPERATION PAGES NUMEROTEES
				// Modification des Pages Numerotées / Pré-Indexées
				// Les Pages doivent être scannées de manière à respecter la nomenclature de fichier standard 
				ImageView imageView1 = new ImageView();
				List<ImageView> imageViews = imageView1.GetSimpleViewsList(RegistreViewParent.Registre);
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

				// Chargement de la première page/ou de la page en cours
				using (var ct = new DocumatContext())
				{
					Models.Image image = ct.Image.OrderBy(i => i.NumeroPage).FirstOrDefault(i => i.StatutActuel == (int)Enumeration.Image.INDEXEE
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
															&& t.TypeTraitement == (int)Enumeration.TypeTraitement.CONTROLE_PH1_DEBUT);
						if (traitementAttr != null)
						{
							Models.Traitement traitementRegistreAgent = ct.Traitement.FirstOrDefault(t => t.TableSelect == DocumatContext.TbRegistre && t.TableID == RegistreViewParent.Registre.RegistreID
													&& t.TypeTraitement == (int)Enumeration.TypeTraitement.CONTROLE_PH1_DEBUT && t.AgentID == MainParent.Utilisateur.AgentID);
							Models.Traitement traitementAutreAgent = ct.Traitement.FirstOrDefault(t => t.TableSelect == DocumatContext.TbRegistre && t.TableID == RegistreViewParent.Registre.RegistreID
													&& t.TypeTraitement == (int)Enumeration.TypeTraitement.CONTROLE_PH1_DEBUT && t.AgentID != MainParent.Utilisateur.AgentID);

							if(traitementAutreAgent != null)
							{
								Models.Agent agent = ct.Agent.FirstOrDefault(t => t.AgentID == traitementAutreAgent.AgentID);
								MessageBox.Show("Ce Registre est en Cours traitement par l'agent : " + agent.Noms, "REGISTRE EN CONTROLE PH1", MessageBoxButton.OK,MessageBoxImage.Information);
								this.Close();
							}
						}
						else
						{
							if (MessageBox.Show("Voulez vous commencez le contrôle ?", "COMMNCER LE CONTROLE PH1", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
							{							
								DocumatContext.AddTraitement(DocumatContext.TbRegistre, RegistreViewParent.Registre.RegistreID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.CONTROLE_PH1_DEBUT,"CONTROLE PH1 COMMENCER");
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

		private void dgSequence_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
		}

        private void tbDateSequence_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void dgSequence_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }

		private void cbxRejetImage_Checked(object sender, RoutedEventArgs e)
		{
			cbxSupprimerImage.IsChecked = false;
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
		}

		private void cbxSupprimerImage_Unchecked(object sender, RoutedEventArgs e)
		{
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
			//PanelSupprimeSequence.Visibility = Visibility.Visible;
		}

		private void cbxSuppimerSequences_Unchecked(object sender, RoutedEventArgs e)
		{
			//PanelSupprimeSequence.Visibility = Visibility.Collapsed;
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
                                RegistreId = RegistreViewParent.Registre.RegistreID,
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
                                    RegistreId = RegistreViewParent.Registre.RegistreID,
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
                                    IdImage = CurrentImageView.Image.ImageID,
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
                    int RejetImage = 0, ASupprimer = 0, StatutImage = 0;
                    string MotifRejet = "";
                    if (cbxRejetImage.IsChecked == true)
                    {
                        RejetImage = 1;
                        MotifRejet = cbRejetImage.Text;
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
                            RegistreId = RegistreViewParent.Registre.RegistreID,
                            ImageID = CurrentImageView.Image.ImageID,
                            SequenceID = null,

                            //Indexes Image mis à null
                            RejetImage_idx = RejetImage,
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
                                RegistreId = RegistreViewParent.Registre.RegistreID,
                                ImageID = CurrentImageView.Image.ImageID,
                                SequenceID = null,

                                //Indexes Image mis à null
                                RejetImage_idx = RejetImage,
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
                        Models.StatutImage statutActuel = ct.StatutImage.FirstOrDefault(s => s.ImageID == CurrentImageView.Image.ImageID && s.Code == (int)Enumeration.Image.INDEXEE);
                        statutActuel.DateModif = DateTime.Now;
                        statutActuel.DateFin = DateTime.Now;

                        Models.StatutImage NouvauStatut = new StatutImage()
                        {
                            ImageID = CurrentImageView.Image.ImageID,
                            Code = (int)Enumeration.Image.PHASE1,
                            DateDebut = DateTime.Now,
                            DateCreation = DateTime.Now,
                            DateModif = DateTime.Now,
                        };
                        ct.StatutImage.Add(NouvauStatut);

                        // Changement du statut actuel de l'image
                        Models.Image image = ct.Image.FirstOrDefault(i => i.ImageID == CurrentImageView.Image.ImageID);
                        image.StatutActuel = (int)Enumeration.Image.PHASE1;
                        ct.SaveChanges();

                        //Enregistrement de la tâche
                        DocumatContext.AddTraitement(DocumatContext.TbImage, image.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION, "CONTROLE PH1 : CONTROLE DE L'IMAGE : " + image.NumeroPage + " DU REGISTRE ID N° " + RegistreViewParent.Registre.RegistreID);

                        //Actualisation de l'aborescence
                        HeaderInfosGetter();

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
			catch(Exception ex)
            {
				ex.ExceptionCatcher();
            }
		}

		private void cbxNumOrdre_Click(object sender, RoutedEventArgs e)
		{
			CheckBox cbxOrdre = (CheckBox)sender;			
			SequenceView sequenceView = (SequenceView)dgSequence.SelectedItem;
			if(dgSequence.SelectedItems.Count == 1)
			{
				if(cbxOrdre.IsChecked == true)
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
					ListeSequences.TryGetValue(sequenceView.Sequence.SequenceID,out newseq);
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

		private void BtnAnnuleSupSequences_Click(object sender, RoutedEventArgs e)
		{
			//tbSuprrimeSequences.Text = "";
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
				if(cbxOrdre.IsChecked == true)
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
                ImageView imageView1 = new ImageView();
                List<ImageView> imageViews = imageView1.GetSimpleViewsList(RegistreViewParent.Registre);
                //Si toute les images sont marqué en phase 1 
                if (imageViews.All(i => i.Image.StatutActuel == (int)Enumeration.Image.PHASE1))
                {
                    //On demande une autorisation
                    if (MessageBox.Show("Voulez vous terminer le contrôle de ce registre ?", "QUESTION", MessageBoxButton.YesNo, MessageBoxImage.Question)
                        == MessageBoxResult.Yes)
                    {
                        //Ne prends en charge pour l'instant les manquants d'image
                        using (var ct = new DocumatContext())
                        {
                            int Registre_is_check = 0;
                            // Cas où un index au moins à été réjété
                            if (ct.Controle.Any(c => c.RegistreId == RegistreViewParent.Registre.RegistreID && c.PhaseControle == 1 && c.StatutControle == 1))
                            {
                                Registre_is_check = 1;
                            }

                            //Cas où il y'a un manquant
                            if (ct.ManquantImage.Any(m => m.IdRegistre == RegistreViewParent.Registre.RegistreID && m.statutManquant == 1))
                            {
                                Registre_is_check = 1;
                            }

							// Récupération du controle registre
							if(Registre_is_check == 1)
                            {
								Models.Controle controleRegistre = ct.Controle.FirstOrDefault(c => c.RegistreId == RegistreViewParent.Registre.RegistreID
																							&& c.ImageID == null && c.SequenceID == null && c.PhaseControle == 1 && c.StatutControle == 1);
								if (controleRegistre == null)
								{
									controleRegistre.StatutControle = 1;
									controleRegistre.DateModif = DateTime.Now;

									// Création de la correction 
									Models.Correction correction = new Models.Correction()
									{
										RegistreId = RegistreViewParent.Registre.RegistreID,
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

                            // Changement du statut du registre 
                            // Récupération de l'ancien statut 
                            Models.StatutRegistre StatutActu = ct.StatutRegistre.FirstOrDefault(s => s.RegistreID == RegistreViewParent.Registre.RegistreID
                                                                                                && s.Code == (int)Enumeration.Registre.INDEXE);
                            StatutActu.DateFin = DateTime.Now;
                            StatutActu.DateModif = DateTime.Now;

                            Models.StatutRegistre NewStatut = new StatutRegistre()
                            {
                                RegistreID = RegistreViewParent.Registre.RegistreID,
                                Code = (int)Enumeration.Registre.PHASE1,
                                DateDebut = DateTime.Now,
                                DateCreation = DateTime.Now,
                                DateModif = DateTime.Now,
                            };
                            ct.StatutRegistre.Add(NewStatut);

                            //On peut Récupérer et changer le statutActuel du registre 
                            Models.Registre registre = ct.Registre.FirstOrDefault(r => r.RegistreID == RegistreViewParent.Registre.RegistreID);
                            registre.StatutActuel = (int)Enumeration.Registre.PHASE1;
                            registre.DateModif = DateTime.Now;
                            ct.SaveChanges();

                            //Enregistrement de la tâche
                            DocumatContext.AddTraitement(DocumatContext.TbRegistre, RegistreViewParent.Registre.RegistreID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.CONTROLE_PH1_TERMINE, "FIN CONTROLE PH1 DU REGISTRE ID N° " + RegistreViewParent.Registre.RegistreID);

                            //On ferme la fenêtre et on met à jour le datagrid du controle
                            MainParent.RefreshRegistrePhase1();
                            this.Close();
                        }
                    }
                }
                else
                {
                    btnValideControle.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
				ex.ExceptionCatcher();
            }
		}

		private void btnSuivant_Click(object sender, RoutedEventArgs e)
		{
			//on vérifie que tous t'es ajouté dans la BD;
			List<ImageView> images = CurrentImageView.GetSimpleViewsList(RegistreViewParent.Registre);

			if (fileInfos.Count == images.Count)
			{
				if ((currentImage + 2) == images.Count)
				{
					currentImage = 1;
				}
				else if ((currentImage + 2) < images.Count)
				{
					currentImage++;
				}

				ChargerImage(currentImage);
			}
			else
			{
				MessageBox.Show("Le nombre d'images dans le dossier n'est pas identique à celui attendu !!! " + fileInfos.Count + " / " + images.Count);
			}
		}

		private void AddSequence_Click(object sender, RoutedEventArgs e)
		{

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

        private void dgSequence_KeyUp(object sender, KeyEventArgs e)
        {
        }

        private void dgSequence_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
        }

        private void dgSequence_PreviewKeyUp(object sender, KeyEventArgs e)
        {
        }

        private void dgSequence_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {
        }

        private void BtnTerminerControlePageDeGarde_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Voulez vous terminer le contrôle de Page de Garde et des Index du Registres ?", "Terminer Le Contrôle ?", MessageBoxButton.YesNo, MessageBoxImage.Question)
				== MessageBoxResult.Yes)
			{
				//Booléen d'identification de rejet sur un index
				bool Image_Index_is_reject = false;

				#region VERFIFICATION DE L'IMAGE 
				// Enregistrement de l'image !!! 
				int RejetImage = 0, ASupprimer = 0, StatutImage = 0;
				string MotifRejet = "";
				if (cbxRejetImage.IsChecked == true)
				{
					RejetImage = 1;
					MotifRejet = cbRejetImage.Text;
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
						RegistreId = RegistreViewParent.Registre.RegistreID,
						ImageID = CurrentImageView.Image.ImageID,
						SequenceID = null,

						//Indexes Image mis à null
						RejetImage_idx = RejetImage,
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
							RegistreId = RegistreViewParent.Registre.RegistreID,
							ImageID = CurrentImageView.Image.ImageID,
							SequenceID = null,

							//Indexes Image mis à null
							RejetImage_idx = RejetImage,
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
					Models.StatutImage statutActuel = ct.StatutImage.FirstOrDefault(s => s.ImageID == CurrentImageView.Image.ImageID && s.Code == (int)Enumeration.Image.INDEXEE);
					statutActuel.DateModif = DateTime.Now;
					statutActuel.DateFin = DateTime.Now;

					Models.StatutImage NouvauStatut = new StatutImage()
					{
						ImageID = CurrentImageView.Image.ImageID,
						Code = (int)Enumeration.Image.PHASE1,
						DateDebut = DateTime.Now,
						DateCreation = DateTime.Now,
						DateModif = DateTime.Now,
					};
					ct.StatutImage.Add(NouvauStatut);

					// Changement du statut actuel de l'image
					Models.Image image = ct.Image.FirstOrDefault(i => i.ImageID == CurrentImageView.Image.ImageID);
					image.StatutActuel = (int)Enumeration.Image.PHASE1;
					ct.SaveChanges();				

					//Enregistrement de la tâche
					DocumatContext.AddTraitement(DocumatContext.TbImage, image.ImageID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION, "CONTROLE PH1 : CONTROLE DE LA PAGE DE GARDE DU REGISTRE ID N° " + RegistreViewParent.Registre.RegistreID);

					//Contrôle des rejets index du Registre 
					int StatutRegistre = 0, NumVolume = 0, NumDepotDebut = 0, DateDepotDebut = 0,
						NumDepotFin = 0, DateDepotFin = 0, NbPage = 0;

					if(cbVolume.IsChecked == true)
                    {
						NumVolume = 1;
						StatutRegistre = 1;
                    }
					if(cbNumDepotDebut.IsChecked == true)
                    {
						NumDepotDebut = 1;
						StatutRegistre = 1;
					}
					if(cbDateDepotDebut.IsChecked == true)
                    {
						DateDepotDebut = 1;
						StatutRegistre = 1;
					}
					if(cbNumDepotFin.IsChecked == true)
                    {
						NumDepotFin = 1;
						StatutRegistre = 1;
                    }
					if(cbDateDepotFin.IsChecked == true)
                    {
						DateDepotFin = 1;
						StatutRegistre = 1;
					}
					if(cbNbPage.IsChecked == true)
                    {
						NbPage = 1;
						StatutRegistre = 1;
                    }

					//Création du controle de registre 
					Models.Controle controleRegistre = new Models.Controle()
					{
						RegistreId = RegistreViewParent.Registre.RegistreID,
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
							RegistreId = RegistreViewParent.Registre.RegistreID,
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
					DocumatContext.AddTraitement(DocumatContext.TbRegistre, RegistreViewParent.Registre.RegistreID, MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION, "CONTROLE PH1 : CONTROLE DES INDEX DU REGISTRE");

					//Actualisation de l'aborescence
					HeaderInfosGetter();

					// Charger l'élément suivant 
					this.btnSuivant_Click(sender, e);
				}
				#endregion
			}
		}

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
			double MyWindowWidth = 1600;
			double MyWindowHeight = 900;
			double dgSMaxHeight = 400;
			double listViewMaxHeight = dgSMaxHeight/3;

			// Gestion de l'IHM
			if(this.ActualHeight < MyWindowHeight)
            {
				dgSequence.MaxHeight = dgSMaxHeight - (MyWindowHeight - this.ActualHeight);
            }
			else
            {
				dgSequence.MaxHeight = dgSMaxHeight + (MyWindowHeight - this.ActualHeight);
			}
		}

        private void dgSequence_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
			//if (dgSequence.SelectedItem != null)
			//{
			//	SequenceView sequenceView = dgSequence.SelectedItem as SequenceView;
			//	int NbRefs = 0;
			//	if(Int32.TryParse(sequenceView.NbRefsManquant,out NbRefs) && NbRefs > 0)
   //             {
			//		SequenceView newseq = new SequenceView();
			//		ListeSequences.TryGetValue(sequenceView.Sequence.SequenceID, out newseq);
			//		ListeSequences.Remove(sequenceView.Sequence.SequenceID);
			//		ListeSequences.Add(sequenceView.Sequence.SequenceID, newseq);
			//	}
			//	else
			//	{
			//		SequenceView newseq = new SequenceView();
			//		ListeSequences.TryGetValue(sequenceView.Sequence.SequenceID, out newseq);
			//		ListeSequences.Remove(sequenceView.Sequence.SequenceID);
			//		ListeSequences.Add(sequenceView.Sequence.SequenceID, newseq);
			//	}
			//}
			//else
			//{
			//	return;
			//}
		}
    }
}
