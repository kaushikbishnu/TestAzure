using BatchWatermark.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BatchWatermark
{
    public class Photo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(propertyName));
        }

        public string Title
        {
            get { return Path.GetFileName(this.FileName); }
        }

        public string FileName { get; set; }

        private string iso = string.Empty;
        public string ISO
        {
            get { return iso; }
            set
            {
                iso = value;
                NotifyPropertyChanged(nameof(ISO));
            }
        }

        private ImageSource image;
        public ImageSource Image
        {
            get { return image; }
            set
            {
                image = value;
                NotifyPropertyChanged(nameof(Image));
            }
        }

        public static Bitmap LoadBitmapUnlocked(string file_name)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (FileStream fs = File.OpenRead(file_name))
                {
                    fs.CopyTo(ms);
                }
                return new System.Drawing.Bitmap(ms);
            }
        }

        public static void Save(Image image, string savePath)
        {
            ImageCodecInfo jpgEncoder = ImageCodecInfo.GetImageDecoders()
                                .First((codec) => codec.FormatID == ImageFormat.Jpeg.Guid);

            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 50L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            image.Save(savePath, jpgEncoder, myEncoderParameters);
        }

        public void Load()
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(this.FileName);
            image.DecodePixelWidth = 200;
            image.EndInit();

            // Set Image.Source  
            Image = image;
            ISO = "1600";
        }

        public void Unload()
        {
            MemoryStream ms = new MemoryStream();
            Resources.EmptyLoad.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();

            // Set Image.Source  
            Image = image;
            ISO = "1600";
        }
    }
}
