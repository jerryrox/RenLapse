using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Renko.LapseFramework.Internal;

namespace Renko.LapseFramework
{
	/// <summary>
	/// RenLapse framework manager.
	/// </summary>
	public sealed class RenLapse : MonoBehaviour {

		public const int Frame_NoSkip = 0;
		public const int Frame_AlwaysSkip = 99999999;

		private static RenLapse I;
		private static int CurID;

		private List<IAddon> addons;

		private bool isLapsersDirty;
		private List<Lapser> lapsers;
		private LapserRecycler recycler;


		/// <summary>
		/// Whether RenLapse should recycle objects created for handling time events, etc.
		/// RenLapse must be initialized first.
		/// Default: true
		/// </summary>
		public static bool ShouldRecycle
		{
			get
			{
				if(I == null)
					throw new NullReferenceException("RenLapse.ShouldRecycle - RenLapse is not initialized!");
				return I.recycler.ShouldRecycle;
			}
			set
			{
				if(I == null)
					throw new NullReferenceException("RenLapse.ShouldRecycle - RenLapse is not initialized!");
				I.recycler.ShouldRecycle = value;
				I.NotifyAddons(addon => addon.OnRecycleFlagSet(value));
			}
		}

		/// <summary>
		/// Returns the next id to assign for a new Lapser object.
		/// </summary>
		private static int NextID
		{
			get { return CurID++; }
		}


		/// <summary>
		/// Initializes the RenLapse manager.
		/// </summary>
		public static void Initialize(int lapserCapacity = 0)
		{
			if(I != null)
				return;
			if(lapserCapacity < 0)
				throw new ArgumentException("RenLapse.Initialize - lapserCapacity must be zero or greater!");

			I = new GameObject("_RenLapse").AddComponent<RenLapse>();
			I.OnInitialize(lapserCapacity);
		}

		/// <summary>
		/// Returns the lapser associated with specified id.
		/// RenLapse must be initialized first.
		/// </summary>
		public static ILapser FindLapserWithID(int id)
		{
			if(I == null)
				throw new NullReferenceException("RenLapse.FindLapserWithID - RenLapse is not initialized!");

			for(int i=0; i<I.lapsers.Count; i++)
			{
				if(I.lapsers[i].ID == id)
					return I.lapsers[i];
			}
			return null;
		}

		/// <summary>
		/// Creates a new lapser and returns it.
		/// RenLapse must be initialized first.
		/// </summary>
		public static ILapser CreateLapser(int listenerCapacity = 0)
		{
			if(I == null)
				throw new NullReferenceException("RenLapse.CreateLapser - RenLapse is not initialized!");
			if(listenerCapacity < 0)
				throw new ArgumentException("RenLapse.CreateLapser - listenerCapacity must be zero or greater!");
			
			return I.recycler.GetNext(NextID, listenerCapacity);
		}

		/// <summary>
		/// (Internal) Adds the specified addon to RenLapse.
		/// </summary>
		public static void AttachAddon(IAddon addon)
		{
			if(I == null)
				throw new NullReferenceException("RenLapse.AttachAddon - RenLapse is not initialized!");
			if(addon == null)
				throw new ArgumentNullException("RenLapse.AttachAddon - addon must not be null!");

			if(!I.addons.Contains(addon))
			{
				I.addons.Add(addon);

				// Notify all events.
				bool shouldRecycle = ShouldRecycle;
				I.NotifyAddons(a => a.OnRecycleFlagSet(shouldRecycle));
			}
		}

		/// <summary>
		/// Attaches the specified lapser for updating.
		/// </summary>
		public void AttachLapser(Lapser lapser)
		{
			// Add lapser to end of the list and make list dirty.
			// During update, lapsers will be resorted by their priority.
			isLapsersDirty = true;
			lapsers.Add(lapser);
		}

		/// <summary>
		/// Detaches the specified lapser from updating.
		/// </summary>
		public void DetachLapser(Lapser lapser)
		{
			for(int i=lapsers.Count-1; i>=0; i--)
			{
				if(lapsers[i] == lapser)
				{
					lapsers[i] = null;
					break;
				}
			}
		}

		/// <summary>
		/// Returns whether specified lapser is currently in update queue.
		/// </summary>
		public bool ContainsLapser(Lapser lapser)
		{
			return lapsers.Contains(lapser);
		}

		void OnInitialize(int lapserCapacity)
		{
			addons = new List<IAddon>();

			lapsers = new List<Lapser>(lapserCapacity);
			recycler = new LapserRecycler(this, lapserCapacity);
		}

		void Update()
		{
			// If lapsers list has been added with new lapsers, it must be sorted by priority first.
			if(isLapsersDirty)
			{
				isLapsersDirty = false;
				SortLapsers();
			}

			float deltaTime = Time.deltaTime;
			for(int i=lapsers.Count-1; i>=0; i--)
			{
				Lapser lapser = lapsers[i];
				if(lapser == null)
				{
					lapsers.RemoveAt(i);
					continue;
				}
				// Do update. If lapser should be destroyed
				if(!lapser.Update(deltaTime))
				{
					// Release the lapser to recycler and remove from update list.
					recycler.ReturnItem(lapser);
					lapsers.RemoveAt(i);
				}
			}
		}

		/// <summary>
		/// Callback from active scene change.
		/// </summary>
		void OnSceneLoaded()
		{
			// Simply search through lapsers, destroy, and nullify them.
			// Removal should be handled during update.
			for(int i=lapsers.Count-1; i>=0; i--)
			{
				if(lapsers[i] != null && lapsers[i].IsDestroyOnLoad)
				{
					recycler.ReturnItem(lapsers[i]);
					lapsers[i] = null;
				}
			}
		}

		/// <summary>
		/// Sorts all lapsers by their priority.
		/// </summary>
		void SortLapsers()
		{
			lapsers.Sort((Lapser a, Lapser b) => {
				if(a == null && b == null)
					return 0;
				else if(a == null)
					return int.MaxValue;
				else if(b == null)
					return int.MinValue;

				return b.Priority.CompareTo(a.Priority);
			});
		}

		/// <summary>
		/// Notifies all RenLapse addons of a specific event.
		/// </summary>
		void NotifyAddons(Action<IAddon> actionHandler)
		{
			for(int i=0; i<addons.Count; i++)
				actionHandler(addons[i]);
		}
	}
}