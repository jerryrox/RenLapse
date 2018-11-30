using System;

namespace Renko.LapseFramework.Internal
{
	public class FramerRecycler : BaseRecycler<FateFramer> {

		private FateFX owner;


		public FramerRecycler(FateFX owner, int capacity) : base(capacity)
		{
			this.owner = owner;
		}

		public new FateFramer GetNext(ILapser lapser, int capacity)
		{
			FateFramer framer = base.GetNext();
			framer.Recycle(owner, lapser, capacity);
			return framer;
		}
	}
}

