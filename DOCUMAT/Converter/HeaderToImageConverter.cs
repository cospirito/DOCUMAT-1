using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DOCUMAT.Converter
{
    /// <summary>
    /// Converts a full path to a specific image type of a drive, folder or file
    /// </summary>
    [ValueConversion(typeof(string), typeof(BitmapImage))]
    public class HeaderToImageConverter : IValueConverter
    {
        public static HeaderToImageConverter Instance = new HeaderToImageConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Get the full path
            var path = (string)value;

            // If the path is null, ignore
            if (path == null)
                return null;

            // Get the name of the file/folder
            var name = Pages.Image.ImageViewer.GetFileFolderName(path);

            // By default, we presume an image
            var image = "Images/simplefile1.png";

            // If the name is blank, we presume it's a drive as we cannot have a blank file or folder name
            if (string.IsNullOrEmpty(name))
            {
                image = "Images/drive.png";
            }
            else if (path == "simple")
            {
                image = "Images/simplefile1.png";
            }
            else if (path == "valide")
            {
                image = "Images/valide2.png";
            }
            else if (path == "instance")
            {
                image = "Images/warning1.png";
            }
            else if (path == "edit")
            {
                image = "Images/edit.png";
            }
            else if (path == "notFound")
            {
                image = "Images/fileNotFound.png";
            }
            else if (path == "fileDelele")
            {
                image = "Images/fileDelete.png";
            }
            else if (path == "Correct")
            {
                image = "Images/fileStar.png";
            }
            else if(path == "ErrorFolder")
            {
                image = "Images/folderError.png";
            }
            else if (new FileInfo(path).Extension == "")
            {
                image = "Images/folder-closed.png";
            }


            return new BitmapImage(new Uri($"pack://application:,,,/{image}"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
