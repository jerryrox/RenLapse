using System;
using System.Collections.Generic;
using UnityEngine;

namespace Renko.LapseFramework.Internal
{
	/// <summary>
	/// Abstract animation class for either frame-based ani or time-based ani.
	/// </summary>
	public abstract class FateAnimation<T, TI> : IFateController<TI>, IFateTrigger<TI>, IComparable<T>, ILapseListener,
		IDisposable where T : FateAnimation<T, TI> where TI : IFateController<TI> {

		protected FateFX owner;
		protected T rootAnimation;
		protected TI selfInterface;
		protected ILapser lapser;
		protected bool isDestroyOnEnd;
		protected bool isValid;
		protected bool isLoop;
		protected bool isRootAnimation;
		protected float originalFPS;
		protected float curSpeed;
		protected int curChildIndex;
		protected List<T> childAnimations;
		protected List<FateFX.EventAction<TI>>[] eventActions;


		public abstract float Progress { get; set; }

		public bool IsValid
		{
			get
			{
				if(isRootAnimation)
					return isValid;
				return rootAnimation != null ? rootAnimation.isValid : false;
			}
		}

		public bool IsPlaying
		{
			get
			{
				if(!isValid)
					throw GetInvalidException("IsPlaying");
				return isRootAnimation ? lapser.IsUpdating : rootAnimation.lapser.IsUpdating;
			}
		}

		public bool IsLoop
		{
			get
			{
				if(!isValid)
					throw GetInvalidException("IsLoop");
				return isRootAnimation ? isLoop : rootAnimation.isLoop;
			}
			set
			{
				if(!isValid)
					throw GetInvalidException("IsLoop");
				if(isRootAnimation)
					isLoop = value;
				else
					rootAnimation.IsLoop = value;
			}
		}

		public bool IsDestroyOnEnd
		{
			get
			{
				if(!isValid)
					throw GetInvalidException("IsDestroyOnEnd");
				return isRootAnimation ? isDestroyOnEnd : rootAnimation.isDestroyOnEnd;
			}
			set
			{
				if(!isValid)
					throw GetInvalidException("IsDestroyOnEnd");
				if(isRootAnimation)
					isDestroyOnEnd = value;
				else
					rootAnimation.IsDestroyOnEnd = value;
			}
		}

		public bool IsFixedDeltaTime
		{
			get
			{
				if(!isValid)
					throw GetInvalidException("IsFixedDeltaTime");
				return isRootAnimation ? lapser.IsFixedDeltaTime : rootAnimation.lapser.IsFixedDeltaTime;
			}
			set
			{
				if(!isValid)
					throw GetInvalidException("IsFixedDeltaTime");
				if(isRootAnimation)
					lapser.IsFixedDeltaTime = value;
				else
					rootAnimation.IsFixedDeltaTime = value;
			}
		}

		public bool IsSkipFrames
		{
			get
			{
				if(!isValid)
					throw GetInvalidException("IsSkipFrames");
				return isRootAnimation ? lapser.MaxSkipFrames != 0 : rootAnimation.IsSkipFrames;
			}
			set
			{
				if(!isValid)
					throw GetInvalidException("IsSkipFrames");
				if(isRootAnimation)
					lapser.MaxSkipFrames = value ? RenLapse.Frame_AlwaysSkip : RenLapse.Frame_NoSkip;
				else
					rootAnimation.IsSkipFrames = value;
			}
		}

		public float Speed
		{
			get
			{
				if(!isValid)
					throw GetInvalidException("Speed");
				return curSpeed;
			}
			set
			{
				if(!isValid)
					throw GetInvalidException("Speed");
				if(isRootAnimation)
				{
					curSpeed = Mathf.Clamp(value, float.Epsilon, value);
					lapser.TargetFrameRate = curSpeed * originalFPS;
				}
				else
					rootAnimation.Speed = value;
			}
		}


		public FateAnimation()
		{
			childAnimations = new List<T>();

			eventActions = new List<FateFX.EventAction<TI>>[8];
			for(int i=0; i<eventActions.Length; i++)
				eventActions[i] = new List<FateFX.EventAction<TI>>();
		}

		public virtual void Recycle(FateFX owner, ILapser lapser, int listCapacity)
		{
			this.owner = owner;
			this.rootAnimation = null;
			this.lapser = lapser;
			this.isDestroyOnEnd = false;
			this.isLoop = false;
			this.isValid = true;
			this.curChildIndex = 0;

			if(lapser != null)
			{
				isRootAnimation = true;
				originalFPS = lapser.TargetFrameRate;
				Speed = 1f;
				childAnimations.Capacity = listCapacity;

				// Attach this object as listener to the lapser
				lapser.AddListener(this);
			}
			else
			{
				isRootAnimation = false;
				childAnimations.Capacity = 0;
			}
		}

		public abstract void HandleUpdate(ILapser lapser);

		public abstract int CompareTo (T other);

		public void OnLapseStart (ILapser lapser) {}

		public bool OnLapseUpdate (ILapser lapser)
		{
			HandleUpdate(lapser);
			return true;
		}

		public void OnLapseResume (ILapser lapser) {}

		public void OnLapsePause (ILapser lapser) {}

		public void OnLapseStop (ILapser lapser) {}

		public void OnLapseEnd (ILapser lapser) {}

		public virtual void Play ()
		{
			lapser.Start();
		}

		public virtual void Pause ()
		{
			lapser.Pause();
		}

		public virtual void Stop ()
		{
			Pause();
		}

		public virtual void Destroy ()
		{
			if(lapser != null)
			{
				lapser.RemoveListener(this);
				lapser.Destroy();
				lapser = null;
			}

			// Invoke OnDestroy event only if root animation
			if(isRootAnimation)
				InvokeEvent(FateEvents.OnDestroy);

			for(int i=0; i<eventActions.Length; i++)
				eventActions[i].Clear();

			isValid = false;

			// Triggering destruction of child animations
			for(int i=0; i<childAnimations.Count; i++)
				childAnimations[i].Destroy();
			childAnimations.Clear();
		}

		public void AddEvent (FateEvents type, FateFX.EventAction<TI> action)
		{
			if(!isValid)
				throw GetInvalidException("AddEvent");
			eventActions[(int)type].Add(action);
		}

		public void RemoveEvent (FateEvents type, FateFX.EventAction<TI> action)
		{
			if(!isValid)
				throw GetInvalidException("RemoveEvent");
			var actions = eventActions[(int)type];
			for(int i=0; i<actions.Count; i++)
			{
				if(actions[i] == action)
				{
					actions[i] = null;
					break;
				}
			}
		}

		public virtual void Dispose () {}

		void IFateTrigger<TI>.AddEvent (FateFX.EventAction<TI> action)
		{
			if(!isValid)
				throw GetInvalidException("AddEvent");
			// Add event to OnStart.
			(this as FateAnimation<T, TI>).AddEvent(FateEvents.OnStart, action);
		}

		void IFateTrigger<TI>.RemoveEvent (FateFX.EventAction<TI> action)
		{
			if(!isValid)
				throw GetInvalidException("RemoveEvent");
			// Remove event from OnStart.
			(this as FateAnimation<T, TI>).RemoveEvent(FateEvents.OnStart, action);
		}

		/// <summary>
		/// Returns whether specified child can update at current time/frame.
		/// </summary>
		protected abstract bool CanUpdateAtCurrent(T child);

		/// <summary>
		/// Returns whether index of specified child should be the new curChildIndex when time/frame seeking.
		/// </summary>
		protected abstract bool CanSetChildIndex(T child);

		/// <summary>
		/// Invokes all registered actions in the specified event type of updatable children.
		/// </summary>
		protected void InvokeUpdatableChildEvent(FateEvents type)
		{
			for(int i=curChildIndex; i<childAnimations.Count; i++)
			{
				T child = childAnimations[i];
				if(!CanUpdateAtCurrent(child))
					return;
				child.InvokeEvent(type);
			}
		}

		/// <summary>
		/// Invokes all registered actions in the specified event type of all children.
		/// </summary>
		protected void InvokeAllChildEvent(FateEvents type)
		{
			for(int i=0; i<childAnimations.Count; i++)
			{
				childAnimations[i].InvokeEvent(type);
			}
		}

		/// <summary>
		/// Invokes all registered actions in the specified event type.
		/// </summary>
		protected void InvokeEvent(FateEvents type)
		{
			List<FateFX.EventAction<TI>> actions = eventActions[(int)type];
			for(int i=0; i<actions.Count; i++)
			{
				// Removing null actions, just in case.
				if(actions[i] == null)
				{
					actions.RemoveAt(i);
					i--;
					continue;
				}
				actions[i].Invoke(selfInterface);
			}
		}

		/// <summary>
		/// Returns an enumerator of all child animations that can update at current time/frame.
		/// </summary>
		protected IEnumerator<T> GetChildAnimations()
		{
			for(int i=curChildIndex; i<childAnimations.Count; i++)
			{
				T child = childAnimations[i];
				if(!CanUpdateAtCurrent(child))
					yield break;
				yield return child;
			}
		}

		/// <summary>
		/// Handles foreach operation for all child animations that can update at current time/frame.
		/// </summary>
		protected void ForEachChildAnimation(Action<int, T> action)
		{
			for(int i=curChildIndex; i<childAnimations.Count; i++)
			{
				T child = childAnimations[i];
				if(!CanUpdateAtCurrent(child))
					return;
				action(i, child);
			}
		}

		/// <summary>
		/// Handles foreach operation for all child animations that can update at current time/frame.
		/// </summary>
		protected void ForEachChildAnimation(Action<T> action)
		{
			for(int i=curChildIndex; i<childAnimations.Count; i++)
			{
				T child = childAnimations[i];
				if(!CanUpdateAtCurrent(child))
					return;
				action(child);
			}
		}

		/// <summary>
		/// Resets curChildIndex that matches the CanSetChildIndex condition.
		/// </summary>
		protected void ResetChildIndex()
		{
			curChildIndex = 0;
			for(int i=0; i<childAnimations.Count; i++)
			{
				T child = childAnimations[i];
				if(CanSetChildIndex(child))
				{
					curChildIndex = i;
					break;
				}
			}
		}

		/// <summary>
		/// Inserts the specified controller to children list, sorting by starting time/frame
		/// in an ascending order.
		/// </summary>
		protected void InsertChild(T child)
		{
			// Find the insertion index of the specified child
			int insertIndex = GetChildInsertIndex(child);
			if(insertIndex < 0)
			{
				// Convert to the actual insert index
				insertIndex = ~insertIndex;
			}
			else
			{
				insertIndex ++;
			}

			// Insert child
			if(insertIndex >= childAnimations.Count)
				childAnimations.Add(child);
			else
				childAnimations.Insert(insertIndex, child);
		}

		/// <summary>
		/// Returns the index where the specified child should be inserted into animation list.
		/// </summary>
		protected int GetChildInsertIndex(T child)
		{
			return childAnimations.BinarySearch(child);
		}

		/// <summary>
		/// Returns an InvalidOperationException object for throwing exception when accessing a member
		/// of an invalid controller.
		/// </summary>
		protected InvalidOperationException GetInvalidException(string memberName)
		{
			return new InvalidOperationException(
				string.Format("FateAnimation.{0} - Attempted to access an invalid controller!", memberName)
			);
		}
	}
}

