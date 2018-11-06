using System;

namespace Renko.Frameworks
{
	public interface ILapseListener {

		/// <summary>
		/// Handles lapse start event from lapser.
		/// </summary>
		void OnLapseStart(ILapser lapser);

		/// <summary>
		/// Handles lapse update event from lapser.
		/// </summary>
		bool OnLapseUpdate(ILapser lapser);

		/// <summary>
		/// Handles lapse resume event from lapser.
		/// </summary>
		void OnLapseResume(ILapser lapser);

		/// <summary>
		/// Handles lapse pause event from lapser.
		/// </summary>
		void OnLapsePause(ILapser lapser);

		/// <summary>
		/// Handles lapse end event from lapser.
		/// </summary>
		void OnLapseEnd(ILapser lapser);
	}
}

