﻿using System.IO;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using RhuEngine.Linker;
using RNumerics;
using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace RhuEngine
{
	public sealed class ImageSharpTexture : IDisposable
	{
		public Image<Rgba32> Image { get; }
		public bool Srgb { get; }

		public int Width => Image.Width;

		public int Height => Image.Height;

		public ImageSharpTexture(string path) : this(SixLabors.ImageSharp.Image.Load<Rgba32>(path)) { }
		public ImageSharpTexture(string path, bool srgb) : this(SixLabors.ImageSharp.Image.Load<Rgba32>(path), srgb) { }
		public ImageSharpTexture(Stream stream) : this(SixLabors.ImageSharp.Image.Load<Rgba32>(stream)) { }
		public ImageSharpTexture(Stream stream, bool srgb) : this(SixLabors.ImageSharp.Image.Load<Rgba32>(stream), srgb) { }
		public ImageSharpTexture(Image<Rgba32> image) : this(image, true) { }
		public ImageSharpTexture(Image<Rgba32> image, bool srgb) {
			Image = image;
			Srgb = srgb;
		}
		RTexture2D _texture2D;
		public unsafe RTexture2D CreateTexture() {
			var colors = new Colorb[Height * Width];
			var hanndel = GCHandle.Alloc(colors, GCHandleType.Pinned);
			var pin = hanndel.AddrOfPinnedObject();
			Parallel.For(0, colors.Length, (i) => {
				var w = i % Width;
				var h = i / Width;
				var color = Image[w, h];
				((Rgba32*)pin)[i] = color;
			});
			hanndel.Free();
			_texture2D = RTexture2D.FromColors(colors, Width, Height, Srgb);
			return _texture2D;
		}
		public unsafe RTexture2D UpdateTexture() {
			if (_texture2D is null) {
				throw new Exception("Not started");
			}
			var colors = new Colorb[Height * Width];
			var hanndel = GCHandle.Alloc(colors, GCHandleType.Pinned);
			var pin = hanndel.AddrOfPinnedObject();
			Parallel.For(0, colors.Length, (i) => {
				var w = i % Width;
				var h = i / Height;
				var color = Image[w, h];
				((Rgba32*)pin)[i] = color;
			});
			hanndel.Free();
			_texture2D.SetColors(Width, Height, colors);
			return _texture2D;
		}
		public RTexture2D CreateTextureAndDisposes() {
			var newtex = CreateTexture();
			Dispose();
			return newtex;
		}
		public void Dispose() {
			Image?.Dispose();
		}
	}
}