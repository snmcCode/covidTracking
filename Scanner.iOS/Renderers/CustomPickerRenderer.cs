using System;
using Scanner.iOS.Renderers;
using Scanner.Visuals;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Material.iOS;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Picker), typeof(CustomPickerRenderer), new[] { typeof(CustomVisual) })]
namespace Scanner.iOS.Renderers
{
    public class CustomPickerRenderer : MaterialPickerRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.Layer.BackgroundColor = UIColor.White.CGColor;
                Control.BackgroundColor = UIColor.White;
                Control.BorderStyle = UITextBorderStyle.RoundedRect;
                Control.Layer.BorderColor = Color.FromHex("#757575").ToCGColor();
                Control.Layer.BorderWidth = 2;
            }

            e.NewElement.Unfocused += (sender, evt) =>
            {
                Control.Layer.BorderColor = Color.FromHex("#757575").ToCGColor();
                Control.Layer.BorderWidth = 2;
            };
            e.NewElement.Focused += (sender, evt) =>
            {
                Control.Layer.BorderColor = Color.FromHex("#448AFF").ToCGColor();
                Control.Layer.BorderWidth = 4;
            };
        }
    }
}
