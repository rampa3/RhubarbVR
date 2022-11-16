﻿using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

using Esprima.Ast;

using RhuEngine.Components.PrivateSpace;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

using static System.Collections.Specialized.BitVector32;

namespace RhuEngine.Components
{
	[PrivateSpaceOnly]
	[UpdateLevel(UpdateEnum.Normal)]
	public sealed class PrivateSpaceWindow : Component
	{
		public ButtonBase MoveButon { get; set; }
		public ButtonBase ResizeButon { get; set; }

		public ProgramWindow Window { get; set; }

		public PrivateSpaceTaskbarItem PrivateSpaceTaskbarItem { get; set; }

		public event Action OnMinimize;

		public void InitPrivateSpaceWindow(ProgramWindow window) {
			Window = window;
			Window.PrivateSpaceWindow = this;
			LoadTaskBarItem();
			LoadUI();
			Window.OnViewportUpdate += Window_OnViewportUpdate;
			window.OnUpdatedData += Window_OnUpdatedData;
			window.OnClosedWindow += Window_OnClosedWindow;
			window.OnUpdatePosAndScale += Window_OnUpdatePosAndScale;
			Window_OnViewportUpdate();
			Window_OnUpdatedData();
		}

		private void Window_OnUpdatePosAndScale() {
			if (RootElement is null) {
				return;
			}
			RootElement.Min.Value = Vector2f.Zero;
			RootElement.Max.Value = Vector2f.Zero;
			var min = Window.Pos;
			var max = Window.Pos + (Vector2f)Window.SizePixels + new Vector2f(0, 35);
			RootElement.MaxOffset.Value = new Vector2f(min.x, max.y);
			RootElement.MinOffset.Value = new Vector2f(max.x, min.y);
			UpdateVRPos();
		}

		private void UpdateVRPos() {
			if (WindowVRElement is null) {
				return;
			}
			var min = Window.Pos;
			var windSize = (Vector2f)Window.SizePixels;
			var max = Window.Pos + windSize + new Vector2f(0, 35);
			if (Collapse) {
				min += windSize * new Vector2f(0, 1);
			}
			if (!(_moveWindow || _resizeWindow)) {
				WindowVRElement.MaxOffset.Value = new Vector2i(max.x, -min.y) + new Vector2i(5);
				WindowVRElement.MinOffset.Value = new Vector2i(min.x, -max.y) - new Vector2i(5);
				if(WindowVRElement.Min.Value + WindowVRElement.Max.Value != new Vector2f(0, 2)) {
					WindowVRElement.Min.Value = WindowVRElement.Max.Value = new Vector2f(0, 1);
				}
			}
			else {
				if (WindowVRElement.Min.Value + WindowVRElement.Max.Value == new Vector2f(0, 2)) {
					WindowVRElement.Min.Value = Vector2f.Zero;
					WindowVRElement.Max.Value = Vector2f.One;

					WindowVRElement.MaxOffset.Value = Vector2i.Zero;
					WindowVRElement.MinOffset.Value = new Vector2i(0,100);

				}
			}
			WindowVRElement.Entity.enabled.Value = NotMinimized;
		}

		private void Window_OnUpdatedData() {
			if (RootElement is null) {
				return;
			}
			Label.Text.Value = Window?.WindowTitle;
			CloseButton.Entity.enabled.Value = Window?.CanClose ?? false;
		}

		public UIElement RootElement { get; set; }
		public CanvasMesh WindowVRElement { get; set; }

		public TextLabel Label { get; set; }
		public Button CloseButton { get; set; }
		public UIElement TopElement { get; set; }

		public RawAssetProvider<RTexture2D> Clapse { get; set; }

		public bool Collapse
		{
			get => !NotCollapse;
			set => NotCollapse = !value;
		}

