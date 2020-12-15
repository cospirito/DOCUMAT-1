using DOCUMAT.Models;
using DOCUMAT.ViewModels;
using Microsoft.Win32;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace DOCUMAT.Pages.Versement
{
    /// <summary>
    /// Logique d'interaction pour FormVersement.xaml
    /// </summary>
    public partial class FormVersement : Window
    {
        bool EditMode = false;
        VersementView VersementViewParent;
        private Models.Agent Utilisateur;
        Inventaire.Inventaire WindowsParent;
        ServiceView ServiceViewParent;
        public string nomDossierRacine = ConfigurationManager.AppSettings["CheminDossier_Scan"];
        private string nomFichierExistant;
        public string nomFichier { get; private set; }
        public string DossierService { get; private set; }

        public FormVersement()
        {
            InitializeComponent();

            //Définition de la date du datetimepicker
            dtVers.Language = XmlLanguage.GetLanguage("fr-FR");
            dtLivraison.Language = XmlLanguage.GetLanguage("fr-FR");
        }

        public FormVersement(ServiceView serviceView,VersementView versement, Inventaire.Inventaire parent,Models.Agent user):this()
        {
            EditMode = true;
            WindowsParent = parent;
            VersementViewParent = versement;
            Utilisateur = user;
            cbLivraison.SelectedItem = versement.context.Livraison.FirstOrDefault(l => l.LivraisonID == versement.Versement.LivraisonID);
            cbLivraison.IsEnabled = false;
            btnAddLivraison.IsEnabled = false;
            ServiceViewParent = serviceView;

            // Remplissage des champs 
            tbNumeroVers.Text = versement.Versement.NumeroVers.ToString();
            dtVers.SelectedDate = versement.Versement.DateVers;
            tbService.Text = serviceView.Service.NomComplet;
            tbNomAgentVersant.Text = versement.Versement.NomAgentVersant;
            tbPrenomsAgentVersant.Text = versement.Versement.PrenomsAgentVersant;
            tbNombreR3.Text = versement.Versement.NombreRegistreR3.ToString();
            tbNombreR4.Text = versement.Versement.NombreRegistreR4.ToString();
            cbLivraison.ItemsSource = serviceView.context.Livraison.Where(l => l.ServiceID == serviceView.Service.ServiceID).ToList();
            cbLivraison.DisplayMemberPath = "Numero";
            cbLivraison.SelectedValuePath = "LivraisonID";
            cbLivraison.SelectedValue = versement.Versement.LivraisonID;      
            
            if(user.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR || user.Affectation == (int)Enumeration.AffectationAgent.SUPERVISEUR)
            {
                tbNumeroVers.IsEnabled = true;
            }
            else
            {
                tbNumeroVers.IsEnabled = false;
            }
        }

        public FormVersement(ServiceView serviceView,Inventaire.Inventaire parent, Models.Agent user) :this()
        {            
            EditMode = false;
            WindowsParent = parent;
            Utilisateur = user;
            
            // Chargement des composantes déja connu
            ServiceViewParent = serviceView;
            dtVers.SelectedDate = DateTime.Now;
            tbService.Text = serviceView.Service.NomComplet;
            dtLivraison.SelectedDate = DateTime.Now;
            cbLivraison.ItemsSource = serviceView.context.Livraison.Where(l=> l.ServiceID == serviceView.Service.ServiceID).ToList();
            cbLivraison.DisplayMemberPath = "Numero";
            cbLivraison.SelectedValuePath = "LivraisonID";
            cbLivraison.SelectedIndex = 0;
        }
        private void cbLivraison_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnSaveLivraison_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (tbNumeroLivraison.Text != "" && dtLivraison.SelectedDate != null)
                {
                    Livraison livraison = new Livraison();
                    int numeroLivraison = 0;
                    if (Int32.TryParse(tbNumeroLivraison.Text, out numeroLivraison))
                    {
                        livraison.Numero = numeroLivraison;
                        livraison.DateCreation = livraison.DateModif = DateTime.Now;
                        livraison.DateLivraison = dtLivraison.SelectedDate.Value;
                        livraison.ServiceID = ServiceViewParent.Service.ServiceID;
                        VersementView vm = new VersementView();
                        int idLivraison =  vm.AddLivraison(livraison);
                        if(idLivraison == 0)
                        {
                            if(MessageBox.Show("Cet élément existe déja, Voulez vous mettre à jour la date de livraison", "QUESTION", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                vm.UpLivraison(livraison);
                                DocumatContext.AddTraitement(DocumatContext.TbLivraison, vm.Versement.Livraison.LivraisonID,WindowsParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION);
                                cbLivraison.ItemsSource = vm.context.Livraison.Where(l => l.ServiceID == ServiceViewParent.Service.ServiceID).ToList();
                                cbLivraison.SelectedIndex = cbLivraison.Items.Count - 1;
                                MessageBox.Show("Livraison Modifié", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            formLivraison.Visibility = Visibility.Collapsed;
                            formVersement.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            // Enregistrement du traitement 
                            DocumatContext.AddTraitement(DocumatContext.TbLivraison, idLivraison, WindowsParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.CREATION);
                            cbLivraison.ItemsSource = vm.context.Livraison.Where(l => l.ServiceID == ServiceViewParent.Service.ServiceID).ToList();
                            cbLivraison.SelectedIndex = cbLivraison.Items.Count - 1;
                            formLivraison.Visibility = Visibility.Collapsed;
                            formVersement.Visibility = Visibility.Visible;
                            MessageBox.Show("Livraison Ajouté", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void ModifVersement()
        {
            try
            {
                VersementView versementView = new VersementView();
                versementView.GetView(VersementViewParent.Versement.VersementID);

                if (tbNomAgentVersant.Text.Trim() == "")
                    throw new Exception("Le champ Nom de l'agent est vide !!!");

                if (tbPrenomsAgentVersant.Text.Trim() == "")
                    throw new Exception("Le champ Prénom de l'agent ne doit pas être vide !!!");

                Models.Versement versement = new Models.Versement();
                int numVers = 0;
                if (!Int32.TryParse(tbNumeroVers.Text, out numVers))
                    throw new Exception("Veuiller entrer un numero de versement valide !!!");

                int nombreR3 = 0;
                if (!Int32.TryParse(tbNombreR3.Text, out nombreR3))
                    throw new Exception("Veuillez entrer un nombre correcte pour le champ nombre de registre R3 versé !!!");

                int nombreR4 = 0;
                if (!Int32.TryParse(tbNombreR4.Text, out nombreR4))
                    throw new Exception("Veuillez entrer un nombre correcte pour le champ nombre de registre R4 versé !!!");

                //Vérification que des changements ont été effectués
                if (versementView.Versement.NumeroVers != numVers || (dtVers.SelectedDate.HasValue && versement.DateVers != dtVers.SelectedDate.Value)
                    || versementView.Versement.NomAgentVersant != tbNomAgentVersant.Text || versementView.Versement.PrenomsAgentVersant != tbPrenomsAgentVersant.Text)
                {
                    versement.NumeroVers = numVers;                
                    versement.DateVers = dtVers.SelectedDate.Value;
                    versement.NomAgentVersant = tbNomAgentVersant.Text;
                    versement.PrenomsAgentVersant = tbPrenomsAgentVersant.Text;
                    versement.NombreRegistreR3 = nombreR3;
                    versement.NombreRegistreR4 = nombreR4;

                    versementView.Update(versement);
                    // enregistrement du traitement effectué 
                    DocumatContext.AddTraitement(DocumatContext.TbVersement, versementView.Versement.VersementID, WindowsParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION);
                    MessageBox.Show("Versement Modifié !!!", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);
                    WindowsParent.RefreshVersement();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void AjouterVersement()
        {
            if (cbLivraison.SelectedItem != null && tbNumeroVers.Text != "" && dtVers.SelectedDate != null && tbNombreR3.Text != "" && tbNombreR4.Text != "")
            {
                if (tbNomAgentVersant.Text.Trim() == "")
                    throw new Exception("Le champ Nom de l'agent est vide !!!");

                if (tbPrenomsAgentVersant.Text.Trim() == "")
                    throw new Exception("Le champ Prénom de l'agent ne doit pas être vide !!!");

                int nombreR3 = 0;
                if (!Int32.TryParse(tbNombreR3.Text, out nombreR3))
                    throw new Exception("Veuillez entrer un nombre correcte pour le champ nombre de registre R3 versé !!!");

                int nombreR4 = 0;
                if (!Int32.TryParse(tbNombreR3.Text, out nombreR4))
                    throw new Exception("Veuillez entrer un nombre correcte pour le champ nombre de registre R4 versé !!!");

                bool aUnBordereau = true;

                try
                {
                    MessageBox.Show("Veuillez sélectionner le bordereau de Versement !!!", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);
                    OpenFileDialog open = new OpenFileDialog();
                    //open.Filter = "JPEG|*.jpg|PNG|*.png|TIF|*.tif";
                    open.Filter = "Fichier PDF |*.pdf";
                    open.Multiselect = false;

                    if (open.ShowDialog() == true)
                    {
                        nomFichier = open.FileNames[0];
                    }
                    else
                    {
                        aUnBordereau = false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "ERREUR", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                try
                {
                    int numeroVers = 0;
                    if (Int32.TryParse(tbNumeroVers.Text, out numeroVers) && aUnBordereau)
                    {
                        // Création des dossiers et transfert de l'image
                        // Création du Dossier de Service
                        VersementView versementView = new VersementView();

                        // Vérification de l'unicité du numero de versement 
                        if (versementView.context.Versement.Any(v => v.NumeroVers == numeroVers && v.LivraisonID == (int)cbLivraison.SelectedValue))
                        {
                            throw new Exception("Ce numero de versement est déja utilisé !!!");
                        }

                        DossierService = nomDossierRacine + @"/" + ServiceViewParent.Service.CheminDossier.Trim();
                        if (!Directory.Exists(DossierService))
                        {
                            Directory.CreateDirectory(DossierService);
                            // Création du dossier R3
                            Directory.CreateDirectory(DossierService + @"/R3");
                            //creat R4
                            Directory.CreateDirectory(DossierService + @"/R4");
                        }
                        else
                        {
                            if (!Directory.Exists(DossierService + @"/R3"))
                                Directory.CreateDirectory(DossierService + @"/R3");

                            if (!Directory.Exists(DossierService + @"/R4"))
                                Directory.CreateDirectory(DossierService + @"/R4");
                        }

                        // Copy du bordereau dans le dossier du service
                        string newFile = "BORDEREAU_" + ServiceViewParent.Service.Nom + "_L" + cbLivraison.Text + "_V" + numeroVers + ".pdf";
                        string newFilePath = DossierService + @"\" + newFile;
                        if(File.Exists(newFilePath))
                        {
                            if(Utilisateur.Affectation == (int)Enumeration.AffectationAgent.ADMINISTRATEUR)
                            {
                                MessageBoxResult question = MessageBox.Show("Voulez vous remplacer le fichier existant ou l'utiliser \n OUI-(REMPLACER) OU NON-(UTILISER) ?", "QUESTION", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                                if (question == MessageBoxResult.Yes)
                                {
                                    File.Copy(nomFichier, newFilePath,true);
                                }
                                else if(question == MessageBoxResult.No)
                                {
                                    nomFichierExistant = newFilePath;
                                }
                                else
                                {
                                    return;
                                }
                            }
                            else
                            {
                                MessageBox.Show("Une image du même nom existe, veuillez contacter un Administrateur, afin de gérer ce problème !!!"
                                               , "FICHIER EXISTANT", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                        }
                        else
                        {
                            File.Copy(nomFichier, newFilePath);
                            nomFichierExistant = newFilePath;
                        }

                        versementView.Versement.cheminBordereau = ServiceViewParent.Service.CheminDossier + @"\" + newFile;
                        versementView.Versement.DateCreation = versementView.Versement.DateModif = DateTime.Now;
                        versementView.Versement.DateVers = dtVers.SelectedDate.Value;
                        versementView.Versement.NomAgentVersant = tbNomAgentVersant.Text;
                        versementView.Versement.PrenomsAgentVersant = tbPrenomsAgentVersant.Text;
                        versementView.Versement.LivraisonID = (int)cbLivraison.SelectedValue;
                        versementView.Versement.NumeroVers = numeroVers;
                        versementView.Versement.NombreRegistreR3 = nombreR3;
                        versementView.Versement.NombreRegistreR4 = nombreR4;
                        versementView.Add();

                        // Enregistrement du traitement de l'agent 
                        DocumatContext.AddTraitement(DocumatContext.TbVersement, versementView.Versement.VersementID,WindowsParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.CREATION);

                        MessageBox.Show("Versement Ajouté !!!", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);
                        WindowsParent.IsEnabled = true;
                        WindowsParent.RefreshVersement();
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    ex.ExceptionCatcher();
                }
            }
            else
            {
                MessageBox.Show("Veuillez Remplir tous les champs !!!", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnAnnuleLivraison_Click(object sender, RoutedEventArgs e)
        {
            formLivraison.Visibility = Visibility.Collapsed;
            formVersement.Visibility = Visibility.Visible;
        }

        private void btnAddLivraison_Click(object sender, RoutedEventArgs e)
        {
            formLivraison.Visibility = Visibility.Visible;
            formVersement.Visibility = Visibility.Collapsed;
        }

        private void btnAnnuleVersement_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSaveVersement_Click(object sender, RoutedEventArgs e)
        {
            if (!EditMode)
            {
                AjouterVersement();
            }
            else
            {
                ModifVersement();
            }            
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            WindowsParent.IsEnabled = true;
        }

        private void tbNumeroVers_TextChanged(object sender, TextChangedEventArgs e)
        {           
            Regex regex = new Regex("^[0-9]{1,}$");
            TextBox textBox = ((TextBox)sender);
            string text = textBox.Text;
            if(text.Length > 0)
            {
                if (!regex.IsMatch(text.Substring(text.Length - 1)))
                {
                    textBox.Text = textBox.Text.Remove(text.Length - 1);
                    textBox.CaretIndex = text.Length;
                }
            }
        }

        private void tbNumeroLivraison_TextChanged(object sender, TextChangedEventArgs e)
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
    }
}