using System;

namespace Renko.LapseFramework.Internal
{
	public class TimerRecycler : BaseRecycler<FateTimer> {

		private FateFX owner;


		public TimerRecycler(FateFX owner, int capacity) : base(capacity)
		{
			this.owner = owner;
		}

		public FateTimer GetNext(ILapser lapser, int capacity)
		{
			FateTimer timer = base.GetNext();
			timer.Recycle(owner, lapser, capacity);
			return timer;
		}
	}
}

