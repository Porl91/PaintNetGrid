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

	public List<Sprite> Extract() {
		var sprites = new List<Sprite>();
		var used = new HashSet<int>();

		for (var y = 0; y < _source.Height; y++) {
			for (var x = 0; x < _source.Width; x++) {
				if (IsUsed(x, y))
					continue;

				var col = _source.GetPixel(x, y);

				if (!IsWhitespaceColour(ref col)) {
					var colours = new List<(int X, int Y, Color Col)>();
					var path = new Stack<PathNode>();

					TryMove(x, y);

					while (path.TryPeek(out var current)) {
						if (!TryMove(current.X, current.Y - 1)
							&& !TryMove(current.X, current.Y + 1)
							&& !TryMove(current.X + 1, current.Y)
							&& !TryMove(current.X - 1, current.Y)) {
							if (!path.TryPop(out current)) {
								break;
							}
						}
					}

					var minX = colours.Min(m => m.X);
					var maxX = colours.Max(m => m.X);
					var minY = colours.Min(m => m.Y);
					var maxY = colours.Max(m => m.Y);

					sprites.Add(new Sprite(
						colours,
						new AABB {
							X = minX,
							Y = minY,
							Width = maxX - minX,
							Height = maxY - minY
						},
						_sheet
					));

					bool TryMove(int x, int y) {
						if (x < 0 || y < 0 || x >= _source.Width || y >= _source.Height)
							return false;

						if (IsUsed(x, y))
							return false;

						var col = _source.GetPixel(x, y);
						if (IsWhitespaceColour(ref col)) {
							return false;
						}

						path.Push(new PathNode {
							X = x,
							Y = y
						});

						used.Add(_sheet.Index(x, y));
						colours.Add((x, y, col));

						return true;
					}
				}
			}
		}

		return OrderSprites(sprites);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool IsUsed(int x, int y) {
			return used.Contains(_sheet.Index(x, y));
		}
	}

	private static List<Sprite> OrderSprites(List<Sprite> sprites) {
		var rows = new List<List<Sprite>>();

		foreach (var sprite in sprites) {
			var bb = sprite.BoundingBox;

			var row = rows.FirstOrDefault(r => r.Any(s => s.BoundingBox.OverlapsVertically(ref bb)));

			if (row is not null) {
				row.Add(sprite);
			} else {
				rows.Add(new List<Sprite> { sprite });
			}
		}

		return rows.Select(r => r.OrderBy(s => s.BoundingBox.X))
			.OrderBy(r => r.Min(s => s.BoundingBox.Y + s.BoundingBox.Height / 2))
			.SelectMany(r => r)
			.ToList();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool IsWhitespaceColour(ref Color c) {
		return c.R == _spaceColour.R
			&& c.G == _spaceColour.G
			&& c.B == _spaceColour.B
			&& c.A == _spaceColour.A;
	}

	public class PathNode {
		public int X;
		public int Y;

		public override string ToString() {
			return $"{X}, {Y}";
		}
	}
}