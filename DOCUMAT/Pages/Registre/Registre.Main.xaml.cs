using DOCUMAT.Models;
using DOCUMAT.ViewModels;
using System;
using System.Collections.Generic;
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

namespace DOCUMAT.Pages.Registre
{
    /// <summary>
    /// Logique d'interaction pour Registre.xaml
    /// </summary>
    public partial class Registre : Page
    {
        private bool EditMode;
        Dictionary<String, Control> tbList = new Dictionary<string, Control>();

        #region Mes Fonctions

        // actualiser la Datagrid
        private void refreshListRegistre()
        {
            try
            {
                //Remplissage partial de  la list
                RegistreView RegistreView = new RegistreView();
                dgRegistre.ItemsSource = RegistreView.GetViewsList();                                
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        // filter la datagrid avec des éléments de recherches
        private void rechercheRegistre()
        {
            try
            {
                if (TbRechercher.Text != "")
                {
                    RegistreView RegistreView = new RegistreView();
                    dgRegistre.ItemsSource = RegistreView.GetViewsList().Where(r => r.ServiceVersant.Nom.Contains(TbRechercher.Text.ToUpper())
                                                                                || r.Registre.Type.Contains(TbRechercher.Text.ToUpper())).ToList();
                }
                else
                {
                    refreshListRegistre();
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

    #endregion



    public Registre()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // chargement des données de la datagridview
                refreshListRegistre();

                // ajout du menuContextuel à la Datagrid
                ContextMenu cm = this.FindResource("cmButton") as ContextMenu;
                dgRegistre.ContextMenu = cm;

                // enregistrement des contrôles dans un dictionnaire        
                tbList.Add(tbNombrePage.Name, tbNombrePage);
                tbList.Add(tbNumeroDepotDebut.Name, tbNumeroDepotDebut);
                tbList.Add(tbNumeroDepotFin.Name, tbNumeroDepotFin);
                tbList.Add(tbNumeroVolume.Name, tbNumeroVolume);
                tbList.Add(tbObservation.Name, tbObservation);
                tbList.Add(tbTypeRegistre.Name, tbTypeRegistre);
                tbList.Add(dtDepotDebut.Name, dtDepotDebut);
                tbList.Add(dtDepotFin.Name, dtDepotFin);

                // Chargement des cbAgent, cbService, cbVersement
                //using (var ct = new DocumatContext())
                //{
                //    var serv = ct.Service.ToList();
                //    cbService.ItemsSource = serv;
                //    cbService.DisplayMemberPath = "Nom";
                //    cbService.SelectedValuePath = "ServiceID";
                //    cbService.SelectedIndex = 0;

                //    if (serv.Count() != 0)
                //    {
                //        var agents = ct.Agent.Where(a => a.ServiceID == ((Models.Service)cbService.SelectedItem).ServiceID).ToList();
                //        cbAgent.ItemsSource = agents;
                //        cbAgent.DisplayMemberPath = "Noms";
                //        cbAgent.SelectedValuePath = "AgentID";
                //        cbAgent.SelectedIndex = 0;

                //        if (agents.Count() != 0)
                //        {
                //            var versement = ct.Versement.Where(v => v.AgentID == ((Models.Agent)cbAgent.SelectedItem).AgentID).ToList();
                //            cbVersement.ItemsSource = versement;
                //            cbVersement.DisplayMemberPath = "NumeroVers";
                //            cbVersement.SelectedValuePath = "VersementID";
                //            cbVersement.SelectedIndex = 0;
                //        }
                //    }
                //}

            }
            catch (Exception ex) {}
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            EditMode = false;

            foreach (var tb in tbList)
            {
                var tbtype = tb.Value.GetType();
                if (tbtype == tbNombrePage.GetType())
                    ((TextBox)tb.Value).Text = "";
                else if (tbtype == dtDepotFin.GetType())
                    ((DatePicker)tb.Value).SelectedDate = DateTime.Now.Date;
            }

            //réactivation des champs fondamentaux
            cbVersement.IsEnabled = true;
            cbAgent.IsEnabled = true;
            cbService.IsEnabled = true;
            tbNumeroVolume.IsEnabled = true;


            txbFormHeader.Text = "FORMULAIRE D'AJOUT";
            cbStatut.Visibility = Visibility.Collapsed;
            panelInteracBtn.Visibility = Visibility.Collapsed;
            gridViewPanelAgent.Visibility = Visibility.Collapsed;
            formRegistre.Visibility = Visibility.Visible;
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgRegistre.SelectedItems.Count == 1)
            {
                EditMode = true;
                txbFormHeader.Text = "FORMULAIRE DE MODIFICATION";

                //Dissimulation ou blocage des champs fondamentaux
                cbVersement.IsEnabled = false;
                cbAgent.IsEnabled = false;
                cbService.IsEnabled = false;
                tbNumeroVolume.IsEnabled = false;

                try
                {
                    RegistreView registreView = new RegistreView();
                    registreView = registreView.GetViewRegistre(((RegistreView)dgRegistre.SelectedItem).Registre.RegistreID);
                    if (registreView != null)
                    {
                        dtDepotDebut.SelectedDate = registreView.Registre.DateDepotDebut;
                        dtDepotFin.SelectedDate = registreView.Registre.DateDepotFin;
                        tbNombrePage.Text = registreView.Registre.NombrePage.ToString();
                        tbNumeroVolume.Text = registreView.Registre.Numero.ToString();
                        tbNumeroDepotDebut.Text = registreView.Registre.NumeroDepotDebut.ToString();
                        tbNumeroDepotFin.Text = registreView.Registre.NumeroDepotFin.ToString();
                        tbObservation.Text = registreView.Registre.Observation;
                        tbTypeRegistre.Text = registreView.Registre.Type;

                        cbStatut.DisplayMemberPath = "Libelle";
                        cbStatut.SelectedValuePath = "StatutRegistreID";
                        cbStatut.ItemsSource = registreView.Registre.StatutRegistres.Where(s => s.RegistreID == registreView.Registre.RegistreID
                                               && s.Code == registreView.Registre.StatutActuel).ToList();
                        cbStatut.SelectedIndex = 0;
                        cbStatut.IsEnabled = false;

                        //cbStatut.SelectedValue = registreView.Registre.StatutRegistres.FirstOrDefault(s => s.RegistreID == registreView.Registre.RegistreID 
                        //                           && s.Code == registreView.Registre.StatutActuel).StatutRegistreID;

                        cbService.SelectedValue = registreView.ServiceVersant.ServiceID;
                        cbVersement.SelectedValue = registreView.Registre.VersementID;
                        //cbAgent.SelectedValue = registreView.context.Agent.SingleOrDefault(a => a.AgentID == registreView.Registre.Versement.AgentID).AgentID;
                        //cbService.SelectedValue = registreView.context.Services.SingleOrDefault(s => s.ServiceID ==  ).ServiceID;
                        cbStatut.Visibility = Visibility.Visible;
                        panelInteracBtn.Visibility = Visibility.Collapsed;
                        gridViewPanelAgent.Visibility = Visibility.Collapsed;
                        formRegistre.Visibility = Visibility.Visible;
                    }
                }
                catch (Exception ex)
                {
                    ex.ExceptionCatcher();
                }
            }
            else
            {
                MessageBox.Show("Veillez sélectionner un élément à modifier", "ATTENTION", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRechercher_Click(object sender, RoutedEventArgs e)
        {
            rechercheRegistre();
        }

        private void btnSupprimer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgRegistre.SelectedItems.Count > 0)
                {
                    if (MessageBox.Show("Voulez-vous vraiment supprimer cet élément ?", "ATTENTION", MessageBoxButton.YesNo, MessageBoxImage.Question)
                        == MessageBoxResult.Yes)
                    {
                        RegistreView registreView = new RegistreView();

                        foreach (RegistreView item in dgRegistre.SelectedItems)
                        {
                            registreView.context.Registre.Remove(registreView.context.Registre.FirstOrDefault(r => r.RegistreID == item.Registre.RegistreID));
                        }
                        registreView.context.SaveChanges();
                        refreshListRegistre();
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

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            refreshListRegistre();
        }

        private void btnAnnule_Click(object sender, RoutedEventArgs e)
        {
            panelInteracBtn.Visibility = Visibility.Visible;
            gridViewPanelAgent.Visibility = Visibility.Visible;
            formRegistre.Visibility = Visibility.Collapsed;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!EditMode)
            {
                #region Ajouter
                //try
                //{
                if (cbService.SelectedItem != null)
                {
                    if (cbAgent.SelectedItem != null)
                    {
                        if (cbVersement.SelectedItem != null)
                        {
                            //Création du Dossier de registre dans le dossier de service
                            //Se fera après l'enregistrement des données: on inialise le dossier avec une valeur : Defaut                            
                            RegistreView registreView = new RegistreView();
                            /*registreView.Registre.CheminDossier = DossierRegistre;*/ //devra être changer après l'ajout
                            registreView.Registre.DateCreation = DateTime.Now;
                            registreView.Registre.DateModif = DateTime.Now;
                            registreView.Registre.DateDepotDebut = dtDepotDebut.DisplayDate;
                            registreView.Registre.DateDepotFin = dtDepotFin.DisplayDate;

                            int NombrePage = 0;
                            if (Int32.TryParse(tbNombrePage.Text, out NombrePage))
                                registreView.Registre.NombrePage = NombrePage;


                            if (tbNumeroVolume.Text != "")
                                registreView.Registre.Numero = tbNumeroVolume.Text;

                            int NumeroDepotDebut = 0;
                            if (Int32.TryParse(tbNumeroDepotDebut.Text, out NumeroDepotDebut))
                                registreView.Registre.NumeroDepotDebut = NumeroDepotDebut;

                            int NumeroDepotFin = 0;
                            if (Int32.TryParse(tbNumeroDepotFin.Text, out NumeroDepotFin))
                                registreView.Registre.NumeroDepotFin = NumeroDepotFin;

                            registreView.Registre.Observation = tbObservation.Text;
                            /*registreView.Registre.QrCode = "Defaut";*/ // Sera changer après l'ajout du registre
                            registreView.Registre.StatutActuel = 0; // Sera changer après l'ajout du registre
                            registreView.Registre.Type = tbTypeRegistre.Text;
                            registreView.Registre.VersementID = ((Models.Versement)cbVersement.SelectedItem).VersementID;
                            registreView.ServiceVersant = registreView.context.Service.FirstOrDefault(s => s.ServiceID == registreView.Registre.VersementID);
                            registreView.Add();

                            panelInteracBtn.Visibility = Visibility.Visible;
                            gridViewPanelAgent.Visibility = Visibility.Visible;
                            formRegistre.Visibility = Visibility.Collapsed;
                            refreshListRegistre();
                            MessageBox.Show("L'élément a bien été ajouté !!!", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Veuillez Ajouter l'agent chargé du versement", "ATTENTION", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Veuillez Ajouter au moins un service à rattacher", "ATTENTION", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
                try
                {
                    if (cbService.SelectedItem != null)
                    {
                        if (cbAgent.SelectedItem != null)
                        {
                            if (cbVersement.SelectedItem != null)
                            {
                                RegistreView registreView = new RegistreView();
                                if (registreView.GetView(((RegistreView)dgRegistre.SelectedItem).Registre.RegistreID))
                                {
                                    Models.Registre registre = new Models.Registre();
                                    registre.DateDepotDebut = dtDepotDebut.DisplayDate;
                                    registre.DateDepotFin = dtDepotFin.DisplayDate;
                                    int NombrePage = 0;
                                    if (Int32.TryParse(tbNombrePage.Text, out NombrePage))
                                        registre.NombrePage = NombrePage;

                                    int NumeroDepotDebut = 0;
                                    if (Int32.TryParse(tbNumeroDepotDebut.Text, out NumeroDepotDebut))
                                        registre.NumeroDepotDebut = NumeroDepotDebut;

                                    int NumeroDepotFin = 0;
                                    if (Int32.TryParse(tbNumeroDepotFin.Text, out NumeroDepotFin))
                                        registre.NumeroDepotFin = NumeroDepotFin;

                                    registre.Observation = tbObservation.Text;
                                    registre.Type = tbTypeRegistre.Text;

                                    registreView.Update(registre);
                                    panelInteracBtn.Visibility = Visibility.Visible;
                                    gridViewPanelAgent.Visibility = Visibility.Visible;
                                    formRegistre.Visibility = Visibility.Collapsed;
                                    refreshListRegistre();
                                    MessageBox.Show("L'élément a bien été Modifié !!!", "NOTIFICATION", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Veuillez Ajouter l'agent chargé du versement", "ATTENTION", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Veuillez Ajouter au moins un service à rattacher", "ATTENTION", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    ex.ExceptionCatcher();
                }
                #endregion
            }
        }

        private void dgRegistre_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //Définition de la colonne des numéros d'odre
            // En plus il faut que EnableRowVirtualization="False"
            RegistreView view = (RegistreView)e.Row.Item;
            view.NumeroOrdre = e.Row.GetIndex() + 1;
            e.Row.Item = view;
        }

        private void TbRechercher_TextChanged(object sender, TextChangedEventArgs e)
        {
            rechercheRegistre();
        }

        private void cbService_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Chargement des cbAgent
            try
            {
                using (var ct = new DocumatContext())
                {

                    if (cbService.SelectedItem != null)
                    {
                        var agents = ct.Agent.ToList();
                        cbAgent.ItemsSource = agents;
                        cbAgent.DisplayMemberPath = "Noms";
                        cbAgent.SelectedValuePath = "AgentID";
                        cbAgent.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void cbAgent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Chargement des cbAgent
            //try
            //{
            //    using (var ct = new DocumatContext())
            //    {
            //        if (cbAgent.SelectedItem != null)
            //        {
            //            var versement = ct.Versement.Where(v => v.AgentID == ((Models.Agent)cbAgent.SelectedItem).AgentID).ToList();
            //            cbVersement.ItemsSource = versement;
            //            cbVersement.DisplayMemberPath = "NumeroVers";
            //            cbVersement.SelectedValuePath = "VersementID";
            //            cbVersement.SelectedIndex = 0;
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    ex.ExceptionCatcher();
            //}
            throw new NotImplementedException();
        }

        private void cbVersement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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
            var image = qrcode.Draw(registre.Registre.QrCode,40);
            var imageConvertie = image.ConvertDrawingImageToWPFImage(null,null) ;
            //((System.Windows.Controls.Image)element).Source = imageConvertie.Source;
        }

        private void ImprimerQrCode_Click(object sender, RoutedEventArgs e)
        {
            if(dgRegistre.SelectedItems.Count == 1)
            {
                string qrCode = ((RegistreView)dgRegistre.SelectedItem).Registre.QrCode;             
                Impression.QrCode PageImp = new Impression.QrCode(qrCode);
                PageImp.Show();
            }
        }

        private void ModifImages_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
