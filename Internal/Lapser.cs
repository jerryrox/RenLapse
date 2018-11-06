using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renko.Frameworks.Internal
{
	public sealed class Lapser : ILapser {

		private int id;
		private int maxSkipFrames;
		private int skippedFrames;
		private float elapsedTime;
		private float lastElapsedTime;
		private float targetFrameRate;
		private float targetDeltaTime;
		private float lastDeltaTime;
		private bool isDestroyOnLoad;
		private bool isFixedDeltaTime;
		private bool isLapseStarted;
		private bool isUpdating;
		private RenLapse owner;
		private List<ILapseListener> listeners;


		public int ID 
		{
			get { return id; }
		}

		public int Priority
		{
			get;
			set;
		}

		public int MaxSkipFrames
		{
			get { return maxSkipFrames; }
			set { maxSkipFrames = Mathf.Clamp(value, RenLapse.Frame_NoSkip, RenLapse.Frame_AlwaysSkip); }
		}

		public int SkippedFrames
		{
			get { return skippedFrames; }
		}

		public float TargetFrameRate
		{
			get { return targetFrameRate; }
			set
			{
				targetFrameRate = Mathf.Clamp(value, float.Epsilon, value);
				targetDeltaTime = 1f / targetFrameRate;
			}
		}

		public float TargetDeltaTime
		{
			get { return targetDeltaTime; }
			set
			{
				targetDeltaTime = Mathf.Clamp(value, float.Epsilon, value);
				targetFrameRate = 1f / targetDeltaTime;
			}
		}

		public float DeltaTime
		{
			get { return lastDeltaTime; }
		}

		public bool IsDestroyOnLoad 
		{
			get { return isDestroyOnLoad; }
			set { isDestroyOnLoad = value; }
		}

		public bool IsFixedDeltaTime 
		{
			get { return isFixedDeltaTime; }
			set { isFixedDeltaTime = value; }
		}

		public bool IsUpdating
		{
			get { return isUpdating; }
		}


		public Lapser(RenLapse owner, int id, int listenerCapacity)
		{
			this.owner = owner;
			this.id = id;
			maxSkipFrames = RenLapse.Frame_NoSkip;
			skippedFrames = 0;
			elapsedTime = 0f;
			lastElapsedTime = 0f;
			lastDeltaTime = 0f;
			isDestroyOnLoad = true;
			isFixedDeltaTime = false;
			listeners = new List<ILapseListener>(listenerCapacity);

			TargetFrameRate = 1f;
		}

		public void Start()
		{
			// If updating, return
			if(isUpdating)
				return;
			
			// If first time starting lapse update
			if(!isLapseStarted)
			{
				isLapseStarted = true;

				// Notify all registered listeners for start event
				for(int i=listeners.Count-1; i>=0; i--)
				{
					if(listeners[i] != null)
						listeners[i].OnLapseStart(this);
				}
			}
			// If not first time starting
			else
			{
				// Fire the resume event.
				for(int i=listeners.Count-1; i>=0; i--)
				{
					if(listeners[i] != null)
						listeners[i].OnLapseResume(this);
				}
			}

			// Set updating flag.
			isUpdating = true;
			// Listen to owner's update calls.
			owner.AttachLapser(this);
		}

		public void Pause()
		{
			// If not updating, return
			if(!isUpdating)
				return;
			
			// Fire the pause event to all listeners
			for(int i=listeners.Count-1; i>=0; i--)
			{
				if(listeners[i] != null)
					listeners[i].OnLapsePause(this);
			}

			// Set updating flag
			isUpdating = false;
			// Don't listen to owner's update calls.
			owner.DetachLapser(this);
		}

		public void AddListener(ILapseListener listener)
		{
			// Add listener only if not already registered.
			if(listener == null || listeners.Contains(listener))
				return;
			listeners.Add(listener);

			// If lapser update has been started 
			if(isLapseStarted)
			{
				// Fire on start event.
				listener.OnLapseStart(this);

				// If this lapser is currently paused, send the pause event
				if(!isUpdating)
					listener.OnLapsePause(this);
			}
		}

		public bool RemoveListener(ILapseListener listener)
		{
			if(listener == null)
				return false;
			
			for(int i=listeners.Count-1; i>=0; i--)
			{
				ILapseListener l = listeners[i];
				if(l == listener)
				{
					// Fire on end event.
					listener.OnLapseEnd(this);
					// Nullify instead of removing, to prevent issues during update.
					listeners[i] = null;
					return true;
				}
			}
			return false;
		}

		public bool ContainsListener(ILapseListener listener)
		{
			return listener != null && listeners.Contains(listener);
		}

		public void ClearListener()
		{
			// Fire on end event for all listeners and clear list.
			for(int i=listeners.Count-1; i>=0; i--)
			{
				if(listeners[i] != null)
					listeners[i].OnLapseEnd(this);
			}
			listeners.Clear();
		}

		public void Update(float deltaTime)
		{
			// Add elapsed time
			elapsedTime += deltaTime;

			if(elapsedTime >= targetDeltaTime)
			{
				if(isFixedDeltaTime)
				{
					// Calculate the number of frames passed since last update.
					int framesPassed = Mathf.Clamp((int)(elapsedTime / targetDeltaTime), 1, maxSkipFrames + 1);

					// Calculate the amount of time that would've past based on current framerate.
					float elapsedDelta = framesPassed * targetDeltaTime;

					// Store skipped frame count
					skippedFrames = framesPassed - 1;

					// Calculate delta time
					lastDeltaTime = elapsedDelta;

					// Calculate new elapsed time
					elapsedTime %= targetDeltaTime;
				}
				else
				{
					// Calculate delta time
					lastDeltaTime = elapsedTime - lastElapsedTime;

					// Calculate the number of frames that would've been skipped.
					skippedFrames = (int)(elapsedTime / targetDeltaTime) - 1;

					// Calculate new elapsed time
					elapsedTime -= lastDeltaTime;
				}

				// Store elapsed time
				lastElapsedTime = elapsedTime;

				// Fire update event.
				for(int i=listeners.Count-1; i>=0; i--)
				{
					ILapseListener listener = listeners[i];
					if(listener == null)
					{
						listeners.RemoveAt(i);
						continue;
					}
					listener.OnLapseUpdate(this);
				}
			}
		}
	}
}