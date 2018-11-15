using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renko.LapseFramework.Internal
{
	public class TimerDelay : ITimerDelay, ILapseListener, IDisposable {

		private Timer owner;
		private ILapser lapser;
		private Timer.CallbackHandler callback;
		private bool isValid;
		private bool isDestroyOnEnd;


		public bool IsValid { get { return isValid; } }

		public bool IsDestroyOnEnd
		{
			get { return isDestroyOnEnd; }
			set { isDestroyOnEnd = value; }
		}


		/// <summary>
		/// Resets this object to is initial state.
		/// </summary>
		public void Recycle(Timer owner, ILapser lapser, Timer.CallbackHandler callback)
		{
			this.owner = owner;
			this.lapser = lapser;
			this.callback = callback;

			isValid = true;
			isDestroyOnEnd = true;

			// Attach this object as lapser event listener.
			lapser.AddListener(this);
		}

		public void OnLapseStart (ILapser lapser) {}

		public bool OnLapseUpdate (ILapser lapser)
		{
			if(callback != null)
				callback();

			if(isDestroyOnEnd)
				Destroy();
			else
				Pause();
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
		}

		public void Destroy()
		{
			if(!isValid)
				return;
			
			isValid = false;
			callback = null;

			lapser.Destroy();
			lapser = null;

			owner.RemoveDelay(this);
		}

		public void Dispose() {}
	}
}