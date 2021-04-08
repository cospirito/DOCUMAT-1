using DOCUMAT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace DOCUMAT.Pages.Image
{
    /// <summary>
    /// Logique d'interaction pour GenererSequenceAuto.xaml
    /// </summary>
    public partial class GenererSequenceAuto : Window
    {
        Models.Image imageParent;
        Models.Agent User;
        ImageIndexSuperviseur MainParent;

        public GenererSequenceAuto()
        {
            InitializeComponent();

            //Définition de la date du datetimepicker
            dtDateSequence.Language = XmlLanguage.GetLanguage("fr-FR");
        }

        public GenererSequenceAuto(Models.Image image, Models.Agent agent, ImageIndexSuperviseur imageIndexSuperviseur) : this()
        {
            imageParent = image;
            User = agent;
            MainParent = imageIndexSuperviseur;
            HeaderText.Text = $"Pour L'Image : {image.NumeroPage}";
        }

        private void btnGenererSequences_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                #region CODE DE GENERATION DES SEQUENCES AUTO
                // Récupération des valeurs des champs
                int NumeroDepotDebut = 0, NombrePage = 0;
                DateTime DateSequence;
                Regex RefsReg = new Regex("^(["+MainParent.RefInitiale+"][0-9]{1,}/[\\w*]{1,}[,]){1,}([TR][0-9]{1,}/[\\w*]{1,})$");
                Regex RefsReg1 = new Regex("^["+MainParent.RefInitiale+"][0-9]{1,}/[\\w*]{1,}$");
                if (Int32.TryParse(tbNumeroDepotDebut.Text.Trim(), out NumeroDepotDebut) && Int32.TryParse(tbNombreDepot.Text.Trim(), out NombrePage)
                    && DateTime.TryParse(dtDateSequence.Text, out DateSequence) && NumeroDepotDebut > 0 && NombrePage > 0
                    && (string.IsNullOrWhiteSpace(tbRefs.Text) || RefsReg.IsMatch(tbRefs.Text.Trim()) || RefsReg1.IsMatch(tbRefs.Text.Trim())))
                {
                    if (MessageBox.Show($"Voulez vous génerer vraiment ces référenes pour la page {imageParent.NumeroPage} !?", "Génerer !!?", MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        using (var ct = new DocumatContext())
                        {
                            // Récupération de la date du serveur 
                            DateTime dateServer = ct.Database.SqlQuery<DateTime>("SELECT GETDATE()").FirstOrDefault();

                            List<Sequence> sequences = new List<Sequence>();
                            for (int i = 0; i < NombrePage; i++)
                            {
                                Models.Sequence sequence = new Sequence()
                                {
                                    DateCreation = dateServer,
                                    DateModif = dateServer,
                                    DateSequence = DateSequence,
                                    isSpeciale = tbIsSpecial.Text.Trim().ToLower(),
                                    NombreDeReferences = 0,
                                    NUmeroOdre = NumeroDepotDebut + i,
                                    PhaseActuelle = 0,
                                    References = tbRefs.Text.Trim(),
                                    ImageID = imageParent.ImageID,
                                };
                                sequence = ct.Sequence.Add(sequence);
                                ct.SaveChanges();
                                //DocumatContext.AddTraitement(DocumatContext.TbSequence, sequence.SequenceID, User.AgentID, (int)Enumeration.TypeTraitement.CREATION,"");
                            }
                            this.Close();
                            MainParent.ChargerImage(imageParent.NumeroPage);
                            MessageBox.Show("les Séquences ont bien été ajoutées", "Séquencees Ajoutées", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Verifier Svp, les champs que vous avez saisie !!", "Erreur de Saisie", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                #endregion
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }

        private void btnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void tbNombreDepot_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void tbNumeroDepotDebut_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
