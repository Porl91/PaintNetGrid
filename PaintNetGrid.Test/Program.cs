using PaintNetGrid.Sprites;

using System.Diagnostics;
using System.Drawing;

var input = new Bitmap(@".\Goldens\CowboySprites-Bug1.png");
var source = new BitmapSourceBuffer2D(input);
var extractor = new SpriteExtractor(source, Color.FromArgb(255, 247, 247, 247));
var sprites = extractor.Extract();

if (sprites.Count == 1) {
	Console.WriteLine("FIXED");
} else {
	Console.WriteLine("NOT FIXED");
}

return;

var output = new Bitmap(input.Width, input.Height);

foreach (var sprite in sprites) {
	foreach (var (X, Y, Col) in sprite.Mask) {
		output.SetPixel(X, Y, Col);
	}
}

var outputFile = @".\Goldens\CowboySprites-Result.png";

output.Save(outputFile);

using var fileopener = new Process();

fileopener.StartInfo.FileName = "explorer";
fileopener.StartInfo.Arguments = $"\"{outputFile}\"";
fileopener.Start();

Console.WriteLine();