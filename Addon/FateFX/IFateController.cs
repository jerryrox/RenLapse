using System;

namespace Renko.LapseFramework
{
	/// <summary>
	/// Allows controlling the encapsulated fate controller object.
	/// </summary>
	public interface IFateController<TI> : IFateSection<TI> {

		/// <summary>
		/// Returns whether the root controller is usable and not destroyed.
		/// </summary>
		bool IsValid { get; }

		/// <summary>
		/// Returns whether the root controller's animation is currently playing.
		/// </summary>
		bool IsPlaying { get; }

		/// <summary>
		/// Whether root controller's playback should loop from the beginning when reaching the end.
		/// Default: false
		/// </summary>
		bool IsLoop { get; set; }

		/// <summary>
		/// Whether the root controller should automatically destroy upon reaching the end.
		/// IsLoop flag is prioritized.
		/// Default: false
		/// </summary>
		bool IsDestroyOnEnd { get; set; }

		/// <summary>
		/// Whether the underlying lapser should use a fixed delta time.
		/// Default: false
		/// </summary>
		bool IsFixedDeltaTime { get; set; }

		/// <summary>
		/// Whether frames can be skipped to accommodate for app FPS drops when IsFixedDeltaTime is true.
		/// Default: false
		/// </summary>
		bool IsSkipFrames { get; set; }

		/// <summary>
		/// Current animation progress from 0 to 1 relative to this controller.
		/// If set from child controller, the root controller's progress 
		/// </summary>
		float Progress { get; set; }

		/// <summary>
		/// Playback speed of the root controller.
		/// Default: 1
		/// </summary>
		float Speed { get; set; }


		/// <summary>
		/// Plays or resumes the root controller's playback.
		/// </summary>
		void Play();

		/// <summary>
		/// Pauses the the root controller's playback.
		/// </summary>
		void Pause();

		/// <summary>
		/// Pauses  and resets the root controller's playback time to beginning of animation.
		/// </summary>
		void Stop();

		/// <summary>
		/// Destroys the root controller to completely stop further updates.
		/// </summary>
		void Destroy();
	}
}

