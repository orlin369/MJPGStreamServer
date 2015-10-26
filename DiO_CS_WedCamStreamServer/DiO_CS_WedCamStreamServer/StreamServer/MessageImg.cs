using System;
using System.Drawing;

namespace DiO_CS_WedCamStreamServer.StreamServer
{
    /// <summary>
    /// Event argument passing image.
    /// </summary>
    class MessageImg : EventArgs
    {

        #region Properties

        /// <summary>
        /// Passed image.
        /// </summary>
        public Bitmap Image { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public MessageImg()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="image">Passed image like argument.</param>
        public MessageImg(Bitmap image)
        {
            this.Image = image;
        }

        #endregion

    }
}
