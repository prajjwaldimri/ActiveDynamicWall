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

            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("wallsFile.txt");
            string[] lines = (await FileIO.ReadTextAsync(file)).Split('\n');
            int i = (int)ApplicationData.Current.LocalSettings.Values["wallIndex"];
            string[] pieces = lines[i].Split(':'); // time/name.png
            string[] nums = pieces[0].Split(' '); // hour/min
            //System.Diagnostics.Debug.WriteLine(nums[0] + " " + nums[1]);
            //System.Diagnostics.Debug.WriteLine(hourMin.Item1 + " " + hourMin.Item2);

            // if current time is time in the current wallpaper
            if (DateTime.Now.Hour >= int.Parse(nums[0]) && DateTime.Now.Minute >= int.Parse(nums[1]))
            {
                // change wallpaper
                await SetWallpaperAsync(pieces[1]);
                if (i == lines.Length - 2) i = -1;
                i++;
                ApplicationData.Current.LocalSettings.Values["wallIndex"] = i;
            }
            _deferral.Complete();
        }

        // Change wallpaper
        // Pass in a relative path to a file inside the assets folder
        async Task<bool> SetWallpaperAsync(string assetsFileName)
        {
            if (UserProfilePersonalizationSettings.IsSupported())
            {
                var uri = new Uri("ms-appx:///local/TimeWallpaper/" + assetsFileName);
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(uri);
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
