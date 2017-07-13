using Windows.ApplicationModel;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas.Effects;

namespace MaterialDemo.Uwp.Brushes
{
    public class InvertBrush : XamlCompositionBrushBase
    {
        protected override void OnConnected()
        {
            if (DesignMode.DesignModeEnabled)
                return;

            if (CompositionBrush == null)
            {
                // 1 - Get the BackdropBrush, this gets the pixels behind the UI element 
                var backdrop = Window.Current.Compositor.CreateBackdropBrush();

                // CompositionCapabilities: Are Tint+Temperature and Saturation supported?
                bool usingFallback = !CompositionCapabilities.GetForCurrentView().AreEffectsSupported();
                if (usingFallback)
                {
                    // If Effects are not supported, Fallback to image without effects
                    CompositionBrush = backdrop;
                    return;
                }

                // 2 - Create your Effect 
                // New-up a Win2D InvertEffect and use the BackdropBrush as its Source 
                // Note – To use InvertEffect, you'll need to add the Win2D NuGet package to your project (search NuGet for "Win2D.uwp" 
                var invertEffect = new InvertEffect
                {
                    Source = new CompositionEffectSourceParameter("backdrop")
                };

                // 3 - Set up the EffectFactory 
                var effectFactory = Window.Current.Compositor.CreateEffectFactory(invertEffect);

                // 4 - Finally, instantiate the CompositionEffectBrush 
                var effectBrush = effectFactory.CreateBrush();

                // and set the backdrop as the original source  
                effectBrush.SetSourceParameter("backdrop", backdrop);

                // 5 - Finally, assign your CompositionEffectBrush to the XCBB's CompositionBrush property 
                CompositionBrush = effectBrush;
            }
        }

        protected override void OnDisconnected()
        {
            // Clean up 
            CompositionBrush?.Dispose();
            CompositionBrush = null;
        }
    }
}
