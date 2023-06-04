using PaintDotNet;

using PaintNetGrid.Sprites;

using System.Drawing;

namespace PaintNetGrid;

public partial class PaintNetGridPlugin {
	#region UICode
	int Width = 32; // [0,100] Width
	int Height = 32; // [0,100] Height
	#endregion

	internal void Render(Surface dst, Surface src, Rectangle rect) {
		for (var y = 0; y < dst.Height; y++) {
			for (var x = 0; x < dst.Width; x++) {
				if (x % Width == 0 || y % Height == 0) {
					dst[x, y] = ColorBgra.FromBgra(255, 0, 255, 255);
				} else {
					dst[x, y] = ColorBgra.Transparent;
				}
			}
		}

		var source = new SurfaceSourceBuffer2D(src);
		var spriteMasks = new SpriteExtractor(source, Color.FromArgb(255, 247, 247, 247))
			.ComputeMasks();

		var cellStartX = 0;
		var cellStartY = 0;

		foreach (var sprite in spriteMasks) {
			if (cellStartX + Width >= dst.Width) {
				cellStartX = 0;
				cellStartY += Height;
			}

			foreach (var (X, Y, Col) in sprite.Mask) {

				// TODO: Add anchor point(s) rather than centering

				var bbEdgeDistanceX = X - sprite.BoundingBox.X;
				var bbEdgeDistanceY = Y - sprite.BoundingBox.Y;

				var bbStartX = (Width - sprite.BoundingBox.Width) / 2;
				var bbStartY = (Height - sprite.BoundingBox.Height) / 2;

				var x = bbStartX + cellStartX + bbEdgeDistanceX;
				var y = bbStartY + cellStartY + bbEdgeDistanceY;

				if (x < 0 || y < 0 || x >= dst.Width || y >= dst.Height) {
					continue;
				}

				dst[x, y] = Col;
			}

			cellStartX += Width;
		}
	}
}
