﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using RhubarbCloudClient;

using RhuEngine.Commads;
using RhuEngine.Components.PrivateSpace.Programs.OverlayDialogues;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

using TextCopy;

namespace RhuEngine.Components
{
	[PrivateSpaceOnly]
	[ProgramOpenWith("text/uri-list", "text/x-uuencode", "text/plain", "nofile/uri")]
	[UpdateLevel(UpdateEnum.Normal)]
	public sealed class OpenBrowserProgram : PrivateSpaceProgram
	{
		public override RhubarbAtlasSheet.RhubarbIcons IconFind => RhubarbAtlasSheet.RhubarbIcons.Link;

		public override string ProgramNameLocName => "Program.OpenBrowser.Name";

		public Uri targetURI;

		public override void StartProgram(Stream file = null, string mimetype = null, string ex = null, params object[] args) {
			if ((args?.Length ?? 0) >= 1) {
				if (args[0] is Uri target) {
					targetURI = target;
				}
				if (args[0] is string targetSting) {
					Uri.TryCreate(targetSting, UriKind.RelativeOrAbsolute, out targetURI);
				}
			}
			if (file is not null) {
				using var reader = new StreamReader(file, true);
				Uri.TryCreate(reader.ReadToEnd(), UriKind.RelativeOrAbsolute, out targetURI);
			}

			if (targetURI is null) {
				CloseProgram();
			}
			else {
				BuildUI(AddWindow());
			}
		}

		private void BuildUI(ViewPortProgramWindow programWindow) {
			programWindow.Size.Value = new Vector2i(programWindow.Size.Value.x * 0.9f, 335);
			programWindow.CenterWindowIntoView();


			var root = programWindow.Entity.AddChild("Root").AttachComponent<ScrollContainer>().Entity.AddChild("Stuff").AttachComponent<BoxContainer>();
			root.Vertical.Value = true;
			root.Alignment.Value = RBoxContainerAlignment.Center;
			root.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			var elemntheader = root.Entity.AddChild().AttachComponent<TextLabel>();
			elemntheader.MinSize.Value = new Vector2i(0, 60);
			elemntheader.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			var elemntheaderlocliztion = elemntheader.Entity.AttachComponent<StandardLocale>();
			elemntheaderlocliztion.TargetValue.Target = elemntheader.Text;
			elemntheaderlocliztion.Key.Value = "Program.OpenBrowser.Name";
			elemntheader.TextSize.Value = 30;


			var elemntsubheader = root.Entity.AddChild().AttachComponent<TextLabel>();
			elemntsubheader.MinSize.Value = new Vector2i(0, 200);
			elemntsubheader.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			elemntsubheader.AutowrapMode.Value = RAutowrapMode.WordSmart;
			elemntsubheader.OverrunBehavior.Value = ROverrunBehavior.NoTrimming;
			var elemntsubheaderlocliztion = elemntsubheader.Entity.AttachComponent<StandardLocale>();
			elemntsubheaderlocliztion.TargetValue.Target = elemntsubheader.Text;
			elemntsubheaderlocliztion.Key.Value = "Program.OpenBrowser.Text;" + targetURI?.ToString() ?? "NULL";
			elemntsubheader.TextSize.Value = 25;

			var colection = root.Entity.AddChild().AttachComponent<BoxContainer>();
			colection.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand | RFilling.ShrinkCenter;
			colection.MinSize.Value = new Vector2i(235, 45);
			colection.Alignment.Value = RBoxContainerAlignment.Center;
			var yesButton = colection.Entity.AddChild().AttachComponent<Button>();
			yesButton.MinSize.Value = new Vector2i(100, 45);
			yesButton.Alignment.Value = RButtonAlignment.Center;
			yesButton.Pressed.Target = OpenWebbrowserCall;
			var local = yesButton.Entity.AttachComponent<StandardLocale>();
			local.TargetValue.Target = yesButton.Text;
			local.Key.Value = "Program.OpenBrowser.Yes";
			colection.Entity.AddChild().AttachComponent<UIElement>().MinSize.Value = new Vector2i(35);
			var noButton = colection.Entity.AddChild().AttachComponent<Button>();
			noButton.MinSize.Value = new Vector2i(100, 45);
			noButton.Alignment.Value = RButtonAlignment.Center;
			noButton.Pressed.Target = CloseProgramCall;
			var localno = noButton.Entity.AttachComponent<StandardLocale>();
			localno.TargetValue.Target = noButton.Text;
			localno.Key.Value = "Program.OpenBrowser.No";

		}

		[Exposed]
		public void OpenWebbrowserCall() {
			Process.Start(new ProcessStartInfo {
				//Todo Could be abused
				FileName = targetURI?.ToString(),
				UseShellExecute = true
			});
			CloseProgram();
		}

		[Exposed]
		public void CloseProgramCall() {
			CloseProgram();
		}

	}
}