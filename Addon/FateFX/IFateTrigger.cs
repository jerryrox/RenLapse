using System;

namespace Renko.LapseFramework
{
	/// <summary>
	/// Allows adding callbacks upon reaching the specified frame/time during animation.
	/// </summary>
	public interface IFateTrigger<TI> where TI : IFateController<TI> {

		/// <summary>
		/// Adds the specified action to listen to this trigger.
		/// </summary>
		void AddEvent(FateFX.EventAction<TI> action);

		/// <summary>
		/// Removes the specified action from listening to this trigger.
		/// </summary>
		void RemoveEvent(FateFX.EventAction<TI> action);
	}
}

