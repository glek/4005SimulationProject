// Copyright (C) 2014 M. Damian Mulligan (G'lek Tarssza)
// All Rights Reserved.
using log4net;
using System;

namespace SimulationProject
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			string file = "simulation.xml";
			if (args.Length >= 1) {
				file = args [0];
			}
			var simulation = new Simulation ();
			if (!simulation.Load (file)) {
				Simulation.Logger.Error ("Failed to load simulation");
			}
		}
	}
}
