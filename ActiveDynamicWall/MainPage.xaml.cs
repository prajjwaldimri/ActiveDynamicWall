using System;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.Background;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;

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

            InitWallpaper();

            this.InitializeComponent();
            AddImageOutput.Text = BackgroundWorkCost.CurrentBackgroundWorkCost.ToString();
        }

        // Code that will run when navigating to this page.
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await TryCreateTimeWallpaperFolder();
        }

        private async Task TryCreateTimeWallpaperFolder()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            await localFolder.CreateFolderAsync("TimeWallpaper", CreationCollisionOption.OpenIfExists);
        }

        private async void InitWallpaper()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            try
            {
                StorageFolder timeFolder = await localFolder.GetFolderAsync("TimeWallpaper");
                if (await ApplicationData.Current.LocalFolder.TryGetItemAsync("WallpaperTextfile.txt") != null)
                {
                    StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("WallpaperTextfile.txt");
                    string[] lines = (await FileIO.ReadTextAsync(file)).Split('\n');
                    for (int i = 0; i < lines.Length - 1; i++)
                    {
                        string[] pieces = lines[i].Split(':'); // time/name.png
                        string[] nums = pieces[0].Split(' '); // hour/min
                        wallpapers.Add(new Wallpaper(pieces[1],
                                new BitmapImage(new Uri(timeFolder.Path + "/" + pieces[1])),
                                int.Parse(nums[0]), int.Parse(nums[1])));
                        dictionary.Add(new Tuple<int, int>(int.Parse(nums[0]), int.Parse(nums[1])), pieces[1]);
                        numList.Add(i);

                    }
                }
            }
            catch (Exception)
            {

            }

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
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFolder timeFolder = await localFolder.GetFolderAsync("TimeWallpaper");

            //Pick an Image
            StorageFile fileName = await pickerWallpaper.PickSingleFileAsync();
            if (fileName != null)
            {
                //Check if the file does not exist
                if (await timeFolder.TryGetItemAsync(fileName.Name) != null)
                {
                    selectedImgName = fileName.Name;
                    await fileName.CopyAsync(timeFolder, selectedImgName, NameCollisionOption.ReplaceExisting);
                    //Preview the Image on the interface
                    selectImg.Source = new BitmapImage(new Uri(timeFolder.Path + "/" + selectedImgName));
                    AddImageOutput.Text = "Image Selected";
                }
                else if (dictionary.ContainsValue(fileName.Name))
                {
                    selectedImgName = "";
                    AddImageOutput.Text = "We already have this Image";
                }
                else
                {
                    //selectImg.Source = new BitmapImage(new Uri("ms-appx:///Assets/wallpaper.png"));
                    selectedImgName = fileName.Name;
                    /*var toDelete = await timeFolder.GetFilesAsync(selectedImgName);
                    await toDelete.DeleteAsync();*/
                    await fileName.CopyAsync(timeFolder, selectedImgName, NameCollisionOption.ReplaceExisting);
                    selectImg.Source = new BitmapImage(new Uri(timeFolder.Path + "/" + selectedImgName));
                }
            }
        }

        //add selected file to the List - w
        private async void AddFile(object sender, RoutedEventArgs e)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFolder TimeFolder = await localFolder.GetFolderAsync("TimeWallpaper");
            BitmapImage bitImageSource = (BitmapImage)selectImg.Source;
            if (bitImageSource != null)
            {
                if (!dictionary.ContainsKey(new Tuple<int, int>(timeSelectedForWallpaper.Time.Hours, timeSelectedForWallpaper.Time.Minutes)))
                {
                    //add to dictionary
                    dictionary.Add(new Tuple<int, int>(timeSelectedForWallpaper.Time.Hours, timeSelectedForWallpaper.Time.Minutes), selectedImgName);
                    //add to ObservableCollection
                    wallpapers.Add(new Wallpaper(selectedImgName, new BitmapImage(new Uri(TimeFolder.Path + "/" + selectedImgName)), timeSelectedForWallpaper.Time.Hours, timeSelectedForWallpaper.Time.Minutes));
                    AddImageOutput.Text = "The Image is in the Wallpaper collection";
                }
                else
                {
                    AddImageOutput.Text = "We already have this Image in the Wallpaper collection";
                }
            }
        }

        //Oops! this is to remove the last added image to the list
        private async void RemoveFile(object sender, RoutedEventArgs e)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFolder timeFolder = await localFolder.GetFolderAsync("TimeWallpaper");
            var name = wallpapers[wallpapers.Count - 1].fileName;
            wallpapers[wallpapers.Count - 1] = null;
            selectImg.Source = new BitmapImage(new Uri("ms-appx:///Assets/imgIconBackground.png"));
            wallpapers.RemoveAt(wallpapers.Count - 1);
            // remove from dictionary
            dictionary.Remove(dictionary.Keys.Last());
            numList.Remove(numList.Count - 1);
            if (name != null && await timeFolder.TryGetItemAsync(name) != null)
            {
                StorageFile toRemove = await timeFolder.GetFileAsync(name);
                await toRemove.DeleteAsync();
                AddImageOutput.Text = "Image Removed";
            }

        }

        //If we want to reset Everything and remove all files from TimeWallpaper folder
        private async void ResetApp(object sender, RoutedEventArgs e)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFolder timeFolder = await localFolder.GetFolderAsync("TimeWallpaper");

            var files = (await timeFolder.GetFilesAsync());
            foreach (var file in files)
            {
                await file.DeleteAsync(StorageDeleteOption.Default);
            }

            AddImageOutput.Text = "We have reset Everything!";
        }


        //Start Background Task
        private async void StartDynamicWall(object sender, RoutedEventArgs e)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFolder timeFolder = await localFolder.GetFolderAsync("TimeWallpaper");
            if (await timeFolder.TryGetItemAsync("wallpaperlistfile.txt") != null)
                await (await timeFolder.TryGetItemAsync("wallpaperlistfile.txt")).DeleteAsync();

            StorageFile wallpaperlistfile = await timeFolder.CreateFileAsync("wallsFile.txt", CreationCollisionOption.ReplaceExisting);
            List<string> wallsList = new List<string>();
            foreach (var item in dictionary)
            {
                // leaves with item1 (space) item2
                string[] hour_min = item.Key.ToString().TrimEnd(')').TrimStart('(').Split(',');
                wallsList.Add(hour_min[0] + hour_min[1] + ":" + item.Value);
            }
            await FileIO.WriteLinesAsync(wallpaperlistfile, wallsList);
            AddImageOutput.Text = "Requesting Background Access";
            RequestBackgroundAccess();
        }

        // CREATING TASK --------------------------------------------------
        private async void RequestBackgroundAccess()
        {
            var result = await BackgroundExecutionManager.RequestAccessAsync();
            AddImageOutput.Text = result.ToString();
            if (result != BackgroundAccessStatus.DeniedByUser)
            {
                RegisterBackgroundTask();
                AddImageOutput.Text = "We have Background Access. Registering the Background Task";
            }

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
            builder.TaskEntryPoint = "BackgroundTaskComponent.BackgroundClass";
            builder.SetTrigger(new TimeTrigger(15, false));
            builder.AddCondition(new SystemCondition(SystemConditionType.SessionConnected));
            BackgroundTaskRegistration task = builder.Register();
            AddImageOutput.Text = "Background Task Registered";
        }

        //Stop background Task
        private void StopBackgroundTask(object sender, RoutedEventArgs e)
        {
            foreach (var bgTask in BackgroundTaskRegistration.AllTasks)
                if (bgTask.Value.Name == "BackgroundTrigger")
                {
                    bgTask.Value.Unregister(true);
                    AddImageOutput.Text = "Background Task Stoped";
                }


        }



        //Settings to be applied to lockscreen and Desktop  - still needs work     
        private void ApplyToLockscreen(object sender, RoutedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["applyLockscreen"] = "checked";
        }
        private void NotApplyToLockscreen(object sender, RoutedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["applyLockscreen"] = "unchecked";
        }
        private void ApplyToDesktop(object sender, RoutedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["applyDesktop"] = "checked";
        }
        private void NotApplyToDesktop(object sender, RoutedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["applyDesktop"] = "unchecked";
        }


    }
}

