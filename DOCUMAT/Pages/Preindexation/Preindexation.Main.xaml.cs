using DOCUMAT.Models;
using DOCUMAT.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace DOCUMAT.Pages.Preindexation
{
    /// <summary>
    /// Logique d'interaction pour Preindexation.xaml
    /// </summary>
    public partial class Preindexation : Page
    {

        Models.Registre registre = new Models.Registre();
        List<Sequence> Sequences = new List<Sequence>();
        Models.Agent Utilisateur = new Models.Agent();
        Models.Image LastImage;

        public Preindexation(Models.Agent user)
        {
            Utilisateur = user;
            InitializeComponent();
            dtDateSequence.Language = XmlLanguage.GetLanguage("fr-FR");
            dtDateSequence.SelectedDate = DateTime.Now;
        }

        private void AjouterFeuillet_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SupprimerFeuillet_Click(object sender, RoutedEventArgs e)
        {
            
        }

        //Affichage de la liste des images du registre à préindexer
        //Pour la préindexation le registre doit être au statut Créer
        private void AfficherFeuillets_Click(object sender, RoutedEventArgs e)
        {            
            if(AfficherFeuillets.IsChecked == true && dgRegistre.SelectedItems.Count == 1)
            {
                using (var ct = new DocumatContext())
                {
                    registre = ((RegistreView)dgRegistre.SelectedItem).Registre;

                    if (registre != null)
                    {
                        Models.Traitement traitement = ct.Traitement.FirstOrDefault(t => t.TableSelect.ToUpper() == DocumatContext.TbRegistre.ToUpper()
                                && t.TableID == registre.RegistreID && t.TypeTraitement == (int)Enumeration.TypeTraitement.PREINDEXATION_REGISTRE && t.AgentID != Utilisateur.AgentID);
                        if(traitement == null)
                        {
                            // Vérification qu'il n'existe pas des registre ouvert non terminée pour l'agent 
                            // récupération des registres traité par l'agent
                            List<Models.Traitement> traitementsAgent = ct.Traitement.Where(t => t.TableSelect.ToUpper() == DocumatContext.TbRegistre.ToUpper() && 
                                                    t.TypeTraitement == (int)Enumeration.TypeTraitement.PREINDEXATION_REGISTRE && t.AgentID == Utilisateur.AgentID).ToList();
                            List<Models.Registre> registreNonTermines = new List<Models.Registre>();
                            Models.Registre registreEnCours = null;
                            // Vérification que ces registres sont terminées
                            foreach(Traitement tr in traitementsAgent)
                            {
                                if (!ct.Traitement.Any(t => t.TableSelect.ToUpper() == DocumatContext.TbRegistre.ToUpper() && t.TableID == tr.TableID
                                    && t.TypeTraitement == (int)Enumeration.TypeTraitement.PREINDEXATION_REGISTRE_TERMINEE) )
                                {
                                    // Si le registre ouvert est le registre en cours 
                                    if(tr.TableID == registre.RegistreID && tr.TableSelect.ToUpper() == DocumatContext.TbRegistre.ToUpper())
                                    {
                                        registreEnCours = ct.Registre.FirstOrDefault(r => r.RegistreID == tr.TableID );
                                    }
                                    else
                                    {
                                        registreNonTermines.Add(ct.Registre.FirstOrDefault(r => r.RegistreID == tr.TableID));  
                                    }
                                } 
                            }

                            if(registreEnCours !=null || registreNonTermines.Count == 0)
                            {
                                // Vérification des images déclaré de ce registre 
                                if(registre.NombrePage > 0)
                                {
                                    // Cas où c'est un nouveau registre qui est ouvert 
                                    if (registreEnCours == null)
                                    {
                                        if (MessageBox.Show("Voulez vous commencer la préindexation de ce Registre, en acceptant, vous serez le seul à pouvoir y travailler ?", "COMMENCER LA PREINDEXATION", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                                        {
                                            AfficherFeuillets.IsChecked = false;
                                            return;
                                        }
                                        else
                                        {
                                            if (ct.Traitement.Any(t => t.TableSelect.ToUpper() == DocumatContext.TbRegistre.ToUpper()
                                                     && t.TableID == registre.RegistreID && t.TypeTraitement == (int)Enumeration.TypeTraitement.PREINDEXATION_REGISTRE && t.AgentID != Utilisateur.AgentID))
                                            {
                                                MessageBox.Show("Ce registre viens malheureusement d'être attribué à un autre Agent !!!", "Registre Attribué !!!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                                return;                                                
                                            }
                                            else
                                            {
                                                DocumatContext.AddTraitement(DocumatContext.TbRegistre, registre.RegistreID, Utilisateur.AgentID, (int)Enumeration.TypeTraitement.PREINDEXATION_REGISTRE, "DEBUT PREINDEXATION DU REGISTRE N°ID : " + registre.RegistreID);
                                            }
                                        }
                                    }

                                    // Chargement de l'image du Qrcode de la ligne
                                    Zen.Barcode.CodeQrBarcodeDraw qrcode = Zen.Barcode.BarcodeDrawFactory.CodeQr;
                                    var image = qrcode.Draw(registre.QrCode, 40);
                                    var imageConvertie = image.ConvertDrawingImageToWPFImage(null, null);
                                    QrCodeImage.Source = imageConvertie.Source;

                                    ImageView imageView = new ImageView();
                                    List<ImageView> imageViews = imageView.GetSimpleViewsList(registre);
                                    dgFeuillet.ItemsSource = imageViews;
                                    if (imageViews.Count() > 0)
                                    {
                                        LastImage = imageViews.OrderByDescending(i => i.Image.ImageID).FirstOrDefault().Image;
                                        tbNumeroFeuillet.Text = (LastImage.NumeroPage + 1) + "";
                                        tbNumeroOrdreDebut.Text = (LastImage.FinSequence + 1).ToString();
                                        if (registre.NombrePage == imageViews.Count)
                                        {
                                            MarquerPreindexer.Visibility = Visibility.Visible;
                                            btnSaveFeuillet.IsEnabled = false;
                                        }
                                        else
                                        {
                                            MarquerPreindexer.Visibility = Visibility.Collapsed;
                                            btnSaveFeuillet.IsEnabled = true;
                                        }
                                    }
                                    else
                                    {
                                        tbNumeroFeuillet.Text = "1";
                                        tbNumeroFeuillet.IsEnabled = false;
                                        tbNumeroOrdreDebut.Text = "1";
                                        btnSaveFeuillet.IsEnabled = true;
                                    }
                                    // Hide dgRegistre et Show du dgFeuillet
                                    dgRegistre.Visibility = Visibility.Collapsed;
                                    PanelFeuillet.Visibility = Visibility.Visible;
                                    PanelRecherche.Visibility = Visibility.Collapsed;
                                }
                                else
                                {
                                    AfficherFeuillets.IsChecked = false;
                                    MessageBox.Show("Ce registre n'a pas de Feuillets à Préindexer ni à Indexer !!!", "AVERTISSEMENT", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                            else if(registreNonTermines.Count > 0) 
                            {
                                AfficherFeuillets.IsChecked = false;
                                MessageBox.Show("Vous avez ouvert le registre : " + registreNonTermines[0].QrCode + ", vous devez le terminer avant d'entamer un autre registre !!!", "AVERTISSEMENT", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        else
                        {
                            AfficherFeuillets.IsChecked = false;
                            Models.Agent agentFeuillet = ct.Agent.FirstOrDefault(a => a.AgentID == traitement.AgentID);
                            MessageBox.Show("Ce registre est déja en cours de traitement par l'agent  : " + agentFeuillet.Noms, "AVERTISSEMENT", MessageBoxButton.OK,MessageBoxImage.Warning);
                        }
                    }
                    else
                    {
                        MarquerPreindexer.Visibility = Visibility.Collapsed;
                        dgRegistre.Visibility = Visibility.Visible;
                        PanelFeuillet.Visibility = Visibility.Collapsed;
                        PanelRecherche.Visibility = Visibility.Visible;
                        QrCodeImage.Source = null;
                    }
                }
            }
            else
            {
                MarquerPreindexer.Visibility = Visibility.Collapsed;
                dgRegistre.Visibility = Visibility.Visible;
                PanelFeuillet.Visibility = Visibility.Collapsed;
                PanelRecherche.Visibility = Visibility.Visible;
                QrCodeImage.Source = null;
            }
        }

        private void dgFeuillet_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //Définition de la colonne des numéros d'odre
            // En plus il faut que EnableRowVirtualization="False"
            ImageView view = (ImageView)e.Row.Item;
            view.NumeroOrdre = e.Row.GetIndex() + 1;
            e.Row.Item = view;
        }

        private void dgFeuillet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void dgFeuillet_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {

        }

        private void tbNumeroFeuillet_TextChanged(object sender, TextChangedEventArgs e)
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

        private void tbNumeroOrdreDebut_TextChanged(object sender, TextChangedEventArgs e)
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

        private void tbNumeroOrdreFin_TextChanged(object sender, TextChangedEventArgs e)
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

        private void btnAnnuleFeuillet_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSaveFeuillet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Attention en Terminant ce feuillet, Vous ne pourrez plus le Modifier !!", "TERMINER LE FEUILLET ?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    ImageView imageView = new ImageView();

                    if (MarquerPreindexer.Visibility != Visibility.Visible)
                    {
                        int numeroPage = 0;
                        if (Int32.TryParse(tbNumeroFeuillet.Text, out numeroPage))
                            imageView.Image.NumeroPage = numeroPage;
                        else
                            throw new Exception("le numero de page du feuillet doit être de type numérique");

                        int numeroFeuilletDebut = 0;
                        if (Int32.TryParse(tbNumeroOrdreDebut.Text, out numeroFeuilletDebut))
                            imageView.Image.DebutSequence = numeroFeuilletDebut;
                        else
                            throw new Exception("le premier numero d'ordre doit être de type numérique");

                        int numeroFeuilletFin = 0;
                        if (Int32.TryParse(tbNumeroOrdreFin.Text, out numeroFeuilletFin))
                            imageView.Image.FinSequence = numeroFeuilletFin;
                        else
                            throw new Exception("le dernier numéro d'ordre doit être de type numérique");

                        if (dtDateSequence.SelectedDate == null)
                            throw new Exception("La date de la première séquence doit être renseignée !!!");

                        if (imageView.Image.DebutSequence > imageView.Image.FinSequence)
                            throw new Exception("Le numéro d'ordre de début doit être inférieur au numéro d'ordre de fin");

                        //imageView.Image.NumeroPageDeclare = 0;
                        imageView.Image.RegistreID = registre.RegistreID;
                        //imageView.Image.CheminImage = "Defaut";
                        imageView.Image.DateCreation = DateTime.Now;
                        imageView.Image.DateModif = DateTime.Now;
                        imageView.Image.DateScan = DateTime.Now;
                        imageView.Image.DateDebutSequence = dtDateSequence.SelectedDate.Value;
                        //imageView.Image.StatutActuel = 0;
                        //imageView.Image.Taille = "0"; // Doit être changer en type numéric
                        //imageView.Image.Type = "Defaut";                
                        LastImage = imageView.AddPreIndex();

                        //Ajout de la liste des séquences à multiréférences
                        if (Sequences.Count > 0)
                        {
                            foreach (var seq in Sequences)
                            {
                                seq.ImageID = LastImage.ImageID;
                            }
                            imageView.AddSequencesPreIndex(Sequences.OrderBy(s => s.NUmeroOdre).ToList());
                        }

                        //On change d'interface 
                        // Pour cacher le formulaire des séquences si elle est visible
                        // On affiche les elements de la préindex de l'image s'ils était cachés
                        Sequences.Clear();
                        dgSequence.Items.Clear();
                        btnSupprimerFeuillet.IsEnabled = true;
                        tbNumeroFeuillet.Visibility = Visibility.Visible;
                        titreFormFeuillet.Visibility = Visibility.Visible;
                        tbNumeroOrdreDebut.Visibility = Visibility.Visible;
                        tbNumeroOrdreFin.Visibility = Visibility.Visible;

                        // Refresh de la datagrid et des TextBox  
                        List<ImageView> imageViews = imageView.GetSimpleViewsList(registre);
                        dgFeuillet.ItemsSource = imageViews;
                        if (dgFeuillet.Items.Count > 0)
                        {
                            dgFeuillet.ScrollIntoView(imageViews.Last());
                        }

                        //tbNumeroFeuillet.Text = (imageViews.Count() + 1).ToString();                    
                        tbNumeroFeuillet.Text = (LastImage.NumeroPage + 1).ToString();
                        //var imageAvant = imageViews.FirstOrDefault(i => i.Image.NumeroPage == imageViews.Count());
                        tbNumeroOrdreDebut.Text = (LastImage.FinSequence + 1).ToString();
                        tbNumeroOrdreFin.Text = "";
                        tbNumeroOrdreSequence.Text = "";
                        tbNombreReferences.Text = "";

                        if (registre.NombrePage == imageView.GetSimpleViewsList(registre).Count)
                        {
                            btnSaveFeuillet.IsEnabled = false;
                            MarquerPreindexer.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            btnSaveFeuillet.IsEnabled = true;
                            MarquerPreindexer.Visibility = Visibility.Collapsed;
                        }

                        // Enregistrement de l'action effectué par l'agent
                        DocumatContext.AddTraitement(DocumatContext.TbImage, LastImage.ImageID, Utilisateur.AgentID, (int)Enumeration.TypeTraitement.CREATION, "PREINDEXATION AJOUT IMAGE N° " + LastImage.NumeroPage + " AU REGISTRE ID N° " + LastImage.RegistreID);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void btnSupprimerFeuillet_Click(object sender, RoutedEventArgs e)
        {
            List<ImageView> imageViews = (List<ImageView>)dgFeuillet.ItemsSource;            
            if (imageViews.Count > 0)
            {
                if(MessageBox.Show("Voulez vous supprimer la dernière image préindexée, et la reprendre à zéro ?","AVERTISSEMENT",MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    using (var ct = new DocumatContext())
                    {
                        ct.Image.Remove(ct.Image.FirstOrDefault(i => i.ImageID == LastImage.ImageID));
                        ct.SaveChanges();
                    }

                    ImageView imageViewGetter = new ImageView();
                    imageViews = imageViewGetter.GetSimpleViewsList(registre);
                    dgFeuillet.ItemsSource = imageViews;
                    if (dgFeuillet.Items.Count > 0)
                    {
                        dgFeuillet.ScrollIntoView(imageViews.Last());
                    }

                    if (imageViews.Count == 0)
                    {
                        tbNumeroFeuillet.Text = "1";
                        tbNumeroOrdreDebut.Text = "1";
                    }
                    else
                    {
                        LastImage = imageViews.OrderByDescending(i => i.Image.ImageID).FirstOrDefault().Image;
                        tbNumeroFeuillet.Text = (LastImage.NumeroPage + 1).ToString();
                        tbNumeroOrdreDebut.Text = (LastImage.FinSequence + 1).ToString();
                    }

                    if(registre.NombrePage != imageViews.Count())
                    {
                        MarquerPreindexer.Visibility = Visibility.Collapsed;
                        btnSaveFeuillet.IsEnabled = true;
                    }

                    // Enregistrement de l'action effectué par l'agent
                    DocumatContext.AddTraitement(DocumatContext.TbImage, LastImage.ImageID, Utilisateur.AgentID, (int)Enumeration.TypeTraitement.SUPPRESSION, "PREINDEXATION SUPPRESSION IMAGE N° " + LastImage.NumeroPage + " AU REGISTRE ID N° " + LastImage.RegistreID);
                }
            }            
        }

        private void BtnSupprimerSequence_Click(object sender, RoutedEventArgs e)
        {
            if(dgSequence.SelectedItems.Count > 0)
            {
                Models.Sequence sequence = (Sequence)dgSequence.SelectedItem;
                Sequences.Remove(sequence);
                dgSequence.ScrollIntoView(sequence);
                dgSequence.Items.Remove(sequence);
                tbNumeroOrdreSequence.Focus();
                //foreach (Models.Sequence sequence in dgSequence.SelectedItems)
                //{
                //}
                //dgSequence.ItemsSource = Sequences;
            }
        }
         
        private void tbNumeroPageSequence_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void BtnAddSequence_Click(object sender, RoutedEventArgs e)
        {            
            if(tbNumeroOrdreFin.Text != "" && tbNumeroOrdreSequence.Text != "" && tbNumeroOrdreSequence.Text != "")
            {
                int numeroOrdreSequence = 0, NombreReference = 0;
                int NombreReferences = 0;
                if (Int32.TryParse(tbNumeroOrdreSequence.Text, out numeroOrdreSequence)  && Int32.TryParse(tbNombreReferences.Text, out NombreReferences))
                {
                    int imageID = 0;                    
                    if (Int32.TryParse(tbNombreReferences.Text,out NombreReference) && NombreReference > 0)
                    {
                        if (Int32.Parse(tbNumeroOrdreSequence.Text) >= Int32.Parse(tbNumeroOrdreDebut.Text)
                                && Int32.Parse(tbNumeroOrdreSequence.Text) <= Int32.Parse(tbNumeroOrdreFin.Text))
                        {
                            Models.Sequence sequenceActu = Sequences.FirstOrDefault(s => s.NUmeroOdre == numeroOrdreSequence);
                            if (sequenceActu == null || cbxBisOrdre.IsChecked == true || cbxDoublonOrdre.IsChecked == true)
                            {
                                string isSpecial = "";
                                if (cbxBisOrdre.IsChecked == true)
                                {
                                    Models.Sequence bisseq = Sequences.Where(s => s.NUmeroOdre == numeroOrdreSequence && s.isSpeciale.ToLower().Contains("bis")).LastOrDefault();
                                    if(bisseq != null)
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
                                    Models.Sequence doubseq = Sequences.Where(s => s.NUmeroOdre == numeroOrdreSequence && s.isSpeciale.ToLower().Contains("doublon")).LastOrDefault();
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

                                Sequences.Add(new Sequence
                                {
                                    DateCreation = DateTime.Now,
                                    DateModif = DateTime.Now,
                                    ImageID = imageID,
                                    NombreDeReferences = NombreReference,
                                    NUmeroOdre = numeroOrdreSequence,
                                    DateSequence = DateTime.Now,
                                    References = "Defaut",
                                    isSpeciale = isSpecial
                                });
                                tbNumeroOrdreSequence.Text = "";
                                tbNombreReferences.Text = "";
                                dgSequence.Items.Add(Sequences.Last());
                                tbNumeroOrdreSequence.Focus();
                                cbxBisOrdre.IsChecked = false;
                                cbxDoublonOrdre.IsChecked = false;
                                dgSequence.ScrollIntoView(Sequences.Last());
                            }
                        }
                    }
                }
            }
        }

        private void dgSequence_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {

        }

        private void dgSequence_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void dgSequence_LoadingRow(object sender, DataGridRowEventArgs e)
        {

        }

        private void MarquerPreindexer_Click(object sender, RoutedEventArgs e)
        {
            if(MarquerPreindexer.IsChecked == true)
            {
                if (MessageBox.Show("Attention vous ne pourrez plus faire de modification sur ce registre, Voulez vous terminer la préindexation",
                                    "QUESTION", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    using (var ct = new DocumatContext())
                    {
                        Models.Registre reg = ct.Registre.FirstOrDefault(r => r.RegistreID == registre.RegistreID);
                        // Récupération du statut actuelle
                        Models.StatutRegistre AncienStatut = ct.StatutRegistre.FirstOrDefault(s => s.RegistreID == registre.RegistreID
                                                             && s.Code == registre.StatutActuel);
                        AncienStatut.DateFin = AncienStatut.DateModif = DateTime.Now;

                        // Création et attribution du nouveau statut
                        Models.StatutRegistre NewStatut = new StatutRegistre();
                        NewStatut.RegistreID = reg.RegistreID;
                        NewStatut.DateModif = NewStatut.DateDebut = NewStatut.DateCreation = DateTime.Now;
                        NewStatut.Code = (int)Enumeration.Registre.PREINDEXE;
                        ct.StatutRegistre.Add(NewStatut);
                        
                        // On change le statut Actuel duu registre
                        reg.StatutActuel = (int)Enumeration.Registre.PREINDEXE;
                        ct.SaveChanges();

                        //Enregistrement du traitement effectué
                        DocumatContext.AddTraitement(DocumatContext.TbRegistre, reg.RegistreID, Utilisateur.AgentID, (int)Enumeration.TypeTraitement.PREINDEXATION_REGISTRE_TERMINEE);
                    }

                    dgFeuillet.Visibility = Visibility.Collapsed;
                    // Rechargement du registre
                    //RegistreView registreView = new RegistreView();
                    //List<RegistreView> registreViews = registreView.GetViewsList();

                    //dgRegistre.ItemsSource = registreViews.Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.CREE);
                    TbRechercher_TextChanged(null,null);
                    //
                    PanelFeuillet.Visibility = Visibility.Collapsed;
                    dgRegistre.Visibility = Visibility.Visible;
                    QrCodeImage.Source = null;
                    AfficherFeuillets.IsChecked = false;
                    MarquerPreindexer.IsChecked = false;
                    MarquerPreindexer.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MarquerPreindexer.IsChecked = false;
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshRegistre();

            ContextMenu Cm = (ContextMenu)FindResource("cmFeuillet");
            dgRegistre.ContextMenu = Cm;
        }

        private void dgRegistre_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
        }

        private void dgRegistre_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            // Chargement de l'image du Qrcode de la ligne
            Zen.Barcode.CodeQrBarcodeDraw qrcode = Zen.Barcode.BarcodeDrawFactory.CodeQr;
            var element = e.DetailsElement.FindName("QrCode");
            RegistreView registre = (RegistreView)e.Row.Item;
            var image = qrcode.Draw(registre.Registre.QrCode, 40);
            var imageConvertie = image.ConvertDrawingImageToWPFImage(null, null);
            ((System.Windows.Controls.Image)element).Source = imageConvertie.Source;
        }

        private void dgRegistre_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //Définition de la colonne des numéros d'odre
            // En plus il faut que EnableRowVirtualization="False"
            RegistreView view = (RegistreView)e.Row.Item;
            view.NumeroOrdre = e.Row.GetIndex() + 1;
            e.Row.Item = view;
        }

        private void tbNombreReferences_TextChanged(object sender, TextChangedEventArgs e)
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

        private void tbNumeroOrdreSequence_TextChanged(object sender, TextChangedEventArgs e)
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

        private void EditFeuillets_Click(object sender, RoutedEventArgs e)
        {
            AfficherFeuillets.IsChecked = true;
            this.AfficherFeuillets_Click(sender, e);
        }

        private void TbRechercher_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void RefreshRegistre()
        {
            try
            {
                // Récupération des registre par code registre
                RegistreView RegistreView = new RegistreView();
                List<RegistreView> registreViews = RegistreView.GetViewsListByTraite((int)Enumeration.TypeTraitement.PREINDEXATION_REGISTRE, false).Where(r=>r.Registre.StatutActuel == (int)Enumeration.Registre.CREE).ToList();
                registreViews.AddRange(RegistreView.GetViewsListByTraite((int)Enumeration.TypeTraitement.PREINDEXATION_REGISTRE, true).Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.CREE).ToList());
                dgRegistre.ItemsSource = registreViews;
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void BtnActualise_Click(object sender, RoutedEventArgs e)
        {
            RefreshRegistre();
        }

        private void BtnEditSequence_Click(object sender, RoutedEventArgs e)
        {
        }

        private void cbxBisOrdre_Checked(object sender, RoutedEventArgs e)
        {
            cbxDoublonOrdre.IsChecked = false;
            tbNombreReferences.Focus();
        }

        private void cbxBisOrdre_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void cbxDoublonOrdre_Checked(object sender, RoutedEventArgs e)
        {
            cbxBisOrdre.IsChecked = false;
            tbNombreReferences.Focus();
        }

        private void cbxDoublonOrdre_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void tbNombreReferences_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                BtnAddSequence_Click(sender, e);
            }
        }

        private void tbNumeroOrdreSequence_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                tbNombreReferences.Focus();
            }
        }

        private void BtnRechercher_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TbRechercher.Text != "")
                {
                    RegistreView RegistreView = new RegistreView();
                    List<RegistreView> registreViews = RegistreView.GetViewsListByTraite((int)Enumeration.TypeTraitement.PREINDEXATION_REGISTRE, false).Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.CREE).ToList();
                    registreViews.AddRange(RegistreView.GetViewsListByTraite((int)Enumeration.TypeTraitement.PREINDEXATION_REGISTRE, true).Where(r => r.Registre.StatutActuel == (int)Enumeration.Registre.CREE).ToList());

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

                            var jointure1 = from r in registreViews
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

                            var jointure2 = from r in registreViews
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
    }
}
