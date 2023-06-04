using System.Drawing;
using System.Runtime.CompilerServices;

namespace PaintNetGrid.Sprites;

public sealed class SpriteExtractor {
	private readonly ISourceBuffer2D _source;
	private readonly Color _spaceColour;
	private readonly ISpritesheet _sheet;

	public SpriteExtractor(ISourceBuffer2D source, Color spaceColour) {
		_source = source;
		_spaceColour = spaceColour;
		_sheet = new Spritesheet(_source);
	}

	public List<SpriteMask> ComputeMasks() {
		var masks = new List<SpriteMask>();
		var used = new HashSet<int>();

		for (var y = 0; y < _source.Height; y++) {
			for (var x = 0; x < _source.Width; x++) {
				if (IsUsed(x, y))
					continue;

				var col = _source.GetPixel(x, y);

				if (!IsWhitespaceColour(ref col)) {
					var mask = new List<(int X, int Y, Color Col)>();
					var path = new Stack<(int X, int Y)>();

					(int X, int Y)? current = null;

					_ = TryUse(x, y);

					while (current is not null) {
						var currentX = current.Value.X;
						var currentY = current.Value.Y;

						if (!TryUse(currentX, currentY - 1)
							&& !TryUse(currentX, currentY + 1)
							&& !TryUse(currentX + 1, currentY)
							&& !TryUse(currentX - 1, currentY)) {
							if (!path.TryPop(out var previous)) {
								break;
							}
							current = previous;
						}
					}

					var minX = mask.Min(m => m.X);
					var maxX = mask.Max(m => m.X);
					var minY = mask.Min(m => m.Y);
					var maxY = mask.Max(m => m.Y);

					masks.Add(new SpriteMask(
						mask,
						new AABB {
							X = minX,
							Y = minY,
							Width = maxX - minX,
							Height = maxY - minY
						},
						_sheet
					));

					bool TryUse(int x, int y) {
						if (x < 0 || y < 0 || x >= _source.Width || y >= _source.Height)
							return false;

						if (IsUsed(x, y))
							return false;

						var col = _source.GetPixel(x, y);
						if (IsWhitespaceColour(ref col)) {
							return false;
						}

						path.Push((x, y));
						used.Add(_sheet.Index(x, y));
						mask.Add((x, y, col));
						current = path.Peek();

						return true;
					}
				}
			}
		}

		return masks;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool IsUsed(int x, int y) {
			return used.Contains(_sheet.Index(x, y));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool IsWhitespaceColour(ref Color c) {
		return c.R == _spaceColour.R
			&& c.G == _spaceColour.G
			&& c.B == _spaceColour.B
			&& c.A == _spaceColour.A;
	}
}