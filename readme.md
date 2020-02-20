# Blurhash.UWP

[![NuGet](https://img.shields.io/nuget/v/Blurhash.UWP.svg)](https://www.nuget.org/packages/BlurHash.UWP/)

A [Blurhash](https://github.com/woltapp/blurhash) implementation based on
[blurhash.net](https://github.com/MarkusPalcer/blurhash.net) for Universal
Windows Platform's native `SoftwareBitmap`.

Allows to encode a `SoftwareBitmap` into a blurhash and decode a blurhash into
`SoftwareBitmap` again. Several portions of the code are directly copy-pasted
from the [System.Drawing implementation](https://github.com/MarkusPalcer/blurhash.net/tree/master/Blurhash-System.Drawing).

Available for x86, x64, ARM and ARM64. Requires .NET Standard 1.6 support, thus
Windows 10 16299 (Fall Creators Update) or later is required.

## Example usage

### Displaying a placeholder for an image in Image control

```csharp
using Blurhash.UWP;

// ...

var decoder = new Decoder();
var bitmap = decoder.Decode("LEHV6nWB2yk8pyo0adR*.7kCMdnj", 269, 173);

var source = new SoftwareBitmapSource();
await source.SetBitmapAsync(bitmap);

// Set the source of the Image control
image1.Source = source;
```

### Calculating the blurhash for an image loaded from file

```csharp
var fileOpenPicker = new FileOpenPicker();
fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
fileOpenPicker.FileTypeFilter.Add(".jpg");
fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;

var inputFile = await fileOpenPicker.PickSingleFileAsync();

if (inputFile == null)
{
    return;
}
else
{
    SoftwareBitmap softwareBitmap;

    using (IRandomAccessStream stream = await inputFile.OpenAsync(FileAccessMode.Read))
    {
        var decoder = await BitmapDecoder.CreateAsync(stream);

        softwareBitmap = await decoder.GetSoftwareBitmapAsync();

        var encoder = new Encoder();
        var blurhash = encoder.Encode(softwareBitmap, 4, 3);
    }
}
```
