﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;

using RhubarbVR.Bindings.TextureBindings;

using RhuEngine.Components;
using RhuEngine.Input;
using RhuEngine.Linker;

using RNumerics;

using Key = RhuEngine.Linker.Key;
namespace RhubarbVR.Bindings.Input
{
	using Input = Godot.Input;

	public class GodotMouse : IMouseInputDriver
	{
		public Vector2f MousePos => EngineRunner._.MousePos;

		public Vector2f MouseDelta => EngineRunner._.MouseDelta;

		public Vector2f ScrollDelta => EngineRunner._.MouseScrollDelta;

		public bool GetIsDown(MouseKeys key) {
			return key switch {
				MouseKeys.MouseLeft => Input.IsMouseButtonPressed(MouseButton.Left),
				MouseKeys.MouseRight => Input.IsMouseButtonPressed(MouseButton.Right),
				MouseKeys.MouseCenter => Input.IsMouseButtonPressed(MouseButton.Middle),
				MouseKeys.MouseForward => Input.IsMouseButtonPressed(MouseButton.Xbutton1),
				MouseKeys.MouseBack => Input.IsMouseButtonPressed(MouseButton.Xbutton2),
				_ => Input.IsMouseButtonPressed(MouseButton.None),
			};
		}

		public bool HideMous = false;

		public static bool CenterMous = false;

		private void UpdateMouseMode() {
			Input.MouseMode = HideMous
				? CenterMous ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Hidden
				: CenterMous ? Input.MouseModeEnum.Confined : Input.MouseModeEnum.Visible;
		}

		public void HideMouse() {
			HideMous = true;
			UpdateMouseMode();
		}

		public void LockMouse() {
			CenterMous = true;
			UpdateMouseMode();
		}

		public void UnHideMouse() {
			HideMous = false;
			UpdateMouseMode();
		}

		public void UnLockMouse() {
			CenterMous = false;
			UpdateMouseMode();
		}

		public void SetCurrsor(RCursorShape currsor, RTexture2D rTexture2D) {
			if (rTexture2D is null) {
				Input.SetDefaultCursorShape((Input.CursorShape)currsor);
			}
			else {
				if (rTexture2D.Inst is GodotTexture2D godotTexture) {
					Input.SetCustomMouseCursor(godotTexture.Texture, (Input.CursorShape)currsor);
				}
			}
		}
	}
}