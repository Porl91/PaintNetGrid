using PaintNetGrid.Sprites;

using System.Drawing;

namespace PaintNetGrid.Tests;

public partial class SpriteExtractorTests {
	[Theory]
	[InlineData(@".\Goldens\1\Source.png", @".\Goldens\1\Result.png", 14 * 10)]
	public void ExtractedSpriteShouldMatchGolden(string sourcePath, string resultPath, int spriteCount) {
		var source = new Bitmap(sourcePath);
		var expected = new Bitmap(resultPath);

		var buffer = new BitmapSourceBuffer2D(source);
		var extractor = new SpriteExtractor(buffer, Color.FromArgb(255, 247, 247, 247));
		var sprites = extractor.Extract();
		var actual = RenderSprites(sprites, source.Width, source.Height);

		Assert.Equal(spriteCount, sprites.Count);
		AssertBitmapsEqual(expected, actual);
	}
}