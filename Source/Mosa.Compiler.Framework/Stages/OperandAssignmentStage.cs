﻿/*
 * (c) 2011 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Phil Garcia (tgiphil) <phil@thinkedge.com>
 *  Simon Wollwage (rootnode) <kintaro@think-in-co.de>
*/

using Mosa.Compiler.Framework.CIL;
using Mosa.Compiler.Framework.IR;
using System.Collections;
using System.Collections.Generic;

namespace Mosa.Compiler.Framework.Stages
{
	/// <summary>
	///
	/// </summary>
	public sealed class OperandAssignmentStage : BaseMethodCompilerStage
	{
		/// <summary>
		///
		/// </summary>
		private sealed class WorkItem
		{
			/// <summary>
			///
			/// </summary>
			public BasicBlock Block;

			/// <summary>
			///
			/// </summary>
			public Stack<Operand> IncomingStack;

			/// <summary>
			/// Initializes a new instance of the <see cref="WorkItem"/> class.
			/// </summary>
			/// <param name="block">The block.</param>
			/// <param name="incomingStack">The incoming stack.</param>
			public WorkItem(BasicBlock block, Stack<Operand> incomingStack)
			{
				Block = block;
				IncomingStack = incomingStack;
			}
		}

		/// <summary>
		///
		/// </summary>
		private Queue<WorkItem> workList = new Queue<WorkItem>();

		/// <summary>
		///
		/// </summary>
		private BitArray processed;

		/// <summary>
		///
		/// </summary>
		private BitArray enqueued;

		/// <summary>
		///
		/// </summary>
		private Stack<Operand>[] outgoingStack;

		/// <summary>
		///
		/// </summary>
		private Stack<Operand>[] scheduledMoves;

		protected override void Run()
		{
			if (MethodCompiler.Method.Code.Count == 0)
				return;

			foreach (BasicBlock headBlock in BasicBlocks.HeadBlocks)
				Trace(headBlock);
		}

		/// <summary>
		/// Traces the specified label.
		/// </summary>
		/// <param name="headBlock">The head block.</param>
		private void Trace(BasicBlock headBlock)
		{
			outgoingStack = new Stack<Operand>[BasicBlocks.Count];
			scheduledMoves = new Stack<Operand>[BasicBlocks.Count];
			processed = new BitArray(BasicBlocks.Count);
			processed.SetAll(false);
			enqueued = new BitArray(BasicBlocks.Count);
			enqueued.SetAll(false);

			processed.Set(headBlock.Sequence, true);
			workList.Enqueue(new WorkItem(headBlock, new Stack<Operand>()));

			while (workList.Count > 0)
			{
				AssignOperands(workList.Dequeue());
			}
		}

		/// <summary>
		/// Assigns the operands.
		/// </summary>
		/// <param name="workItem">The work item.</param>
		private void AssignOperands(WorkItem workItem)
		{
			var operandStack = workItem.IncomingStack;
			var block = workItem.Block;

			operandStack = CreateMovesForIncomingStack(block, operandStack);

			//System.Diagnostics.Debug.WriteLine("IN:    Block: " + block.Label + " Operand Stack Count: " + operandStack.Count);
			AssignOperands(block, operandStack);

			//System.Diagnostics.Debug.WriteLine("AFTER: Block: " + block.Label + " Operand Stack Count: " + operandStack.Count);
			operandStack = CreateScheduledMoves(block, operandStack);

			outgoingStack[block.Sequence] = operandStack;
			processed.Set(block.Sequence, true);

			foreach (var b in block.NextBlocks)
			{
				if (enqueued.Get(b.Sequence))
					continue;

				workList.Enqueue(new WorkItem(b, new Stack<Operand>(operandStack)));
				enqueued.Set(b.Sequence, true);
			}
		}

		/// <summary>
		/// Creates the scheduled moves.
		/// </summary>
		/// <param name="block">The block.</param>
		/// <param name="operandStack">The operand stack.</param>
		/// <returns></returns>
		private Stack<Operand> CreateScheduledMoves(BasicBlock block, Stack<Operand> operandStack)
		{
			if (scheduledMoves[block.Sequence] != null)
			{
				CreateOutgoingMoves(block, new Stack<Operand>(operandStack), new Stack<Operand>(scheduledMoves[block.Sequence]));
				operandStack = new Stack<Operand>(scheduledMoves[block.Sequence]);
				scheduledMoves[block.Sequence] = null;
			}
			return operandStack;
		}

