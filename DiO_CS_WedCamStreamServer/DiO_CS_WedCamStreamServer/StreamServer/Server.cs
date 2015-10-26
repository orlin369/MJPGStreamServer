using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace DiO_CS_WedCamStreamServer.StreamServer
{
    class Server
    {

        #region Variables

        /// <summary>
        /// Http listener.
        /// </summary>
        private HttpListener httpListener = new System.Net.HttpListener();

        /// <summary>
        /// ASCII Encoder.
        /// </summary>
        private ASCIIEncoding asciiEncodeing = new ASCIIEncoding();

        /// <summary>
        /// Thread for event controller.
        /// </summary>
        private Thread eventControlThread = null;

        /// <summary>
        /// Bit for request controller to stop.
        /// </summary>
        private bool requestToStopTheThread = false;

        #endregion

        #region Properties

        /// <summary>
        /// Is the server is runing.
        /// </summary>
        public bool IsRuning { get; private set; }

        /// <summary>
        /// Addres of the server.
        /// </summary>
        public IPAddress Address { get; private set; }

        /// <summary>
        /// Port of the service.
        /// </summary>
        public int Port { get; private set; }

        #endregion

        #region Events

        /// <summary>
        /// Via this event send the put the image for sending.
        /// </summary>
        public event EventHandler<MessageImg> OnGetImage;

        public event EventHandler<EventArgs> OnStop;

        public event EventHandler<EventArgs> OnStart;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="address">Address of the service.</param>
        /// <param name="port">Port number of the service.</param>
        public Server(IPAddress address, int port)
        {
            this.Address = address;
            this.Port = port;
            
            this.httpListener.Prefixes.Add(String.Format("http://{0}:{1}/", this.Address, this.Port));
        }

        #endregion

        #region Private Methods

        private void PoolMethod()
        {
            try
            {
                if (!this.httpListener.IsListening)
                {
                    return;
                }

                HttpListenerContext context = this.httpListener.GetContext();
                HttpListenerResponse response = context.Response;
                response.ContentType = "multipart/x-mixed-replace; boundary=--myboundary";
                Stream responseStream = response.OutputStream;
                MessageImg arg = new MessageImg();

                for (; ; )
                {
                    if (requestToStopTheThread)
                    {
                        break;
                    }

                    if (this.OnGetImage == null)
                    {
                        continue;
                    }

                    // Get image.
                    this.OnGetImage(this, arg);

                    MemoryStream imageStream = new MemoryStream();
                    JpegUtils.SaveJPG100(arg.Image, imageStream);

                    byte[] headerBytes = asciiEncodeing.GetBytes("\r\n--myboundary\r\nContent-Type: image/jpeg\r\nContent-Length:" + imageStream.Length + "\r\n\r\n");
                    MemoryStream headerStream = new System.IO.MemoryStream(headerBytes);

                    headerStream.WriteTo(responseStream);

                    imageStream.WriteTo(responseStream);

                    responseStream.Flush();

                    System.Threading.Thread.Sleep(10);
                }
            }
            catch (Exception exception)
            {
                this.Stop();
            }

            this.Stop();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start the server.
        /// </summary>
        public void Start()
        {
            if (this.OnStart != null)
            {
                this.OnStart(this, new EventArgs());
            }

            this.httpListener.Start();

            this.eventControlThread = new Thread(new ThreadStart(this.PoolMethod));
            this.eventControlThread.Name = "";
            // Start the thread
            this.eventControlThread.Start();

            this.IsRuning = true;

        }

        /// <summary>
        /// Stop the server.
        /// </summary>
        public void Stop()
        {
            this.requestToStopTheThread = true;

            this.eventControlThread = null;

            this.httpListener.Stop();

            this.IsRuning = false;

            if (this.OnStop != null)
            {
                this.OnStop(this, new EventArgs());
            }
        }

        #endregion

    }
}
