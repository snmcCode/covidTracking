using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace Scanner.Droid
{
    [Activity(Label = "Scanner", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {

        private string[] Permissions = new string[]
        {
            Android.Manifest.Permission.Camera,
            Android.Manifest.Permission.Flashlight,
            Android.Manifest.Permission.Internet,
            Android.Manifest.Permission.AccessNetworkState,
            Android.Manifest.Permission.ReadExternalStorage,
            Android.Manifest.Permission.WriteExternalStorage
        };

        private const int RequestCode = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            // Ask for Permissions
            RequestPermissions(Permissions, RequestCode);

            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            ZXing.Net.Mobile.Forms.Android.Platform.Init();

            LoadApplication(new App());
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            // global::ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == RequestCode)
            {
                if (IsPermissionsGranted(grantResults))
                {
                    Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                }
                else
                {
                    Finish();
                }
            }
        }

        private bool IsPermissionsGranted(Permission[] grantResults)
        {
            bool isAllPermissionsGranted = false;
            if (grantResults.Length > 0)
            {
                isAllPermissionsGranted = true;
                for (int i = 0; i < grantResults.Length; i++)
                {
                    if (grantResults[i] == Permission.Denied)
                    {
                        isAllPermissionsGranted = false;
                    }
                }
            }
            return isAllPermissionsGranted;
        }
    }
}