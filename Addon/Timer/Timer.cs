using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Renko.LapseFramework.Internal;

namespace Renko.LapseFramework
{
	/// <summary>
	/// RenLapse framework addon.
	/// Handles simple delayed callbacks and intervals.
	/// </summary>
	public class Timer : IAddon {

		private static Timer I;

		private List<TimerDelay> delayItems;
		private List<TimerInterval> intervalItems;
		private DelayRecycler delayRecycler;
		private IntervalRecycler intervalRecycler;

		public delegate void CallbackHandler();


		public Timer(int capacity)
		{
			delayItems = new List<TimerDelay>(capacity);
			intervalItems = new List<TimerInterval>(capacity);
			delayRecycler = new DelayRecycler(this, capacity);
			intervalRecycler = new IntervalRecycler(this, capacity);
		}

		/// <summary>
		/// Initializes the Timer addon.
		/// RenLapse is initialized together if not already done.
		/// </summary>
		public static void Initialize(int timerCapacity = 0)
		{
			if(I != null)
				return;
			if(timerCapacity < 0)
				throw new ArgumentException("Timer.Initialize - timerCapacity must be zero or greater!");
			
			// Init RenLapse
			RenLapse.Initialize(timerCapacity);
			// Initialize timer
			I = new Timer(timerCapacity);
			// Attach to RenLapse as addon.
			RenLapse.AttachAddon(I);
		}

		/// <summary>
		/// Creates a new delayed event to be called after specified time in seconds.
		/// </summary>
		public static ITimerDelay CreateDelay(Timer.CallbackHandler callback, float time, int priority = 0)
		{
			if(I == null)
				throw new NullReferenceException("Timer.CreateDelay - Timer not initialized!");
			if(callback == null)
				throw new ArgumentNullException("Timer.CreateDelay - callback must not be null!");

			// Allocate a lapser for delay object.
			ILapser lapser = RenLapse.CreateLapser(1);
			lapser.TargetDeltaTime = time;
			lapser.Priority = priority;

			TimerDelay delay = I.delayRecycler.GetNext(lapser, callback);
			I.delayItems.Add(delay);
			return delay;
		}

		/// <summary>
		/// Creates a new interval event to be called every after specified time in seconds.
		/// </summary>
		public static ITimerInterval CreateInterval(Timer.CallbackHandler callback, float time,
			int repeats = int.MaxValue, int priority = 0)
		{
			if(I == null)
				throw new NullReferenceException("Timer.CreateInterval - Timer not initialized!");
			if(callback == null)
				throw new ArgumentNullException("Timer.CreateInterval - callback must not be null!");

			// Allocate a lapser for delay object.
			ILapser lapser = RenLapse.CreateLapser(1);
			lapser.TargetDeltaTime = time;
			lapser.Priority = priority;

			TimerInterval interval = I.intervalRecycler.GetNext(lapser, callback);
			interval.RepeatsLeft = repeats;
			I.intervalItems.Add(interval);
			return interval;
		}

		/// <summary>
		/// Removes specified TimerDelay from update and adds to recycler.
		/// </summary>
		public void RemoveDelay(TimerDelay delay)
		{
			delayItems.Remove(delay);
			delayRecycler.ReturnItem(delay);
		}

		/// <summary>
		/// Removes specified TimerInterval from update and adds to recycler.
		/// </summary>
		public void RemoveInterval(TimerInterval interval)
		{
			intervalItems.Remove(interval);
			intervalRecycler.ReturnItem(interval);
		}

		/// <summary>
		/// Event that is called when ShouldRecycle property of RenLapse was set.
		/// </summary>
		public void OnRecycleFlagSet(bool shouldRecycle)
		{
			delayRecycler.ShouldRecycle = shouldRecycle;
		}
	}
}