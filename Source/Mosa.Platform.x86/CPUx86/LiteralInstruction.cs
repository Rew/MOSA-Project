﻿/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Michael Ruck (grover) <sharpos@michaelruck.de>
 */


using Mosa.Runtime.CompilerFramework;
using Mosa.Runtime.CompilerFramework.Operands;

namespace Mosa.Platform.x86.CPUx86
{
	/// <summary>
	/// Intermediate representation of the literal instruction.
	/// </summary>
	public sealed class LiteralInstruction : BaseInstruction
	{

		#region Methods

		/// <summary>
		/// Creates the operand.
		/// </summary>
		/// <param name="ctx">The CTX.</param>
		/// <returns></returns>
		public Operand CreateOperand(Context ctx)
		{
			/* HACK: 
			 * Position independent code on x86 requires EIP relative addressing, which
			 * unfortunately isn't available. We try to work around this limitation by
			 * storing the EIP of the first instruction on the stack, however this isn't
			 * enough. So PIC with Literals doesn't work for now.
			 */
			//return new LabelOperand(this.Type, GeneralPurposeRegister.EBP, -8, this.Label);
			return new LabelOperand(ctx.LiteralData.Type, null, 0, ctx.LiteralData.Label);
		}

		#endregion // Methods


	}
}
