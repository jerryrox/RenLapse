using System;

namespace Renko.LapseFramework.Internal
{
	public class DelayRecycler : BaseRecycler<TimerDelay> {

		private Timer owner;


		public DelayRecycler(Timer owner, int capacity) : base(capacity)
		{
			this.owner = owner;
		}

		public new TimerDelay GetNext(ILapser lapser, Timer.CallbackHandler callback)
		{
			TimerDelay delay = base.GetNext();
			delay.Recycle(owner, lapser, callback);
			return delay;
		}
	}
}

