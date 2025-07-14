using UnityEngine.Events;

namespace vz777.Foundations
{
	/// <summary>
	/// An interface that contains generic show and hide function, and should invoke the event afterward.
	/// </summary>
	public interface IShowBehavior
	{
		/// <summary>
		/// These events will be invoked after showing or hiding.
		/// </summary>
		event UnityAction Showed, Hid;
		
		/// <summary>
		/// To request for a show behavior.
		/// </summary>
		void Show();
		
		/// <summary>
		/// To request for a hide behavior.
		/// </summary>
		void Hide();
	}
}