using System;
using System.Collections.Generic;

namespace Renko.LapseFramework.Internal
{
	/// <summary>
	/// A recycler framework for RenLapse.
	/// </summary>
	public class BaseRecycler<T> where T : class, IDisposable, new() {

		protected Stack<T> items;
		protected bool isRecycling;


		/// <summary>
		/// Whether items should be recycled.
		/// Default: true
		/// </summary>
		public bool ShouldRecycle
		{
			get { return isRecycling; }
			set
			{
				isRecycling = value;
				// If not recycling, instantly remove all items in the recycle stack.
				if(!isRecycling)
					items.Clear();
			}
		}


		public BaseRecycler(int capacity)
		{
			items = new Stack<T>(capacity);
			isRecycling = true;
		}

		/// <summary>
		/// Returns the next reusable lapser in the stack.
		/// If none, a new lapser will be created.
		/// </summary>
		public virtual T GetNext()
		{
			if(items.Count > 0)
				return items.Pop();
			return new T();
		}

		/// <summary>
		/// Adds specified item to recycle stack after disposing.
		/// </summary>
		public virtual void ReturnItem(T item)
		{
			// Dispose item to release references to other objects.
			item.Dispose();

			// Push the item to recycling stack only if recycling
			if(isRecycling)
				items.Push(item);
		}
	}
}

