﻿/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Michael Ruck (<mailto:sharpos@michaelruck.de>)
 *  Simon Wollwage (<mailto:kintaro@think-in-co.de>)
 *  Scott Balmos (<mailto:sbalmos@fastmail.fm>)
 */

using System;
using System.Collections.Generic;
using System.Text;
using Mosa.Platforms.x86.Instructions;
using Mosa.Platforms.x86.Instructions.Intrinsics;
using Mosa.Runtime.CompilerFramework.IR;

namespace Mosa.Platforms.x86
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="ArgType">The type of the rg type.</typeparam>
    interface IX86InstructionVisitor<ArgType> : IIRVisitor<ArgType>
    {
        void Add(AddInstruction addInstruction, ArgType arg);
        void Adc(AdcInstruction adcInstruction, ArgType arg);
        
        void And(Instructions.LogicalAndInstruction andInstruction, ArgType arg);
        void Or(Instructions.LogicalOrInstruction orInstruction, ArgType arg);
        void Xor(Instructions.LogicalXorInstruction xorInstruction, ArgType arg);

        void Sub(SubInstruction subInstruction, ArgType arg);
        void Sbb(SbbInstruction sbbInstruction, ArgType arg);
        void Mul(MulInstruction mulInstruction, ArgType arg);
        void DirectMultiplication(DirectMultiplicationInstruction mulInstruction, ArgType arg);
        void DirectDivision(DirectDivisionInstruction divInstruction, ArgType arg);
        void Div(DivInstruction divInstruction, ArgType arg);
        void UDiv(Instructions.UDivInstruction divInstruction, ArgType arg);
        void SseAdd(SseAddInstruction addInstruction, ArgType arg);
        void SseSub(SseSubInstruction subInstruction, ArgType arg);
        void SseMul(SseMulInstruction mulInstruction, ArgType arg);
        void SseDiv(SseDivInstruction mulInstruction, ArgType arg);
        void Sar(SarInstruction shiftInstruction, ArgType arg);
        void Sal(SalInstruction shiftInstruction, ArgType arg);
        void Shl(ShlInstruction shiftInstruction, ArgType arg);
        void Shr(ShrInstruction shiftInstruction, ArgType arg);
        void Rcr(RcrInstruction rotateInstruction, ArgType arg);

        void Cvtsi2ss(Cvtsi2ssInstruction instruction, ArgType arg);
        void Cvtsi2sd(Cvtsi2sdInstruction instruction, ArgType arg);
        void Cvtsd2ss(Cvtsd2ssInstruction instruction, ArgType arg);
        void Cvtss2sd(Cvtss2sdInstruction instruction, ArgType arg);

        void Cmp(CmpInstruction instruction, ArgType arg);
        void Setcc(SetccInstruction instruction, ArgType arg);
        void Cdq(CdqInstruction instruction, ArgType arg);

        void Shld(ShldInstruction instruction, ArgType arg);
        void Shrd(ShrdInstruction instruction, ArgType arg);

        void Comisd(ComisdInstruction instruction, ArgType arg);
        void Comiss(ComissInstruction instruction, ArgType arg);
        void Ucomisd(UcomisdInstruction instruction, ArgType arg);
        void Ucomiss(UcomissInstruction instruction, ArgType arg);

        void Jns(JnsBranchInstruction instruction, ArgType arg);

        #region Intrinsics
        /// <summary>
        /// Emits xchg bx, bx to force bochs into debug mode
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void BochsDebug(BochsDebug instruction, ArgType arg);
        /// <summary>
        /// Disable interrupts
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void Cli(CliInstruction instruction, ArgType arg);
        /// <summary>
        /// Clear Direction Flag
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void Cld(CldInstruction instruction, ArgType arg);
        /// <summary>
        /// Compare and exchange register - memory
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void CmpXchg(CmpXchgInstruction instruction, ArgType arg);
        /// <summary>
        /// Retrieves the CPU ID
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void CpuId(CpuIdInstruction instruction, ArgType arg);
        void CpuIdEax(CpuIdEaxInstruction instruction, ArgType arg);
        void CpuIdEbx(CpuIdEbxInstruction instruction, ArgType arg);
        void CpuIdEcx(CpuIdEcxInstruction instruction, ArgType arg);
        void CpuIdEdx(CpuIdEdxInstruction instruction, ArgType arg);
        /// <summary>
        /// Halts the machine
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void Hlt(HltInstruction instruction, ArgType arg);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="arg"></param>
        void Invlpg(InvlpgInstruction instruction, ArgType arg);
        /// <summary>
        /// Read in from port
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void In(InInstruction instruction, ArgType arg);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="arg"></param>
        void Inc(IncInstruction instruction, ArgType arg);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="arg"></param>
        void Dec(DecInstruction instruction, ArgType arg);
        /// <summary>
        /// Call interrupt
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void Int(IntInstruction instruction, ArgType arg);
        /// <summary>
        /// Return from interrupt
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void Iretd(IretdInstruction instruction, ArgType arg);
        /// <summary>
        /// Load global descriptor table
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void Lgdt(LgdtInstruction instruction, ArgType arg);
        /// <summary>
        /// Load interrupt descriptor table
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void Lidt(LidtInstruction instruction, ArgType arg);
        /// <summary>
        /// Locks
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void Lock(LockIntruction instruction, ArgType arg);
        /// <summary>
        /// Negate with Two-Complement
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void Neg(NegInstruction instruction, ArgType arg);
		/// <summary>
		/// No operation
		/// </summary>
		/// <param name="instruction">The instruction.</param>
		/// <param name="arg">The arguments</param>
		void Nop(NopInstruction instruction, ArgType arg);
		/// <summary>
        /// Output to port
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void Out(OutInstruction instruction, ArgType arg);
        /// <summary>
        /// Pause
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void Pause(PauseInstruction instruction, ArgType arg);
        /// <summary>
        /// Pop from the stack
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void Pop(Instructions.Intrinsics.PopInstruction instruction, ArgType arg);
        /// <summary>
        /// Pops All General-Purpose Registers
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void Popad(PopadInstruction instruction, ArgType arg);
        /// <summary>
        /// Pop Stack into EFLAGS Register
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void Popfd(PopfdInstruction instruction, ArgType arg);
        /// <summary>
        /// Push on the stack
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void Push(Instructions.Intrinsics.PushInstruction instruction, ArgType arg);
        /// <summary>
        /// Push All General-Purpose Registers
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void Pushad(PushadInstruction instruction, ArgType arg);
        /// <summary>
        /// Push EFLAGS Register onto the Stack
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void Pushfd(PushfdInstruction instruction, ArgType arg);
        /// <summary>
        /// Read time stamp counter
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void Rdmsr(RdmsrInstruction instruction, ArgType arg);
        /// <summary>
        /// Read from model specific register
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void Rdpmc(RdpmcInstruction instruction, ArgType arg);
        /// <summary>
        /// Read time stamp counter
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void Rdtsc(RdtscInstruction instruction, ArgType arg);
        /// <summary>
        /// Repeat String Operation Prefix
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void Rep(RepInstruction instruction, ArgType arg);
        /// <summary>
        /// Enable interrupts
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void Sti(StiInstruction instruction, ArgType arg);
        /// <summary>
        /// Store String
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void Stosb(StosbInstruction instruction, ArgType arg);
        /// <summary>
        /// Store String
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void Stosd(StosdInstruction instruction, ArgType arg);
        /// <summary>
        /// Exchanges register/memory
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arg">The arguments</param>
        void Xchg(XchgInstruction instruction, ArgType arg);
        #endregion
    }
}