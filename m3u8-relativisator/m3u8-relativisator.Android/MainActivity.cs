using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Xamarin.Essentials;
using System.Threading.Tasks;

namespace m3u8_relativisator.Droid
{
    public class AndroidPlatformSpecificCode : IPlatformSpecificCode
    {
        //Android specific code for saving the file
        public async Task<string> SaveAs(string temporaryFilePath, string fileName, string originalFilePath)
        {
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = fileName,
                File = new ShareFile(temporaryFilePath)
            });

            return null;
        }
    }

    [Activity(Label = "m3u8 relativisator", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            App.Init(new AndroidPlatformSpecificCode());

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}