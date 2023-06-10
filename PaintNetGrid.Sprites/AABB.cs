namespace PaintNetGrid.Sprites;

public struct AABB {
	public int X;
	public int Y;
	public int Width;
	public int Height;

	public bool Contains(int x, int y) {
		return !(x < X || y < Y || x >= X + Width || y >= Y + Height);
	}

	public bool OverlapsVertically(ref AABB other) {
		return !(Y + Height < other.Y || Y > other.Y + other.Height);
	}
}