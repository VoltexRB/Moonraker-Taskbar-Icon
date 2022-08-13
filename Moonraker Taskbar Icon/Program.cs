using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Moonraker_Taskbar_Icon
{
    namespace Moonraker_Taskbar
    {
        internal static class Program
        {
            /// <summary>
            /// The main entry point for the application.
            /// </summary>
            [STAThread]
            static void Main()
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Application.Run(new MoonrakerApplicationContext());
            }
        }


        public class MoonrakerApplicationContext : ApplicationContext
        {
            private NotifyIcon trayIcon;
            Thread refreshThread;

            public MoonrakerApplicationContext()
            {
                if (Properties.Settings.Default.UserColor.IsEmpty) Properties.Settings.Default.UserColor = Color.White;
                // Initialize Tray Icon
                trayIcon = new NotifyIcon()
                {
                    Icon = CreateTextIcon("NC", Color.Red),
                    ContextMenu = new ContextMenu(new MenuItem[] {new MenuItem("Configure", Configure), new MenuItem("Exit", Exit)}),
                    Visible = true
                };
                refreshThread = new Thread(refreshIconThread);
                refreshThread.IsBackground = true;
                refreshThread.Start();
            }

            void Exit(object sender, EventArgs e)
            {
                // Hide tray icon, otherwise it will remain shown until user mouses over it
                trayIcon.Visible = false;
                refreshThread.Abort();
                Application.Exit();
            }

            void Configure(object sender, EventArgs e)
            {
                Configuration conf = new Configuration();
                conf.ShowDialog();
                refreshIcon();
            }

            public Icon CreateTextIcon(string str, Color color)
            {
                Font fontToUse = new Font("Microsoft Sans Serif", 16, FontStyle.Regular, GraphicsUnit.Pixel);
                Brush brushToUse = new SolidBrush(color);
                Bitmap bitmapText = new Bitmap(16, 16);
                Graphics g = System.Drawing.Graphics.FromImage(bitmapText);

                IntPtr hIcon;

                g.Clear(Color.Transparent);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                g.DrawString(str, fontToUse, brushToUse, -4, -2);
                hIcon = (bitmapText.GetHicon());
                Icon result = System.Drawing.Icon.FromHandle(hIcon);
                return result;
            }

            void refreshIconThread()
            {
                while (true)
                {
                    refreshIcon();
                    Thread.Sleep(60000);
                }
            }

            void refreshIcon()
            {
                Properties.Settings.Default.Reload();
                string progress = "NC";
                string input = Properties.Settings.Default.IPAdress;

                //fetch progress
                IPAddress address = null;
                try
                {
                    IPAddress.TryParse(input, out address);
                }
                catch { address = null; }
                if (address == null) progress = "IP";
                else
                {
                    string completeURL = @"http://" + address + @"/printer/objects/query?virtual_sdcard";
                    try
                    {
                        HttpClient client = new HttpClient();
                        client.Timeout = TimeSpan.FromSeconds(5);
                        var content = client.GetAsync(completeURL).Result.Content;
                        string contentstring = content.ReadAsStringAsync().Result;
                        if (contentstring.Contains("\"is_active\": false") || contentstring.Contains("\"progress\": 1.0"))
                            progress = "OK";
                        else
                            progress = contentstring.Substring(contentstring.IndexOf("\"progress\":") + "\"progress\":".Length + 3, 2);
                    }
                    catch
                    {
                        progress = "NC";
                    }
                }
                //

                if (progress != "NC" && progress != "IP")
                    trayIcon.Icon = CreateTextIcon(progress, Properties.Settings.Default.UserColor);
                else
                    trayIcon.Icon = CreateTextIcon(progress, Color.Red);
            }
        }

        public class Conf
        {
            public string ipadress;
            public Color color;
            public Conf(string str, Color col)
            {
                ipadress = str;
                color = col;
            }
        }
    }
}
