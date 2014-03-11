// Copyright (C) 2014 M. Damian Mulligan (G'lek Tarssza)
// All Rights Reserved.
using System;
using System.Collections.Generic;

namespace SimulationProject
{
	public class NumberGenerator
	{
		#region Fields

		/// <summary>
		/// The a parameter.
		/// </summary>
		public readonly double a;
		/// <summary>
		/// The b parameter.
		/// </summary>
		public readonly double b;
		/// <summary>
		/// The v rand.
		/// </summary>
		readonly Random vRand;
		double u;
		double v;

		#endregion

		#region Properties

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="SimulationProject.NumberGenerator"/> class.
		/// </summary>
		/// <remarks>
		/// Generator is seeded with the current millisecond time.
		/// </remarks>
		public NumberGenerator (double a, double b)
		{
			this.a = a;
			this.b = b;
			vRand = new Random (DateTime.Now.Millisecond);
			u = vRand.NextDouble ();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SimulationProject.NumberGenerator"/> class.
		/// </summary>
		/// <param name="seed">The seed to use during generation.</param>
		public NumberGenerator (int seed, double a, double b)
		{
			this.a = a;
			this.b = b;
			vRand = new Random (seed);
			u = vRand.NextDouble ();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets the next random number.
		/// </summary>
		/// <returns>The next random number.</returns>
		public double GetNextRandomNumber ()
		{
			v = a + ((a + b) * vRand.NextDouble ());
			double temp = u + v;
			u = temp - Math.Floor (temp);
			return u;
		}

		#endregion
	}
}

