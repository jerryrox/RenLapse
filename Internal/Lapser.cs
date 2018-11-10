using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renko.LapseFramework.Internal
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
		private bool isDestroyed;
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

		public bool IsDestroyed
		{
			get { return isDestroyed; }
		}


		public Lapser(RenLapse owner, int id, int listenerCapacity)
		{
			this.owner = owner;
			listeners = new List<ILapseListener>();

			// Reset to initial state.
			Recycle(id, listenerCapacity);
		}

		/// <summary>
		/// Resets this lapser's state to its initial state for recycling.
		/// </summary>
		public void Recycle(int newID, int newCapacity)
		{
			id = newID;
			maxSkipFrames = RenLapse.Frame_NoSkip;
			skippedFrames = 0;
			elapsedTime = 0f;
			lastElapsedTime = 0f;
			lastDeltaTime = 0f;
			isDestroyOnLoad = true;
			isFixedDeltaTime = false;
			isLapseStarted = false;
			isUpdating = false;
			isDestroyed = false;
			listeners.Capacity = newCapacity;

			TargetFrameRate = 1f;
		}

		/// <summary>
		/// Handles resetting process before completely removing from lapse update.
		/// Not exactly disposing to make it unusable, but I couldn't find a better name :P
		/// </summary>
		public void Dispose()
		{
			// Lapser should no longer update
			isUpdating = false;
			// Clear all listeners
			ClearListeners();
		}

		public void Start()
		{
			// If updating, or is destroyed return
			if(isUpdating || isDestroyed)
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
			// If not updating, or is destroyed return
			if(!isUpdating || isDestroyed)
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

		public void Destroy()
		{
			// Simply flag the lapser so it's destroyed in the next update.
			isDestroyed = true;
		}

		public void AddListener(ILapseListener listener)
		{
			// Add listener only if not already registered and is not destroyed
			if(isDestroyed || listener == null || listeners.Contains(listener))
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
			// Return if invalid listener or lapser is destroyed
			if(isDestroyed || listener == null)
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
			return !isDestroyed && listener != null && listeners.Contains(listener);
		}

		public void ClearListeners()
		{
			// Fire on end event for all listeners and clear list.
			for(int i=listeners.Count-1; i>=0; i--)
			{
				if(listeners[i] != null)
					listeners[i].OnLapseEnd(this);
			}
			listeners.Clear();
		}

		/// <summary>
		/// Handles the update process for this frame.
		/// </summary>
		public bool Update(float deltaTime)
		{
			// If lapser is destroyed before this update, just stop
			if(isDestroyed)
				return false;

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
					// Do update. If the listener should stop listening to further calls
					if(!listener.OnLapseUpdate(this))
					{
						// If the developer destroyed the lapser during update, just break out of the loop.
						if(isDestroyed)
							break;
						
						// Send end event and remove from update list.
						listener.OnLapseEnd(this);
						listeners.RemoveAt(i);

						// If the developer destroyed the lapser during end event, just break out of the loop.
						if(isDestroyed)
							break;
					}
				}
			}

			// Lapser should continue as long as it's not destroyed.
			return !isDestroyed;
		}
	}
}