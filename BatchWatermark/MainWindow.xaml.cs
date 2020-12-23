using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Threading;

namespace BatchWatermark
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>btnOpenBatchFileList_Click
    public partial class MainWindow : Window
    {
        Uri waterMarkUri = new Uri(@"C:\Users\kaush\Pictures\shoroshi.jpg");
        string batchUri = @"E:\Photos\NikonW300\Majorca2018\2018_07_11";
        System.Drawing.Image waterMarkImage;

        public ObservableCollection<Photo> Photos { get; }
        = new ObservableCollection<Photo>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnOpenWaterMarkFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                waterMarkUri = new Uri(openFileDialog.FileName);
                waterMarkImage = System.Drawing.Image.FromFile(waterMarkUri.AbsolutePath);
                imgWatermark.Source = new BitmapImage(waterMarkUri);
            }
        }

        private async void btnOpenBatchFileList_ClickAsync(object sender, RoutedEventArgs e)
        {
            Photos.Clear();
            batchUri = openFolderDialog();
            if (String.IsNullOrEmpty(batchUri)) return;

            var imagesFromFolder = Directory.GetFiles(batchUri, "*.jpg", SearchOption.AllDirectories).ToList();
            //var listImages = new ObservableCollection<ImageWrapper>();
            lbBatchFileList.ItemsSource = Photos;

            foreach (var file in imagesFromFolder)
            {
                Photos.Add(new Photo { FileName = file });
            }

            await AsyncDataLoad();
        }

        public async Task AsyncDataLoad()
        {
            await Task.Run(() => AddItemsToList());
        }

        public void AddItemsToList()
        {
            foreach (var photo in Photos)
            {
                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => photo.Load()));
            }
        }

        private string openFolderDialog()
        {
            var dialog = new CommonOpenFileDialog();
            var result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                return Directory.Exists(dialog.FileName) ? dialog.FileName : Path.GetDirectoryName(dialog.FileName);
            }
            return null;
        }

        private async void btnApplyWaterMark_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string saveFilePath = openFolderDialog();

                spin.Visibility = Visibility.Visible;

                string waterMarkPath = waterMarkUri.AbsolutePath; if (string.IsNullOrEmpty(saveFilePath)) return;
                List<string> imagePathList = new List<string>();

                foreach (Photo photo in lbBatchFileList.SelectedItems)
                {
                    imagePathList.Add(photo.FileName);
                }

                await Task.Run(() =>
                {
                    foreach (string imagePath in imagePathList)
                    {
                        MergeAndSave(imagePath, waterMarkPath, saveFilePath + "\\" + Path.GetFileName(imagePath));
                    }
                }).ContinueWith((result) =>
                {
                    if (result.Status == TaskStatus.RanToCompletion)
                        MessageBox.Show("Batch Completed.");

                    if (result.IsFaulted)
                        MessageBox.Show(result.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    Dispatcher.Invoke(() => spin.Visibility = Visibility.Collapsed);
                });
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }


        private System.Drawing.Image Merge(System.Drawing.Image image, System.Drawing.Image waterMark)
        {
            var bitmap = new Bitmap(image.Width, image.Height);
            try
            {
                var waterMarkHeight = image.Width / 10;
                var waterMarkWidth = image.Width / 10;
                var waterMarkTopCenter = new Rectangle(image.Width / 2 - waterMarkWidth / 2, 0, waterMarkHeight, waterMarkWidth);
                var waterMarkLeftCenter = new Rectangle(0, image.Height / 2 - waterMarkHeight / 2, waterMarkHeight, waterMarkWidth);
                var waterMarkCenterBottom = new Rectangle(image.Width / 2 - waterMarkHeight / 2, image.Height - waterMarkHeight, waterMarkHeight, waterMarkWidth);
                var waterMarkRightCenter = new Rectangle(image.Width - waterMarkWidth, image.Height / 2 - waterMarkHeight / 2, waterMarkHeight, waterMarkWidth);
                var waterMarkCenter = new Rectangle(image.Width / 2 - waterMarkWidth / 2, image.Height / 2 - waterMarkHeight / 2, waterMarkHeight, waterMarkWidth);

                using (var g = Graphics.FromImage(bitmap))
                {

                    ColorMatrix matrix = new ColorMatrix();
                    //set the opacity
                    matrix.Matrix33 = 0.3f;
                    //create image attributes
                    ImageAttributes attributes = new ImageAttributes();
                    //set the color(opacity) of the image
                    attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    g.DrawImage(image, 0, 0, image.Width, image.Height);

                    g.DrawImage(waterMark, waterMarkTopCenter, 0, 0, waterMark.Width, waterMark.Height, GraphicsUnit.Pixel, attributes);
                    g.DrawImage(waterMark, waterMarkCenterBottom, 0, 0, waterMark.Width, waterMark.Height, GraphicsUnit.Pixel, attributes);
                    g.DrawImage(waterMark, waterMarkLeftCenter, 0, 0, waterMark.Width, waterMark.Height, GraphicsUnit.Pixel, attributes);
                    g.DrawImage(waterMark, waterMarkRightCenter, 0, 0, waterMark.Width, waterMark.Height, GraphicsUnit.Pixel, attributes);

                    g.DrawImage(waterMark, waterMarkCenter, 0, 0, waterMark.Width, waterMark.Height, GraphicsUnit.Pixel, attributes);
                    g.Save();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return bitmap;
        }

        private void MergeAndSave(string imagePath, string waterMarkPath, string savePath)
        {
            using (var image = Photo.LoadBitmapUnlocked(imagePath))
            {
                using (var waterMark = Photo.LoadBitmapUnlocked(waterMarkPath))
                {
                    using (var mergedImage = Merge(image, waterMark))
                    {
                        Photo.Save(mergedImage, savePath);
                    }
                }
            }
        }

        public async Task MergeAndSaveAsync(string imagePath, string waterMarkPath, string savePath)
        {
            await Task.Run(() =>
                {
                    MergeAndSave(imagePath, waterMarkPath, savePath);
                });
        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (cbSelectAll.IsChecked.Value)
            {
                lbBatchFileList.SelectAll();
            }
            else
            { lbBatchFileList.UnselectAll(); }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = (e.Source as TextBox).Text;
            var imageNames = text.Split(',');

            lbBatchFileList.Items.Filter = (item) => { return imageNames.Any(image => (item as Photo).Title.Contains(image)); };
        }

        private async void btnRotateImage_ClickAsync(object sender, RoutedEventArgs e)
        {
            spin.Visibility = Visibility.Visible;

            List<string> imagePathList = new List<string>();
            var selectedPhotos = lbBatchFileList.SelectedItems.Cast<Photo>();
            foreach (var photo in selectedPhotos)
            {
                photo.Image = null;
            }

            await Task.Run(() =>
            {
                Thread.Sleep(1000);
                foreach (Photo photo in selectedPhotos)
                {
                    Rotate(photo);
                }
            }).ContinueWith((result) =>
            {
                if (result.IsFaulted)
                    MessageBox.Show(result.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Dispatcher.Invoke(() => spin.Visibility = Visibility.Collapsed);
            });
        }

        private void Rotate(Photo photo)
        {
            using (var originalBmp = Photo.LoadBitmapUnlocked(photo.FileName))
            {
                originalBmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                Photo.Save(originalBmp, photo.FileName + ".temp");
            }
            File.Delete(photo.FileName);
            File.Move(photo.FileName + ".temp", photo.FileName);
            Dispatcher.Invoke(() => photo.Load());
        }
    }
}
