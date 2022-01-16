using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace m3u8_relativisator
{
    public partial class MainPage : ContentPage
    {
        private string[] paths;

        public MainPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Let the user choose a m3u(8) file and use it to initialize the program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectFile(object sender, EventArgs e)
        {
            //TODO Let user choose a file

            const string filename = "[filename here]";  //TODO Replace with actual filename
            button_selectFile.Text = $"\"{filename}\" currently selected";

            paths = new string[] { "Ex/", "am/", "pl/", "e/", "filenames.ext" };  //TODO Replace with all the possible paths after getting it from the selected file

            slider_path.Maximum = paths.Length - 1;  //Minimum is 0
            slider_path.Value = 0;
            slider_path.IsEnabled = true;

            label_sliderPath.Text = GetChoosenPath();
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
    }
}
