using System.Drawing;

namespace PaintNetGrid.Sprites;

public interface ISourceBuffer2D {
	int Width { get; }
	int Height { get; }
	Color GetPixel(int x, int y);
}