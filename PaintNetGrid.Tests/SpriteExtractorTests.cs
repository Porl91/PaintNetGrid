using PaintNetGrid.Sprites;

using System.Drawing;

namespace PaintNetGrid.Tests;

public partial class SpriteExtractorTests {
	[Theory]
	[InlineData(@".\Goldens\1\Source.png", @".\Goldens\1\Result.png", 14 * 10, 0xfff7f7f7)]
	[InlineData(@".\Goldens\2\Source.png", @".\Goldens\2\Result.png", 14 * 10, 0xff616367)]
	public void ExtractedSpriteShouldMatchGolden(string sourcePath, string resultPath, int spriteCount, uint colour) {
		var source = new Bitmap(sourcePath);
		var expected = new Bitmap(resultPath);

		var buffer = new BitmapSourceBuffer2D(source);
		var extractor = new SpriteExtractor(buffer, Color.FromArgb((int)colour));
		var sprites = extractor.Extract();
		var actual = RenderSprites(sprites, source.Width, source.Height);

		Assert.Equal(spriteCount, sprites.Count);
		AssertBitmapsEqual(expected, actual);
	}
}