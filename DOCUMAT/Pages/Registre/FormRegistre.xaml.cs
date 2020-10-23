using DOCUMAT.Models;
using DOCUMAT.ViewModels;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace DOCUMAT.Pages.Registre
{
    /// <summary>
    /// Logique d'interaction pour FormRegistre.xaml
    /// </summary>
    public partial class FormRegistre : Window
    {
        VersementView VersementViewParent;
        RegistreView RegistreViewParent;
        Inventaire.Inventaire WindowsParent;
        Models.Agent Utilisateur;
        bool EditMode = false;

        public FormRegistre()
        {
            InitializeComponent();
            dtDepotFin.Language = XmlLanguage.GetLanguage("fr-FR");
            dtDepotDebut.Language = XmlLanguage.GetLanguage("fr-FR");
        }

        public FormRegistre(VersementView versementView,RegistreView registreView, Inventaire.Inventaire parent) : this()
        {
            EditMode = true;
            VersementViewParent = versementView;
            RegistreViewParent = registreView;
            WindowsParent = parent;
            Utilisateur = parent.Utilisateur;

            // Remplissage des éléments du formulaire
            tbNombrePageDeclarer.Text = registreView.Registre.NombrePageDeclaree.ToString();
            tbNombrePage.Text = registreView.Registre.NombrePage.ToString();
            tbNumeroDepotDebut.Text = registreView.Registre.NumeroDepotDebut.ToString();
            tbNumeroDepotFin.Text = registreView.Registre.NumeroDepotFin.ToString();
            tbNumeroVolume.Text = registreView.Registre.Numero.ToString();
            tbObservation.Text = registreView.Registre.Observation;
            Livraison livraison = versementView.context.Livraison.FirstOrDefault(l => l.LivraisonID == versementView.Versement.LivraisonID);
            tbService.Text = versementView.context.Service.FirstOrDefault(s => s.ServiceID == livraison.ServiceID).Nom;
            tbVersement.Text = versementView.Versement.NumeroVers.ToString();
            dtDepotDebut.SelectedDate = RegistreViewParent.Registre.DateDepotDebut;
            dtDepotFin.SelectedDate = RegistreViewParent.Registre.DateDepotFin;

            if (RegistreViewParent.Registre.Type == "R3")
                cbTypeRegistre.SelectedIndex = 0;
            else
                cbTypeRegistre.SelectedIndex = 1;

            cbTypeRegistre.IsEnabled = false;                
            tbNumeroVolume.IsEnabled = false;                
        }

        public FormRegistre(VersementView versementView,Inventaire.Inventaire parent):this()
        {            
            EditMode = false;
            VersementViewParent = versementView;
            WindowsParent = parent;
            Utilisateur = parent.Utilisateur;
            Livraison livraison = versementView.context.Livraison.FirstOrDefault(l=>l.LivraisonID == versementView.Versement.LivraisonID);
            tbService.Text = versementView.context.Service.FirstOrDefault(s => s.ServiceID == livraison.ServiceID).Nom;
            tbVersement.Text = versementView.Versement.NumeroVers.ToString();
            dtDepotDebut.SelectedDate = dtDepotFin.SelectedDate = DateTime.Now;           
        }

        /// <summary>
        ///   Modification directe d'un registre 
        /// </summary>
        /// <param name="registreView"></param>
        /// <param name="user"></param>
        public FormRegistre(RegistreView registreView, Models.Agent user):this()
        {
            EditMode = true;
            VersementViewParent = new VersementView();
            VersementViewParent.Versement = registreView.Versement;
            RegistreViewParent = registreView;
            Utilisateur = user;

            // Remplissage des éléments du formulaire
            tbNombrePageDeclarer.Text = registreView.Registre.NombrePageDeclaree.ToString();
            tbNombrePage.Text = registreView.Registre.NombrePage.ToString();
            tbNumeroDepotDebut.Text = registreView.Registre.NumeroDepotDebut.ToString();
            tbNumeroDepotFin.Text = registreView.Registre.NumeroDepotFin.ToString();
            tbNumeroVolume.Text = registreView.Registre.Numero.ToString();
            tbObservation.Text = registreView.Registre.Observation;
            using(var ct = new DocumatContext())
            {
                Livraison livraison = ct.Livraison.FirstOrDefault(l => l.LivraisonID == registreView.Versement.LivraisonID);
                tbService.Text = ct.Service.FirstOrDefault(s => s.ServiceID == livraison.LivraisonID).Nom;
                tbVersement.Text = registreView.Versement.NumeroVers.ToString();
            }
            dtDepotDebut.SelectedDate = RegistreViewParent.Registre.DateDepotDebut;
            dtDepotFin.SelectedDate = RegistreViewParent.Registre.DateDepotFin;

            if (RegistreViewParent.Registre.Type == "R3")
                cbTypeRegistre.SelectedIndex = 0;
            else
                cbTypeRegistre.SelectedIndex = 1;

            cbTypeRegistre.IsEnabled = false;
            tbNumeroVolume.IsEnabled = false;
        }

        private void cbService_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void cbVersement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnAnnule_Click(object sender, RoutedEventArgs e)
        {
            if(WindowsParent != null)
            {
                WindowsParent.IsEnabled = true;
            }
            this.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if(!EditMode)
            {
                AjouterRegistre();
            }
            else
            {
                ModifierRegistre();
            }
        }

        private void ModifierRegistre()
        {
            try
            {                
                //Création du Dossier de registre dans le dossier de service                                       
                Models.Registre registre = new Models.Registre();
                RegistreView registreView = new RegistreView();
                registreView.GetView(RegistreViewParent.Registre.RegistreID);
                registre.DateModif = DateTime.Now;
                registre.DateDepotDebut = dtDepotDebut.SelectedDate.Value;
                registre.DateDepotFin = dtDepotFin.SelectedDate.Value;
                registre.Observation = tbObservation.Text;

                int NombrePageD = 0;
                if (Int32.TryParse(tbNombrePageDeclarer.Text, out NombrePageD))
                    registre.NombrePageDeclaree = NombrePageD;

                int NombrePage = 0;
                if(Int32.TryParse(tbNombrePage.Text, out NombrePage))
                    registre.NombrePage = NombrePage;
                else
                    throw new Exception("Nombre de Page Incorrecte !");

                if (tbNumeroVolume.Text != "")
                    registre.Numero = tbNumeroVolume.Text;
                else
                    throw new Exception("Numero de Volume Incorrecte !");                  

                int NumeroDepotDebut = 0;
                if (Int32.TryParse(tbNumeroDepotDebut.Text, out NumeroDepotDebut))
                    registre.NumeroDepotDebut = NumeroDepotDebut;
                else
                    throw new Exception("Numero dépôt début Incorrecte !");

                int NumeroDepotFin = 0;
                if (Int32.TryParse(tbNumeroDepotFin.Text, out NumeroDepotFin))
                    registre.NumeroDepotFin = NumeroDepotFin;
                else
                    throw new Exception("Numero dépôt fin Incorrecte !");
               
                registreView.Update(registre);

                // Enregistrement du traitement de l'agent 
                DocumatContext.AddTraitement(DocumatContext.TbRegistre, registreView.Registre.RegistreID, Utilisateur.AgentID,(int)Enumeration.TypeTraitement.MODIFICATION);

                MessageBox.Show("Registre Modifié !!!", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);
                
                if(WindowsParent != null)
                {
                    WindowsParent.RefreshRegistre();
                }
                this.Close();
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void AjouterRegistre()
        {
            try
            {
                if(MessageBox.Show("Le champs Type de registre ne pourra plus être modifié utlérieurement, voulez-vous enregistrer ce registre ?","ATTENTION",MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    //Création du Dossier de registre dans le dossier de service                       
                    RegistreView registreView = new RegistreView();
                    /*registreView.Registre.CheminDossier = DossierRegistre;*/ //devra être changer après l'ajout
                    registreView.Registre.DateCreation = DateTime.Now;
                    registreView.Registre.DateModif = DateTime.Now;
                    registreView.Registre.DateDepotDebut = dtDepotDebut.SelectedDate.Value;
                    registreView.Registre.DateDepotFin = dtDepotFin.SelectedDate.Value;

                    int NombrePageD = 0;
                    if (Int32.TryParse(tbNombrePageDeclarer.Text, out NombrePageD))
                        registreView.Registre.NombrePageDeclaree = NombrePageD;

                    int NombrePage = 0;
                    if (Int32.TryParse(tbNombrePage.Text, out NombrePage))
                        registreView.Registre.NombrePage = NombrePage;
                    else
                        throw new Exception("Nombre de Page Incorrecte !");

                    if (tbNumeroVolume.Text != "")
                        registreView.Registre.Numero = tbNumeroVolume.Text;
                    else
                        throw new Exception("Numero de Volume Incorrecte !");

                    int NumeroDepotDebut = 0;
                    if (Int32.TryParse(tbNumeroDepotDebut.Text, out NumeroDepotDebut))
                        registreView.Registre.NumeroDepotDebut = NumeroDepotDebut;
                    else
                        throw new Exception("Numero dépôt début Incorrecte !");
                    int NumeroDepotFin = 0;
                    if (Int32.TryParse(tbNumeroDepotFin.Text, out NumeroDepotFin))
                        registreView.Registre.NumeroDepotFin = NumeroDepotFin;
                    else
                        throw new Exception("Numero dépôt fin Incorrecte !");

                    registreView.Registre.Observation = tbObservation.Text;
                    /*registreView.Registre.QrCode = "Defaut";*/ // Sera changer après l'ajout du registre
                    registreView.Registre.StatutActuel = 0; // Sera changer après l'ajout du registre
                    registreView.Registre.Type = ((ComboBoxItem)cbTypeRegistre.SelectedItem).Content.ToString();
                    registreView.Registre.VersementID = VersementViewParent.Versement.VersementID;
                    int registreID = registreView.AddRegistre();

                    // Enregistrement du traitement de l'agent 
                    DocumatContext.AddTraitement(DocumatContext.TbRegistre,registreID, Utilisateur.AgentID, (int)Enumeration.TypeTraitement.CREATION);

                    MessageBox.Show("Registre Ajouté !!!", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);
                    if(WindowsParent != null)
                    {
                        WindowsParent.RefreshRegistre();
                    }
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void tbNumeroVolume_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void tbNombrePageDeclarer_TextChanged(object sender, TextChangedEventArgs e)
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

        private void tbNombrePage_TextChanged(object sender, TextChangedEventArgs e)
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

        private void tbNumeroDepotDebut_TextChanged(object sender, TextChangedEventArgs e)
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

        private void tbNumeroDepotFin_TextChanged(object sender, TextChangedEventArgs e)
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

        private void tbObservation_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(WindowsParent != null)
                WindowsParent.IsEnabled = true;
        }
    }
}
