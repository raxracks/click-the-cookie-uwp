using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Gaming.Input;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

namespace CanvasEngine
{
    public class Engine
    {
        Canvas ctx;
        Grid grid;
        private Gamepad _Gamepad = null;
        bool gamepadConnected = false;

        public Engine(Canvas canvas, Grid grid)
        {
            this.grid = grid;
            ctx = canvas;

            Gamepad.GamepadAdded += Gamepad_GamepadAdded;
            Gamepad.GamepadRemoved += Gamepad_GamepadRemoved;
        }

        private void Gamepad_GamepadRemoved(object sender, Gamepad e)
        {
            _Gamepad = null;
            gamepadConnected = false;
        }

        private void Gamepad_GamepadAdded(object sender, Gamepad e)
        {
            _Gamepad = e;
            gamepadConnected = true;
        }

        public void Rect(double x, double y, double width, double height, Color color, double rotation = 0)
        {
            RotateTransform rotateTransform = new RotateTransform();
            rotateTransform.Angle = rotation;
            Rectangle rect = new Rectangle();
            rect.Stroke = new SolidColorBrush(color);
            rect.Fill = new SolidColorBrush(color);
            rect.Width = width;
            rect.Height = height;
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            rect.RenderTransform = rotateTransform;

            ctx.Children.Add(rect);
        }

        public void UnfilledRect(double x, double y, double width, double height, Color color, double rotation = 0)
        {
            RotateTransform rotateTransform = new RotateTransform();
            rotateTransform.Angle = rotation;
            Rectangle rect = new Rectangle();
            rect.Stroke = new SolidColorBrush(color);
            rect.Fill = null;
            rect.Width = width;
            rect.Height = height;
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            rect.RenderTransform = rotateTransform;

            ctx.Children.Add(rect);
        }

        public void Text(double x, double y, string text, int fontSize, Color color, bool manualWidth = false, double width = 0, Windows.UI.Xaml.TextAlignment textAlignment = Windows.UI.Xaml.TextAlignment.Left)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.FontSize = fontSize;
            textBlock.Foreground = new SolidColorBrush(color);
            if(manualWidth)
            {
                textBlock.Width = width;
            }
            
            textBlock.HorizontalTextAlignment = textAlignment;

            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);

            ctx.Children.Add(textBlock);
        }

        private ImageSource GetImageSource(string imagePath)
        {
            BitmapImage glowIcon = new BitmapImage();

            glowIcon.UriSource = new Uri("ms-appx:///Assets/GameAssets/" + imagePath);

            return glowIcon;
        }

        public struct EngineTexture
        {
            public ImageBrush imgBrush;
        }

        public EngineTexture GenerateTexture(string imagePath, Stretch stretchMethod)
        {
            EngineTexture engineTexture = new EngineTexture();
            ImageBrush imgBrush = new ImageBrush();
            imgBrush.Stretch = stretchMethod;
            imgBrush.ImageSource = GetImageSource(imagePath);
            engineTexture.imgBrush = imgBrush;
            return engineTexture;
        }

        public void TexturedRect(double x, double y, double width, double height, EngineTexture texture, double rotation = 0)
        {
            RotateTransform rotateTransform = new RotateTransform();
            rotateTransform.Angle = rotation;
            rotateTransform.CenterX = width / 2;
            rotateTransform.CenterY = height / 2;
            Rectangle rect = new Rectangle();
            rect.Fill = texture.imgBrush;
            rect.Width = width;
            rect.Height = height;
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            rect.RenderTransform = rotateTransform;

            ctx.Children.Add(rect);
        }

        public async Task<string> GetKeyboardInputAsync()
        {
            TextBox input = new TextBox();
            input.Width = 0;
            input.Height = 0;
            Canvas.SetLeft(input, 0);
            Canvas.SetTop(input, 0);

            grid.Children.Add(input);

            input.Focus(Windows.UI.Xaml.FocusState.Programmatic);

            input.KeyDown += Input_KeyDown;
            input.LostFocus += Input_LostFocus;
            input.FocusDisengaged += Input_FocusDisengaged;

            string text = "";

            while(grid.Children.Contains(input))
            {
                text = input.Text;
                await Task.Delay(1);
            }

            return text;
        }

        private void Input_FocusDisengaged(Control sender, FocusDisengagedEventArgs args)
        {
            grid.Children.Remove(sender as TextBox);
        }

        private void Input_LostFocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            grid.Children.Remove(sender as TextBox);
        }

        private void Input_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            e.Handled = true;
            if(e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.GamepadB)
            {
                grid.Children.Remove(sender as TextBox);
            }
        }

        public void Clear(Color color)
        {
            ctx.Children.Clear();
            ctx.Background = new SolidColorBrush(color);
        }

        public struct GamepadData
        {
            public double LT;
            public double RT;
            public double LSX;
            public double LSY;
            public double RSX;
            public double RSY;
            public bool A;
            public bool B;
            public bool X;
            public bool Y;
            public bool LB;
            public bool RB;
            public bool LS;
            public bool RS;
            public bool DPadLeft;
            public bool DPadRight;
            public bool DPadUp;
            public bool DPadDown;
            public bool Connected;
        }

        public GamepadData GetGamepad()
        {
            GamepadData data;

            if (gamepadConnected)
            {
                var reading = _Gamepad.GetCurrentReading();

                data.LT = reading.LeftTrigger;
                data.RT = reading.RightTrigger;
                data.LSX = reading.LeftThumbstickX;
                data.LSY = reading.LeftThumbstickY;
                data.RSX = reading.RightThumbstickX;
                data.RSY = reading.RightThumbstickY;
                data.A = (reading.Buttons & GamepadButtons.A) == GamepadButtons.A;
                data.B = (reading.Buttons & GamepadButtons.B) == GamepadButtons.B;
                data.X = (reading.Buttons & GamepadButtons.X) == GamepadButtons.X;
                data.Y = (reading.Buttons & GamepadButtons.Y) == GamepadButtons.Y;
                data.LB = (reading.Buttons & GamepadButtons.LeftShoulder) == GamepadButtons.LeftShoulder;
                data.RB = (reading.Buttons & GamepadButtons.RightShoulder) == GamepadButtons.RightShoulder;
                data.LS = (reading.Buttons & GamepadButtons.LeftThumbstick) == GamepadButtons.LeftThumbstick;
                data.RS = (reading.Buttons & GamepadButtons.RightThumbstick) == GamepadButtons.RightThumbstick;
                data.DPadLeft = (reading.Buttons & GamepadButtons.DPadLeft) == GamepadButtons.DPadLeft;
                data.DPadRight = (reading.Buttons & GamepadButtons.DPadRight) == GamepadButtons.DPadRight;
                data.DPadUp = (reading.Buttons & GamepadButtons.DPadUp) == GamepadButtons.DPadUp;
                data.DPadDown = (reading.Buttons & GamepadButtons.DPadDown) == GamepadButtons.DPadDown;
                data.Connected = true;
            } else
            {
                data.LT = 0;
                data.RT = 0;
                data.LSX = 0;
                data.LSY = 0;
                data.RSX = 0;
                data.RSY = 0;
                data.A = false;
                data.B = false;
                data.X = false;
                data.Y = false;
                data.LB = false;
                data.RB = false;
                data.LS = false;
                data.RS = false;
                data.DPadLeft = false;
                data.DPadRight = false;
                data.DPadUp = false;
                data.DPadDown = false;
                data.Connected = false;
            }

            return data;
        }
    }
}
