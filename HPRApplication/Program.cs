﻿using System;

namespace RelayHolePuncher
{
	internal sealed class Program
	{
		static void Main(string[] args) {
			if (args is null) {
				throw new ArgumentNullException(nameof(args));
			}
			var server = new LiteNetLibService();
			server.Initialize();
			server.StartUpdateLoop();
		}
	}
}