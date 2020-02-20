using Blurhash.Core;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace Blurhash.UWP
{
    /// <summary>
    /// The Blurhash encoder for UWP
    /// Creates a very compact hash from an image to use as a blurred image placeholder
    /// </summary>
    public class Encoder : CoreEncoder
    {
        /// <summary>
        /// Encodes a <c>SoftwareBitmap</c> into a Blurhash string
        /// </summary>
        /// <param name="image">The bitmap to encode</param>
        /// <param name="componentsX">The number of components used on the X-Axis for the DCT</param>
        /// <param name="componentsY">The number of components used on the Y-Axis for the DCT</param>
        /// <returns>The resulting Blurhash string</returns>
        public string Encode(SoftwareBitmap image, int componentsX, int componentsY)
        {
            return CoreEncode(ConvertBitmap(image), componentsX, componentsY);
        }

        /// <summary>
        /// Converts the given bitmap to the library-independent representation used within the Blurhash-core
        /// </summary>
        /// <param name="sourceBitmap">The bitmap to encode</param>
        public unsafe static Pixel[,] ConvertBitmap(SoftwareBitmap sourceBitmap)
        {
            var width = sourceBitmap.PixelWidth;
            var height = sourceBitmap.PixelHeight;

            using (BitmapBuffer buffer = sourceBitmap.LockBuffer(BitmapBufferAccessMode.Read))
            using (var reference = buffer.CreateReference())
            {
                ((IMemoryBufferByteAccess)reference).GetBuffer(out byte* bgrValues, out uint capacity);

                var result = new Pixel[width, height];

                Parallel.ForEach(Enumerable.Range(0, height), y =>
                {
                    var index = width * y * 4;

                    for (var x = 0; x < width; x++)
                    {
                        result[x, y].Red = MathUtils.SRgbToLinear(bgrValues[index + 2]);
                        result[x, y].Green = MathUtils.SRgbToLinear(bgrValues[index + 1]);
                        result[x, y].Blue = MathUtils.SRgbToLinear(bgrValues[index]);
                        index += 4;
                    }
                });

                return result;
            }
        }
    }
}