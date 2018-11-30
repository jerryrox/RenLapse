using System;

namespace Renko.LapseFramework
{
	/// <summary>
	/// Controller for time-based fate animations.
	/// </summary>
	public interface IFateTimer : IFateController<IFateTimer> {

		/// <summary>
		/// Returns the current time.
		/// </summary>
		float CurrentTime { get; }

		/// <summary>
		/// Returns the total duration in seconds.
		/// </summary>
		float Duration { get; }


		/// <summary>
		/// Sets playback time to specified value.
		/// </summary>
		void SeekTo(float time);

		/// <summary>
		/// Creates a new section at specified times.
		/// </summary>
		IFateSection<IFateTimer> AddSection(float startTime, float endTime, int listCapacity);

		/// <summary>
		/// Creates a new trigger at specified time.
		/// </summary>
		IFateTrigger<IFateTimer> AddTrigger(float time, int listCapacity);
	}
}

