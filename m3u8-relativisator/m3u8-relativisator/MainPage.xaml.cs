using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace m3u8_relativisator
{
    public partial class MainPage : ContentPage
    {
        /// <summary>
        /// Contain all the folders of the path in order
        /// </summary>
        private string[] paths;

        public MainPage()
        {
            InitializeComponent();
            WhenLoadingFinished();
        }

        /// <summary>
        /// Method waiting for the page to finish loading before executing some code
        /// </summary>
        private async void WhenLoadingFinished()
        {
            while (button_validate.Width == -1)
            {
                await Task.Delay(10);
            }

            //For some reason, setting IsVisible back to true doesn't work if it's set to false before the window loaded
            button_validate.IsVisible = false;
        }

        /// <summary>
        /// Let the user choose a m3u(8) file and use it to initialize the program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SelectFile(object sender, EventArgs e)
        {
            FileResult fileUri = await SelectM3u8File();

            if (fileUri != null)
            {
                button_selectFile.Text = $"\"{fileUri.FileName}\" currently selected";
                label_selectFileError.Text = "";  //Empty the label as it may have been used

                Stream filecontent = await fileUri.OpenReadAsync();

                paths = new string[] { "Ex/", "am/", "pl/", "e/", "filenames.ext" };  //TODO Replace with all the possible paths after reading the selected file

                slider_path.Maximum = paths.Length - 1;  //Minimum is 0
                slider_path.Value = 0;
                slider_path.IsEnabled = true;

                label_sliderPath.Text = GetChoosenPath();

                button_validate.IsVisible = true;
            }
            else
            {
                //The selected file returned was null
                button_selectFile.Text = "Select a file";
                label_selectFileError.Text = "An error occured during the selection of a file";
                label_sliderPath.Text = "";
                paths = new string[0];
                slider_path.Value = 0;
                slider_path.IsEnabled = false;
                button_validate.IsVisible = false;
            }
        }

        /// <summary>
        /// Refresh the displayed choosen path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChoosenPathChanged(object sender, ValueChangedEventArgs e)
        {
            slider_path.Value = Math.Round(slider_path.Value);

            label_sliderPath.Text = GetChoosenPath();
        }

        /// <summary>
        /// Get the choosen path by using the slider value
        /// </summary>
        /// <returns>choosen path</returns>
        private string GetChoosenPath()
        {
            string choosenPath = "";
            for (int i = Convert.ToInt32(slider_path.Value); i < paths.Length; i++)
            {
                choosenPath += paths[i];
            }

            return choosenPath;
        }

        /// <summary>
        /// Let the user select a m3u(8) file and returns it
        /// </summary>
        /// <returns>selected file, or null</returns>
        private async Task<FileResult> SelectM3u8File()
        {
            //Specify selectable files
            PickOptions options = new PickOptions();
            options.PickerTitle = "Select a m3u(8) file";
            FilePickerFileType customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] {"audio/x-mpegurl"} },  //Android want MIME
                { DevicePlatform.UWP, new[] {".m3u", ".m3u8"} },  //UWP want extensions
            });
            options.FileTypes = customFileType;

            //Ask the user for a m3u(8) file
            return await FilePicker.PickAsync(options);
        }

        /// <summary>
        /// Quit the program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Quit(object sender, EventArgs e)
        {
            //TODO Close file stream if file opened
            Environment.Exit(0);
        }

        /// <summary>
        /// Apply the choosen path to the selected file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Validate(object sender, EventArgs e)
        {
            //TODO implement this
            Console.WriteLine("method \"Validate\" called, but not implemented");
        }
    }
}
