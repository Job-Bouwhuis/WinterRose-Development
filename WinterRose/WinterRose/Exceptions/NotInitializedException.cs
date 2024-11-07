using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Exceptions
{

	/// <summary>
	/// Thrown when a certain object is not initialized when its trying to be used. <br></br>
	/// This may also apply on a certain value that is not requred for the object to be initialized, But is required to be initialized for a certain method.
	/// </summary>
	[Serializable]
	public class NotInitializedException : WinterException
    {
		public NotInitializedException() { }
		public NotInitializedException(string message) : base(message) { }
		public NotInitializedException(string message, Exception inner) : base(message, inner) { }
	}
}
