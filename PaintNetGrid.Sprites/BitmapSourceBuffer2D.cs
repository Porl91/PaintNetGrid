using System.Drawing;

namespace PaintNetGrid.Sprites;

public sealed class BitmapSourceBuffer2D : ISourceBuffer2D {
	private readonly Bitmap _source;

	public BitmapSourceBuffer2D(Bitmap source) {
		_source = source;
	}

	public int Width => _source.Width;

	public int Height => _source.Height;

	public Color GetPixel(int x, int y) {
		return _source.GetPixel(x, y);
	}
}