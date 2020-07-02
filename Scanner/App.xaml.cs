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

            ContentPage contentPage = null;

            if (Application.Current.Properties.ContainsKey("IsVerified"))
            {
                bool? IsVerified = Application.Current.Properties["IsVerified"] as bool?;

                if (IsVerified != null && IsVerified == true)
                {
                    contentPage = new SettingsPage();
                }
            }
            else
            {
                contentPage = new LoginPage();
            }

            MainPage = new NavigationPage(contentPage);
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
