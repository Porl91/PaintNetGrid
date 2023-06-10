using System.Drawing;

namespace PaintNetGrid.Sprites;

public sealed class Sprite {
	public Sprite(List<(int, int, Color Col)> mask, AABB boundingBox, ISpritesheet sheet) {
		Mask = mask;
		BoundingBox = boundingBox;

		_sheet = sheet;
	}

	public List<(int X, int Y, Color Col)> Mask { get; }
	public AABB BoundingBox { get; }

	private readonly ISpritesheet _sheet;
	private HashSet<int> _maskIndex;

	public bool OverlapsMask(int x, int y) {
		_maskIndex ??= Mask.Select(m => m.Y * _sheet.Width + m.X).ToHashSet();

		return _maskIndex.Contains(_sheet.Index(x, y));
	}
}
