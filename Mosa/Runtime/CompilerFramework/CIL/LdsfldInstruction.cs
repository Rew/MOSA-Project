﻿/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Phil Garcia (tgiphil) <phil@thinkedge.com>
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Mosa.Runtime.Metadata;
using Mosa.Runtime.Vm;

namespace Mosa.Runtime.CompilerFramework.CIL
{
	/// <summary>
	/// 
	/// </summary>
	public class LdsfldInstruction : CILInstruction
	{
		#region Construction

		/// <summary>
		/// Initializes a new instance of the <see cref="LdsfldInstruction"/> class.
		/// </summary>
		/// <param name="opcode">The opcode.</param>
		public LdsfldInstruction(OpCode opcode)
			: base(opcode)
		{
		}

		#endregion // Construction

		#region CILInstruction Overrides

		/// <summary>
		/// Decodes the specified instruction.
		/// </summary>
		/// <param name="instruction">The instruction.</param>
		/// <param name="decoder">The instruction decoder, which holds the code stream.</param>
		public override void Decode(ref InstructionData instruction, IInstructionDecoder decoder)
		{
			// Decode base classes first
			base.Decode(ref instruction, decoder);

			// Read the _stackFrameIndex From the code
			TokenTypes token;
			decoder.Decode(out token);
			instruction.Field = RuntimeBase.Instance.TypeLoader.GetField(decoder.Compiler.Assembly, token);

			Debug.Assert((instruction.Field.Attributes & FieldAttributes.Static) == FieldAttributes.Static, @"Static field access on non-static field.");
			instruction.Result = decoder.Compiler.CreateTemporary(instruction.Field.Type);
		}

		#endregion // CILInstruction Overrides

	}
}
