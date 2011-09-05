﻿/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Michael Fröhlich (grover) <sharpos@michaelruck.de>
 */

using Mosa.Compiler.Linker;
using Mosa.Runtime.CompilerFramework;
using Mosa.Runtime.InternalTrace;
using Mosa.Runtime.Options;
using Mosa.Runtime.TypeSystem;
using Mosa.Tools.Compiler.Linker;
using Mosa.Tools.Compiler.TypeInitializers;

namespace Mosa.Tools.Compiler
{
	public class AotAssemblyCompiler : AssemblyCompiler
	{
		public AotAssemblyCompiler(IArchitecture architecture, ITypeInitializerSchedulerStage typeInitializerSchedulerStage, IAssemblyLinker linker, ITypeSystem typeSystem, ITypeLayout typeLayout, IInternalTrace internalLog, CompilerOptionSet compilerOptionSet)
			: base(architecture, typeSystem, typeLayout, internalLog, compilerOptionSet)
		{
			Pipeline.AddRange(new IAssemblyCompilerStage[] 
			{
				new AssemblyMemberCompilationSchedulerStage(),
				new MethodCompilerSchedulerStage(),
				new TypeInitializerSchedulerStageProxy(typeInitializerSchedulerStage),
				new TypeLayoutStage(),
				new MethodTableBuilderStage(),
				new LinkerProxy(linker),
			});
		}

		public void Run()
		{
			// Build the default assembly compiler pipeline
			Architecture.ExtendAssemblyCompilerPipeline(this.Pipeline);

			// Run the compiler
			Compile();
		}

		/// <summary>
		/// Creates the method compiler.
		/// </summary>
		/// <param name="compilationScheduler">The compilation scheduler.</param>
		/// <param name="type">The type.</param>
		/// <param name="method">The method.</param>
		/// <returns></returns>
		public override IMethodCompiler CreateMethodCompiler(ICompilationSchedulerStage compilationScheduler, RuntimeType type, RuntimeMethod method)
		{
			IMethodCompiler mc = new AotMethodCompiler(this, compilationScheduler, type, method, internalTrace);
			this.Architecture.ExtendMethodCompilerPipeline(mc.Pipeline);
			return mc;
		}
	}
}
