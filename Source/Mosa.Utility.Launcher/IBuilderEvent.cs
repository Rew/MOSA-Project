﻿/*
 * (c) 2014 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Phil Garcia (tgiphil) <phil@thinkedge.com>
 */

namespace Mosa.Utility.Launcher
{
	public interface IBuilderEvent
	{
		void NewStatus(string status);

		void UpdateProgress(int total, int at);
	}
}