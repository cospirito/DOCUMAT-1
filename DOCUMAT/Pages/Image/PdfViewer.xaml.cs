using Microsoft.Win32;
using System.Windows;
using System.Windows.Xps.Packaging;
using System;

namespace DOCUMAT.Pages.Image
{
    /// <summary>
    /// Logique d'interaction pour PdfViewer.xaml
    /// </summary>
    public partial class PdfViewer : Window
    {
        XpsDocument xpsDocument;

        public PdfViewer()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void OpenPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.Filter = "PDF files (*.pdf)|*.pdf";

                if (fileDialog.ShowDialog() == true)
                {
                    Viewer.axAcroPDF1.LoadFile(fileDialog.FileName);
                    //using (var document = PdfDocument.Load(fileDialog.FileName))
                    //{
                    //    // XpsDocument needs to stay referenced so that DocumentViewer can access additional required resources.
                    //    // Otherwise, GC will collect/dispose XpsDocument and DocumentViewer will not work.
                    //    //this.xpsDocument = document.ConvertToXpsDocument(SaveOptions.Xps);
                    //    //this.PDDocument.Document = this.xpsDocument.GetFixedDocumentSequence();
                    //}
                }
            }
            catch (Exception ex)
            {
                ex.ExceptionCatcher();
            }
        }
    }
}
