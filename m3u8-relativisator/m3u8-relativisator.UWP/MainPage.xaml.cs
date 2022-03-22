using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Xamarin.Essentials;

namespace m3u8_relativisator.UWP
{
    public class UWPPlatformSpecificCode : IPlatformSpecificCode
    {
        //UWP specific code for saving the file
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

    public sealed partial class MainPage
    {
        public MainPage()
        {
            m3u8_relativisator.App.Init(new UWPPlatformSpecificCode());

            this.InitializeComponent();

            LoadApplication(new m3u8_relativisator.App());
        }
    }
}
