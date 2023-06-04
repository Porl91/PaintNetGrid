using PaintDotNet;

using System.Drawing;

namespace PaintNetGrid.Sprites;

public sealed class SurfaceSourceBuffer2D : ISourceBuffer2D {
	private readonly Surface _source;

	public SurfaceSourceBuffer2D(Surface source) {
		_source = source;
	}

	public int Width => _source.Width;

	public int Height => _source.Height;

	public Color GetPixel(int x, int y) {
		return _source[x, y];
	}
}