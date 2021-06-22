using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using CanvasEngine;
using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Diagnostics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace xbox_canvas_game
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Engine engine;
        DateTime time1 = DateTime.Now;
        DateTime time2 = DateTime.Now;
        bool cookieClicked = false;
        int defaultCookieSize = 256;
        int cookieSize = 256;
        int maxCookieSize = 265;
        double cookieCount = 0;
        double cps = 0;
        string data;
        int selectedStoreItem = 0;
        bool dpadDownPreviousFrame = false; // stupid
        bool dpadUpPreviousFrame = false; // stupid, will add built in checker in engine
        Timer timer;

        /*                  Textures                    */

        Engine.EngineTexture cookieTexture;
        Engine.EngineTexture bgBlueTexture;
        Engine.EngineTexture seperatorTexture;
        Engine.EngineTexture storeTileTexture;
        Engine.EngineTexture storeMoneyTexture;

        Engine.EngineTexture aButtonTexture;
        Engine.EngineTexture bButtonTexture;
        Engine.EngineTexture xButtonTexture;
        Engine.EngineTexture yButtonTexture;

        /*              End of textures                 */

        public struct StoreItem
        {
            public string name;
            public double basePrice;
            public double price;
            public double quantity;
            public Engine.EngineTexture icon;
            public double additionalCPS;
        };

        List<StoreItem> storeItems = new List<StoreItem>();

        public MainPage()
        {
            this.InitializeComponent();

            engine = new Engine(canvas, grid);

            startAsync();
        }

        async void startAsync()
        {
            start();

            while (true)
            {
                update();
                await Task.Delay(1);
            }
        }

        void TickTimer(object state)
        {
            cookieCount += cps;
        }

        void start()
        {
            /*                  Textures                    */

            cookieTexture    = engine.GenerateTexture("cookie.png", Stretch.Fill);
            bgBlueTexture    = engine.GenerateTexture("bgBlue.jpg", Stretch.Fill);
            seperatorTexture = engine.GenerateTexture("panelVertical.png", Stretch.Fill);
            storeTileTexture = engine.GenerateTexture("storeTile.jpg", Stretch.Fill);
            storeMoneyTexture = engine.GenerateTexture("StoreIcons/money.png", Stretch.Fill);

            aButtonTexture   = engine.GenerateTexture("GamepadIcons/A.png", Stretch.Fill);
            bButtonTexture   = engine.GenerateTexture("GamepadIcons/B.png", Stretch.Fill);
            xButtonTexture   = engine.GenerateTexture("GamepadIcons/X.png", Stretch.Fill);
            yButtonTexture   = engine.GenerateTexture("GamepadIcons/Y.png", Stretch.Fill);

            /*              End of textures                 */

            /*          
                      Store Data Reader
                      
                      File Format (in order):
                        Index 0 = Name (string, name of item)
                        Index 1 = Base Price (double, base price of item)
                        Index 2 = Icon (string, converted into texture during parsing process)
                        Index 3 = Additional CPS (double, the gain in CPS caused by this item)
            */              
            data = File.ReadAllText("Assets/GameAssets/Data/store_items.txt");

            string[] dataLines = data.Split('\n');

            foreach(string dataLine in dataLines)
            {
                string[] dataSectors = dataLine.Split(';');

                StoreItem storeItem = new StoreItem();
                storeItem.name = dataSectors[0];
                storeItem.basePrice = double.Parse(dataSectors[1]);
                storeItem.icon = engine.GenerateTexture("StoreIcons/" + dataSectors[2], Stretch.Fill);
                storeItem.additionalCPS = double.Parse(dataSectors[3]);
                storeItems.Add(storeItem);
            }

            timer = new Timer(TimerCallBack, null, 0, 1000);
        }

        void TimerCallBack(object state)
        {
            cookieCount += cps;
        }

        // literally fucking tons of hardcoded values below, very bad idea, will change, although for now these values work on almost every screen

        void update()
        {
            if (ActualWidth > 0)
            {
                time2 = DateTime.Now;
                float deltaTime = (time2.Ticks - time1.Ticks) / 10000000f;

                engine.Clear(Colors.Black);

                engine.TexturedRect(0, 0, ActualWidth / 3.5, (int)ActualHeight, bgBlueTexture);

                engine.TexturedRect(ActualWidth / 3.5, 0, 16, (int)ActualHeight, seperatorTexture);

                engine.TexturedRect((ActualWidth / 3.5) + 16, 0, ActualWidth / 2.5, (int)ActualHeight, bgBlueTexture);

                engine.TexturedRect(((ActualWidth / 3.5) + 16) + ActualWidth / 2.5, 0, 16, (int)ActualHeight, seperatorTexture);

                double remainingWidth = ActualWidth - (((ActualWidth / 3.5) + 16) + ((ActualWidth / 2.5) + 16));
                double finalX = (((ActualWidth / 3.5) + 16) + ((ActualWidth / 2.5) + 16));

                engine.TexturedRect(finalX, 0, remainingWidth, (int)ActualHeight, bgBlueTexture);

                engine.TexturedRect(((ActualWidth / 2) / 3.5) - (cookieSize / 2), (ActualHeight / 2) - (cookieSize / 2), cookieSize, cookieSize, cookieTexture);

                engine.TexturedRect(((ActualWidth / 2) / 3.5) - 20, (ActualHeight / 2) + 128, 40, 40, aButtonTexture);

                engine.Rect(0, ActualHeight / 6, ActualWidth / 3.5, 50, Color.FromArgb(150, 70, 70, 70));

                engine.Text(0, (ActualHeight / 6) + 5, Math.Round(cookieCount) + " cookies", 20, Colors.White, true, ActualWidth / 3.5, Windows.UI.Xaml.TextAlignment.Center);
                engine.Text(0, (ActualHeight / 6) + 30, "per second: " + cps, 11, Colors.White, true, ActualWidth / 3.5, Windows.UI.Xaml.TextAlignment.Center);

                int y = 0;

                for (int i = 0; i < storeItems.Count; i++)
                {
                    StoreItem item = storeItems[i];

                    item.price = Math.Round(item.basePrice * Math.Pow(1.15, item.quantity));

                    engine.TexturedRect(finalX, y, remainingWidth, 64, storeTileTexture);
                    engine.Text(finalX + 64, y + 2, item.name, 35, Colors.White);
                    engine.Text(finalX + 64 + 16, y + 40, item.price.ToString(), 11, Colors.White);
                    engine.Text(finalX, y + 40, item.quantity.ToString(), 11, Colors.White, true, remainingWidth - 20, Windows.UI.Xaml.TextAlignment.Right);
                    engine.TexturedRect(finalX, y, 64, 64, item.icon);
                    engine.TexturedRect(finalX + 64, y + 40, 16, 16, storeMoneyTexture);
                    if (i == selectedStoreItem)
                    {
                        engine.UnfilledRect(finalX, y, remainingWidth, 64, Colors.Blue);
                    }
                    y += 64;

                    storeItems[i] = item;
                }

                if (engine.GetGamepad().A)
                {
                    cookieClicked = true;
                    cookieCount++;
                }

                if (engine.GetGamepad().DPadDown && !dpadDownPreviousFrame)
                {
                    dpadDownPreviousFrame = true;
                    selectedStoreItem++;
                    if (selectedStoreItem > (storeItems.Count - 1)) selectedStoreItem = storeItems.Count - 1;
                }

                if (!engine.GetGamepad().DPadDown)
                {
                    dpadDownPreviousFrame = false;
                }

                if (engine.GetGamepad().DPadUp && !dpadUpPreviousFrame)
                {
                    dpadUpPreviousFrame = true;
                    selectedStoreItem--;
                    if (selectedStoreItem < 0) selectedStoreItem = 0;
                }

                if (!engine.GetGamepad().DPadUp)
                {
                    dpadUpPreviousFrame = false;
                }

                if (engine.GetGamepad().X)
                {
                    StoreItem selectedItem = storeItems[selectedStoreItem];

                    if (cookieCount > selectedItem.price)
                    {
                        cookieCount -= selectedItem.price;
                        selectedItem.quantity++;
                        cps += selectedItem.additionalCPS;
                    }

                    storeItems[selectedStoreItem] = selectedItem;
                }
                
                /* testing purposes
                if (engine.GetGamepad().Y)
                {
                    StoreItem selectedItem = storeItems[selectedStoreItem];

                    
                        //cookieCount -= selectedItem.price;
                        selectedItem.quantity++;
                        cps += selectedItem.additionalCPS;

                    storeItems[selectedStoreItem] = selectedItem;
                }*/

                if (cookieClicked)
                {
                    cookieSize += 4;
                    if (cookieSize > maxCookieSize)
                    {
                        cookieClicked = false;
                        cookieSize = defaultCookieSize;
                        cookieCount++;
                    }
                }

                time1 = time2;
            } else
            {
                Debug.WriteLine("Wait! Still Loading...");
            }
        }
    }
}
