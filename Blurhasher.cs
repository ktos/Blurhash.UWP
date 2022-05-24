using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace Blurhash.UWP
{
    /// <summary>
    /// The Blurhash encoder/decoder for UWP
    /// Creates a very compact hash from an image to use as a blurred image placeholder
    /// </summary>
    public static class Blurhasher
    {
        /// <summary>
        /// Encodes a <c>SoftwareBitmap</c> into a Blurhash string
        /// </summary>
        /// <param name="image">The bitmap to encode</param>
        /// <param name="componentsX">The number of components used on the X-Axis for the DCT</param>
        /// <param name="componentsY">The number of components used on the Y-Axis for the DCT</param>
        /// <returns>The resulting Blurhash string</returns>
        public static string Encode(SoftwareBitmap image, int componentsX, int componentsY)
        {
            return Core.Encode(ConvertBitmap(image), componentsX, componentsY);
        }

        /// <summary>
        /// Converts the given bitmap to the library-independent representation used within the Blurhash-core
        /// </summary>
        /// <param name="sourceBitmap">The bitmap to encode</param>
        public static unsafe Pixel[,] ConvertBitmap(SoftwareBitmap sourceBitmap)
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

        /// <summary>
        /// Decodes a Blurhash string into a <c>SoftwareBitmap</c>
        /// </summary>
        /// <param name="blurhash">The blurhash string to decode</param>
        /// <param name="outputWidth">The desired width of the output in pixels</param>
        /// <param name="outputHeight">The desired height of the output in pixels</param>
        /// <param name="punch">A value that affects the contrast of the decoded image. 1 means normal, smaller values will make the effect more subtle, and larger values will make it stronger.</param>
        /// <returns>The decoded preview</returns>
        public static SoftwareBitmap Decode(string blurhash, int outputWidth, int outputHeight, double punch = 1.0)
        {
            var pixelData = new Pixel[outputWidth, outputHeight];
            Core.Decode(blurhash, pixelData, punch);
            return ToSoftwareBitmap(pixelData);
        }

        /// <summary>
        /// Converts the library-independent representation of pixels into a bitmap
        /// </summary>
        /// <param name="pixelData">The library-independent representation of the image</param>
        /// <returns>A <c>Windows.Graphics.Imaging.SoftwareBitmap</c> in Bgra8 representation</returns>
        public static unsafe SoftwareBitmap ToSoftwareBitmap(Blurhash.Pixel[,] pixelData)
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