using System;
using System.Windows.Forms;

namespace DOCUMAT
{
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
        }

        private void UserControl1_Load(object sender, EventArgs e)
        {
            //axAcroPDF1.LoadFile(@"C:\DOCUMAT\RegistreTest.pdf");
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        //public Boolean bReadonly = false;

        //private void axPDFViewer1_OnDocumentOpened(object sender, AxPDFViewerLib._DPDFViewerEvents_OnDocumentOpenedEvent e)
        //{
        //    if (bReadonly)
        //    {
        //        axPDFViewer1.SetReadOnly();
        //    }
        //    else
        //    {

        //    }
        //}

        //public void OpenEditable()
        //{
        //    OpenFileDialog openFileDialog = new OpenFileDialog();
        //    openFileDialog.Filter = "PDF File Formats (*.pdf)|*.pdf|All Files (*.*) | *.* ||";
        //    openFileDialog.RestoreDirectory = true;
        //    openFileDialog.FilterIndex = 1;
        //    if (openFileDialog.ShowDialog() == DialogResult.OK)
        //    {
        //        axPDFViewer1.Toolbars = true;
        //        axPDFViewer1.LoadFile(openFileDialog.FileName);
        //        bReadonly = false;
        //    }
        //}

        //public void OpenReadOnly()
        //{
        //    OpenFileDialog openFileDialog = new OpenFileDialog();
        //    openFileDialog.Filter = "PDF File Formats (*.pdf)|*.pdf|All Files (*.*) | *.* ||";
        //    openFileDialog.RestoreDirectory = true;
        //    openFileDialog.FilterIndex = 1;
        //    if (openFileDialog.ShowDialog() == DialogResult.OK)
        //    {
        //        axPDFViewer1.Toolbars = false;
        //        axPDFViewer1.LoadFile(openFileDialog.FileName);
        //        bReadonly = true;
        //    }
        //}

        //public void SaveAs()
        //{
        //    axPDFViewer1.SaveCopyAs();
        //}

        //public void Print()
        //{
        //    axPDFViewer1.PrintWithDialog();
        //}

        //public void ClosePDF()
        //{
        //    axPDFViewer1.Clear();
        //}

        //private void UserControl1_Load(object sender, EventArgs e)
        //{

        //}
    }
}
