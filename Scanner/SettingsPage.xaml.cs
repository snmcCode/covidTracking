using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Scanner
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();

            // Disable Back-Navigation
            NavigationPage.SetHasBackButton(this, false);
        }

        async void ScanButtonClickedAsync(System.Object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new ScannerPage());
        }
    }
}
