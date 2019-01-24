using System;

namespace Renko.LapseFramework
{
	/// <summary>
	/// Controller for frame-based fate animations.
	/// </summary>
	public interface IFateFramer : IFateController<IFateFramer> {

		/// <summary>
		/// Returns the current frame number relative to this controller.
		/// </summary>
		int CurrentFrame { get; }

		/// <summary>
		/// Returns the total duration in frames relative to this controller.
		/// </summary>
		int Duration { get; }


		/// <summary>
		/// Sets the root controller's playback frame number to specified value.
		/// </summary>
		void SeekTo(int frame);

		/// <summary>
		/// Creates a new section at specified frames to the root controller.
		/// </summary>
		IFateSection<IFateFramer> AddSection(int startFrame, int endFrame, int listCapacity);

		/// <summary>
		/// Creates an update action at specified frames.
		/// </summary>
		IFateSection<IFateFramer> AddSection(int startFrame, int endFrame, FateFX.EventAction<IFateFramer> updateAction);

		/// <summary>
		/// Creates a new trigger at specified frame to the root controller.
		/// </summary>
		IFateTrigger<IFateFramer> AddTrigger(int frame, int listCapacity);

		/// <summary>
		/// Creates a trigger action at specified frame.
		/// </summary>
		IFateTrigger<IFateFramer> AddTrigger(int frame, FateFX.EventAction<IFateFramer> triggerAction);
	}
}

