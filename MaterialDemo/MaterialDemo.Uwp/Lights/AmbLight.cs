using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace MaterialDemo.Uwp.Lights
{
    public sealed class AmbLight : XamlLight
    {
        protected override void OnConnected(UIElement newElement)
        {
            Compositor compositor = Window.Current.Compositor;

            // Create AmbientLight and set its properties
            AmbientLight ambientLight = compositor.CreateAmbientLight();
            ambientLight.Color = Colors.White;

            // Associate CompositionLight with XamlLight
            CompositionLight = ambientLight;

            // Add UIElement to the Light's Targets
            AddTargetElement(GetId(), newElement);
        }

        protected override void OnDisconnected(UIElement oldElement)
        {
            // Dispose Light when it is removed from the tree
            RemoveTargetElement(GetId(), oldElement);
            CompositionLight.Dispose();
        }

        protected override string GetId()
        {
            return typeof(HoverLight).FullName;
        }
    }
}
