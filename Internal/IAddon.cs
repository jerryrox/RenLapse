using System;

namespace Renko.LapseFramework.Internal
{
	/// <summary>
	/// Interface for RenLapse addons.
	/// </summary>
	public interface IAddon {

		/// <summary>
		/// Event that is called when ShouldRecycle property of RenLapse was set.
		/// </summary>
		void OnRecycleFlagSet(bool shouldRecycle);
	}
}

