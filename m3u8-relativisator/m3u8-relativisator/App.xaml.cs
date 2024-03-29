﻿using Xamarin.Forms;

namespace m3u8_relativisator
{
    public partial class App : Application
    {
        public static IPlatformSpecificCode PlatformSpecificCode { get; private set; }

        public static void Init(IPlatformSpecificCode platformSpecificCodeImpl)
        {
            App.PlatformSpecificCode = platformSpecificCodeImpl;
        }

        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
