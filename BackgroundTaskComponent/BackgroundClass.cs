using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.System.UserProfile;
using Windows.UI.Notifications;

namespace BackgroundTaskComponent
{
    public sealed class BackgroundClass : IBackgroundTask
    {
        BackgroundTaskDeferral deferral;
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            SendToast("Active Dynamic Wallpaper is now running in the background");
        }
        public static void SendToast(string message)
        {
            var template = ToastTemplateType.ToastText01;
            var xml = ToastNotificationManager.GetTemplateContent(template);
            var elements = xml.GetElementsByTagName("Test");
            var text = xml.CreateTextNode(message);
            elements[0].AppendChild(text);
            var toast = new ToastNotification(xml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
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
