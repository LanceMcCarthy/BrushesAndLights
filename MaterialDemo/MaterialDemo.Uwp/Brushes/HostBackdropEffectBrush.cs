using System;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas.Effects;

namespace MaterialDemo.Uwp.Brushes
{
    public sealed class HostBackdropEffectBrush : XamlCompositionBrushBase
    {
        ColorKeyFrameAnimation _stateChangeAnimation;

        public static readonly DependencyProperty OverlayColorProperty = DependencyProperty.Register(
            "OverlayColor",
            typeof(Color),
            typeof(HostBackdropEffectBrush),
            new PropertyMetadata(Colors.Transparent, OnOverlayColorChanged)
        );

        public Color OverlayColor
        {
            get => (Color)GetValue(OverlayColorProperty);
            set => SetValue(OverlayColorProperty, value);
        }

        private static void OnOverlayColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var brush = (HostBackdropEffectBrush)d;
            // Unbox and set a new OverlayColor if the CompositionBrush exists
            brush.CompositionBrush?.Properties.InsertColor("OverlayColor.Color", (Color)e.NewValue);
        }

        protected override void OnConnected()
        {
            Compositor compositor = Window.Current.Compositor;

            // CompositionCapabilities: Are HostBackdrop Effects supported?
            bool usingFallback = !CompositionCapabilities.GetForCurrentView().AreEffectsFast();
            if (usingFallback)
            {
                // If Effects are not supported, use Fallback Solid Color
                CompositionBrush = compositor.CreateColorBrush(FallbackColor);
                return;
            }

            // Define Effect graph
            var graphicsEffect = new BlendEffect
            {
                Mode = BlendEffectMode.Overlay,
                Background = new CompositionEffectSourceParameter("Backdrop"),
                Foreground = new ColorSourceEffect
                {
                    Name = "OverlayColor",
                    Color = OverlayColor,
                },
            };

            // Create EffectFactory and EffectBrush
            CompositionEffectFactory effectFactory = compositor.CreateEffectFactory(graphicsEffect, new[] { "OverlayColor.Color" });
            CompositionEffectBrush effectBrush = effectFactory.CreateBrush();

            // Create HostBackdropBrush, a kind of BackdropBrush that provides blurred backdrop content
            CompositionBackdropBrush hostBrush = compositor.CreateHostBackdropBrush();
            effectBrush.SetSourceParameter("Backdrop", hostBrush);

            // Set EffectBrush as the brush that XamlCompBrushBase paints onto Xaml UIElement
            CompositionBrush = effectBrush;

            // When the Window loses focus, animate HostBackdrop to FallbackColor
            Window.Current.CoreWindow.Activated += CoreWindow_Activated;

            // Configure color animation to for state change
            _stateChangeAnimation = compositor.CreateColorKeyFrameAnimation();
            _stateChangeAnimation.InsertKeyFrame(0, OverlayColor);
            _stateChangeAnimation.InsertKeyFrame(1, FallbackColor);
            _stateChangeAnimation.Duration = TimeSpan.FromSeconds(1);

        }

        private void CoreWindow_Activated(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.WindowActivatedEventArgs args)
        {
            // Change animation direction depending on Window state and animate OverlayColor
            if (args.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
            {
                _stateChangeAnimation.Direction = AnimationDirection.Normal;
            }
            else
            {
                _stateChangeAnimation.Direction = AnimationDirection.Reverse;
            }
            CompositionBrush?.Properties.StartAnimation("OverlayColor.Color", _stateChangeAnimation);
        }

        protected override void OnDisconnected()
        {
            // Dispose CompositionBrushes if XamlCompBrushBase is removed from tree
            CompositionBrush?.Dispose();
            CompositionBrush = null;
        }
    }
}
