using DOCUMAT.Models;
using DOCUMAT.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DOCUMAT.Pages.Image
{
    /// <summary>
    /// Logique d'interaction pour FormTransfertSequence.xaml
    /// </summary>
    public partial class FormTransfertSequence : Window
    {
        List<SequenceView> SequenceViews;
        Models.Registre RegistreParent;
        ImageView ImageParent;
        ImageIndexSuperviseur MainParent;

        public FormTransfertSequence()
        {
            InitializeComponent();
        }

        public FormTransfertSequence(List<SequenceView> sequenceViews, ImageView imageView, Models.Registre registre, ImageIndexSuperviseur imageIndexSuperviseur) : this()
        {
            RegistreParent = registre;
            ImageParent = imageView;
            SequenceViews = sequenceViews;
            MainParent = imageIndexSuperviseur;
        }

        private void btnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnTransfererSequences_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cbListeImage.SelectedItem != null && dgSequence.Items.Count > 0)
                {
                    if (MessageBox.Show("Confirmez vous l'Opération ?", "Confirmez", MessageBoxButton.YesNo, MessageBoxImage.Question)
                            == MessageBoxResult.Yes)
                    {
                        using (var ct = new DocumatContext())
                        {
                            // Récupération de la date du serveur 
                            DateTime dateServer = ct.Database.SqlQuery<DateTime>("SELECT GETDATE()").FirstOrDefault();

                            if (cbChoix.SelectedIndex == 0)
                            {
                                #region TRANSFERT DES SEQUENCES 
                                // Récupération de l'image de Reception  
                                Models.Image imageRec = (Models.Image)cbListeImage.SelectedItem;

                                foreach (SequenceView seq in dgSequence.ItemsSource)
                                {
                                    Models.Sequence sequence = ct.Sequence.FirstOrDefault(s => s.SequenceID == seq.Sequence.SequenceID);
                                    if (sequence != null)
                                    {
                                        sequence.ImageID = imageRec.ImageID;
                                        ct.SaveChanges();
                                    }
                                }

                                this.Close();
                                MainParent.ChargerImage(ImageParent.Image.NumeroPage);
                                MessageBox.Show($"Dépôts transférés à l'Image : {imageRec.NumeroPage} !", "Dépôts transférées", MessageBoxButton.OK, MessageBoxImage.Information);
                                #endregion
                            }
                            else
                            {
                                #region COPIE DES SEQUENCES 
                                // Récupération de l'image de Reception  
                                Models.Image imageRec = (Models.Image)cbListeImage.SelectedItem;

                                foreach (SequenceView seq in dgSequence.ItemsSource)
                                {
                                    Models.Sequence sequence1 = new Sequence()
                                    {
                                        DateCreation = dateServer,
                                        DateModif = dateServer,
                                        NUmeroOdre = seq.Sequence.NUmeroOdre,
                                        DateSequence = seq.Sequence.DateSequence,
                                        ImageID = imageRec.ImageID,
                                        isSpeciale = seq.Sequence.isSpeciale,
                                        PhaseActuelle = 0,
                                        References = seq.Sequence.References,
                                        NombreDeReferences = seq.Sequence.References.Split(',').Count()
                                    };
                                    ct.Sequence.Add(sequence1);
                                }
                                ct.SaveChanges();
                                this.Close();
                                MessageBox.Show($"Dépôts Copié vers l'Image : {imageRec.NumeroPage} !", "Dépôts Copiées", MessageBoxButton.OK, MessageBoxImage.Information);
                                #endregion
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Affichage des séquences
                dgSequence.ItemsSource = SequenceViews;

                // Chargement des images à utiliser !!
                using (var ct = new DocumatContext())
                {
                    List<Models.Image> images = ct.Image.Where(i => i.RegistreID == RegistreParent.RegistreID
                                                                && i.ImageID != ImageParent.Image.ImageID
                                                                && i.NumeroPage != 0
                                                                && i.NumeroPage != -1).ToList();

                    // Affichage des images dans la combobox
                    cbListeImage.ItemsSource = images.OrderBy(i => i.NumeroPage);
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }
    }
}