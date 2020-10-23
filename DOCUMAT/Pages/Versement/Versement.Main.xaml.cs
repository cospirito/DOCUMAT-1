using DOCUMAT.Models;
using DOCUMAT.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DOCUMAT.Pages.Versement
{
    /// <summary>
    /// Logique d'interaction pour Versement.xaml
    /// </summary>
    public partial class Versement : Page
    {

        #region Mes Fonctions

        // actualiser la Datagrid
        private void refreshListVersement()
        {
            try
            {
                //Remplissage partial de  la list
                VersementView versementView = new VersementView();
                dgVersement.ItemsSource = versementView.GetViewsList();
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void rechercheVersement()
        {
            try
            {
                if (TbRechercher.Text != "")
                {
                    VersementView  VersementView = new VersementView();
                    dgVersement.ItemsSource = VersementView.GetViewsList().Where(v => v.Versement.NumeroVers.ToString().Contains(TbRechercher.Text));
                }
                else
                {
                    refreshListVersement();
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }
        #endregion

        public bool EditMode = false;
        private string newFilePath = "";
		private readonly string nomDossierRacine = @"C:\DOCUMAT";

        public string nomFichier { get; private set; }

        public Versement()
        {
            InitializeComponent();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            refreshListVersement();
        }

        private void BtnRechercher_Click(object sender, RoutedEventArgs e)
        {
            rechercheVersement();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            EditMode = false;
            tbNumeroVers.Text = "";
            tbBorderau.Text = "";
            dtVers.SelectedDate = DateTime.Now;
            
            txbFormHeader.Text = "FORMULAIRE D'AJOUT";
            panelInteracBtn.Visibility = Visibility.Collapsed;
            gridViewPanelAgent.Visibility = Visibility.Collapsed;
            formAgent.Visibility = Visibility.Visible;
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgVersement.SelectedItems.Count == 1)
            {
                EditMode = true;
                txbFormHeader.Text = "FORMULAIRE DE MODIFICATION";



            }
            else
            {
                MessageBox.Show("Veillez sélectionner un élément à modifier", "ATTENTION", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSupprimer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgVersement.SelectedItems.Count > 0)
                {
                    if (MessageBox.Show("Voulez-vous vraiment supprimer cet élément ?", "ATTENTION", MessageBoxButton.YesNo, MessageBoxImage.Question)
                        == MessageBoxResult.Yes)
                    {
                        VersementView versementView = new VersementView();

                        foreach (VersementView item in dgVersement.SelectedItems)
                        {
                            versementView.context.Versement.Remove(versementView.context.Versement.FirstOrDefault( v=> v.VersementID == item.Versement.VersementID));
                        }
                        versementView.context.SaveChanges();
                        refreshListVersement();
                        MessageBox.Show("Suppression éffectuée", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Veuillez sélecctionner au moins un élément ", "ATTENTION", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }

        }

        private void btnAnnule_Click(object sender, RoutedEventArgs e)
        {
            panelInteracBtn.Visibility = Visibility.Visible;
            gridViewPanelAgent.Visibility = Visibility.Visible;
            formAgent.Visibility = Visibility.Collapsed;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!EditMode)
            {
                #region Ajouter
                //try
                //{
                //    if (cbService.SelectedItem != null)
                //    {
                //        if (cbAgent.SelectedItem != null)
                //        {
                //            VersementView VersementView = new VersementView();

                //            VersementView.Versement.cheminBordereau = newFilePath;
                //            VersementView.Versement.DateVers = dtVers.DisplayDate;
                //            int numvers = 0;
                //            if (Int32.TryParse(tbNumeroVers.Text, out numvers))
                //                VersementView.Versement.NumeroVers = numvers;
                //            else
                //                throw new Exception("Veuillez entrer un numéro de versement valide");
                //            VersementView.Versement.AgentID = ((Models.Agent)cbAgent.SelectedItem).AgentID;
                //            VersementView.Versement.DateCreation = DateTime.Now;
                //            VersementView.Versement.DateModif = DateTime.Now;
                //            VersementView.Add();

                //            panelInteracBtn.Visibility = Visibility.Visible;
                //            gridViewPanelAgent.Visibility = Visibility.Visible;
                //            formAgent.Visibility = Visibility.Collapsed;
                //            refreshListVersement();
                //            MessageBox.Show("L'élément a bien été ajouté !!!", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);
                //        }else
                //        {
                //            MessageBox.Show("Veuillez Ajouter l'agent chargé du versement", "ATTENTION", MessageBoxButton.OK, MessageBoxImage.Error);
                //        }
                //    }
                //    else
                //    {
                //        MessageBox.Show("Veuillez Ajouter au moins un service à rattacher", "ATTENTION", MessageBoxButton.OK, MessageBoxImage.Error);
                //    }
                //}
                //catch (Exception ex)
                //{
                //    ex.ExceptionCatcher();
                //}
                #endregion
            }
            else
            {
                #region Modifier
                //try
                //{
                    //if (cbService.SelectedItem != null)
                    //{
                    //    if (cbAgent.SelectedItem != null)
                    //    {
                    //        VersementView VersementView = new VersementView();
                    //        if(VersementView.GetView(((VersementView)dgVersement.SelectedItem).Versement.VersementID))
                    //        {
                    //            Models.Versement versement = new Models.Versement();
                    //            versement.cheminBordereau = newFilePath;
                    //            versement.DateVers = dtVers.DisplayDate;
                    //            int numvers = 0;
                    //            if (Int32.TryParse(tbNumeroVers.Text, out numvers))
                    //                versement.NumeroVers = numvers;
                    //            else
                    //                throw new Exception("Veuillez entrer un numéro de versement valide");

                    //            versement.AgentID = ((Models.Agent)cbAgent.SelectedItem).AgentID;
                    //            versement.DateModif = DateTime.Now;
                    //            VersementView.Update(versement);
                    //            panelInteracBtn.Visibility = Visibility.Visible;
                    //            gridViewPanelAgent.Visibility = Visibility.Visible;
                    //            formAgent.Visibility = Visibility.Collapsed;
                    //            refreshListVersement();
                    //            MessageBox.Show("L'élément à bien été modifié !!!", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        MessageBox.Show("Veuillez Ajouter l'agent chargé du versement", "ATTENTION", MessageBoxButton.OK, MessageBoxImage.Error);
                    //    }
                    //}
                    //else
                    //{
                    //    MessageBox.Show("Veuillez Ajouter au moins un service à rattacher", "ATTENTION", MessageBoxButton.OK, MessageBoxImage.Error);
                    //}
                //}
                //catch (Exception ex)
                //{
                //    ex.ExceptionCatcher();
                //}
                #endregion
            throw new NotImplementedException();

            }

        }

        private void TbRechercher_TextChanged(object sender, TextChangedEventArgs e)
        {
            rechercheVersement();
        }

        private void dgVersement_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //Définition de la colonne des numéros d'odre
            // En plus il faut que EnableRowVirtualization="False"
            VersementView view = (VersementView)e.Row.Item;
            view.NumeroOrdre = e.Row.GetIndex() + 1;
            e.Row.Item = view;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                refreshListVersement();
                ContextMenu cm = this.FindResource("cmButton") as ContextMenu;
                dgVersement.ContextMenu = cm;

                using (var ct = new DocumatContext())
                {
                    var serv = ct.Service.ToList();
                    cbService.ItemsSource = serv;
                    cbService.DisplayMemberPath = "Nom";
                    cbService.SelectedValuePath = "ServiceID";

                    if (serv.Count() != 0)
                    {
                        var agents = ct.Agent.ToList();
                        cbAgent.ItemsSource = agents;
                        cbAgent.DisplayMemberPath = "Noms";
                        cbAgent.SelectedValuePath = "AgentID";
                    }
                }
            }
            catch (Exception ex){}
        }

        private void cbService_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                using (var ct = new DocumatContext())
                {
                    var agents = ct.Agent.ToList();
                    cbAgent.ItemsSource = agents;
                    cbAgent.DisplayMemberPath = "Noms";
                    cbAgent.SelectedValuePath = "AgentID";
                    cbAgent.SelectedIndex = 0;
                }
            }catch(Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void cbService_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void BtnChoixImg_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnChoixBordereau_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string DossierBordereau = nomDossierRacine + @"/" + ((Models.Service)cbService.SelectedItem).CheminDossier;

                if (!Directory.Exists(DossierBordereau))
                {
                    if (MessageBox.Show("Ce service n'a pas de dossier, voulez vous en créer un ?", "QUESTION", MessageBoxButton.OK, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        Directory.CreateDirectory(DossierBordereau);
                    else
                        return;
                }

                OpenFileDialog open = new OpenFileDialog();
                open.Filter = "JPEG|*.jpg";
                open.Multiselect = false;

                if (open.ShowDialog() == true)
                {
                    nomFichier = open.FileNames[0];
                    string newFile = "Bordereau" + DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + "" + new Random().Next(001, 999) + ".jpg";
                    newFilePath = DossierBordereau + @"\" + ((Models.Service)cbService.SelectedItem).CheminDossier + newFile;
                    //mettre le bout de code suivant dans la suvegarde 
                    File.Copy(nomFichier, newFilePath);
                    tbBorderau.Text = nomFichier;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERREUR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
