using System;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas.Effects;

namespace MaterialDemo.Uwp.Brushes
{
    public sealed class BackdropTintBlurBrush : XamlCompositionBrushBase
    {
        protected override void OnConnected()
        {
            Compositor compositor = Window.Current.Compositor;

            // CompositionCapabilities: Are Effects supported?
            bool usingFallback = !CompositionCapabilities.GetForCurrentView().AreEffectsSupported();
            if (usingFallback)
            {
                // If Effects are not supported, use Fallback Solid Color
                CompositionBrush = compositor.CreateColorBrush(FallbackColor);
                return;
            }

            // Define Effect graph
            var graphicsEffect = new BlendEffect
            {
                Mode = BlendEffectMode.LinearBurn,
                Background = new ColorSourceEffect
                {
                    Name = "Tint",
                    Color = Windows.UI.Colors.Silver,
                },
                Foreground = new GaussianBlurEffect
                {
                    Name = "Blur",
                    Source = new CompositionEffectSourceParameter("Backdrop"),
                    BlurAmount = 0,
                    BorderMode = EffectBorderMode.Hard,
                }
            };

            // Create EffectFactory and EffectBrush
            CompositionEffectFactory effectFactory = compositor.CreateEffectFactory(graphicsEffect, new[] { "Blur.BlurAmount" });
            CompositionEffectBrush effectBrush = effectFactory.CreateBrush();

            // Create BackdropBrush
            CompositionBackdropBrush backdrop = compositor.CreateBackdropBrush();
            effectBrush.SetSourceParameter("backdrop", backdrop);

            // Trivial looping animation to demonstrate animated effects
            TimeSpan _duration = TimeSpan.FromSeconds(5);
            ScalarKeyFrameAnimation blurAnimation = compositor.CreateScalarKeyFrameAnimation();
            blurAnimation.InsertKeyFrame(0, 0);
            blurAnimation.InsertKeyFrame(0.5f, 10f);
            blurAnimation.InsertKeyFrame(1, 0);
            blurAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            blurAnimation.Duration = _duration;
            effectBrush.Properties.StartAnimation("Blur.BlurAmount", blurAnimation);

            // Set EffectBrush to paint Xaml UIElement
            CompositionBrush = effectBrush;
        }

        protected override void OnDisconnected()
        {
            // Dispose CompositionBrushes if XamlCompBrushBase is removed from tree
            CompositionBrush?.Dispose();
            CompositionBrush = null;
        }
    }
}
