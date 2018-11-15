using System;

namespace Renko.LapseFramework.Internal
{
	public class IntervalRecycler : BaseRecycler<TimerInterval> {

		private Timer owner;


		public IntervalRecycler(Timer owner, int capacity) : base(capacity)
		{
			this.owner = owner;
		}

		public new TimerInterval GetNext(ILapser lapser, Timer.CallbackHandler callback)
		{
			TimerInterval delay = base.GetNext();
			delay.Recycle(owner, lapser, callback);
			return delay;
		}
	}
}

