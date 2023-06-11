using PaintDotNet;
using PaintDotNet.Imaging;

namespace PaintNetGrid.Sprites;

public sealed class RegionPtrSourceBuffer2D : ISourceBuffer2D {
	private readonly RegionPtr<ColorBgra32> _source;

	public RegionPtrSourceBuffer2D(RegionPtr<ColorBgra32> source) {
		_source = source;
	}

	public int Width => _source.Width;

	public int Height => _source.Height;

	public Color GetPixel(int x, int y) {
		return _source[x, y];
	}
}