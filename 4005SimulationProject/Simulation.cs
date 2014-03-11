// Copyright (C) 2014 M. Damian Mulligan (G'lek Tarssza)
// All Rights Reserved.
using log4net;
using System;
using System.Collections.Generic;
using System.Xml;

namespace SimulationProject
{
	/// <summary>
	/// Scheduling type.
	/// </summary>
	public enum SchedulingType
	{
		Random,
		RoundRobin,
		LongestFirst,
		AnyLongestFirst,
		LeastConnectedLongestFirst
	}

	/// <summary>
	/// The simulation, containing all simulation details.
	/// </summary>
	public class Simulation
	{
		#region Fields

		/// <summary>
		/// The logger.
		/// </summary>
		public static readonly ILog Logger = LogManager.GetLogger ("Simulation");
		/// <summary>
		/// The result logger.
		/// </summary>
		public static readonly ILog ResultLogger = LogManager.GetLogger ("Result");
		/// <summary>
		/// The CSV logger.
		/// </summary>
		public static readonly ILog CSVLogger = LogManager.GetLogger ("CSV");
		/// <summary>
		/// The number of replications to run.
		/// </summary>
		uint replications;
		/// <summary>
		/// The number of time slots per replication.
		/// </summary>
		uint timeSlots;
		/// <summary>
		/// The current time slot.
		/// </summary>
		uint currentTimeSlot;
		/// <summary>
		/// The current replication.
		/// </summary>
		uint currentReplication;
		/// <summary>
		/// The queues to hold packets.
		/// </summary>
		PacketQueue[] queues;
		/// <summary>
		/// The queue connectivities.
		/// </summary>
		uint[,] queueConnectivity;
		/// <summary>
		/// The server count.
		/// </summary>
		uint serverCount;
		/// <summary>
		/// The queue selected by a server.
		/// </summary>
		int[] serverSelectedQueue;
		/// <summary>
		/// The random number generator.
		/// </summary>
		NumberGenerator rand;
		/// <summary>
		/// The a parameter.
		/// </summary>
		double a;
		/// <summary>
		/// The b parameter.
		/// </summary>
		double b;
		/// <summary>
		/// The scheduling type.
		/// </summary>
		SchedulingType scheduling;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the server count.
		/// </summary>
		/// <value>The server count.</value>
		public uint ServerCount {
			get {
				return serverCount;
			}
		}

		/// <summary>
		/// Gets the number of replications.
		/// </summary>
		/// <value>The number of replications.</value>
		public uint Replications {
			get {
				return replications;
			}
		}

		/// <summary>
		/// Gets the number of time slots per replication.
		/// </summary>
		/// <value>The number of time slots per replication.</value>
		public uint TimeSlots {
			get {
				return timeSlots;
			}
		}

