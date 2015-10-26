//#define USE_EMGUCV
#define NOT_USE_EMGUCV

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

using DiO_CS_WedCamStreamServer.StreamServer;

namespace DiO_CS_WedCamStreamServer
{
    public partial class MainForm : Form
    {

        #region Variables

#if USE_EMGUCV

        /// <summary>
        /// Camera capture.
        /// </summary>
        Emgu.CV.Capture capture = new Emgu.CV.Capture(0);

        /// <summary>
        /// Date & Time font.
        /// </summary>
        Emgu.CV.Structure.MCvFont f = new Emgu.CV.Structure.MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_COMPLEX, 1.0, 1.0);

#endif
        /// <summary>
        /// MJPG Streaming server.
        /// </summary>
        private Server server = null;

        #endregion

        #region Constructor

        /// <summary>
        /// Construcotr
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Buttons

        private void btnStart_Click(object sender, EventArgs e)
        {
            this.StartServer();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            // Start the server.
            if (this.server.IsRuning)
            {
                this.server.Stop();
            }
        }

        #endregion

        #region Form

        private void MainForm_Load(object sender, EventArgs e)
        {
            IPAddress[] addresses = this.GetAddresses();

            if (addresses.Length > 0)
            {
                this.cmbIPAddresses.DataSource = addresses;
                this.cmbIPAddresses.Text = addresses[0].ToString();
            }

            IPAddress localIPAddress = addresses[0];
            int localPort = 8080;

            // Get settings
            IPAddress.TryParse(this.cmbIPAddresses.Text, out localIPAddress);
            int.TryParse(this.txtPort.Text, out localPort);

            // Set the server.
            this.server = new Server(localIPAddress, localPort);

            // Add a link to the LinkLabel.
            string linkString = String.Format("http://{0}:{1}/", this.server.Address, this.server.Port);
            LinkLabel.Link link = new LinkLabel.Link();
            link.LinkData = linkString;
            this.linkLblPageLink.Links.Add(link);
            this.linkLblPageLink.Text = linkString;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Start the server.
            if (this.server.IsRuning)
            {
                this.server.OnGetImage -= this.server_GetImage;
                this.server.Stop();
            }
        }

        #endregion

        #region Private Methods

