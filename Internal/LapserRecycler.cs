using System;
using System.Collections.Generic;

namespace Renko.LapseFramework.Internal
{
	public class LapserRecycler : BaseRecycler<Lapser> {

		private RenLapse owner;


		public LapserRecycler(RenLapse owner, int capacity) : base(capacity)
		{
			this.owner = owner;
		}

		public new Lapser GetNext(int nextId, int capacity)
		{
			Lapser lapser = base.GetNext();
			lapser.Recycle(owner, nextId, capacity);
			return lapser;
		}
	}
}

