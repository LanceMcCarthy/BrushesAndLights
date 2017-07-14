using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace MaterialDemo.Uwp.Lights
{
    public sealed class CobblestoneLight : XamlLight
    {
        private ExpressionAnimation _lightPositionExpression;
        private Vector3KeyFrameAnimation _offsetAnimation;

        protected override void OnConnected(UIElement targetElement)
        {
            if (targetElement == null || DesignMode.DesignModeEnabled)
                return;

            Compositor compositor = Window.Current.Compositor;

            // Create SpotLight and set its properties
            SpotLight spotLight = compositor.CreateSpotLight();
            spotLight.Offset = new Vector3(348.5f,263f,100f);
            spotLight.InnerConeColor = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xF2);
            spotLight.InnerConeAngleInDegrees = 21f;
            spotLight.InnerConeAngle = 0.3665192f;
            spotLight.OuterConeAngleInDegrees = 58;
            spotLight.OuterConeAngle = 1.012291f;
            spotLight.OuterConeColor = Color.FromArgb(0xFF, 0xFD, 0xFF, 0xF2);
            spotLight.LinearAttenuation = 0f;
            spotLight.ConstantAttenuation = 1f;
            spotLight.QuadraticAttenuation = 0f;

            // Associate CompositionLight with XamlLight
            CompositionLight = spotLight;

            // define PointLight
            PointLight pointLight = compositor.CreatePointLight();
            pointLight.Offset = new Vector3(348.5f,263f,100f);
            pointLight.QuadraticAttenuation = 0f;
            pointLight.LinearAttenuation = 0f;
            pointLight.ConstantAttenuation = 1f;
            pointLight.Color = Color.FromArgb(0xFF, 0xFE, 0xFF, 0xED);
            

            // Define resting position Animation
            Vector3 restingPosition = new Vector3(200, 200, 400);

            CubicBezierEasingFunction cbEasing = compositor.CreateCubicBezierEasingFunction(new Vector2(0.3f, 0.7f), new Vector2(0.9f, 0.5f));

            _offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            _offsetAnimation.InsertKeyFrame(1, restingPosition, cbEasing);
            _offsetAnimation.Duration = TimeSpan.FromSeconds(0.5f);

            spotLight.Offset = restingPosition;

            // Define expression animation that relates light's offset to pointer position 
            CompositionPropertySet hoverPosition = ElementCompositionPreview.GetPointerPositionPropertySet(targetElement);
            _lightPositionExpression = compositor.CreateExpressionAnimation("Vector3(hover.Position.X, hover.Position.Y, height)");
            _lightPositionExpression.SetReferenceParameter("hover", hoverPosition);
            _lightPositionExpression.SetScalarParameter("height", 15.0f);

            // Configure pointer entered/ exited events
            targetElement.PointerMoved += TargetElement_PointerMoved;
            targetElement.PointerExited += TargetElement_PointerExited;

            // Add UIElement to the Light's Targets
            AddTargetElement(GetId(), targetElement);
        }

        private void MoveToRestingPosition()
        {
            // Start animation on SpotLight's Offset 
            CompositionLight?.StartAnimation("Offset", _offsetAnimation);
        }

        private void TargetElement_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (CompositionLight == null) return;

            // touch input is still UI thread-bound as of the Creator's Update
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            {
                Vector2 offset = e.GetCurrentPoint((UIElement)sender).Position.ToVector2();
                (CompositionLight as SpotLight).Offset = new Vector3(offset.X, offset.Y, 15);
            }
            else
            {
                // Get the pointer's current position from the property and bind the SpotLight's X-Y Offset
                CompositionLight.StartAnimation("Offset", _lightPositionExpression);
            }
        }

        private void TargetElement_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            // Move to resting state when pointer leaves targeted UIElement
            MoveToRestingPosition();
        }

        protected override void OnDisconnected(UIElement oldElement)
        {
            // Dispose Light and Composition resources when it is removed from the tree
            RemoveTargetElement(GetId(), oldElement);

            try
            {
                _lightPositionExpression?.Dispose();
                _lightPositionExpression = null;

                _offsetAnimation?.Dispose();
                _offsetAnimation = null;

                CompositionLight?.Dispose();
                CompositionLight = null; // AccessViolationException (run app, go to Lights page, navigate back to MainPage)
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CobblestoneLight OnDisconnect Exception: {ex}");
            }
        }

        protected override string GetId() => typeof(HoverLight).FullName;
    }
}
