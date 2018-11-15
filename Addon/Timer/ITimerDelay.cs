using System;

namespace Renko.LapseFramework
{
	/// <summary>
	/// Interface for controlling Timer delay object.
	/// </summary>
	public interface ITimerDelay {

		/// <summary>
		/// Returns whether this object is valid and usable.
		/// </summary>
		bool IsValid { get; }

		/// <summary>
		/// Whether this object should be automatically destroyed after callback.
		/// Default: true
		/// </summary>
		bool IsDestroyOnEnd { get; set; }


		/// <summary>
		/// Starts/resumes time update.
		/// </summary>
		void Start();

		/// <summary>
		/// Pauses time update.
		/// </summary>
		void Pause();

		/// <summary>
		/// Pauses time update and resets time to 0.
		/// </summary>
		void Stop();

		/// <summary>
		/// Destroys this object to completely stop further updates.
		/// </summary>
		void Destroy();
	}
}

