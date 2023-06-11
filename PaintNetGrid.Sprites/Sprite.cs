namespace PaintNetGrid.Sprites;

public sealed class Sprite {
	public Sprite(List<(int, int, Color Colour)> colours, AABB boundingBox, ISpritesheet sheet) {
		Colours = colours;
		BoundingBox = boundingBox;

		_sheet = sheet;
	}

	public List<(int X, int Y, Color Colour)> Colours { get; }
	public AABB BoundingBox { get; }

	private readonly ISpritesheet _sheet;
	private HashSet<int> _colourIndex;

	public bool Overlaps(int x, int y) {
		_colourIndex ??= Colours.Select(m => m.Y * _sheet.Width + m.X).ToHashSet();

		return _colourIndex.Contains(_sheet.Index(x, y));
	}
}
