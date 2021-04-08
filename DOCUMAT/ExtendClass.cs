using System;
using System.Configuration;
using System.Data.Entity.Validation;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DOCUMAT
{
    public static class ExtendClass
    {
        /// <summary>
        /// ExtendMethod permettant de formater Les Exceptions
        /// </summary>
        /// <param name="exception"> Exception à traiter</param>
        public static void ExceptionCatcher(this Exception exception)
        {
            // définition d'un obj de type DbEntityValidationException pour faire la vérification
            DbEntityValidationException exceptionVal = new DbEntityValidationException();
            // obj non implement
            NotImplementedException notImplementedException = new NotImplementedException();

            // récupération du type de l'exception levé
            Type typeException = exception.GetType();
            if (typeException.Name == exceptionVal.GetType().Name)
            {
                string ErrorMessages = "";
                int ErrorNum = 1;
                foreach (DbEntityValidationResult validation in ((DbEntityValidationException)exception).EntityValidationErrors.ToList())
                {
                    foreach (DbValidationError validationError in validation.ValidationErrors.ToList())
                    {
                        ErrorMessages += "Erreur " + ErrorNum + " : " + validationError.ErrorMessage + "\n";
                        ErrorNum++;
                    }
                }
                MessageBox.Show(ErrorMessages, "ATTENTION", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(exception.Message, "ERREUR INATTENDUE", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // Enregistrement de Toutes les exceptions dans un fichier log 
            FileStream stream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(),
                                                          ConfigurationManager.AppSettings["Log_Exception_File"]), FileMode.Append);
            using (var csW = new StreamWriter(stream, Encoding.UTF8))
            {
                // Ajout de L'exception au fichier 
                csW.WriteLine($"DATE : {DateTime.Now}  ::: {exception.Source}");
                csW.WriteLine($"{exception.TargetSite}");
                csW.WriteLine($"{exception.Message}");
                csW.WriteLine($"{exception.StackTrace}");
                csW.WriteLine($"{exception.Data}");
                csW.WriteLine($"{exception.InnerException}");
                csW.WriteLine($"{exception.HResult}");
                csW.WriteLine($"{exception.HelpLink}");
                csW.WriteLine();
                csW.WriteLine();
                csW.Close();
            }
        }

        /// <summary>
        ///  Convertir une image de type System.Drawing.Image en System.Controls.Image
        /// </summary>
        public static System.Windows.Controls.Image ConvertDrawingImageToWPFImage(this Image image, int? width, int? height)
        {
            System.Windows.Controls.Image img = new System.Windows.Controls.Image();
            //convert System.Drawing.Image to WPF image
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(image);
            IntPtr hBitmap = bmp.GetHbitmap();
            System.Windows.Media.ImageSource WpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            img.Source = WpfBitmap;
            img.Width = (width == null) ? 400 : (double)width;
            img.Height = (height == null) ? 400 : (double)height;
            img.Stretch = System.Windows.Media.Stretch.Fill;
            return img;
        }

        /// <summary> Non utilisée
        /// Permet de crypter une chaine de caractère
        /// </summary>
        /// <param name="mdp"> La chaine à crypter </param>
        /// <returns></returns>
        public static string CryptMd5(this String mdp)
        {
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(mdp));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        /// <summary> Non utilisée
        /// Permet de vérifier la correspondance entre une chaine cryptée et une chaine de caractère
        /// </summary>
        /// <param name="mdpClair"> La chaine de caractère normale  </param>
        /// <param name="mdpHash">La chaine de caractère cryptée </param>
        /// <returns></returns>
        public static bool IsCryptResult(this String mdpClair, String mdpHash)
        {
            string hashOfClair = mdpClair.CryptMd5();
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            if (0 == comparer.Compare(hashOfClair, mdpHash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
