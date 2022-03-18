using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        /// <summary>
        /// Contain the path prefix if needed, like "file:/"
        /// </summary>
        private string pathPrefix;

        /// <summary>
        /// Contain the original path that will be replaced
        /// </summary>
        private string originalPath;

        /// <summary>
        /// Currently selected file
        /// </summary>
        private FileResult selectedFile;

        public MainPage()
        {
            InitializeComponent();

            double fontSize = (Device.RuntimePlatform == Device.Android) ? 15 : Device.GetNamedSize(NamedSize.Medium, typeof(Button));
            button_selectFile.FontSize = fontSize;
            button_validate.FontSize = fontSize;
            button_quit.FontSize = fontSize;
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
            selectedFile = await SelectM3u8File();

            if (selectedFile != null)
            {
                button_selectFile.Text = "Reselect a file";
                label_selectFileError.Text = $"\"{selectedFile.FileName}\" currently selected";

                pathPrefix = "";
                List<string[]> allDifferentPaths = new List<string[]>();
                //"using" take care of closing the stream when exiting out of the brackets
                using (Stream fileStream = await selectedFile.OpenReadAsync())
                {
                    using (StreamReader fileStreamR = new StreamReader(fileStream))
                    {
                        while (!fileStreamR.EndOfStream)
                        {
                            //Trim to remove white-space characters at the start and end
                            string currentLine = fileStreamR.ReadLine().Trim();

                            //The last part of a path correspond to the file, so isn't needed
                            int indexLastSlash = -1;
                            //Check if the current line is a path
                            int pathType = 0;
                            if (currentLine[0] == '/')
                            {
                                //Absolute Linux path "/..."
                                pathType = 1;

                                indexLastSlash = currentLine.LastIndexOf('/');
                            }
                            else if (currentLine.Contains('/'))
                            {
                                //Relative Linux path "<folder>/..."
                                pathType = 2;

                                indexLastSlash = currentLine.LastIndexOf('/');
                            }
                            else if (char.IsLetter(currentLine[0]) && currentLine[1] == ':' && currentLine[2] == '\\')
                            {
                                //Absolute Windows path "<letter>:\..."
                                pathType = 3;

                                indexLastSlash = currentLine.LastIndexOf('\\');
                            }
                            else if (currentLine.Contains('\\'))
                            {
                                //Relative Windows path "<folder>\..."
                                pathType = 4;

                                indexLastSlash = currentLine.LastIndexOf('\\');
                            }
                            else if (currentLine.Substring(0, Math.Min(6, currentLine.Length)).ToLower() == "file:/")
                            {
                                //URI file path "<lowercase>file:/</lowercase>..."
                                //All possibilities have to be taken into account (https://superuser.com/a/479262)
                                if (currentLine[6] == '/')
                                {
                                    //Three slashes prefix "file://<maybe something>/..."
                                    pathType = 5;
                                }
                                else
                                {
                                    //One slash prefix "file:/..."
                                    pathType = 6;
                                }

                                indexLastSlash = currentLine.LastIndexOf('/');
                            }
                            else
                            {
                                //Not a file path, skip back to the start of the while
                                goto SkipWhileToNextIteration;
                            }

                            //Separate the path into multiple strings depending on pathType
                            string[] currentPath = new string[0]; 
                            switch (pathType)
                            {
                                case 1:
                                case 2:
                                    currentPath = currentLine.Remove(indexLastSlash).Split('/');
                                    break;
                                case 3:
                                case 4:
                                    currentPath = currentLine.Remove(indexLastSlash).Split('\\');
                                    break;
                                case 5:
                                    int indexThirdPrefixSlash = currentLine.Substring(7).IndexOf('/');

                                    //Setting the value every time is faster than checking if a value is set
                                    pathPrefix = currentLine.Substring(0, indexThirdPrefixSlash + 1);

                                    currentPath = currentLine.Substring(indexThirdPrefixSlash, indexLastSlash - indexThirdPrefixSlash).Split('/');
                                    break;
                                case 6:
                                    const int indexPrefixSlash = 5;

                                    //Setting the value every time is faster than checking if a value is set
                                    pathPrefix = "file:/";

                                    currentPath = currentLine.Substring(indexPrefixSlash, indexLastSlash - indexPrefixSlash).Split('/');
                                    break;
                            }

                            //Re-add the slashes as Split don't keep the delimiter
                            if (pathType == 3 || pathType == 4)
                            {
                                //Backward slash
                                for (int i = 0; i < currentPath.Length; i++)
                                {
                                    currentPath[i] += '\\';
                                }
                            }
                            else
                            {
                                //Forward slash
                                for (int i = 0; i < currentPath.Length; i++)
                                {
                                    currentPath[i] += '/';
                                }
                            }

                            //Check whether currentPath is already in allDifferentPaths
                            foreach (string[] differentPath in allDifferentPaths)
                            {
                                int currentPathLength = currentPath.Length;

                                if (currentPathLength == differentPath.Length)
                                {
                                    if (Enumerable.SequenceEqual(currentPath, differentPath))
                                    {
                                        //Path already present, skip back to the start of the while
                                        goto SkipWhileToNextIteration;
                                    }
                                }
                            }
                            allDifferentPaths.Add(currentPath);

                        SkipWhileToNextIteration:
                            continue;
                        }
                    }
                }

                List<string> pathsList = new List<string>();

                //Get the id of a path in allDifferentPaths with the highest number of folders
                int idLongestPath = -1;
                int longestLength = 0;
                for (int i = 0; i < allDifferentPaths.Count; i++)
                {
                    int currentLength = allDifferentPaths[i].Length;
                    if (currentLength > longestLength)
                    {
                        longestLength = currentLength;
                        idLongestPath = i;
                    }
                }

                //Going left to right, add each folder that is present in every path
                for (int i = 0; i < longestLength; i++)
                {
                    string folderToAdd = allDifferentPaths[idLongestPath][i];

                    //Stop adding folders as soon as one folder isn't in every path
                    for (int j = 0; j < allDifferentPaths.Count; j++)
                    {
                        if (allDifferentPaths[j][i] != folderToAdd)
                        {
                            goto StopAddingFolders;
                        }
                    }

                    pathsList.Add(folderToAdd);
                }
            StopAddingFolders:
                pathsList.Add("");  //Add empty path

                //Set originalPath value
                originalPath = "";
                foreach (string path in pathsList)
                {
                    originalPath += path;
                }

                paths = pathsList.Skip(1).ToArray();  //Skip the first element as it would allow replacing the original path to itself

                int maximum = paths.Length - 1;
                if (maximum > 0)
                {
                    slider_path.Maximum = maximum;  //Maximum must be greater than 0, because Minimum is 0
                    slider_path.Value = 0;
                    slider_path.IsEnabled = true;

                    label_sliderPath.Text = GetChoosenPath();

                    button_validate.IsVisible = true;
                }
                else
                {
                    //The file doesn't contain any modifiable path
                    slider_path.Value = 0;
                    slider_path.IsEnabled = false;

                    label_selectFileError.Text += ", but doesn't contain any modifiable path";
                    label_sliderPath.Text = "";

                    button_validate.IsVisible = false;
                }
            }
            else
            {
                //The selected file returned was null
                slider_path.Value = 0;
                slider_path.IsEnabled = false;

                label_selectFileError.Text = "An error occured during the selection of a file";
                label_sliderPath.Text = "";
                paths = new string[0];
                button_selectFile.Text = "Select a file";

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
            string choosenPath = pathPrefix + originalPath + "... → " + pathPrefix;
            for (int i = Convert.ToInt32(slider_path.Value); i < paths.Length; i++)
            {
                choosenPath += paths[i];
            }
            choosenPath += "...";

            return choosenPath;
        }

        /// <summary>
        /// Let the user select a m3u(8) file and returns it
        /// </summary>
        /// <returns>selected file, or null</returns>
        private async Task<FileResult> SelectM3u8File()
        {
            //Specify selectable files
            PickOptions options = new PickOptions
            {
                PickerTitle = "Select a m3u(8) file"
            };
            FilePickerFileType customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] {"audio/x-mpegurl"} },  //Android want MIME
                    { DevicePlatform.UWP, new[] {".m3u", ".m3u8"} },  //UWP want extensions
                }
            );
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
