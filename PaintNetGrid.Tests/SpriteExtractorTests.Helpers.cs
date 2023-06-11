using PaintNetGrid.Sprites;

using System.Drawing;
using System.Drawing.Imaging;

namespace PaintNetGrid.Tests;

public partial class SpriteExtractorTests {
	private unsafe static Bitmap RenderSprites(List<Sprite> sprites, int width, int height) {
		var source = new Bitmap(width, height);
		var data = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.WriteOnly, source.PixelFormat);
		var bytesPerPixel = Image.GetPixelFormatSize(data.PixelFormat) / 8;
		var startPtr = (byte*)data.Scan0.ToPointer();

		foreach (var sprite in sprites) {
			foreach (var (X, Y, Colour) in sprite.Colours) {
				var ptr = startPtr + Y * data.Stride + X * bytesPerPixel;

				*ptr = Colour.A;
				*(ptr + 1) = Colour.R;
				*(ptr + 2) = Colour.G;
				*(ptr + 3) = Colour.B;
			}
		}

		source.UnlockBits(data);

		return source;
	}

	private unsafe static void AssertBitmapsEqual(Bitmap expected, Bitmap actual) {
		Assert.NotNull(expected);
		Assert.NotNull(actual);

		Assert.Equal(expected.Width, actual.Width);
		Assert.Equal(expected.Height, actual.Height);

		Assert.Equal(expected.PixelFormat, actual.PixelFormat);

		var expectedData = expected.LockBits(new Rectangle(0, 0, expected.Width, expected.Height), ImageLockMode.ReadOnly, expected.PixelFormat);
		var actualData = actual.LockBits(new Rectangle(0, 0, actual.Width, actual.Height), ImageLockMode.ReadOnly, actual.PixelFormat);

		var bytesPerPixel = Image.GetPixelFormatSize(expectedData.PixelFormat) / 8;

		var expectedStartPtr = (byte*)expectedData.Scan0.ToPointer();
		var actualStartPtr = (byte*)actualData.Scan0.ToPointer();

		for (var y = 0; y < expected.Height; y++) {
			var topBytesOffset = y * expectedData.Stride;

			for (var x = 0; x < expected.Width; x++) {
				var expectedPtr = expectedStartPtr + topBytesOffset + x * bytesPerPixel;
				var actualPtr = actualStartPtr + topBytesOffset + x * bytesPerPixel;

				Assert.Equal(*expectedPtr, *expectedPtr);
				Assert.Equal(*(expectedPtr + 1), *(expectedPtr + 1));
				Assert.Equal(*(expectedPtr + 2), *(expectedPtr + 2));
				Assert.Equal(*(expectedPtr + 3), *(expectedPtr + 3));
			}
		}

		expected.UnlockBits(expectedData);
		actual.UnlockBits(actualData);
	}
}