﻿/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Michael Ruck (grover) <sharpos@michaelruck.de>
 */

using System;
using System.IO;

namespace Mosa.Compiler.Pdb
{
	/// <summary>
	///
	/// </summary>
	public class CvPublicSymbol3 : CvSymbol
	{
		private readonly int symtype;
		private readonly int offset;
		private readonly short segment;
		private readonly string name;

		/// <summary>
		/// Initializes a new instance of the <see cref="CvPublicSymbol3"/> struct.
		/// </summary>
		/// <param name="length">The length of the symbol in the stream.</param>
		/// <param name="type">The type of the CodeView entry.</param>
		/// <param name="reader">The reader used to access the stream.</param>
		internal CvPublicSymbol3(ushort length, CvEntryType type, BinaryReader reader) :
			base(length, type)
		{
			this.symtype = reader.ReadInt32();
			this.offset = reader.ReadInt32();
			this.segment = reader.ReadInt16();
			this.name = CvUtil.ReadString(reader);
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			return String.Format("Public {0} {1}:{2} {3}", this.symtype, this.segment, this.offset, this.name);
		}
	}
}