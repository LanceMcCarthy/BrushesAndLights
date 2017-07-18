using System;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Composition.Effects;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas.Effects;

namespace MaterialDemo.Uwp.Brushes
{
    public sealed class MaterialBrush : XamlCompositionBrushBase
    {
        private LoadedImageSurface _surface;
        private CompositionSurfaceBrush _normalMap;

        public static readonly DependencyProperty ImageUriStringProperty = DependencyProperty.Register(
            "ImageUri",
            typeof(string),
            typeof(MaterialBrush),
            new PropertyMetadata(string.Empty, OnImageUriStringChanged)
        );

        public string ImageUriString
        {
            get => (String)GetValue(ImageUriStringProperty);
            set => SetValue(ImageUriStringProperty, value);
        }

        private static void OnImageUriStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var materialBrush = (MaterialBrush)d;
            // Unbox and update surface if CompositionBrush exists     
            if (materialBrush._normalMap != null)
            {
                var newSurface = LoadedImageSurface.StartLoadFromUri(new Uri((String)e.NewValue));
                materialBrush._surface = newSurface;
                materialBrush._normalMap.Surface = newSurface;
            }
        }

        protected override void OnConnected()
        {
            if (DesignMode.DesignModeEnabled)
                return;

            Compositor compositor = Window.Current.Compositor;

            // CompositionCapabilities: Are Effects supported?
            bool usingFallback = !CompositionCapabilities.GetForCurrentView().AreEffectsSupported();
            FallbackColor = Color.FromArgb(100, 60, 60, 60);

            if (usingFallback)
            {
                // If Effects are not supported, use Fallback Solid Color
                CompositionBrush = compositor.CreateColorBrush(FallbackColor);
                return;
            }
            
            // BrokenGlass 512x384
            // Load NormalMap onto an ICompositionSurface using LoadedImageSurface
            _surface = LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/Brick_NormalMap.jpg"), new Size(512, 384));

            // Load Surface onto SurfaceBrush
            _normalMap = compositor.CreateSurfaceBrush(_surface);
            _normalMap.Stretch = CompositionStretch.Uniform;

            // Define Effect graph
            const float glassLightAmount = 0.5f;
            const float glassBlurAmount = 0.95f;
            Color tintColor = Color.FromArgb(255, 128, 128, 128);

            var graphicsEffect = new ArithmeticCompositeEffect()
            {
                Name = "LightComposite",
                Source1Amount = 1,
                Source2Amount = glassLightAmount,
                MultiplyAmount = 0,
                Source1 = new ArithmeticCompositeEffect()
                {
                    Name = "BlurComposite",
                    Source1Amount = 1 - glassBlurAmount,
                    Source2Amount = glassBlurAmount,
                    MultiplyAmount = 0,
                    Source1 = new ColorSourceEffect()
                    {
                        Name = "Tint",
                        Color = tintColor,
                    },
                    Source2 = new GaussianBlurEffect()
                    {
                        BlurAmount = 20,
                        Source = new CompositionEffectSourceParameter("Backdrop"),
                        Optimization = EffectOptimization.Balanced,
                        BorderMode = EffectBorderMode.Hard,
                    },
                },
                Source2 = new SceneLightingEffect()
                {
                    AmbientAmount = 0.15f,
                    DiffuseAmount = 1,
                    SpecularAmount = 0.1f,
                    NormalMapSource = new CompositionEffectSourceParameter("NormalMap")
                },
            };

            // Create EffectFactory and EffectBrush
            CompositionEffectFactory effectFactory = compositor.CreateEffectFactory(graphicsEffect);
            CompositionEffectBrush effectBrush = effectFactory.CreateBrush();

            // Create BackdropBrush
            CompositionBackdropBrush backdrop = compositor.CreateBackdropBrush();

            // Set Sources to Effect
            effectBrush.SetSourceParameter("NormalMap", _normalMap);
            effectBrush.SetSourceParameter("Backdrop", backdrop);

            // Set EffectBrush as the brush that XamlCompBrushBase paints onto Xaml UIElement
            CompositionBrush = effectBrush;
        }

        protected override void OnDisconnected()
        {
            // Dispose Surface and CompositionBrushes if XamlCompBrushBase is removed from tree
            _surface?.Dispose();
            _surface = null;

            CompositionBrush?.Dispose();
            CompositionBrush = null;
        }
    }
}
