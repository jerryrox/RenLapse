using System;

namespace Renko.LapseFramework
{
	public interface ILapser {

		/// <summary>
		/// Returns the identifier assigned to this lapser.
		/// </summary>
		int ID { get; }

		/// <summary>
		/// The priority which affects the order of lapse update.
		/// Highest value indicates the first call in update.
		/// </summary>
		int Priority { get; set; }

		/// <summary>
		/// The max number of frames to skip when update is delayed.
		/// Note that this value is ignored if IsFixedDeltaTime is false.
		/// Default: RenLapse.Frame_NoSkip
		/// </summary>
		int MaxSkipFrames { get; set; }

		/// <summary>
		/// Returns the number of frames skipped since last update call.
		/// </summary>
		int SkippedFrames { get; }

		/// <summary>
		/// The target rate which the update should be processed at.
		/// Similar to TargetDeltaTime property.
		/// Default: 1
		/// </summary>
		float TargetFrameRate { get; set; }

		/// <summary>
		/// The target delta time which the update should be processed after.
		/// Similar to TargetFrameRate property.
		/// Default: 1
		/// </summary>
		float TargetDeltaTime { get; set; }

		/// <summary>
		/// Returns the delta time since last update call.
		/// </summary>
		float DeltaTime { get; }

		/// <summary>
		/// Whether this lapser should be destroyed on changing active scene.
		/// Default: true
		/// </summary>
		bool IsDestroyOnLoad { get; set; }

		/// <summary>
		/// Whether delta time should be fixed based on frame rate, or unfixed to use the actual delta time.
		/// Default: false
		/// </summary>
		bool IsFixedDeltaTime { get; set; }

		/// <summary>
		/// Returns whether this lapser is currently being updated.
		/// </summary>
		bool IsUpdating { get; }

		/// <summary>
		/// Returns whether this lapser is destroyed.
		/// </summary>
		bool IsDestroyed { get; }


		/// <summary>
		/// Starts or resumes updating.
		/// Fires OnLapseStart event if first time calling this method.
		/// Otherwise, fires OnLapseResume event.
		/// </summary>
		void Start();

		/// <summary>
		/// Pauses updating.
		/// Fires OnLapsePause event.
		/// </summary>
		void Pause();

		/// <summary>
		/// Pauses updating and resets time to 0. Start() method will call OnLapseStart event instead afterwards.
		/// Fires OnLapseStop event.
		/// </summary>
		void Stop();

		/// <summary>
		/// Destroys the lapser to completely remove it from updating.
		/// </summary>
		void Destroy();

		/// <summary>
		/// Adds the specified listener to this lapser.
		/// </summary>
		void AddListener(ILapseListener listener);

		/// <summary>
		/// Removes the specified listener from this lapser.
		/// Returns whether the listener was removed.
		/// </summary>
		bool RemoveListener(ILapseListener listener);

		/// <summary>
		/// Returns whether the specified listener is already listening to this lapser.
		/// </summary>
		bool ContainsListener(ILapseListener listener);

		/// <summary>
		/// Removes all listeners.
		/// </summary>
		void ClearListeners();
	}
}

