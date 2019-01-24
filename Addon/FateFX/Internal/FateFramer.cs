using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renko.LapseFramework.Internal
{
	public class FateFramer : FateAnimation<FateFramer, IFateFramer>, IFateFramer {
		
		private int startFrame;
		private int endFrame;
		private int curFrame;
		private int prevFrame;
		private bool resetTime;

		
		public override float Progress
		{
			get
			{
				if(!isValid)
					throw GetInvalidException("Progress");
				if(endFrame == startFrame)
					return 0f;
				return Mathf.Clamp01((float)(curFrame - startFrame) / (float)Duration);
			}
			set
			{
				if(!isValid)
					throw GetInvalidException("Progress");
				if(isRootAnimation)
					SeekToInternal(Mathf.Clamp((int)(value * endFrame), 0, endFrame), true);
				else
				{
					if(rootAnimation.endFrame == 0)
						rootAnimation.Progress = 0f;
					else
					{
						float absoluteFrame = value * Duration + startFrame;
						rootAnimation.Progress = absoluteFrame / rootAnimation.endFrame;
					}
				}
			}
		}

		public int CurrentFrame
		{
			get
			{
				if(!isValid)
					throw GetInvalidException("CurrentFrame");
				return Mathf.Clamp(curFrame, startFrame, endFrame) - startFrame;
			}
		}

		public int Duration
		{
			get
			{
				if(!isValid)
					throw GetInvalidException("Duration");
				return endFrame - startFrame;
			}
		}


		public FateFramer() : base()
		{
			selfInterface = this;
		}

		public override void Recycle (FateFX owner, ILapser lapser, int listCapacity)
		{
			base.Recycle(owner, lapser, listCapacity);
			startFrame = 0;
			endFrame = 0;
			curFrame = 0;
			prevFrame = 0;
			resetTime = true;
		}

		public override void HandleUpdate (ILapser lapser)
		{
			// Handle frame skipping support
			int framesPassed = lapser.SkippedFrames + 1;
			while(framesPassed > 0)
			{
				framesPassed --;
				// Add frame
				curFrame ++;

				// Invoke OnStart event for this animation.
				if(prevFrame < 0 && curFrame >= 0)
					InvokeEvent(FateEvents.OnStart);
				
				// Reset time to 0 if required.
				if(resetTime)
				{
					resetTime = false;
					// Reset frame to beginning without invoking OnUpdate
					SeekToInternal(0, false);
				}
				
				// Invoke OnUpdate event for this animation.
				InvokeEvent(FateEvents.OnUpdate);
				// Handle update process for all updatable child animations
				var children = GetChildAnimations();
				bool isFirstChild = true;
				while(children.MoveNext())
				{
					FateFramer child = children.Current;
					child.curFrame = curFrame;

					// Invoke start action?
					if(prevFrame < child.startFrame && curFrame >= child.startFrame)
						child.InvokeEvent(FateEvents.OnStart);
					// Invoke update action?
					if(curFrame >= child.startFrame && curFrame <= child.endFrame)
						child.InvokeEvent(FateEvents.OnUpdate);
					// Invoke end action?
					if(prevFrame < child.endFrame && curFrame >= child.endFrame)
						child.InvokeEvent(FateEvents.OnEnd);

					if(isFirstChild)
					{
						isFirstChild = false;
						if(curFrame >= child.endFrame)
							curChildIndex ++;
					}
				}

				// If reached the end of animation
				if(curFrame >= endFrame)
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
						curFrame = endFrame;
						// Pause the playback
						base.Pause();
						// Seek to the last frame without invoking OnUpdate
						SeekToInternal(curFrame, false);
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
					prevFrame = curFrame;
				}
			}
		}

		public override int CompareTo(FateFramer other)
		{
			return startFrame.CompareTo(other.startFrame);
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
			if(endFrame == 0)
				return;
			if(curFrame >= endFrame && !isLoop)
				return;
			if(IsPlaying)
				return;
			
			base.Play();
			SeekToInternal(curFrame, true);
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
			SeekToInternal(curFrame, false);
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
			SeekToInternal(0, true);
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
			owner.RemoveFramer(this);
		}

		public void SeekTo(int frame)
		{
			if(!isValid)
				throw GetInvalidException("SeekTo");
			if(!isRootAnimation)
			{
				rootAnimation.SeekTo(frame);
				return;
			}

			resetTime = false;
			SeekToInternal(frame, true);
		}

		public IFateSection<IFateFramer> AddSection (int startFrame, int endFrame, int listCapacity = 0)
		{
			if(!isValid)
				throw GetInvalidException("AddSection");
			if(!isRootAnimation)
				return rootAnimation.AddSection(startFrame, endFrame, listCapacity);
			
			FateFramer framer = owner.GetNewFramer(null, listCapacity);
			framer.startFrame = startFrame;
			framer.endFrame = endFrame;
			framer.rootAnimation = this;

			EvaluateEndFrame(endFrame);
			InsertChild(framer);
			return framer;
		}

		public IFateSection<IFateFramer> AddSection(int startFrame, int endFrame,
			FateFX.EventAction<IFateFramer> updateAction)
		{
			var section = AddSection(startFrame, endFrame, 1);
			section.AddEvent(FateEvents.OnUpdate, updateAction);
			return section;
		}

		public IFateTrigger<IFateFramer> AddTrigger (int frame, int listCapacity = 0)
		{
			if(!isValid)
				throw GetInvalidException("AddTrigger");
			if(!isRootAnimation)
				return rootAnimation.AddTrigger(frame, listCapacity);
			
			FateFramer framer = owner.GetNewFramer(null, listCapacity);
			framer.startFrame = framer.endFrame = frame;
			framer.rootAnimation = this;

			EvaluateEndFrame(frame);
			InsertChild(framer);
			return framer;
		}

		public IFateTrigger<IFateFramer> AddTrigger(int frame, FateFX.EventAction<IFateFramer> triggerAction)
		{
			var trigger = AddTrigger(frame, 1);
			trigger.AddEvent(triggerAction);
			return trigger;
		}

		protected override bool CanUpdateAtCurrent(FateFramer framer)
		{
			return framer.startFrame <= curFrame;
		}

		protected override bool CanSetChildIndex (FateFramer child)
		{
			return child.endFrame >= curFrame;
		}

		/// <summary>
		/// Internal process of SeekTo method to support more arguments.
		/// </summary>
		void SeekToInternal(int frame, bool invokeUpdate)
		{
			// Set current time
			curFrame = Mathf.Clamp(frame, 0, Duration);
			prevFrame = curFrame - 1;

			// Find the child index to update from at this time.
			ResetChildIndex();

			// If the OnUpdate event should be called
			if(invokeUpdate)
			{
				// Invoke OnUpdate event for this animation
				InvokeEvent(FateEvents.OnUpdate);
				// Invoke OnUpdate event for child animations after updating current frame.
				ForEachChildAnimation((FateFramer child) => {
					child.curFrame = curFrame;
					child.InvokeEvent(FateEvents.OnUpdate);
				});
			}
		}

		/// <summary>
		/// Evaluates the new endFrame by selecting the higher endFrame value.
		/// </summary>
		void EvaluateEndFrame(int endFrame)
		{
			this.endFrame = Mathf.Max(this.endFrame, endFrame);
		}
	}
}

