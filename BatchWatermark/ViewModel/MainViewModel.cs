using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BatchWatermark.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        Uri waterMarkUri = new Uri(@"C:\Users\kaush\Pictures\shoroshi.jpg");
        string batchUri = @"E:\Photos\NikonW300\Majorca2018\2018_07_11";
        System.Drawing.Image waterMarkImage;

        public ObservableCollection<Photo> Photos { get; }
        = new ObservableCollection<Photo>();

        public RelayCommand OpenWaterMarkFile => new RelayCommand(new System.Action(btnOpenWaterMarkFile_Click));
        public RelayCommand OpenBatchFileListAsync => new RelayCommand(new System.Action(btnOpenBatchFileList_ClickAsync));

        public Image WaterMarkImage { get => waterMarkImage; set => Set(ref waterMarkImage, value); }
        public Uri WaterMarkUri { get => waterMarkUri; set => Set(ref waterMarkUri, value); }

        private void btnOpenWaterMarkFile_Click()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                WaterMarkUri = new Uri(openFileDialog.FileName);
                WaterMarkImage = System.Drawing.Image.FromFile(WaterMarkUri.AbsolutePath);                
            }
        }

        private async void btnOpenBatchFileList_ClickAsync()
        {
            Photos.Clear();
            batchUri = openFolderDialog();
            if (String.IsNullOrEmpty(batchUri)) return;

            var imagesFromFolder = Directory.GetFiles(batchUri, "*.jpg", SearchOption.AllDirectories).ToList();
            //var listImages = new ObservableCollection<ImageWrapper>();
            //lbBatchFileList.ItemsSource = Photos;

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
                System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => photo.Load()));
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

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
            }
            else
            {

            }
        }
    }
}