		public bool NotCollapse
		{
			get => TopElement.Entity.enabled.Value;
			set {
				TopElement.Entity.enabled.Value = value;
				if (NotCollapse) {
					Clapse.LoadAsset(Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.Collapse));
				}
				else {
					Clapse.LoadAsset(Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.Uncollapse));
				}
				UpdateVRPos();
			}
		}

		public bool Minimized
		{
			get => !NotMinimized;
			set => NotMinimized = !value;
		}

		public bool NotMinimized
		{
			get => RootElement?.Entity?.enabled?.Value ?? false;
			set {
				if (RootElement?.Entity?.enabled is null) {
					return;
				}
				RootElement.Entity.enabled.Value = value;
				OnMinimize?.Invoke();
				UpdateVRPos();
			}
		}

		private void Window_OnViewportUpdate() {
			RootElement?.Entity?.Destroy();
			WindowVRElement?.Entity?.Destroy();
			if (Window?.TargetViewport is null) {
				return;
			}
			if (PrivateSpaceManager?.UserInterfaceManager?.VrElements is null) {
				return;
			}

			WindowVRElement = PrivateSpaceManager.UserInterfaceManager.VrElements.AddChild("StartVR").AttachMesh<CanvasMesh>(PrivateSpaceManager.UserInterfaceManager.UImaterial);
			WindowVRElement.TopOffset.Value = false;
			WindowVRElement.FrontBindRadus.Value += 1f;
			WindowVRElement.Scale.Value += new Vector3f(1, 0, 0);
			WindowVRElement.Min.Value = WindowVRElement.Max.Value = new Vector2f(0, 1);
			var ee = WindowVRElement.Entity.AttachComponent<ValueCopy<Vector2i>>();
			ee.Target.Target = WindowVRElement.Resolution;
			ee.Source.Target = PrivateSpaceManager.VRViewPort.Size;
			WindowVRElement.InputInterface.Target = PrivateSpaceManager.VRViewPort;


			var root = PrivateSpaceManager.UserInterfaceManager.Windows;
			RootElement = root.Entity.AddChild(Window.Name).AttachComponent<UIElement>();
			RootElement.InputFilter.Value = RInputFilter.Pass;
			var top = RootElement.Entity.AddChild("Top").AttachComponent<UIElement>();
			TopElement = top;
			top.MaxOffset.Value = new Vector2f(0, -35);
			top.GrowHorizontal.Value = RGrowHorizontal.Both;
			top.GrowVertical.Value = RGrowVertical.Both;
			top.Entity.AddChild("Back").AttachComponent<Panel>();
			var viewportConnector = top.Entity.AddChild("UI").AttachComponent<ViewportConnector>();
			viewportConnector.Target.Target = Window.TargetViewport;

			var bottom = RootElement.Entity.AddChild("Bottom").AttachComponent<UIElement>();
			bottom.Min.Value = new Vector2f(0, 1);
			bottom.MinOffset.Value = new Vector2f(0, -35);
			bottom.GrowHorizontal.Value = RGrowHorizontal.Both;
			bottom.GrowVertical.Value = RGrowVertical.Top;
			bottom.Entity.AddChild("BackGround").AttachComponent<Panel>();
			var elements = bottom.Entity.AddChild("Elements").AttachComponent<BoxContainer>();
			(Button, RawAssetProvider<RTexture2D>) AddButton(string buttonName, RTexture2D icon, Action action = null) {
				var buton = elements.Entity.AddChild(buttonName).AttachComponent<Button>();
				buton.IconAlignment.Value = RButtonAlignment.Center;
				buton.ExpandIcon.Value = true;
				buton.MinSize.Value = new Vector2i(37, 0);
				var texture = buton.Entity.AttachComponent<RawAssetProvider<RTexture2D>>();
				buton.Icon.Target = texture;
				texture.LoadAsset(icon);
				buton.Pressed.Target = action;
				return (buton, texture);
			}
			var buton = elements.Entity.AddChild("Resize").AttachComponent<ButtonBase>();
			buton.CursorShape.Value = RCursorShape.Bdiagsize;
			ResizeButon = buton;
			buton.MinSize.Value = new Vector2i(37, 0);
			buton.ButtonDown.Target = ResizeDown;
			buton.ButtonUp.Target = ResizeUp;

			MoveButon = elements.Entity.AddChild("MoveWindow").AttachComponent<ButtonBase>();
			MoveButon.CursorShape.Value = RCursorShape.Move;
			MoveButon.MinSize.Value = new Vector2i(37, 0);
			MoveButon.ButtonMask.Value = RButtonMask.Primary | RButtonMask.Secondary;
			MoveButon.ButtonDown.Target = MoveWindowDown;
			MoveButon.ButtonUp.Target = MoveWindowUp;
			MoveButon.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			Label = MoveButon.Entity.AddChild("Label").AttachComponent<TextLabel>();
			Label.InputFilter.Value = RInputFilter.Pass;
			Label.TextSize.Value = 15;
			Label.Text.Value = Window.WindowTitle;
			Label.HorizontalAlignment.Value = RHorizontalAlignment.Center;
			Label.VerticalAlignment.Value = RVerticalAlignment.Center;

			AddButton("PopOut", Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.Share), Popout);
			Clapse = AddButton("Clapse", Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.Collapse), ClapseToggle).Item2;
			AddButton("Minimize", Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.Minimize), MinimizeWindow);
			CloseButton = AddButton("Close", Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.Close), CloseWindow).Item1;
			OnMinimize?.Invoke();
			UpdateVRPos();
		}

		[Exposed]
		public void Popout() {
			var newWin = Engine.windowManager.CreateNewWindow();
			newWin.WaitOnLoadedIn((win) => {
				void SizeChange() {
					if (win.Size != Window.SizePixels) {
						Window.SizePixels = win.Size;
					}
				}
				win.SizeChanged += SizeChange;
				void Resize() {
					if (win.Size != Window.SizePixels) {
						win.Size = Window.SizePixels;
					}
				}
				Window.OnUpdatePosAndScale += Resize;
				win.Viewport = Window.TargetViewport;
				win.CloseRequested += () => {
					Window.OnUpdatePosAndScale -= Resize;
					win.SizeChanged -= SizeChange;
					win.Dispose();
				};
				win.Title = Window.WindowTitle;
				win.Size = Window.TargetViewport.Size.Value;
			});
		}

		private bool _moveWindow;
		private Vector2f _offsetPos;

		private bool _resizeWindow;
		private Vector2f _offsetResize;
		private Vector2f _offsetResizeLastWindow;
		private Vector2f _offsetResizeLastWindowPos;

		[Exposed]
		public void MoveWindowDown() {
			_moveWindow = true;
			_offsetPos = MoveButon.MainPos - Window.Pos;
			UpdateVRPos();
		}
		[Exposed]
		public void ResizeDown() {
			_resizeWindow = true;
			_offsetResize = ResizeButon.MainPos;
			_offsetResizeLastWindow = (Vector2f)Window.SizePixels;
			_offsetResizeLastWindowPos = Window.Pos;
			UpdateVRPos();
		}

		[Exposed]
		public void MoveWindowUp() {
			_moveWindow = false;
			UpdateVRPos();
		}
		[Exposed]
		public void ResizeUp() {
			_resizeWindow = false;
			UpdateVRPos();
		}

		[Exposed]
		public void ClapseToggle() {
			NotCollapse = !NotCollapse;
		}

		[Exposed]
		public void MinimizeWindow() {
			NotMinimized = false;
		}

		[Exposed]
		public void CloseWindow() {
			if (!(Window?.CanClose ?? false)) {
				return;
			}
			Window?.Close();
		}

		private void Window_OnClosedWindow() {
			Entity.Destroy();
		}

		public override void Dispose() {
			base.Dispose();
			PrivateSpaceTaskbarItem?.WindowClosed();
			RootElement?.Entity?.Destroy();
			WindowVRElement?.Entity?.Destroy();
		}

		private void LoadTaskBarItem() {
			PrivateSpaceTaskbarItem = PrivateSpaceManager.UserInterfaceManager._taskbarElementHolder?.Entity?.AddChild(Window.WindowTitle)?.AttachComponent<PrivateSpaceTaskbarItem>();
			PrivateSpaceTaskbarItem?.OpennedPorgram(this);
		}

		private void LoadUI() {

		}

		protected override void AlwaysStep() {
			base.AlwaysStep();
			if (_moveWindow) {
				Window.Pos = MoveButon.MainPos - _offsetPos;
			}

			if (_resizeWindow) {
				var change = (_offsetResize - ResizeButon.MainPos) * new Vector2f(1, -1);
				var newSIze = change + _offsetResizeLastWindow;
				var sizeDef = MathUtil.Abs(Window.SizePixels - new Vector2i(newSIze.x, newSIze.y));
				if (sizeDef.x + sizeDef.y > 5) {
					Window.SizePixels = new Vector2i(newSIze.x, newSIze.y);
					Window.Pos = _offsetResizeLastWindowPos - new Vector2f(change.x, 0);
				}
			}
		}

	}
}