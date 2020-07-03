using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Scanner
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();

            // Disable Back-Navigation
            NavigationPage.SetHasBackButton(this, false);
        }

        async void LoginButtonClickedAsync(System.Object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }
    }
}
