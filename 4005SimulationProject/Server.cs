// Copyright (C) 2014 M. Damian Mulligan (G'lek Tarssza)
// All Rights Reserved.
using System;
using System.Xml;

namespace SimulationProject
{
	/// <summary>
	/// The server that is processing packets.
	/// </summary>
	public class Server
	{
		#region Fields

		/// <summary>
		/// The simulation.
		/// </summary>
		Simulation simulation;

		#endregion

		#region Properties

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="SimulationProject.Server"/> class.
		/// </summary>
		/// <param name="simulation">The simulation.</param>
		public Server (Simulation simulation)
		{
			this.simulation = simulation;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Prints the results.
		/// </summary>
		public void PrintResults ()
		{
			Simulation.ResultLogger.Info (string.Format ("Server running in {0} mode", scheduling));
		}

		/// <summary>
		/// Load the server from the specified XML node.
		/// </summary>
		/// <param name="node">The XML node to load from.</param>
		public bool Load (XmlNode node)
		{

		}

		#endregion
	}
}
