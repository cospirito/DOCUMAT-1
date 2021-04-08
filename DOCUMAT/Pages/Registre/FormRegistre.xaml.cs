using DOCUMAT.Models;
using DOCUMAT.ViewModels;
using System;
using System.Configuration;
using System.IO;
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
        string DossierRacine = ConfigurationManager.AppSettings["CheminDossier_Scan"];
        Models.Versement VersementParent;
        Models.Registre RegistreParent;
        Inventaire.Inventaire WindowsParent;
        Models.Agent Utilisateur;
        bool EditMode = false;

        public FormRegistre()
        {
            InitializeComponent();
            dtDepotFin.Language = XmlLanguage.GetLanguage("fr-FR");
            dtDepotDebut.Language = XmlLanguage.GetLanguage("fr-FR");
        }

        public FormRegistre(Models.Versement versement, Models.Registre registre, Inventaire.Inventaire parent) : this()
        {
            try
            {
                using (var ct = new DocumatContext())
                {
                    EditMode = true;
                    VersementParent = versement;
                    RegistreParent = registre;
                    WindowsParent = parent;
                    Utilisateur = parent.Utilisateur;

                    // Remplissage des éléments du formulaire
                    tbNombrePageDeclarer.Text = registre.NombrePageDeclaree.ToString();
                    tbNombrePage.Text = registre.NombrePage.ToString();
                    tbNumeroDepotDebut.Text = registre.NumeroDepotDebut.ToString();
                    tbNumeroDepotFin.Text = registre.NumeroDepotFin.ToString();
                    tbNumeroVolume.Text = registre.Numero.ToString();
                    tbObservation.Text = registre.Observation;
                    Livraison livraison = ct.Livraison.FirstOrDefault(l => l.LivraisonID == versement.LivraisonID);
                    tbService.Text = ct.Service.FirstOrDefault(s => s.ServiceID == livraison.ServiceID).NomComplet;
                    tbVersement.Text = versement.NumeroVers.ToString();
                    dtDepotDebut.SelectedDate = registre.DateDepotDebut;
                    dtDepotFin.SelectedDate = registre.DateDepotFin;

                    if (registre.Type == "R3")
                    {
                        cbTypeRegistre.SelectedIndex = 0;
                    }
                    else
                    {
                        cbTypeRegistre.SelectedIndex = 1;
                    }

                    cbTypeRegistre.IsEnabled = false;
                    tbNumeroVolume.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        public FormRegistre(Models.Versement versement, Inventaire.Inventaire parent) : this()
        {
            try
            {
                using (var ct = new DocumatContext())
                {
                    EditMode = false;
                    VersementParent = versement;
                    WindowsParent = parent;
                    Utilisateur = parent.Utilisateur;
                    Livraison livraison = ct.Livraison.FirstOrDefault(l => l.LivraisonID == versement.LivraisonID);
                    tbService.Text = ct.Service.FirstOrDefault(s => s.ServiceID == livraison.ServiceID).NomComplet;
                    tbVersement.Text = versement.NumeroVers.ToString();
                    dtDepotDebut.SelectedDate = dtDepotFin.SelectedDate = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        /// <summary>
        ///   Modification directe d'un registre 
        /// </summary>
        /// <param name="registreView"></param>
        /// <param name="user"></param>
        public FormRegistre(Models.Registre registre, Models.Agent user) : this()
        {
            EditMode = true;
            VersementParent = new Models.Versement();
            VersementParent = registre.Versement;
            RegistreParent = registre;
            Utilisateur = user;

            // Remplissage des éléments du formulaire
            tbNombrePageDeclarer.Text = registre.NombrePageDeclaree.ToString();
            tbNombrePage.Text = registre.NombrePage.ToString();
            tbNumeroDepotDebut.Text = registre.NumeroDepotDebut.ToString();
            tbNumeroDepotFin.Text = registre.NumeroDepotFin.ToString();
            tbNumeroVolume.Text = registre.Numero.ToString();
            tbObservation.Text = registre.Observation;

            using (var ct = new DocumatContext())
            {
                Models.Versement versement = ct.Versement.FirstOrDefault(v => v.VersementID == RegistreParent.VersementID);
                Livraison livraison = ct.Livraison.FirstOrDefault(l => l.LivraisonID == versement.LivraisonID);
                tbService.Text = ct.Service.FirstOrDefault(s => s.ServiceID == livraison.ServiceID).Nom;
                tbVersement.Text = versement.NumeroVers.ToString();
            }
            dtDepotDebut.SelectedDate = registre.DateDepotDebut;
            dtDepotFin.SelectedDate = registre.DateDepotFin;

            if (RegistreParent.Type == "R3")
            {
                cbTypeRegistre.SelectedIndex = 0;
            }
            else
            {
                cbTypeRegistre.SelectedIndex = 1;
            }

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
            if (WindowsParent != null)
            {
                WindowsParent.IsEnabled = true;
            }
            this.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!EditMode)
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
                registreView.GetView(RegistreParent.RegistreID);
                registre.DateModif = DateTime.Now;
                registre.DateDepotDebut = dtDepotDebut.SelectedDate.Value;
                registre.DateDepotFin = dtDepotFin.SelectedDate.Value;
                registre.Observation = tbObservation.Text;

                int NombrePageD = 0;
                if (Int32.TryParse(tbNombrePageDeclarer.Text, out NombrePageD))
                {
                    registre.NombrePageDeclaree = NombrePageD;
                }

                int NombrePage = 0;
                if (Int32.TryParse(tbNombrePage.Text, out NombrePage))
                {
                    registre.NombrePage = NombrePage;
                }
                else
                {
                    throw new Exception("Nombre de Page Incorrecte !");
                }

                if (tbNumeroVolume.Text != "")
                {
                    registre.Numero = tbNumeroVolume.Text;
                }
                else
                {
                    throw new Exception("Numero de Volume Incorrecte !");
                }

                int NumeroDepotDebut = 0;
                if (Int32.TryParse(tbNumeroDepotDebut.Text, out NumeroDepotDebut))
                {
                    registre.NumeroDepotDebut = NumeroDepotDebut;
                }
                else
                {
                    throw new Exception("Numero dépôt début Incorrecte !");
                }

                int NumeroDepotFin = 0;
                if (Int32.TryParse(tbNumeroDepotFin.Text, out NumeroDepotFin))
                {
                    registre.NumeroDepotFin = NumeroDepotFin;
                }
                else
                {
                    throw new Exception("Numero dépôt fin Incorrecte !");
                }

                registreView.Update(registre);

                // Enregistrement du traitement de l'agent 
                DocumatContext.AddTraitement(DocumatContext.TbRegistre, registreView.Registre.RegistreID, Utilisateur.AgentID, (int)Enumeration.TypeTraitement.MODIFICATION);

                // Suppression du Bordereau du Registre 
                string BordereauRegistre = System.IO.Path.Combine(DossierRacine, registreView.Registre.CheminDossier, $"Bordereau{registreView.Registre.QrCode}.xps");

                try
                {
                    if (File.Exists(BordereauRegistre))
                    {
                        File.Delete(BordereauRegistre);
                    }
                }
                catch (Exception ein) { };

                MessageBox.Show("Registre Modifié !!!", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);

                if (WindowsParent != null)
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
                if (MessageBox.Show("Les champs \"Type de registre\" et \"Numéro Volume\" ne pourront plus être modifié utlérieurement, voulez-vous enregistrer ce registre ?", "ATTENTION", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    //Création du Dossier de registre dans le dossier de service                       
                    RegistreView registreView = new RegistreView();

                    // Vérification du nombre de Registre de Type 'R3 ou R4' enregistrée
                    if (((ComboBoxItem)cbTypeRegistre.SelectedItem).Content.ToString() == "R3")
                    {
                        if (VersementParent.NombreRegistreR3 < (registreView.context.Registre.Where(r => r.VersementID == VersementParent.VersementID && r.Type == "R3").Count() + 1))
                        {
                            throw new Exception("Le nombre de Registre R3 doit être doit être de " + VersementParent.NombreRegistreR3);
                        }
                    }
                    else
                    {
                        if (VersementParent.NombreRegistreR4 < (registreView.context.Registre.Where(r => r.VersementID == VersementParent.VersementID && r.Type == "R4").Count() + 1))
                        {
                            throw new Exception("Le nombre de Registre R4 doit être doit être de " + VersementParent.NombreRegistreR4);
                        }
                    }

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
                    registreView.Registre.VersementID = VersementParent.VersementID;
                    int registreID = registreView.AddRegistre();

                    // Enregistrement du traitement de l'agent 
                    DocumatContext.AddTraitement(DocumatContext.TbRegistre, registreID, Utilisateur.AgentID, (int)Enumeration.TypeTraitement.CREATION);

                    if (WindowsParent != null)
                    {
                        WindowsParent.RefreshRegistre();
                    }
                    this.Close();
                    MessageBox.Show("Registre Ajouté !!!", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);

                    //Impression du Code Barre
                    // Affichage de l'impression du code barre
                    Impression.BordereauRegistre bordereauRegistre = new Impression.BordereauRegistre(registreView.Registre);
                    bordereauRegistre.Show();
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
            if (WindowsParent != null)
            {
                WindowsParent.IsEnabled = true;
            }
        }
    }
}
