using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renko.LapseFramework.Internal
{
	public class FateTimer : FateAnimation<FateTimer, IFateTimer>, IFateTimer {

		private float startTime;
		private float endTime;
		private float curTime;
		private float prevTime;
		private bool resetTime;


		public override float Progress
		{
			get
			{
				if(!isValid)
					throw GetInvalidException("Progress");
				if(endTime == startTime)
					return 0f;
				return Mathf.Clamp01((curTime - startTime) / Duration);
			}
			set
			{
				if(!isValid)
					throw GetInvalidException("Progress");
				if(isRootAnimation)
					SeekToInternal(Mathf.Clamp(value * endTime, 0f, endTime), true);
				else
				{
					if(rootAnimation.endTime == 0)
						rootAnimation.Progress = 0f;
					else
					{
						float absoluteTime = value * Duration + startTime;
						rootAnimation.Progress = absoluteTime / rootAnimation.endTime;
					}
				}
			}
		}

		public float CurrentTime
		{
			get
			{
				if(!isValid)
					throw GetInvalidException("CurrentTime");
				return Mathf.Clamp(curTime, startTime, endTime) - startTime;
			}
		}

		public float Duration
		{
			get
			{
				if(!isValid)
					throw GetInvalidException("Duration");
				return endTime - startTime;
			}
		}


		public FateTimer() : base()
		{
			selfInterface = this;
		}

		public override void Recycle (FateFX owner, ILapser lapser, int listCapacity)
		{
			base.Recycle(owner, lapser, listCapacity);
			startTime = 0f;
			endTime = 0f;
			curTime = 0f;
			prevTime = 0f;
			resetTime = true;
		}

		public override void HandleUpdate (ILapser lapser)
		{
			// Cache delta time from lapser
			float deltaTime = lapser.DeltaTime * curSpeed;
			while(deltaTime > 0f)
			{
				if(!resetTime)
				{
					// If adding delta time would go past the end time
					if(curTime + deltaTime > endTime)
					{
						// Add cur time only up to where it reaches the end time.
						deltaTime -= endTime - curTime;
						curTime = endTime;
					}
					else
					{
						curTime += deltaTime;
						deltaTime = 0f;
					}

					// Invoke OnStart event for this animation.
					if(prevTime < 0 && curTime >= 0)
						InvokeEvent(FateEvents.OnStart);
				}
				else
				{
					resetTime = false;
					SeekToInternal(0, false);
				}

				// Invoke OnUpdate event for this animation.
				InvokeEvent(FateEvents.OnUpdate);

				// Handle update process for all updatable child animations
				var children = GetChildAnimations();
				bool isFirstChild = true;
				while(children.MoveNext())
				{
					FateTimer child = children.Current;
					child.curTime = curTime;

					// Invoke start action?
					if(prevTime < child.startTime && curTime >= child.startTime)
					{
						child.InvokeEvent(FateEvents.OnStart);
						child.InvokeEvent(FateEvents.OnUpdate);

						// Invoke end action as well?
						if(curTime >= child.endTime)
							child.InvokeEvent(FateEvents.OnEnd);
					}
					// Invoke update action?
					else if(curTime >= child.startTime && curTime < child.endTime)
						child.InvokeEvent(FateEvents.OnUpdate);
					// Invoke end action?
					else if(prevTime < child.endTime && curTime >= child.endTime)
					{
						child.InvokeEvent(FateEvents.OnUpdate);
						child.InvokeEvent(FateEvents.OnEnd);
					}

					if(isFirstChild)
					{
						isFirstChild = false;
						if(curTime >= child.endTime)
							curChildIndex ++;
					}
				}

				// If reached the end of animation
				if(curTime >= endTime)
				{
					// If looping
					if(isLoop)
					{
						// Should reset time on next update.
						resetTime = true;
						// Invoke OnLoop event
						InvokeEvent(FateEvents.OnLoop);
					}
					else
					{
						// Clamp time
						curTime = endTime;
						// Pause the playback
						base.Pause();
						// Seek to the last frame without invoking OnUpdate
						SeekToInternal(curTime, false);
						// Invoke OnEnd event on this animation
						InvokeEvent(FateEvents.OnEnd);
						// If controller is flagged to destroy on end, just call Destroy().
						if(isDestroyOnEnd)
							Destroy();
						// Stop further update
						break;
					}
				}
				else
				{
					// Store previous frame
					prevTime = curTime;
				}
			}
		}

		public override int CompareTo(FateTimer other)
		{
			return startTime.CompareTo(other.startTime);
		}

		public override void Play()
		{
			if(!isValid)
				throw GetInvalidException("Play");
			if(!isRootAnimation)
			{
				rootAnimation.Play();
				return;
			}
			if(endTime == 0f)
				return;
			if(curTime >= endTime && !isLoop)
				return;
			if(IsPlaying)
				return;
			
			base.Play();
			SeekToInternal(curTime, true);
			InvokeEvent(FateEvents.OnPlay);

			// Invoke OnPlay event to child animations
			InvokeUpdatableChildEvent(FateEvents.OnPlay);
		}

		public override void Pause()
		{
			if(!isValid)
				throw GetInvalidException("Pause");
			if(!isRootAnimation)
			{
				rootAnimation.Pause();
				return;
			}
			base.Pause();
			SeekToInternal(curTime, false);
			InvokeEvent(FateEvents.OnPause);

			// Invoke OnPause event to chlid animations
			InvokeUpdatableChildEvent(FateEvents.OnPause);
		}

		public override void Stop()
		{
			if(!isValid)
				throw GetInvalidException("Stop");
			if(!isRootAnimation)
			{
				rootAnimation.Stop();
				return;
			}
			base.Stop();
			SeekToInternal(0f, true);
			InvokeEvent(FateEvents.OnStop);

			resetTime = true;

			// Invoke OnStop event to child animations
			InvokeUpdatableChildEvent(FateEvents.OnStop);
		}

		public override void Destroy ()
		{
			if(!isValid)
				throw GetInvalidException("Destroy");
			if(!isRootAnimation)
			{
				if(rootAnimation != null && rootAnimation.isValid)
				{
					rootAnimation.Destroy();
					return;
				}
			}

			// Internal destruction of this object
			base.Destroy ();

			// Returning instance to recycler.
			owner.RemoveTimer(this);
		}

		public void SeekTo(float time)
		{
			if(!isValid)
				throw GetInvalidException("SeekTo");
			if(!isRootAnimation)
			{
				rootAnimation.SeekTo(time);
				return;
			}

			resetTime = false;
			SeekToInternal(time, true);
		}

		public IFateSection<IFateTimer> AddSection (float startTime, float endTime, int listCapacity = 0)
		{
			if(!isValid)
				throw GetInvalidException("AddSection");
			if(!isRootAnimation)
				return rootAnimation.AddSection(startTime, endTime, listCapacity);
			
			FateTimer timer = owner.GetNewTimer(null, listCapacity);
			timer.startTime = startTime;
			timer.endTime = endTime;
			timer.rootAnimation = this;

			EvaluateEndTime(endTime);
			InsertChild(timer);
			return timer;
		}

		public IFateTrigger<IFateTimer> AddTrigger (float time, int listCapacity = 0)
		{
			if(!isValid)
				throw GetInvalidException("AddTrigger");
			if(!isRootAnimation)
				return rootAnimation.AddTrigger(time, listCapacity);
			
			FateTimer timer = owner.GetNewTimer(null, listCapacity);
			timer.startTime = timer.endTime = time;
			timer.rootAnimation = this;

			EvaluateEndTime(time);
			InsertChild(timer);
			return timer;
		}

		protected override bool CanUpdateAtCurrent(FateTimer child)
		{
			return child.startTime <= curTime;
		}

		protected override bool CanSetChildIndex (FateTimer child)
		{
			return child.endTime >= curTime;
		}

		/// <summary>
		/// Internal process of SeekTo method to support more arguments.
		/// </summary>
		void SeekToInternal(float time, bool invokeUpdate)
		{
			// Set current time
			curTime = Mathf.Clamp(time, 0f, Duration);
			prevTime = curTime - (curSpeed / originalFPS);

			// Find the child index to update from at this time.
			ResetChildIndex();

			// If the OnUpdate event should be called
			if(invokeUpdate)
			{
				// Invoke OnUpdate event for this animation
				InvokeEvent(FateEvents.OnUpdate);
				// Invoke OnUpdate event for child animations after updating current frame.
				ForEachChildAnimation((FateTimer child) => {
					child.curTime = curTime;
					child.InvokeEvent(FateEvents.OnUpdate);
				});
			}
		}

		/// <summary>
		/// Evaluates the new endTime by selecting the higher endTime value.
		/// </summary>
		void EvaluateEndTime(float endTime)
		{
			this.endTime = Mathf.Max(this.endTime, endTime);
		}
	}
}

