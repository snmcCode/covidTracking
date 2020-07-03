using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Scanner
{
    public partial class ScannerPage : ContentPage
    {
        public ScannerPage()
        {
            InitializeComponent();

            // Disable Back-Navigation and Hide Navigation Bar
            NavigationPage.SetHasBackButton(this, false);
            NavigationPage.SetHasNavigationBar(this, false);
        }

        void Handle_OnScanResult(ZXing.Result result)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Scanned Result", "VisitorID: " + result.Text, "OK");
            });
        }
    }
}
