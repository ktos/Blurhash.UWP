using Blurhash.Core;
using System.Linq;
using Windows.Graphics.Imaging;

namespace Blurhash.UWP
{
    /// <summary>
    /// The Blurhash decoder for UWP
    /// Creates a bitmap placeholder from a Blurhash
    /// </summary>
    public class Decoder : CoreDecoder
    {
        /// <summary>
        /// Decodes a Blurhash string into a <c>SoftwareBitmap</c>
        /// </summary>
        /// <param name="blurhash">The blurhash string to decode</param>
        /// <param name="outputWidth">The desired width of the output in pixels</param>
        /// <param name="outputHeight">The desired height of the output in pixels</param>
        /// <param name="punch">A value that affects the contrast of the decoded image. 1 means normal, smaller values will make the effect more subtle, and larger values will make it stronger.</param>
        /// <returns>The decoded preview</returns>
        public SoftwareBitmap Decode(string blurhash, int outputWidth, int outputHeight, double punch = 1.0)
        {
            var pixelData = CoreDecode(blurhash, outputWidth, outputHeight, punch);
            return ToSoftwareBitmap(pixelData);
        }

        /// <summary>
        /// Converts the library-independent representation of pixels into a bitmap
        /// </summary>
        /// <param name="pixelData">The library-independent representation of the image</param>
        /// <returns>A <c>Windows.Graphics.Imaging.SoftwareBitmap</c> in Bgra8 representation</returns>
        private static unsafe SoftwareBitmap ToSoftwareBitmap(Blurhash.Core.Pixel[,] pixelData)
        {
            var width = pixelData.GetLength(0);
            var height = pixelData.GetLength(1);

            var data = Enumerable.Range(0, height)
                .SelectMany(y => Enumerable.Range(0, width).Select(x => (x, y)))
                .Select(tuple => pixelData[tuple.x, tuple.y])
                .SelectMany(pixel => new byte[]
                {
                    (byte) MathUtils.LinearTosRgb(pixel.Blue), (byte) MathUtils.LinearTosRgb(pixel.Green),
                    (byte) MathUtils.LinearTosRgb(pixel.Red), 255
                })
                .ToArray();

            var softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, width, height, BitmapAlphaMode.Premultiplied);

            using (BitmapBuffer buffer = softwareBitmap.LockBuffer(BitmapBufferAccessMode.Write))
            using (var reference = buffer.CreateReference())
            {
                ((IMemoryBufferByteAccess)reference).GetBuffer(out byte* dataInBytes, out uint capacity);

                for (int i = 0; i < data.Length; i++)
                {
                    dataInBytes[i] = data[i];
                }
            }
            return softwareBitmap;
        }
    }
}