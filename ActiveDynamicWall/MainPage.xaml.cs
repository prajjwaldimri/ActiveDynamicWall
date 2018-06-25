using System;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Diagnostics;
using Windows.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.Background;
using System.Collections.Generic;
using System.Collections.ObjectModel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ActiveDynamicWall
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Dictionary<Tuple<int, int>, string> dictionary = new Dictionary<Tuple<int, int>, string>();
        private string selectedImgName;
        public ObservableCollection<Wallpaper> wallpapers = new ObservableCollection<Wallpaper>();
        public List<int> numList = new List<int>();
        public MainPage()
        {
            this.InitializeComponent();
            AddImageOutput.Text = BackgroundWorkCost.CurrentBackgroundWorkCost.ToString();
        }

        //Pickup Image file
        private async void FilePickerWallpaper(object sender, RoutedEventArgs e)
        {
            FileOpenPicker pickerWallpaper = new FileOpenPicker();
            pickerWallpaper.ViewMode = PickerViewMode.Thumbnail;
            pickerWallpaper.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            pickerWallpaper.FileTypeFilter.Add(".jpg");
            pickerWallpaper.FileTypeFilter.Add(".jpeg");
            pickerWallpaper.FileTypeFilter.Add(".png");

            //Get to the App local folder
            StorageFolder appFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            // Create a subfoloder
            String timeNameFile = "TimeWallpaper";
            StorageFolder timeFolder = await appFolder.CreateFolderAsync(timeNameFile, CreationCollisionOption.OpenIfExists);
            //Check if the folder was created
            if (await appFolder.TryGetItemAsync(timeNameFile) != null) Debug.WriteLine("Folder" + timeNameFile + "exist");
            else Debug.WriteLine("Folder" + timeNameFile + "does not exist");

            //Pick an Image
            StorageFile fileName = await pickerWallpaper.PickSingleFileAsync();
            if (fileName != null)
            {
                //Check if the file does not exist
                if (await timeFolder.TryGetItemAsync(fileName.Name) == null)
                {
                    string selectedImgName = fileName.Name;
                    await fileName.CopyAsync(timeFolder, selectedImgName, NameCollisionOption.ReplaceExisting);
                    //Preview the Image on the interface
                    selectImg.Source = new BitmapImage(new Uri("ms-appx:///TimeWallpaper/" + selectedImgName));
                }
                else
                {
                    selectImg.Source = new BitmapImage(new Uri("ms-appx:///Assets/wallpaper.png"));
                }
            }
        }

        //add selected file to the List - w
        private void AddFile00(object sender, RoutedEventArgs e)
        {
            BitmapImage bitImageSource = (BitmapImage)selectImg.Source;



        }

        //Oops! this is to remove the last added image to the list
        private void RemoveFile(object sender, RoutedEventArgs e)
        {



        }

        //If we want to reset Everything and remove all wallpapers
        private async void ResetApp(object sender, RoutedEventArgs e)
        {
            //Delete the wallpaper folder
            StorageFolder appFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            String timeNameFile = "TimeWallpaper";
            if (await appFolder.TryGetItemAsync(timeNameFile) != null) await (await appFolder.TryGetItemAsync(timeNameFile)).DeleteAsync();
            //Has it been Deleted?
            if (await appFolder.TryGetItemAsync(timeNameFile) != null) Debug.WriteLine("Folder" + timeNameFile + "Was not deleted");
            else Debug.WriteLine("Folder" + timeNameFile + "Was Deleted");
        }

        //Start Background Task
        private async void StartDynamicWall(object sender, RoutedEventArgs e)
        {



        }

        // REGISTER TASK
        private void RegisterBackgroundTask()
        {
            // Unregister old task
            foreach (var bgTask in BackgroundTaskRegistration.AllTasks)
                if (bgTask.Value.Name == "BackgroundTrigger")
                    bgTask.Value.Unregister(true);
            // Register new task
            BackgroundTaskBuilder builder = new BackgroundTaskBuilder();
            builder.Name = "BackgroundTrigger";
            builder.TaskEntryPoint = "BackgroundTaskComponent.BackgroundClass.cs";
            builder.SetTrigger(new TimeTrigger(15, false));
            builder.AddCondition(new SystemCondition(SystemConditionType.SessionConnected));
            BackgroundTaskRegistration task = builder.Register();
        }

        //Stop background Task
        private void StopBackgroundTask(object sender, RoutedEventArgs e)
        {
            foreach (var bgTask in BackgroundTaskRegistration.AllTasks)
                if (bgTask.Value.Name == "BackgroundTrigger")
                    bgTask.Value.Unregister(true);
        }






    }
}

