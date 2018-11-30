using System;
using System.Collections.Generic;
using Renko.LapseFramework.Internal;

namespace Renko.LapseFramework
{
	/// <summary>
	/// RenLapse framework addon.
	/// Provides a frame-based and time-based animation framework.
	/// </summary>
	public class FateFX : IAddon {

		private static FateFX I;

		private FramerRecycler framerRecycler;
		private TimerRecycler timerRecycler;

		public delegate void EventAction<T>(T controller);


		public FateFX(int capacity)
		{
			framerRecycler = new FramerRecycler(this, capacity);
			timerRecycler = new TimerRecycler(this, capacity);
		}

		/// <summary>
		/// Initializes the FateFX addon and RenLapse if not already done.
		/// </summary>
		public static void Initialize(int listCapacity = 0)
		{
			if(I != null)
				return;
			if(listCapacity < 0)
				throw new ArgumentException("FateFX.Initialize - listCapacity must be zero or greater!");

			// Init RenLapse
			RenLapse.Initialize(listCapacity);
			// Initialize timer
			I = new FateFX(listCapacity);
			// Attach to RenLapse as addon.
			RenLapse.AttachAddon(I);
		}

		/// <summary>
		/// Creates a new frame-based FateFX controller and returns it.
		/// </summary>
		public static IFateFramer CreateFramed(float fps, int listCapacity = 0, int priority = 0)
		{
			if(I == null)
				throw new NullReferenceException("FateFX.CreateFramed - FateFX not initialized!");
			if(listCapacity < 0)
				throw new ArgumentException("FateFX.CreateFramed - listCapacity must be zero or greater!");

			// Allocate a lapser for the new controller
			ILapser lapser = RenLapse.CreateLapser(1);
			lapser.TargetFrameRate = fps;
			lapser.Priority = priority;

			// Create a new framer controller
			IFateFramer framer = I.GetNewFramer(lapser, listCapacity);
			return framer;
		}

		/// <summary>
		/// Creates a new time-based FateFX controller and returns it.
		/// </summary>
		public static IFateTimer CreateTimed(float fps, int listCapacity = 0, int priority = 0)
		{
			if(I == null)
				throw new NullReferenceException("FateFX.CreateTimed - FateFX not initialized!");
			if(listCapacity < 0)
				throw new ArgumentException("FateFX.CreateTimed - listCapacity must be zero or greater!");

			// Allocate a lapser for the new controller
			ILapser lapser = RenLapse.CreateLapser(1);
			lapser.TargetFrameRate = fps;
			lapser.Priority = priority;

			// Create a new timer controller
			IFateTimer framer = I.GetNewTimer(lapser, listCapacity);
			return framer;
		}

		/// <summary>
		/// Retrieves a new framer from the recycler and returns it.
		/// </summary>
		public FateFramer GetNewFramer(ILapser lapser, int listCapacity)
		{
			return I.framerRecycler.GetNext(lapser, listCapacity);
		}

		/// <summary>
		/// Retrieves a new timer from the recycler and returns it.
		/// </summary>
		public FateTimer GetNewTimer(ILapser lapser, int listCapacity)
		{
			return I.timerRecycler.GetNext(lapser, listCapacity);
		}

		/// <summary>
		/// Removes specified FateFramer from update and adds to recycler.
		/// </summary>
		public void RemoveFramer(FateFramer framer)
		{
			I.framerRecycler.ReturnItem(framer);
		}

		/// <summary>
		/// Removes specified FateTimer from update and adds to recycler.
		/// </summary>
		public void RemoveTimer(FateTimer timer)
		{
			I.timerRecycler.ReturnItem(timer);
		}

		/// <summary>
		/// Event that is called when ShouldRecycle property of RenLapse was set.
		/// </summary>
		public void OnRecycleFlagSet (bool shouldRecycle)
		{
			framerRecycler.ShouldRecycle = shouldRecycle;
			timerRecycler.ShouldRecycle = shouldRecycle;
		}
	}
}

