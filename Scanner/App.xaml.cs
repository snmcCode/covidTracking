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

            // Initialize Pages
            LoginPage loginPage = new LoginPage();
            SettingsPage settingsPage = new SettingsPage();

            // MainPage Logic
            if (Application.Current.Properties.ContainsKey("IsVerified"))
            {
                bool? IsVerified = Application.Current.Properties["IsVerified"] as bool?;

                if (IsVerified != null && IsVerified == true)
                {
                    MainPage = new NavigationPage(settingsPage);
                }
            }
            else
            {
                MainPage = new NavigationPage(loginPage);
            }
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
