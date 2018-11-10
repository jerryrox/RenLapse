using System;
using System.Collections.Generic;

namespace Renko.LapseFramework.Internal
{
	public class LapseRecycler {

		private RenLapse owner;
		private Stack<Lapser> lapsers;


		public LapseRecycler(RenLapse owner, int lapserCapacity)
		{
			this.owner = owner;
			lapsers = new Stack<Lapser>(lapserCapacity);
		}

		/// <summary>
		/// Returns the next reusable lapser in the stack.
		/// If none, a new lapser will be created.
		/// </summary>
		public Lapser GetNextLapser(int id, int listenerCapacity)
		{
			if(lapsers.Count > 0)
			{
				Lapser lapser = lapsers.Pop();
				lapser.Recycle(id, listenerCapacity);
				return lapser;
			}
			return new Lapser(owner, id, listenerCapacity);
		}

		/// <summary>
		/// Adds specified lapser to recycle stack after disposing.
		/// </summary>
		public void ReleaseLapser(Lapser lapser)
		{
			// Dispose the lapser so it releases references to other objects.
			lapser.Dispose();

			// Push the lapser to recycling stack.
			lapsers.Push(lapser);
		}
	}
}

