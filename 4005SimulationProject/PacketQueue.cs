// Copyright (C) 2014 M. Damian Mulligan (G'lek Tarssza)
// All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Xml;

namespace SimulationProject
{
	/// <summary>
	/// A queue of packets.
	/// </summary>
	public class PacketQueue
	{
		#region Fields

		/// <summary>
		/// The queue.
		/// </summary>
		Queue<Packet> queue;
		/// <summary>
		/// The simulation.
		/// </summary>
		Simulation simulation;
		/// <summary>
		/// The arrival rate.
		/// </summary>
		double arrivalRate;
		/// <summary>
		/// The average occupancy of the queue.
		/// </summary>
		double averageOccupancy;
		/// <summary>
		/// The connectivity chance per server.
		/// </summary>
		double[] connectivityChance;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the arrival rate.
		/// </summary>
		/// <value>The arrival rate.</value>
		public double ArrivalRate {
			get {
				return arrivalRate;
			}
		}

		/// <summary>
		/// Gets the count.
		/// </summary>
		/// <value>The count.</value>
		public int Count {
			get {
				return queue.Count;
			}
		}

		/// <summary>
		/// Gets the average occupancy of the queue.
		/// </summary>
		/// <value>The average occupancy of the queue.</value>
		public double AverageOccupancy {
			get {
				return averageOccupancy;
			}
		}

		/// <summary>
		/// Gets the connectivity chance per server.
		/// </summary>
		/// <value>The connectivity chance per server.</value>
		public double[] ConnectivityChance {
			get {
				return connectivityChance;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="SimulationProject.PacketQueue"/> class.
		/// </summary>
		public PacketQueue (Simulation simulation)
		{
			queue = new Queue<Packet> ();
			this.simulation = simulation;
			averageOccupancy = 0.0;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Update this instance.
		/// </summary>
		public void Update (int n, NumberGenerator gen)
		{
			if (gen.GetNextRandomNumber () <= arrivalRate) {
				//Simulation.Logger.Info (string.Format ("Packet added at time {0}", n));
				AddPacket ();
			}
			averageOccupancy = (queue.Count + ((n - 1) * averageOccupancy)) / (n);
		}

		/// <summary>
		/// Adds the packet.
		/// </summary>
		public void AddPacket ()
		{
			queue.Enqueue (new Packet ());
		}

		/// <summary>
		/// Service this instance.
		/// </summary>
		public void Service ()
		{
			//Simulation.Logger.Info ("Queue serviced");
			queue.Dequeue ();
		}

		/// <summary>
		/// Prints the results.
		/// </summary>
		public void PrintResults ()
		{
			Simulation.ResultLogger.Info (string.Format ("Arrival rate: {0}", arrivalRate));
			Simulation.ResultLogger.Info (string.Format ("Average occupancy: {0}", averageOccupancy));
			//Simulation.CSVLogger.Info (string.Format ("{0};{1}", arrivalRate, averageOccupancy));
		}

		/// <summary>
		/// Load packet from the specified XML node.
		/// </summary>
		/// <param name="node">The XML node to load from.</param>
		public bool Load (XmlNode node)
		{
			var xmlRootElement = node as XmlElement;
			if (xmlRootElement == null) {
				return false;
			}
			XmlNodeList list = xmlRootElement.GetElementsByTagName ("ArrivalRate");
			if (list.Count != 1) {
				return false;
			}
			var xmlElement = list [0] as XmlElement;
			if (xmlElement == null) {
				return false;
			}
			string value = xmlElement.GetAttribute ("value");
			if (!double.TryParse (value, out arrivalRate)) {
				return false;
			}
			list = xmlRootElement.GetElementsByTagName ("ConnectivityChance");
			if (list.Count != simulation.ServerCount) {
				return false;
			}
			connectivityChance = new double[list.Count];
			for (int i = 0; i < list.Count; i++) {
				connectivityChance [i] = -1.0;
			}
			for (int i = 0; i < list.Count; i++) {
				xmlElement = list [i] as XmlElement;
				if (xmlElement == null) {
					return false;
				}
				string index = xmlElement.GetAttribute ("server");
				value = xmlElement.GetAttribute ("value");
				int ind;
				if (!int.TryParse (index, out ind)) {
					return false;
				}
				if (connectivityChance [ind] >= 0.0) {
					return false;
				}
				if (!double.TryParse (value, out connectivityChance [ind])) {
					return false;
				}
			}
			return true;
		}

		#endregion
	}
}

