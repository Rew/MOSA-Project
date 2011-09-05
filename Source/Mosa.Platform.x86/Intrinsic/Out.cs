﻿/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Simon Wollwage (rootnode) <kintaro@think-in-co.de>
 */


using System.Collections.Generic;
using Mosa.Runtime.CompilerFramework;
using Mosa.Runtime.CompilerFramework.Operands;
using Mosa.Runtime.TypeSystem;

namespace Mosa.Platform.x86.Intrinsic
{
	/// <summary>
	/// Representations the x86 out instruction.
	/// </summary>
	public sealed class Out : IIntrinsicMethod
	{

		#region Methods

		/// <summary>
		/// Replaces the intrinsic call site
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="typeSystem">The type system.</param>
		public void ReplaceIntrinsicCall(Context context, ITypeSystem typeSystem, IList<RuntimeParameter> parameters)
		{
			Operand operand1 = context.Operand1;
			Operand operand2 = context.Operand2;

			RegisterOperand edx = new RegisterOperand(operand1.Type, GeneralPurposeRegister.EDX);
			RegisterOperand eax = new RegisterOperand(operand2.Type, GeneralPurposeRegister.EAX);

			context.SetInstruction(CPUx86.Instruction.MovInstruction, edx, operand1);
			context.AppendInstruction(CPUx86.Instruction.MovInstruction, eax, operand2);
			context.AppendInstruction(CPUx86.Instruction.OutInstruction, null, edx, eax);
		}

		#endregion // Methods

	}
}
