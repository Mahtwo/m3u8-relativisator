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
            //Specify options for saving as
            FileSavePicker savePicker = new FileSavePicker
            {
                //Default to the filename of the original file
                SuggestedFileName = fileName
            };
            //Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Playlist file", new List<string>() {".m3u8", ".m3u"});
            savePicker.FileTypeChoices.Add("All types", new List<string>() {"."});  //A wildcard throws an exception

            //Open the save file dialog
            StorageFile file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                //Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                CachedFileManager.DeferUpdates(file);

                //Copy 
                using (Stream fileStreamW = await file.OpenStreamForWriteAsync())
                {
                    using (Stream temporaryFileStream = new System.IO.FileStream(temporaryFilePath, FileMode.Open))
                    {
                        await temporaryFileStream.CopyToAsync(fileStreamW);
                    }
                }

                //Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                //Completing updates may require Windows to ask for user input.
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                if (status == FileUpdateStatus.Complete)
                {
                    return "File saved as \"" + file.Name + '"';
                }
                else
                {
                    return "Couldn't save the file";
                }
            }
            else
            {
                return "Saving cancelled";
            }
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
