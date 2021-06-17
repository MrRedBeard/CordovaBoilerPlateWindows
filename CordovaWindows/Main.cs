using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;

namespace CordovaWindows
{
    public partial class Main : Form
    {
        ChromiumWebBrowser browser { get; set; }

        public Main()
        {
            InitializeComponent();

            StartBrowser();
        }

        private void StartBrowser()
        {
            CefSettings cefSettings = new CefSettings();
            cefSettings.CachePath = Path.Combine(System.IO.Path.GetTempPath(), "cef");
            cefSettings.IgnoreCertificateErrors = true;
            cefSettings.RootCachePath = Path.Combine(System.IO.Path.GetTempPath(), "cef");
            cefSettings.BackgroundColor = 0xFF;
            cefSettings.LocalesDirPath = Path.Combine(System.IO.Path.GetTempPath(), "cef", "Locales");

            Cef.Initialize(cefSettings);

            browser = new ChromiumWebBrowser("http://localhost:5150");
            browser.BackColor = Color.Black;
            browser.TitleChanged += Browser_TitleChanged;
            browser.FrameLoadStart += Browser_FrameLoadStart;
            browser.FrameLoadEnd += Browser_FrameLoadEnd;

            this.Controls.Add(browser);
            browser.Dock = DockStyle.Fill;
            browser.BringToFront();
        }

        private void Browser_FrameLoadStart(object sender, FrameLoadStartEventArgs e)
        {
            this.Invoke(new MethodInvoker(delegate
            {
                browser.Hide();
            }));
        }

        private void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            this.Invoke(new MethodInvoker(delegate
            {
                browser.Show();
            }));
        }

        private void Browser_TitleChanged(object sender, CefSharp.TitleChangedEventArgs e)
        {
            if (!e.Title.ToLower().Contains("localhost"))
            {
                if (InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate 
                    {
                        try
                        {
                            // load the control with the appropriate data
                            Main.ActiveForm.Text = e.Title;
                        }
                        catch (Exception)
                        {
                            //Do Nothing
                        }

                        try
                        {
                            // www/img/icon.png
                            //Change Icon
                            Image img = Image.FromFile(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "www", "img", "icon.png"));
                            img = img.GetThumbnailImage(512, 512, null, IntPtr.Zero);

                            Icon icon;
                            using (var msImg = new MemoryStream())
                            using (var msIco = new MemoryStream())
                            {
                                img.Save(msImg, ImageFormat.Png);
                                using (var bw = new BinaryWriter(msIco))
                                {
                                    bw.Write((short)0);           //0-1 reserved
                                    bw.Write((short)1);           //2-3 image type, 1 = icon, 2 = cursor
                                    bw.Write((short)1);           //4-5 number of images
                                    bw.Write((byte)64);           //6 image width
                                    bw.Write((byte)64);           //7 image height
                                    bw.Write((byte)0);            //8 number of colors
                                    bw.Write((byte)0);            //9 reserved
                                    bw.Write((short)0);           //10-11 color planes
                                    bw.Write((short)32);          //12-13 bits per pixel
                                    bw.Write((int)msImg.Length);  //14-17 size of image data
                                    bw.Write(22);                 //18-21 offset of image data
                                    bw.Write(msImg.ToArray());    //write image data
                                    bw.Flush();
                                    bw.Seek(0, SeekOrigin.Begin);
                                    icon = new Icon(msIco);
                                }
                            }

                            Main.ActiveForm.Icon = icon;
                        }
                        catch (Exception)
                        {
                            //Do Nothing
                        }
                    }));
                }
            }
        }
    }
}
