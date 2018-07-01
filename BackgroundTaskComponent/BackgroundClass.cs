using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.System.UserProfile;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;

namespace BackgroundTaskComponent
{
    public sealed class BackgroundClass : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral; // Used when you have async code in the Run Method
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            SendToast("Active Dynamic Wallpaper is now running in the background");

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFolder timeFolder = await localFolder.GetFolderAsync("TimeWallpaper");

            StorageFile file = await timeFolder.GetFileAsync("wallsFile.txt");
            string[] lines = (await FileIO.ReadTextAsync(file)).Split('\n');

            
            // Local Settings values are always null by default. Make sure you check if
            // they are null before using them or give them a value when the
            // app starts for the first time.

            int i = 0;
            if (ApplicationData.Current.LocalSettings.Values["wallIndex"] != null)
            {
               i = (int)ApplicationData.Current.LocalSettings.Values["wallIndex"];
            }

             
            string[] pieces = lines[i].Split(':'); // time/name.png
            string[] nums = pieces[0].Split(' '); // hour/min
            //System.Diagnostics.Debug.WriteLine(nums[0] + " " + nums[1]);
            //System.Diagnostics.Debug.WriteLine(hourMin.Item1 + " " + hourMin.Item2);

            int hours = int.Parse(nums[0]);
            int minutes = int.Parse(nums[1]);

            // if current time is time in the current wallpaper
            if (CheckIfTimeForWallpaperChange(hours,minutes))
            {
                // change wallpaper
                await SetWallpaperAsync(pieces[1]);
                if (i == lines.Length - 2) i = -1;
                i++;
                ApplicationData.Current.LocalSettings.Values["wallIndex"] = i;
            }
            _deferral.Complete();

        }

        // Properly determines if the it is time for the wallpaper to change
        private bool CheckIfTimeForWallpaperChange(int hours, int minutes)
        {
            bool timeForChange = false;
            if (DateTime.Now.Hour > hours)
            {
                timeForChange = true; 
            }

            else if (DateTime.Now.Hour == hours && DateTime.Now.Minute >= minutes)
            {
                timeForChange = true;
            }

            return timeForChange;
        }

        // Change wallpaper
        // Pass in a relative path to a file inside the assets folder
        async Task<bool> SetWallpaperAsync(string assetsFileName)
        {
            if (UserProfilePersonalizationSettings.IsSupported())
            {
                // There is a space due to the formatting of the file at the end of some of the lines.
                // This is represented as "\r".
                // Need to remove it so that timeFolder.GetFileAsync() can work correctly.

                if (assetsFileName.Contains("\r"))
                {
                    assetsFileName = assetsFileName.Replace("\r", "");
                };
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFolder timeFolder = await localFolder.GetFolderAsync("TimeWallpaper");
                StorageFile file = await timeFolder.GetFileAsync(assetsFileName);
                UserProfilePersonalizationSettings profileSettings = UserProfilePersonalizationSettings.Current;
                return await profileSettings.TrySetWallpaperImageAsync(file);

            }
            return false;
        }

        //SendToast msg
        public static void SendToast(string message)
        {
            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = message
                                }
                            }
                    }
                }
            };

            // Create the toast notification
            var toastNotif = new ToastNotification(toastContent.GetXml());

            // And send the notification
            ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
        }
        /*
        async Task<bool> SetWallpaperAsync(string assetsFileName)
        {
            if (UserProfilePersonalizationSettings.IsSupported())
            {
                var uri = new Uri("ms-appx:///TimeWallpaper/" + assetsFileName);
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(uri);
                UserProfilePersonalizationSettings profileSettings = UserProfilePersonalizationSettings.Current;
                return await profileSettings.TrySetWallpaperImageAsync(file);
            }
            return false;
        }
        */
    }
}
