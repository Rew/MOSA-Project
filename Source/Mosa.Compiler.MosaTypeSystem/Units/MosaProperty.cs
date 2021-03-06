﻿/*
 * (c) 2014 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Stefan Andres Charsley (charsleysa) <charsleysa@gmail.com>
 */

using System;

namespace Mosa.Compiler.MosaTypeSystem
{
	public class MosaProperty : MosaUnit, IEquatable<MosaProperty>
	{
		public MosaPropertyAttributes PropertyAttributes { get; private set; }

		public MosaType DeclaringType { get; private set; }

		public MosaType PropertyType { get; private set; }

		public string GetterMethodName { get; private set; }

		public MosaMethod GetterMethod
		{
			get
			{
				return this.DeclaringType.FindMethodByName(this.GetterMethodName);
			}
		}

		public string SetterMethodName { get; private set; }

		public MosaMethod SetterMethod
		{
			get
			{
				return this.DeclaringType.FindMethodByName(this.SetterMethodName);
			}
		}

		internal MosaProperty()
		{
		}

		internal MosaProperty Clone()
		{
			return (MosaProperty)base.MemberwiseClone();
		}

		public bool Equals(MosaProperty other)
		{
			return SignatureComparer.Equals(this.PropertyType, other.PropertyType);
		}

		public class Mutator : MosaUnit.MutatorBase
		{
			private MosaProperty property;

			internal Mutator(MosaProperty property)
				: base(property)
			{
				this.property = property;
			}

			public MosaPropertyAttributes PropertyAttributes { set { property.PropertyAttributes = value; } }

			public MosaType DeclaringType { set { property.DeclaringType = value; } }

			public MosaType PropertyType { set { property.PropertyType = value; } }

			private string GetCleanMethodName(string fullName)
			{
				if (!fullName.Contains("."))
					return fullName;
				return fullName.Substring(fullName.LastIndexOf(".") + 1);
			}

			private string GetUncleanMethodPrefix(string fullName)
			{
				if (!fullName.Contains("."))
					return fullName;
				return fullName.Substring(0, fullName.LastIndexOf("."));
			}

			public override void Dispose()
			{
				if (property.PropertyType != null)
				{
					property.FullName = string.Concat(property.PropertyType.FullName, " ", property.DeclaringType.FullName, "::", property.Name);
					property.ShortName = string.Concat(property.Name, " : ", property.PropertyType.ShortName);

					if (GetCleanMethodName(property.Name) != GetUncleanMethodPrefix(property.Name))
						property.GetterMethodName = GetUncleanMethodPrefix(property.Name) + ".get_" + GetCleanMethodName(property.Name);
					else
						property.GetterMethodName = "get_" + property.Name;

					if (GetCleanMethodName(property.Name) != GetUncleanMethodPrefix(property.Name))
						property.SetterMethodName = GetUncleanMethodPrefix(property.Name) + ".set_" + GetCleanMethodName(property.Name);
					else
						property.SetterMethodName = "set_" + property.Name;
				}
			}
		}
	}
}