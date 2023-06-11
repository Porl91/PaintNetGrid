using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.Imaging;
using PaintDotNet.PropertySystem;
using PaintDotNet.Rendering;

using PaintNetGrid.Sprites;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PaintNetGrid;

internal sealed class PaintNetGridPlugin : PropertyBasedBitmapEffect {
	private IBitmapSource<ColorBgra32> _sourceBitmap;
	private IBitmapSource<ColorAlpha8> _anchorsBitmap;
	private int _width;
	private int _height;
	private Alignment _alignment = Alignment.Center;

	public PaintNetGridPlugin() : base(
		"PaintNetGrid",
		"AAA", // TODO
		BitmapEffectOptionsFactory.Create() with {
			IsConfigurable = true
		}) {
	}

	private enum PropertyNames {
		Width,
		Height,
		Alignment
	}

	private enum Alignment {
		TopLeft,
		TopMiddle,
		TopRight,
		Left,
		Center,
		Right,
		BottomLeft,
		BottomCenter,
		BottomRight,
	}

	protected override PropertyCollection OnCreatePropertyCollection() {
		var alignments = Enum.GetNames<Alignment>();

		return new PropertyCollection(new List<Property> {
			new Int32Property(PropertyNames.Width, 32, 1, 256),
			new Int32Property(PropertyNames.Height, 32, 1, 256),
			new StaticListChoiceProperty(PropertyNames.Alignment, alignments, Array.IndexOf(alignments, _alignment.ToString()))
		});
	}

	protected override void OnInitializeRenderInfo(IBitmapEffectRenderInfo renderInfo) {
		var currentLayer = Environment.Document.Layers[Environment.SourceLayerIndex];

		_sourceBitmap = currentLayer.GetBitmap<ColorBgra32>();

		var anchorsLayer = Environment.Document.Layers.FirstOrDefault(l => l.Name.Equals($"{currentLayer.Name}-Anchors", StringComparison.OrdinalIgnoreCase));

		if (anchorsLayer is not null) {
			_anchorsBitmap = anchorsLayer.GetBitmap<ColorAlpha8>();
		}

		renderInfo.Schedule = BitmapEffectRenderingSchedule.None;

		base.OnInitializeRenderInfo(renderInfo);
	}

	protected override void OnSetToken(PropertyBasedEffectConfigToken newToken) {
		_width = newToken.GetProperty<Int32Property>(PropertyNames.Width).Value;
		_height = newToken.GetProperty<Int32Property>(PropertyNames.Height).Value;

		var alignmentValue = (string)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.Alignment).Value;
		if (!Enum.TryParse<Alignment>(alignmentValue, out var alignment)) {
			throw new InvalidOperationException($"Failed to parse invalid {nameof(Alignment)} name {alignmentValue}");
		}

		_alignment = alignment;

		base.OnSetToken(newToken);
	}

	protected override void OnRender(IBitmapEffectOutput output) {
		using var outputLock = output.LockBgra32();

		var outputRegion = outputLock.AsRegionPtr();

		using var sourceBitmap = _sourceBitmap.ToBitmap();

		if (IsCancelRequested) {
			return;
		}

		using var sourceLock = sourceBitmap.Lock(BitmapLockOptions.Read);

		var sourceRegion = sourceLock.AsRegionPtr();

		var source = new RegionPtrSourceBuffer2D(sourceRegion);
		var sprites = new SpriteExtractor(source, Color.FromArgb(255, 247, 247, 247)).Extract()
			.Select(s => new SpriteCandidate(s))
			.ToArray();

		if (IsCancelRequested) {
			return;
		}

		AttachAnchorPoints(sprites);

		if (IsCancelRequested) {
			return;
		}

		for (var y = 0; y < outputRegion.Height; y++) {
			if (IsCancelRequested) {
				return;
			}

			for (var x = 0; x < outputRegion.Width; x++) {
				if (x % _width == 0 || y % _height == 0) {
					outputRegion[x, y] = ColorBgra.FromBgra(255, 0, 255, 255);
				} else {
					outputRegion[x, y] = ColorBgra.Transparent;
				}
			}
		}

		var cellStartX = 0;
		var cellStartY = 0;

		foreach (var spriteCandidate in sprites) {
			var sprite = spriteCandidate.Sprite;
			var anchor = spriteCandidate.Anchor;

			if (IsCancelRequested) {
				return;
			}

			if (cellStartX + _width >= outputRegion.Width) {
				cellStartX = 0;
				cellStartY += _height;
			}

			// TODO: Fallback to sprite center point if the user hasn't specified an anchor or just leave as top-left?
			// Add another property and let the user set a fallback?

			(int X, int Y) sourceAnchorOffset = anchor switch {
				null => (0, 0),
				_ => (
					-(anchor.Value.X - sprite.BoundingBox.X),
					-(anchor.Value.Y - sprite.BoundingBox.Y)
				)
			};

			(int X, int Y) destAnchorOffset = _alignment switch {
				Alignment.TopLeft => (0, 0),
				Alignment.TopMiddle => (_width / 2, 0),
				Alignment.TopRight => (_width, 0),
				Alignment.Left => (0, _height / 2),
				Alignment.Center => (_width / 2, _height / 2),
				Alignment.Right => (_width, _height / 2),
				Alignment.BottomLeft => (0, _height),
				Alignment.BottomCenter => (_width / 2, _height),
				Alignment.BottomRight => (_width, _height),
				_ => throw new InvalidOperationException($"Unsupported {nameof(Alignment)}: {_alignment}")
			};

			foreach (var (X, Y, Colour) in sprite.Colours) {
				var spriteX = X - sprite.BoundingBox.X;
				var spriteY = Y - sprite.BoundingBox.Y;

				var x = cellStartX + spriteX + sourceAnchorOffset.X + destAnchorOffset.X;
				var y = cellStartY + spriteY + sourceAnchorOffset.Y + destAnchorOffset.Y;

				if (x < 0 || y < 0 || x >= outputRegion.Width || y >= outputRegion.Height) {
					continue;
				}

				outputRegion[x, y] = Colour;
			}

			cellStartX += _width;
		}
	}

	private void AttachAnchorPoints(SpriteCandidate[] sprites) {
		if (_anchorsBitmap is null) {
			return;
		}

		using var anchorsBitmap = _anchorsBitmap.ToBitmap();

		if (IsCancelRequested) {
			return;
		}

		using var anchorsLock = anchorsBitmap.Lock(BitmapLockOptions.Read);

		var anchorsRegion = anchorsLock.AsRegionPtr();

		for (var y = 0; y < anchorsRegion.Height; y++) {
			if (IsCancelRequested) {
				return;
			}

			for (var x = 0; x < anchorsRegion.Width; x++) {
				if (anchorsRegion[x, y].A > 0) {
					var sprite = sprites.FirstOrDefault(s => s.Sprite.BoundingBox.Contains(x, y));

					if (sprite != default) {
						sprite.Anchor = (x, y);
					}
				}
			}
		}
	}

	private record SpriteCandidate(Sprite Sprite) {
		public (int X, int Y)? Anchor { get; set; }
	}
}
