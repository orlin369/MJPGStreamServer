using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace DiO_CS_WedCamStreamServer.StreamServer
{
    public static class JpegUtils
    {
        public static void SaveJPG100(this Bitmap image, MemoryStream stream)
        {
            stream.Flush();
            EncoderParameters encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 75L);
            image.Save(stream, GetEncoder(ImageFormat.Jpeg), encoderParameters);
        }

        public static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
