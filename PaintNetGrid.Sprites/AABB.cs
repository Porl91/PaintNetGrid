namespace PaintNetGrid.Sprites;

public struct AABB {
	public int X;
	public int Y;
	public int Width;
	public int Height;

	public bool Contains(int x, int y) {
		return !(x < X || y < Y || x >= X + Width || y >= Y + Height);
	}
}