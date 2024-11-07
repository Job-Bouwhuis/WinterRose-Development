using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Serialization.Version2;


[Serializable]
public class DeserializationFailedException : Exception
{
	public DeserializationFailedException() { }
	public DeserializationFailedException(string message) : base(message) { }
	public DeserializationFailedException(string message, Exception inner) : base(message, inner) { }
	protected DeserializationFailedException(
	  System.Runtime.Serialization.SerializationInfo info,
	  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
