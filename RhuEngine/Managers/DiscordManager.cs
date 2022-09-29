﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using RhuEngine.Components;
using RhuEngine.DataStructure;
using RhuEngine.WorldObjects;

using SharedModels;
using SharedModels.GameSpecific;

using RNumerics;
using RhuEngine.Linker;
using DataModel.Enums;
using Esprima;
using DiscordRPC;
using DiscordRPC.Logging;
using Jint;

namespace RhuEngine.Managers
{
	public sealed class DiscordManager : IManager, ILogger
	{

		public DiscordRpcClient discordRpcClient;

		public DiscordRPC.Logging.LogLevel Level { get; set; }

		public void Dispose() {
			discordRpcClient?.Dispose();
		}

		public void Error(string message, params object[] args) {
			RLog.Err(message + string.Join(",", args));
		}

		public void Info(string message, params object[] args) {
			RLog.Info(message + string.Join(",", args));
		}
		public void Trace(string message, params object[] args) {
			RLog.Info(message + string.Join(",", args));
		}

		public void Warning(string message, params object[] args) {
			RLog.Warn(message + string.Join(",", args));
		}

		private Engine _engine;

		public void Init(Engine engine) {
			try {
				_engine = engine;
				discordRpcClient = new DiscordRpcClient("678074691738402839") {
					Logger = this,
				};
				discordRpcClient.OnReady += DiscordRpcClient_OnReady;
				discordRpcClient.OnPresenceUpdate += DiscordRpcClient_OnPresenceUpdate;
				discordRpcClient.Initialize();
				StartUpData();
			}
			catch {
				RLog.Err("Failed to start discordRPC");
				discordRpcClient = null;
			}
		}

		private void WorldManager_WorldChanged(World obj) {
			FocusWorld(obj);
		}

		public void FocusWorld(World world) {
			if (world is null) {
				return;
			}
			try {
				var stateMsg = "In Private World";
				var haveParty = false;
				if (world == world.worldManager.LocalWorld) {
					stateMsg = "In Local World";
				}
				else {
					if (!world.IsHidden.Value) {
						if (world.AccessLevel.Value == AccessLevel.Public) {
							stateMsg = $"In {world.SessionName.Value}";
							haveParty = true;
						}
					}
				}
				if (haveParty) {
					try {
						discordRpcClient?.SetPresence(new RichPresence() {
							Details = "In the game",
							State = stateMsg,
							Assets = new Assets() {
								LargeImageKey = "rhubarbvr",
								LargeImageText = "RhubarbVR",
								SmallImageKey = "rhubarbvr2"
							},
							Timestamps = new Timestamps() {
								StartUnixMilliseconds = (ulong)new DateTimeOffset(world.StartTime.Value).ToUnixTimeMilliseconds()
							},
							Party = new Party() {
								ID = world.SessionID.Value ?? "null",
								Max = world.MaxUserCount,
								Size = world.ConnectedUserCount,
								Privacy = Party.PrivacySetting.Private,
							}
						});
					}
					catch {
						discordRpcClient?.SetPresence(new RichPresence() {
							Details = "In the game",
							State = stateMsg,
							Assets = new Assets() {
								LargeImageKey = "rhubarbvr",
								LargeImageText = "RhubarbVR",
								SmallImageKey = "rhubarbvr2"
							},
						});
					}
				}
				else {
					discordRpcClient?.SetPresence(new RichPresence() {
						Details = "In the game",
						State = stateMsg,
						Assets = new Assets() {
							LargeImageKey = "rhubarbvr",
							LargeImageText = "RhubarbVR",
							SmallImageKey = "rhubarbvr2"
						},
					});
				}
			}
			catch { }
		}
		public void StartUpData() {
			discordRpcClient?.SetPresence(new RichPresence() {
				Details = "In the Engine",
				State = "Starting Rhubarb",
				Assets = new Assets() {
					LargeImageKey = "rhubarbvr",
					LargeImageText = "RhubarbVR",
					SmallImageKey = "rhubarbvr2"
				}
			});
		}

		private void DiscordRpcClient_OnPresenceUpdate(object sender, DiscordRPC.Message.PresenceMessage args) {
		}

		private void DiscordRpcClient_OnReady(object sender, DiscordRPC.Message.ReadyMessage args) {
		}

		public void RenderStep() {

		}

		public short TimeLastUpdate { get; set; }

		public void Step() {
			if (discordRpcClient is not null) {
				if (TimeLastUpdate > 90) {
					WorldManager_WorldChanged(_engine.worldManager.FocusedWorld);
					TimeLastUpdate = 0;
				}
				else {
					TimeLastUpdate++;
				}
			}
		}


	}
}
