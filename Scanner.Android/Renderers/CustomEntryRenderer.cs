using System;
using Android.Content;
using Android.Graphics.Drawables;
using Scanner.Visuals;
using Scanner.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Material.Android;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Entry), typeof(CustomEntryRenderer), new[] { typeof(CustomVisual) })]
namespace Scanner.Droid.Renderers
{
    public class CustomEntryRenderer : MaterialEntryRenderer
    {
        GradientDrawable UnfocusedBackground = new GradientDrawable();
        GradientDrawable FocusedBackground = new GradientDrawable();

        public CustomEntryRenderer(Context context) : base(context)
        {
            GenerateBackgrounds();
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                if (Control != null)
                {
                    Control.EditText.Background = null;
                    Control.EditText.SetBackgroundColor(Android.Graphics.Color.Transparent);
                    Control.SetBackground(UnfocusedBackground);
                }

                e.NewElement.Unfocused += (sender, evt) =>
                {
                    Control.SetBackground(UnfocusedBackground);
                };
                e.NewElement.Focused += (sender, evt) =>
                {
                    Control.SetBackground(FocusedBackground);
                };
            }

            if (e.OldElement != null)
            {

            }

        }

        private void GenerateBackgrounds()
        {
            UnfocusedBackground.SetShape(ShapeType.Rectangle);
            UnfocusedBackground.SetColor(Resources.GetColor(Resource.Color.launcher_background, null));
            UnfocusedBackground.SetCornerRadii(new float[] { 10, 10, 10, 10, 10, 10, 10, 10 });
            UnfocusedBackground.SetStroke(2, Resources.GetColor(Resource.Color.colorTextSecondary, null));

            FocusedBackground.SetShape(ShapeType.Rectangle);
            FocusedBackground.SetColor(Resources.GetColor(Resource.Color.launcher_background, null));
            FocusedBackground.SetCornerRadii(new float[] { 10, 10, 10, 10, 10, 10, 10, 10 });
            FocusedBackground.SetStroke(4, Resources.GetColor(Resource.Color.colorAccent, null));
        }
    }
}