		/// <summary>
		/// Assigns the operands.
		/// </summary>
		/// <param name="block">The block.</param>
		/// <param name="operandStack">The operand stack.</param>
		private void AssignOperands(BasicBlock block, Stack<Operand> operandStack)
		{
			for (var ctx = new Context(block); !ctx.IsBlockEndInstruction; ctx.GotoNext())
			{
				if (ctx.IsEmpty)
					continue;

				if (ctx.IsBlockEndInstruction || ctx.IsBlockStartInstruction)
					continue;

				if (ctx.Instruction == IRInstruction.Jmp)
					continue;

				if (!(ctx.Instruction.FlowControl == FlowControl.ConditionalBranch || ctx.Instruction.FlowControl == FlowControl.UnconditionalBranch || ctx.Instruction.FlowControl == FlowControl.Return)
					&& !(ctx.Instruction is BaseCILInstruction)
					&& ctx.Instruction != IRInstruction.ExceptionStart)
					continue;

				if (ctx.Instruction == IRInstruction.ExceptionStart)
				{
					AssignOperandsFromCILStack(ctx, operandStack);
					PushResultOperands(ctx, operandStack);
				}
				else
				{
					AssignOperandsFromCILStack(ctx, operandStack);
					(ctx.Instruction as BaseCILInstruction).Resolve(ctx, MethodCompiler);
					PushResultOperands(ctx, operandStack);
				}
			}
		}

		/// <summary>
		/// Creates the moves for incoming stack.
		/// </summary>
		/// <param name="operandStack">The operand stack.</param>
		/// <returns></returns>
		private Stack<Operand> CreateMovesForIncomingStack(BasicBlock block, Stack<Operand> operandStack)
		{
			var joinStack = new Stack<Operand>();

			foreach (var operand in operandStack)
			{
				joinStack.Push(MethodCompiler.CreateVirtualRegister(operand.Type));
			}

			foreach (var b in block.PreviousBlocks)
			{
				if (processed.Get(b.Sequence) && joinStack.Count > 0)
				{
					CreateOutgoingMoves(b, new Stack<Operand>(outgoingStack[b.Sequence]), new Stack<Operand>(joinStack));
					outgoingStack[b.Sequence] = new Stack<Operand>(joinStack);
				}
				else if (joinStack.Count > 0)
				{
					scheduledMoves[b.Sequence] = new Stack<Operand>(joinStack);
				}
			}
			return joinStack;
		}

		/// <summary>
		/// Creates the outgoing moves.
		/// </summary>
		/// <param name="block">The block.</param>
		/// <param name="operandStack">The operand stack.</param>
		/// <param name="joinStack">The join stack.</param>
		private void CreateOutgoingMoves(BasicBlock block, Stack<Operand> operandStack, Stack<Operand> joinStack)
		{
			var context = new Context(block.Last);

			context.GotoPrevious();

			while ((context.Instruction.FlowControl == FlowControl.ConditionalBranch || context.Instruction.FlowControl == FlowControl.UnconditionalBranch || context.Instruction.FlowControl == FlowControl.Return) || context.Instruction == IRInstruction.Jmp)
			{
				context.GotoPrevious();
			}

			while (operandStack.Count > 0)
			{
				var operand = operandStack.Pop();
				var destination = joinStack.Pop();
				context.AppendInstruction(IRInstruction.Move, destination, operand);
			}
		}

		/// <summary>
		/// Assigns the operands from CIL stack.
		/// </summary>
		/// <param name="ctx">The context.</param>
		/// <param name="currentStack">The current stack.</param>
		private void AssignOperandsFromCILStack(Context ctx, Stack<Operand> currentStack)
		{
			for (int index = ctx.OperandCount - 1; index >= 0; --index)
			{
				if (ctx.GetOperand(index) != null)
					continue;

				ctx.SetOperand(index, currentStack.Pop());
			}
		}

		/// <summary>
		/// Pushes the result operands on to the stack
		/// </summary>
		/// <param name="ctx">The context.</param>
		/// <param name="currentStack">The current stack.</param>
		private static void PushResultOperands(Context ctx, Stack<Operand> currentStack)
		{
			if (ctx.Instruction != IRInstruction.ExceptionStart)
				if (!(ctx.Instruction as BaseCILInstruction).PushResult)
					return;

			if (ctx.ResultCount == 0)
				return;

			currentStack.Push(ctx.Result);

			if (ctx.Instruction is CIL.DupInstruction)
				currentStack.Push(ctx.Result);
		}
	}
}