        private IPAddress[] GetAddresses()
        {
            List<IPAddress> addresses = new List<IPAddress>();
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    addresses.Add(ip);
                }
            }

            return addresses.ToArray();
        }

        /// <summary>
        /// Example image
        /// </summary>
        /// <returns>random colored image.</returns>
        private static Bitmap GetImage()
        {
            Bitmap bitmap = new Bitmap(720, 400, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            //Create random generator
            Random random = new Random();
            //Create ne graphics
            Graphics graphics = Graphics.FromImage(bitmap);
            //Create color
            int r = random.Next(0, 255);
            int g = random.Next(0, 255);
            int b = random.Next(0, 255);
            //Set graphics color
            graphics.FillRectangle(new SolidBrush(Color.FromArgb(r, g, b)), 0, 0, bitmap.Width, bitmap.Height);
            //Create image stream
            MemoryStream imageStream = new MemoryStream();
            //Save image
            EncoderParameters encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 75L);
            bitmap.Save(imageStream, JpegUtils.GetEncoder(ImageFormat.Jpeg), encoderParameters);

            return bitmap;
        }

        private void StartServer()
        {
            if (this.server != null && this.server.IsRuning)
            {
                this.server.Stop();
            }

            IPAddress localIPAddress = IPAddress.Loopback;
            int localPort = 8080;

            // Get settings
            IPAddress.TryParse(this.cmbIPAddresses.Text, out localIPAddress);
            int.TryParse(this.txtPort.Text, out localPort);

            // Set the server.
            this.server = new Server(localIPAddress, localPort);
            this.server.OnStart += server_OnStart;
            this.server.OnStop += server_OnStop;
            this.server.OnGetImage += this.server_GetImage;
            this.server.Start();

            //try
            //{
            //
            //}
            //catch (Exception exception)
            //{
            //    MessageBox.Show(String.Format("Message: {0}", exception.ToString()), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }

        #endregion

        #region Linklabel page link.

        private void linkLblPageLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!this.server.IsRuning)
            {
                this.StartServer();
            }
            else
            {
                return;
            }


            if (this.server.IsRuning && e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                // Send the URL to the operating system.
                Process.Start((string)e.Link.LinkData);
            }
        }

        #endregion

        #region Server

        private void server_GetImage(object sender, MessageImg e)
        {
            
#if USE_EMGUCV

            Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte> image = this.capture.QueryFrame();
            string dateAndTime = DateTime.Now.ToString("yyyy.MM.dd/HH:mm:ss.fff", System.Globalization.DateTimeFormatInfo.InvariantInfo);
            image.Draw(dateAndTime, ref f, new Point(10, 30), new Emgu.CV.Structure.Bgr(0, 255, 0));
            e.Image = image.Bitmap;

#elif NOT_USE_EMGUCV

            e.Image = GetImage();

#endif

        }

        private void server_OnStop(object sender, EventArgs e)
        {
            if (this.btnStart.InvokeRequired)
            {
                this.btnStart.BeginInvoke((MethodInvoker)delegate()
                {
                    this.btnStart.Enabled = true;
                });
            }
            else
            {
                this.btnStart.Enabled = true;
            }

            if (this.btnStop.InvokeRequired)
            {
                this.btnStop.BeginInvoke((MethodInvoker)delegate()
                {
                    this.btnStop.Enabled = false;
                });
            }
            else
            {
                this.btnStop.Enabled = false;
            }

            if (this.lblServerState.InvokeRequired)
            {
                this.lblServerState.BeginInvoke((MethodInvoker)delegate()
                {
                    this.lblServerState.Text = "Stoped";
                    this.lblServerState.BackColor = Color.Red;
                });
            }
            else
            {
                this.lblServerState.Text = "Stoped";
                this.lblServerState.BackColor = Color.Red;
            }

            string linkString = String.Format("http://{0}:{1}/", this.server.Address, this.server.Port);
            LinkLabel.Link link = new LinkLabel.Link();
            if (this.linkLblPageLink.InvokeRequired)
            {
                this.linkLblPageLink.BeginInvoke((MethodInvoker)delegate()
                {
                    // Add a link to the LinkLabel.
                    link.LinkData = linkString;
                    this.linkLblPageLink.Links.Clear();
                    this.linkLblPageLink.Links.Add(link);
                    this.linkLblPageLink.Text = linkString;
                });
            }
            else
            {
                // Add a link to the LinkLabel.
                link.LinkData = linkString;
                this.linkLblPageLink.Links.Clear();
                this.linkLblPageLink.Links.Add(link);
                this.linkLblPageLink.Text = linkString;
            }


        }

        private void server_OnStart(object sender, EventArgs e)
        {
            if (this.btnStart.InvokeRequired)
            {
                this.btnStart.BeginInvoke((MethodInvoker)delegate()
                {
                    this.btnStart.Enabled = false;
                });
            }
            else
            {
                this.btnStart.Enabled = false;
            }

            if (this.btnStop.InvokeRequired)
            {
                this.btnStop.BeginInvoke((MethodInvoker)delegate()
                {
                    this.btnStop.Enabled = true;
                });
            }
            else
            {
                this.btnStop.Enabled = true;
            }

            if (this.lblServerState.InvokeRequired)
            {
                this.lblServerState.BeginInvoke((MethodInvoker)delegate()
                {
                    this.lblServerState.Text = "Started";
                    this.lblServerState.BackColor = Color.Green;
                });
            }
            else
            {
                this.lblServerState.Text = "Started";
                this.lblServerState.BackColor = Color.Green;
            }
        }

        #endregion
    }
}
