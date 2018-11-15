using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renko.LapseFramework.Internal
{
	public class TimerInterval : ITimerInterval, ILapseListener, IDisposable {

		private Timer owner;
		private ILapser lapser;
		private Timer.CallbackHandler callback;
		private bool isValid;
		private bool isDestroyOnEnd;
		private int savedRepeat;
		private int curRepeat;


		public bool IsValid { get { return isValid; } }

		public bool IsDestroyOnEnd
		{
			get { return isDestroyOnEnd; }
			set { isDestroyOnEnd = value; }
		}

		public int RepeatsLeft
		{
			get { return curRepeat; }
			set
			{
				if(value < 0)
					value = 0;
				savedRepeat = value;
				curRepeat = value;
			}
		}


		/// <summary>
		/// Resets this object to its initial state.
		/// </summary>
		public void Recycle(Timer owner, ILapser lapser, Timer.CallbackHandler callback)
		{
			this.owner = owner;
			this.lapser = lapser;
			this.callback = callback;

			isValid = true;
			isDestroyOnEnd = true;
			RepeatsLeft = int.MaxValue;

			// Attach this object as lapser event listener.
			lapser.AddListener(this);
		}

		public void OnLapseStart (ILapser lapser) {}

		public bool OnLapseUpdate (ILapser lapser)
		{
			if(callback != null)
				callback();

			if(curRepeat > 0)
			{
				curRepeat --;
			}
			else
			{
				if(isDestroyOnEnd)
					Destroy();
				else
					Pause();
			}
			return true;
		}

		public void OnLapseResume (ILapser lapser) {}

		public void OnLapsePause (ILapser lapser) {}

		public void OnLapseStop (ILapser lapser) {}

		public void OnLapseEnd (ILapser lapser) {}

		public void Start()
		{
			if(!isValid)
				return;

			lapser.Start();
		}

		public void Pause()
		{
			if(!isValid)
				return;

			lapser.Pause();
		}

		public void Stop()
		{
			if(!isValid)
				return;

			lapser.Stop();

			curRepeat = savedRepeat;
		}

		public void Destroy()
		{
			if(!isValid)
				return;

			isValid = false;
			callback = null;

			lapser.Destroy();
			lapser = null;

			owner.RemoveInterval(this);
		}

		public void Dispose() {}
	}
}