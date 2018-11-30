using System;

namespace Renko.LapseFramework
{
	/// <summary>
	/// Represents a certain section between two frames/times in a fate controller.
	/// </summary>
	public interface IFateSection<TI> {

		/// <summary>
		/// Adds specified action to listen to a certain event in this section.
		/// </summary>
		void AddEvent(FateEvents type, FateFX.EventAction<TI> action);

		/// <summary>
		/// Removes specified action from a certain event in this section.
		/// </summary>
		void RemoveEvent(FateEvents type, FateFX.EventAction<TI> action);
	}
}

