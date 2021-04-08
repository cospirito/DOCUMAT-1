using System;
using System.Windows;
using System.Windows.Controls;

namespace DOCUMAT
{
    /// <summary>
    /// Logique d'interaction pour TestWindows.xaml
    /// </summary>
    public partial class TestWindows : Window
    {
        double defaultPick = 15.71, defaultWidth = 300, defaultHeight = 300, defaultThickness = 50, percent = 90;

        private void level_TextChanged(object sender, TextChangedEventArgs e)
        {
            int perc = 0;
            if (Int32.TryParse(level.Text, out perc))
            {
                percent = perc;
                SetProgress();
            }
        }

        public TestWindows()
        {
            InitializeComponent();
            SetProgress();
        }

        public void SetProgress()
        {
            //Configuration de l'indicateur
            double pixelSize = ProgressBarCirculaire.Height = ProgressBarCirculaire.Width = 300;
            double pixelPick = (defaultPick * pixelSize) / defaultWidth;

            double pixelPercent = (pixelPick * percent) / 100;

            ProgressBarCirculaire.ProgressIndicator = pixelPercent + " " + pixelPick;
            ProgressBarCirculaire.TextIndicator = percent + "%";
            indicator.Text = " pixelPick : " + pixelPick + "; \n pixelSize : " + pixelSize + ";\n pixelPercent : " + pixelPercent
                            + "; \n ProgressBarIndicator : " + ProgressBarCirculaire.ProgressIndicator;
        }
    }
}
