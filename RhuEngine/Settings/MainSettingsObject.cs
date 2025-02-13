﻿using System;
using System.Collections.Generic;
using System.Text;

using DataModel.Enums;

using RhuSettings;

namespace RhuEngine.Settings
{
	public class MainSettingsObject<T> : MainSettingsObject where T : RenderSettingsBase, new()
	{
		[SettingsField("Render Settings")]
		public T renderSettings = new();

		public override RenderSettingsBase RenderSettings => renderSettings;
	}

	public abstract class MainSettingsObject : SettingsObject
	{
		public abstract RenderSettingsBase RenderSettings { get; }

		[SettingsField("Input Settings")]
		public InputSettingsObject InputSettings = new();

		[SettingsField("Desktop Fov")]
		public float Fov = 90;

		[SettingsField("Three Letter Language Name")]
		public string ThreeLetterLanguageName = null;
		[SettingsField()]
		public string MainMic = null;
	}
}
