using System.Runtime.CompilerServices;

namespace PaintNetGrid.Sprites;

public sealed class Spritesheet : ISpritesheet {
	private readonly ISourceBuffer2D _source;

	public Spritesheet(ISourceBuffer2D source) {
		_source = source;
	}

	public int Width => _source.Width;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int Index(int x, int y) {
		return y * _source.Width + x;
	}
}