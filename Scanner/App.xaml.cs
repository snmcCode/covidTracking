using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Scanner
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            SettingsPage SettingsPage = new SettingsPage();
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
