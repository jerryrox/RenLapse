namespace Renko.LapseFramework
{
	/// <summary>
	/// Types of events available to listen from a FateFX controller.
	/// </summary>
	public enum FateEvents {
		/// <summary>
		/// Event when the animation enters the starting time of the controller.
		/// </summary>
		OnStart = 0,

		/// <summary>
		/// Event when Play() method is manually called.
		/// </summary>
		OnPlay,

		/// <summary>
		/// Event when the controller is currently being updated.
		/// </summary>
		OnUpdate,

		/// <summary>
		/// Event when Pause() method is manually called.
		/// </summary>
		OnPause,

		/// <summary>
		/// Event when the animation reaches the end and resets back to time 0.
		/// Called for root controller only.
		/// </summary>
		OnLoop,

		/// <summary>
		/// Event when Stop() method is manually called.
		/// </summary>
		OnStop,

		/// <summary>
		/// Event when current time reaches the end time of the controller.
		/// </summary>
		OnEnd,

		/// <summary>
		/// Event when the controller was destroyed manually from Destroy() method, or IsDestroyOnEnd property.
		/// Called for root controller only.
		/// </summary>
		OnDestroy
	}
}