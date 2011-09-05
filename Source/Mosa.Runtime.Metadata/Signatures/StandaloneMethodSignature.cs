/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Michael Ruck (grover) <sharpos@michaelruck.de>
 */


namespace Mosa.Runtime.Metadata.Signatures
{
	/// <summary>
	/// 
	/// </summary>
	public class StandaloneMethodSignature : MethodReferenceSignature
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="StandaloneMethodSignature"/> class.
		/// </summary>
		/// <param name="reader">The reader.</param>
		public StandaloneMethodSignature(SignatureReader reader)
			: base(reader)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StandaloneMethodSignature"/> class.
		/// </summary>
		/// <param name="provider">The provider.</param>
		/// <param name="token">The token.</param>
		public StandaloneMethodSignature(IMetadataProvider provider, HeapIndexToken token)
			: base(provider, token)
		{
		}
	}
}
