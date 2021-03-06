﻿/*
 * (c) 2012 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Phil Garcia (tgiphil) <phil@thinkedge.com>
 */

using Mosa.Compiler.Framework;

namespace Mosa.Platform.x86.Stages
{
	/// <summary>
	///
	/// </summary>
	public class FloatingPointStage : BaseTransformationStage
	{
		protected override void Run()
		{
			foreach (var block in BasicBlocks)
			{
				for (var node = block.First; !node.IsBlockEndInstruction; node = node.Next)
				{
					if (node.IsEmpty || !(node.Instruction is X86Instruction))
						continue;

					if (node.Instruction == X86.Jmp || node.Instruction == X86.FarJmp)
						continue;

					// Convert any floating point constants into labels
					EmitFloatingPointConstants(node);

					// No floating point opcode allows both the result and operand to be a memory location
					// if necessary, load into register first
					if (node.OperandCount == 1
						&& node.ResultCount == 1
						&& node.Operand1.IsMemoryAddress
						&& node.Result.IsMemoryAddress
						&& (node.Result.IsR || node.Operand1.IsR))
					{
						LoadFirstOperandIntoRegister(node);
					}
					else
						// No two-operand floating point opcode allows the first operand to a memory operand
						if (node.OperandCount == 2 && node.Operand1.IsMemoryAddress && node.Operand1.IsR)
						{
							if (IsCommutative(node.Instruction))
							{
								// swap operands
								var t = node.Operand2;
								node.Operand2 = node.Operand1;
								node.Operand1 = t;
							}
							else
							{
								LoadFirstOperandIntoRegister(node);
							}
						}
				}
			}
		}

		private void LoadFirstOperandIntoRegister(InstructionNode node)
		{
			// load into a register
			Operand operand = node.Operand1;

			Operand register = AllocateVirtualRegister(operand.Type);
			node.Operand1 = register;

			var move = GetMove(register, operand);

			var newNode = new InstructionNode(move, register, operand);
			node.Previous.Insert(newNode);
		}

		private bool IsCommutative(BaseInstruction instruction)
		{
			return (instruction == X86.Addsd || instruction == X86.Addss || instruction == X86.Mulsd || instruction == X86.Mulss);
		}
	}
}
