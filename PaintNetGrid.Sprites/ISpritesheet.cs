namespace PaintNetGrid.Sprites;

public interface ISpritesheet {
	int Width { get; }

	int Index(int x, int y);
}