		/// <summary>
		/// Gets the queues.
		/// </summary>
		/// <value>The queues.</value>
		public PacketQueue[] Queues {
			get {
				return queues;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="SimulationProject.Simulation"/> class.
		/// </summary>
		public Simulation ()
		{
			currentTimeSlot = 0;
			currentReplication = 0;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Run this instance.
		/// </summary>
		public void Run ()
		{
			var averages = new Dictionary<double, double> ();
			var counts = new Dictionary<double, int> ();
			var stdDev = new Dictionary<double, List<double>> ();
			CSVLogger.Info (string.Format ("{0}", scheduling));
			serverSelectedQueue = new int[serverCount];
			rand = new NumberGenerator (a, b);
			DateTime now = DateTime.Now;
			Logger.Info ("Running simulation");
			while (currentReplication < replications) {
				Array.Clear (serverSelectedQueue, 0, (int)serverCount);
				Logger.Info (string.Format ("Running replication {0}", currentReplication));
				DateTime replicationNow = DateTime.Now;
				while (currentTimeSlot < timeSlots) {
					currentTimeSlot++;
					for (int i = 0; i < serverCount; i++) {
						for (int j = 0; j < queues.Length; j++) {
							double chance = queues [j].ConnectivityChance [i];
							double random = rand.GetNextRandomNumber ();
							queueConnectivity [i, j] = random > chance ? (uint)0 : (uint)1;
						}
					}
					switch (scheduling) {
					case SchedulingType.Random:
						for (int i = 0; i < serverCount; i++) {
							bool connected = false;
							for (int j = 0; j < queues.Length; j++) {
								if (queueConnectivity [i, j] == 1 && queues [j].Count > 0) {
									connected = true;
									break;
								}
							}
							if (!connected) {
								serverSelectedQueue [i] = -1;
								continue;
							}
							int m;
							do {
								m = (int)(rand.GetNextRandomNumber () * queues.Length);
							} while(queueConnectivity [i, m] != 1 || queues [m].Count == 0);
							serverSelectedQueue [i] = m;
						}
						break;
					case SchedulingType.RoundRobin:
						if (serverCount != 1) {
							Logger.Error ("Invalid server count for round robin scheduling");
							return;
						}
						int initial = serverSelectedQueue [0];
						do {
							serverSelectedQueue [0] = (serverSelectedQueue [0] + 1) % queues.Length;
						} while(serverSelectedQueue [0] != initial);
						break;
					case SchedulingType.LongestFirst:
						if (serverCount != 1) {
							Logger.Error ("Invalid server count for longest first scheduling");
						}
						int index = -1;
						for (int i = 0; i < queues.Length; i++) {
							if (queueConnectivity [0, i] == 0 || queues [i].Count == 0) {
								continue;
							}
							if (index == -1) {
								index = i;
								continue;
							}
							if (queues [i].Count > queues [index].Count && queueConnectivity [0, i] == 1) {
								index = i;
							}
						}
						serverSelectedQueue [0] = index;
						break;
					case SchedulingType.AnyLongestFirst:
						var serverList = new List<int> ();
						while (serverList.Count < serverCount) {
							int index2 = (int)(rand.GetNextRandomNumber () * serverCount);
							if (!serverList.Contains (index2)) {
								serverList.Add (index2);
							}
						}
						foreach (int index2 in serverList) {
							int queueIndex = -1;
							for (int j = 0; j < queues.Length; j++) {
								if (queueIndex == -1 && queueConnectivity [index2, j] == 1 && queues [j].Count > 0) {
									queueIndex = j;
									continue;
								}
								if (queues [j].Count > queues [queueIndex].Count && queueConnectivity [index2, j] == 1) {
									queueIndex = j;
								}
							}
							serverSelectedQueue [index2] = queueIndex;
						}
						break;
					case SchedulingType.LeastConnectedLongestFirst:
						var serverList2 = new List<int> ();
						while (serverList2.Count < serverCount) {
							int leastServer = -1;
							int leastCount = 0;
							for (int i = 0; i < serverCount; i++) {
								if (leastServer == -1) {
									leastServer = i;
									for (int j = 0; j < queues.Length; j++) {
										if (queueConnectivity [i, j] == 1) {
											leastCount++;
										}
									}
									continue;
								}
								int tempLeastCount = 0;
								for (int j = 0; j < queues.Length; j++) {
									if (queueConnectivity [i, j] == 1) {
										tempLeastCount++;
									}
								}
								if (tempLeastCount < leastCount && !serverList2.Contains (i)) {
									leastServer = i;
									leastCount = tempLeastCount;
								}
							}
							serverList2.Add (leastServer);
						}
						foreach (int index3 in serverList2) {
							int queueIndex = -1;
							for (int j = 0; j < queues.Length; j++) {
								if (queueConnectivity [index3, j] == 0 || queues [j].Count == 0) {
									continue;
								}
								if (queueIndex == -1) {
									queueIndex = j;
									continue;
								}
								if (queues [j].Count > queues [queueIndex].Count) {
									queueIndex = j;
								}
							}
							serverSelectedQueue [index3] = queueIndex;
						}
						break;
					default:
						Logger.Error ("Unknown scheduling type");
						return;
					}
					for (int i = 0; i < serverCount; i++) {
						if (serverSelectedQueue [i] == -1) {
							continue;
						}
						if (queues [serverSelectedQueue [i]].Count == 0) {
							continue;
						}
						queues [serverSelectedQueue [i]].Service ();
					}
					foreach (PacketQueue queue in queues) {
						queue.Update ((int)currentTimeSlot, rand);
					}
				}
				foreach (PacketQueue q in queues) {
					if (!averages.ContainsKey (q.ArrivalRate)) {
						averages [q.ArrivalRate] = q.AverageOccupancy;
						counts [q.ArrivalRate] = 1;
						stdDev [q.ArrivalRate] = new List<double> ();
					} else {
						averages [q.ArrivalRate] += q.AverageOccupancy;
						counts [q.ArrivalRate]++;
					}
					stdDev [q.ArrivalRate].Add (q.AverageOccupancy);
				}
				TimeSpan delta = DateTime.Now - replicationNow;
				Logger.Info (string.Format ("Completed replication {0}, took {1} seconds, {2} milliseconds", currentReplication, delta.Seconds, delta.Milliseconds));
				PrintResults ();
				currentTimeSlot = 0;
				currentReplication++;
			}
			foreach (double d in counts.Keys) {
				averages [d] /= counts [d];
			}
			foreach (double d in averages.Keys) {
				double standardDev = 0.0;
				foreach (double dd in stdDev[d]) {
					standardDev += Math.Pow (dd - d, 2.0);
				}
				standardDev /= counts [d];
				standardDev = Math.Sqrt (standardDev);
				CSVLogger.Info (string.Format ("{0};{1};{2}", d, averages [d], standardDev));
			}
			currentReplication = 0;
			TimeSpan deltaNow = DateTime.Now - now;
			Logger.Info (string.Format ("Completed simulation, took {0} seconds", deltaNow.TotalSeconds));
		}

		/// <summary>
		/// Prints the results.
		/// </summary>
		void PrintResults ()
		{
			ResultLogger.Info (string.Format ("Replication: {0}", currentReplication + 1));
			//CSVLogger.Info (string.Format ("Replication {0})", currentReplication + 1));
			ResultLogger.Info (string.Format ("Total time slots: {0}", timeSlots));
			ResultLogger.Info (string.Format ("Scheduling mode: {0}", scheduling));
			for (int i = 0; i < queues.Length; i++) {
				ResultLogger.Info (string.Format ("Queue {0}", i));
				queues [i].PrintResults ();
			}
		}

		/// <summary>
		/// Load the specified filename for the simulation.
		/// </summary>
		/// <returns>True if the simulation was loaded successfully, false otherwise.</returns>
		/// <param name="filename">The file to load.</param>
		public bool Load (string filename)
		{
			var doc = new XmlDocument ();
			XmlReader reader = XmlReader.Create (filename);
			doc.Load (reader);
			XmlNodeList list = doc.GetElementsByTagName ("Simulation");
			if (list.Count == 0) {
				return false;
			}
			for (int i = 0; i < list.Count; i++) {
				ResultLogger.Info (string.Format ("Simulation {0}", i));
				Logger.Info (string.Format ("Simulation {0}", i));
				CSVLogger.Info (string.Format ("Simulation {0}", i));
				if (!Load (list [i])) {
					return false;
				}
				Run ();
			}
			return true;
		}

		/// <summary>
		/// Load the simulation from the specified XML node.
		/// </summary>
		/// <param name="node">The XML node to load from.</param>
		public bool Load (XmlNode node)
		{
			var xmlRootElement = node as XmlElement;
			if (xmlRootElement == null) {
				Logger.Error ("Root node is not an element");
				return false;
			}
			XmlNodeList list = xmlRootElement.GetElementsByTagName ("Replications");
			if (list.Count != 1) {
				Logger.Error ("Incorrect amount of replication elements, must be only one");
				return false;
			}
			var xmlElement = list [0] as XmlElement;
			if (xmlElement == null) {
				Logger.Error ("Replication node is not an element");
				return false;
			}
			string value = xmlElement.GetAttribute ("value");
			if (!uint.TryParse (value, out replications)) {
				Logger.Error ("Replication value is not a uint");
				return false;
			}
			list = xmlRootElement.GetElementsByTagName ("Timeslots");
			if (list.Count != 1) {
				Logger.Error ("Incorrect amount of timeslots elements, must be only one");
				return false;
			}
			xmlElement = list [0] as XmlElement;
			if (xmlElement == null) {
				Logger.Error ("Timeslots node is not an element");
				return false;
			}
			value = xmlElement.GetAttribute ("value");
			if (!uint.TryParse (value, out timeSlots)) {
				Logger.Error ("Timeslots value is not a uint");
				return false;
			}
			list = xmlRootElement.GetElementsByTagName ("NumberGenerator");
			if (list.Count != 1) {
				Logger.Error ("Incorrect amount of number generator elements, must be one");
				return false;
			}
			xmlElement = list [0] as XmlElement;
			if (xmlElement == null) {
				Logger.Error ("Number generator node is not an element");
				return false;
			}
			value = xmlElement.GetAttribute ("a");
			if (!double.TryParse (value, out a)) {
				Logger.Error ("a parameter value is not a double");
				return false;
			}
			value = xmlElement.GetAttribute ("b");
			if (!double.TryParse (value, out b)) {
				Logger.Error ("b parameter value is not a double");
				return false;
			}
			list = xmlRootElement.GetElementsByTagName ("Scheduling");
			if (list.Count != 1) {
				return false;
			}
			xmlElement = list [0] as XmlElement;
			if (xmlElement == null) {
				Logger.Error ("Scheduling node is not an element");
				return false;
			}
			value = xmlElement.GetAttribute ("value");
			if (!Enum.TryParse<SchedulingType> (value, out scheduling)) {
				Logger.Error (string.Format ("Invalid scheduling type {0}", value));
				return false;
			}
			list = xmlRootElement.GetElementsByTagName ("Servers");
			if (list.Count != 1) {
				Logger.Error ("Incorrect amount of server elements, must be at least one");
				return false;
			}
			xmlElement = list [0] as XmlElement;
			if (xmlElement == null) {
				Logger.Error ("Servers node is not an element");
				return false;
			}
			value = xmlElement.GetAttribute ("count");
			if (!uint.TryParse (value, out serverCount)) {
				Logger.Error ("Server count is not a uint");
				return false;
			}
			serverSelectedQueue = new int[serverCount];
			list = xmlRootElement.GetElementsByTagName ("Queue");
			if (list.Count == 0) {
				Logger.Error ("Incorrect amount of queue elements, must be at least one");
				return false;
			}
			queues = new PacketQueue[list.Count];
			for (int i = 0; i < queues.Length; i++) {
				var queue = new PacketQueue (this);
				if (!queue.Load (list [i])) {
					Logger.Error ("Queue failed to load");
					return false;
				}
				queues [i] = queue;
			}
			queueConnectivity = new uint[serverCount, queues.Length];
			return true;
		}

		#endregion
	}
}

