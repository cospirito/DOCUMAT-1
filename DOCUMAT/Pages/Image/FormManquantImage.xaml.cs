using DOCUMAT.Models;
using DOCUMAT.ViewModels;
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
    /// Logique d'interaction pour FormManquantImage.xaml
    /// </summary>
    public partial class FormManquantImage : Window
    {
		public ImageController ImageController;

        public FormManquantImage()
        {
            InitializeComponent();
			dtDateSequenceManquant.Language = XmlLanguage.GetLanguage("fr-FR");
		}

		public FormManquantImage(ImageController imageController) :this()
        {
			ImageController = imageController;
		}

        private void tbNumeroImageManquant_TextChanged(object sender, TextChangedEventArgs e)
        {
			Regex regex = new Regex("^[0-9]$");
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
			Regex regex = new Regex("^[0-9]$");
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
			Regex regex = new Regex("^[0-9]$");
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

        private void btnAnnulerManquant_Click(object sender, RoutedEventArgs e)
        {
			this.Close();
        }

        private void btnValiderManquant_Click(object sender, RoutedEventArgs e)
        {
			try
			{
				using (var ct = new DocumatContext())
				{
					int numPage = 0;
					if (!Int32.TryParse(tbNumeroImageManquant.Text.Trim(), out numPage))
						throw new Exception("Numero de Page Incorrect");

					int numDebut = 0;
					if (!Int32.TryParse(tbDebutSequenceManquant.Text.Trim(), out numDebut))
						throw new Exception("Numero de Séquence de début Incorrect");

					int numFin = 0;
					if (!Int32.TryParse(tbFinSequenceManquant.Text.Trim(), out numFin))
						throw new Exception("Numero de Séquence de fin Incorrect");

					if (!dtDateSequenceManquant.SelectedDate.HasValue)
						throw new Exception("La date de la première séquence est incorrecte !!!");
					Models.ManquantImage manquantImage = new ManquantImage()
					{
						IdRegistre = ImageController.RegistreViewParent.Registre.RegistreID,
						IdImage = null,
						NumeroPage = numPage,
						DebutSequence = numDebut,
						FinSequence = numFin,
						statutManquant = 1,
						DateDeclareManquant = DateTime.Now,
						DateSequenceDebut = dtDateSequenceManquant.SelectedDate.Value,
						DateCreation = DateTime.Now,
						DateModif = DateTime.Now,
					};
					ct.ManquantImage.Add(manquantImage);
					ct.SaveChanges();

					ImageController.LoadAborescence();

					//Enregistrement de la tâche
					DocumatContext.AddTraitement(DocumatContext.TbManquantImage, manquantImage.ManquantImageID, ImageController.MainParent.Utilisateur.AgentID, (int)Enumeration.TypeTraitement.CREATION, "CONTROLE PH1 : AJOUT D'UNE IMAGE MANQUANT DE NUMERO PAGE : " + manquantImage.NumeroPage + " AU REGISTRE ID N° " + ImageController.RegistreViewParent.Registre.RegistreID);
					this.Close();
					MessageBox.Show("Manquant Ajouter", "Ajouté", MessageBoxButton.OK,MessageBoxImage.Information);
				}
			}
			catch (Exception ex)
			{
				ex.ExceptionCatcher();
			}
		}
    }
}
