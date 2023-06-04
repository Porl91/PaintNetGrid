using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

[assembly: AssemblyTitle("PaintNetGrid plugin for Paint.NET")]
[assembly: AssemblyDescription("PaintNetGrid selected pixels")]
[assembly: AssemblyConfiguration("paintnetgrid")]
[assembly: AssemblyCompany("PaintNetGrid")]
[assembly: AssemblyProduct("PaintNetGrid")]
[assembly: AssemblyCopyright("Copyright ©2023 by PaintNetGrid")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyMetadata("BuiltByCodeLab", "Version=6.8.8422.38978")]
[assembly: SupportedOSPlatform("Windows")]

namespace PaintNetGrid;

public class PluginSupportInfo : IPluginSupportInfo {
	public string Author => GetType().Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
	public string Copyright => GetType().Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;
	public string DisplayName => GetType().Assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
	public Version Version => GetType().Assembly.GetName().Version;
	public Uri WebsiteUri => new Uri("https://www.getpaint.net/redirect/plugins.html");
}

[PluginSupportInfo(typeof(PluginSupportInfo), DisplayName = "PaintNetGrid")]
public partial class PaintNetGridPlugin : PropertyBasedEffect {
	public static string StaticName => "PaintNetGrid";
	public static Image StaticIcon => null;
	public static string SubmenuName => "Advanced";

	public PaintNetGridPlugin()
		: base(StaticName, StaticIcon, SubmenuName, new EffectOptions {
			Flags = EffectFlags.Configurable,
			RenderingSchedule = EffectRenderingSchedule.None
		}) {
	}

	public enum PropertyNames {
		Width,
		Height
	}

	protected override PropertyCollection OnCreatePropertyCollection() {
		var props = new List<Property> {
			new Int32Property(PropertyNames.Width, 32, 0, 100),
			new Int32Property(PropertyNames.Height, 32, 0, 100)
		};

		return new PropertyCollection(props);
	}

	protected override ControlInfo OnCreateConfigUI(PropertyCollection props) {
		var configUI = CreateDefaultConfigUI(props);

		configUI.SetPropertyControlValue(PropertyNames.Width, ControlInfoPropertyNames.DisplayName, "Width");
		configUI.SetPropertyControlValue(PropertyNames.Width, ControlInfoPropertyNames.ShowHeaderLine, false);
		configUI.SetPropertyControlValue(PropertyNames.Height, ControlInfoPropertyNames.DisplayName, "Height");
		configUI.SetPropertyControlValue(PropertyNames.Height, ControlInfoPropertyNames.ShowHeaderLine, false);

		return configUI;
	}

	protected override void OnCustomizeConfigUIWindowProperties(PropertyCollection props) {
		// Change the effect's window title
		props[ControlInfoPropertyNames.WindowTitle].Value = "PaintNetGrid";

		base.OnCustomizeConfigUIWindowProperties(props);
	}

	protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken token, RenderArgs dstArgs, RenderArgs srcArgs) {
		Width = token.GetProperty<Int32Property>(PropertyNames.Width).Value;
		Height = token.GetProperty<Int32Property>(PropertyNames.Height).Value;

		base.OnSetRenderInfo(token, dstArgs, srcArgs);
	}

	protected override unsafe void OnRender(Rectangle[] rois, int startIndex, int length) {
		if (length == 0)
			return;

		for (var i = startIndex; i < startIndex + length; ++i) {
			Render(DstArgs.Surface, SrcArgs.Surface, rois[i]);
		}
	}
}
