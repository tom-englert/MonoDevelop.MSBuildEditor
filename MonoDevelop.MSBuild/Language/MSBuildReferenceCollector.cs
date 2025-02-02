// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

using Microsoft.Extensions.Logging;

using MonoDevelop.MSBuild.Language.Expressions;
using MonoDevelop.MSBuild.Language.Syntax;
using MonoDevelop.MSBuild.Language.Typesystem;
using MonoDevelop.Xml.Dom;
using MonoDevelop.Xml.Parser;

namespace MonoDevelop.MSBuild.Language
{
	[Flags]
	public enum ReferenceUsage
	{
		Unknown = 0,
		Declaration = 1,
		Read = 1 << 1,
		Write = 1 << 2,
	}

	abstract class MSBuildReferenceCollector : MSBuildResolvingVisitor
	{
		readonly Action<(int Offset, int Length, ReferenceUsage Usage)> reportResult;

		protected MSBuildReferenceCollector (MSBuildDocument document, ITextSource textSource, ILogger logger, string name, Action<(int Offset, int Length, ReferenceUsage Usage)> reportResult)
			: base (document, textSource, logger)
		{
			if (string.IsNullOrEmpty (name)) {
				throw new ArgumentException ("Name cannot be null or empty", name);
			}
			Name = name;
			this.reportResult = reportResult;
		}

		public string Name { get; }

		protected bool IsMatch (string name) => string.Equals (name, Name, StringComparison.OrdinalIgnoreCase);
		protected bool IsMatch (INamedXObject obj) => IsMatch (obj.Name.Name);

		protected bool IsPureMatchIgnoringWhitespace (ExpressionText t, out int offset, out int length)
		{
			offset = 0;
			length = 0;
			if (!t.IsPure) {
				return false;
			}

			//FIXME do this more efficiently than trimming
			var trimmed = t.Value.Trim ();
			if (IsMatch (trimmed)) {
				offset = t.Offset + (t.Value.Length - t.Value.TrimStart ().Length);
				length = trimmed.Length;
				return true;
			}

			return false;
		}

		protected void AddResult (XElement el, ReferenceUsage usage) => reportResult ((el.Span.Start + 1, el.Name.Name.Length, usage));
		protected void AddResult (XAttribute att, ReferenceUsage usage) => reportResult ((att.Span.Start, att.Name.Name.Length, usage));
		protected void AddResult (int offset, int length, ReferenceUsage usage) => reportResult ((offset, length, usage));

		public static bool CanCreate (MSBuildResolveResult rr)
		{
			if (rr == null || rr.ElementSyntax == null) {
				return false;
			}

			switch (rr.ReferenceKind) {
			case MSBuildReferenceKind.Property:
			case MSBuildReferenceKind.Item:
			case MSBuildReferenceKind.Task:
			case MSBuildReferenceKind.Metadata:
			case MSBuildReferenceKind.Target:
			case MSBuildReferenceKind.ItemFunction:
			case MSBuildReferenceKind.PropertyFunction:
			case MSBuildReferenceKind.StaticPropertyFunction:
			case MSBuildReferenceKind.ClassName:
			case MSBuildReferenceKind.Enum:
				return rr.Reference != null;
			}

			return false;
		}

		public static MSBuildReferenceCollector Create (MSBuildDocument document, ITextSource textSource, ILogger logger, MSBuildResolveResult rr, IFunctionTypeProvider functionTypeProvider, Action<(int Offset, int Length, ReferenceUsage Usage)> reportResult)
		{
			switch (rr.ReferenceKind) {
			case MSBuildReferenceKind.Property:
				return new MSBuildPropertyReferenceCollector (document, textSource, logger, (string)rr.Reference, reportResult);
			case MSBuildReferenceKind.Item:
				return new MSBuildItemReferenceCollector (document, textSource, logger, (string)rr.Reference, reportResult);
			case MSBuildReferenceKind.Metadata:
				var m = rr.ReferenceAsMetadata;
				return new MSBuildMetadataReferenceCollector (document, textSource, logger, m.itemName, m.metaName, reportResult);
			case MSBuildReferenceKind.Task:
				return new MSBuildTaskReferenceCollector (document, textSource, logger, (string)rr.Reference, reportResult);
			case MSBuildReferenceKind.Target:
				return new MSBuildTargetReferenceCollector (document, textSource, logger, (string)rr.Reference, reportResult);
			case MSBuildReferenceKind.ItemFunction:
				return new MSBuildItemFunctionReferenceCollector (document, textSource, logger, (string)rr.Reference, reportResult);
			case MSBuildReferenceKind.StaticPropertyFunction:
				(string className, string name) = ((string, string))rr.Reference;
				return new MSBuildStaticPropertyFunctionReferenceCollector (document, textSource, logger, className, name, reportResult);
			case MSBuildReferenceKind.PropertyFunction:
				(MSBuildValueKind valueKind, string funcName) = ((MSBuildValueKind, string))rr.Reference;
				return new MSBuildPropertyFunctionReferenceCollector (document, textSource, logger, valueKind, funcName, functionTypeProvider, reportResult);
			case MSBuildReferenceKind.ClassName:
				return new MSBuildClassReferenceCollector (document, textSource, logger, (string)rr.Reference, reportResult);
			case MSBuildReferenceKind.Enum:
				return new MSBuildEnumReferenceCollector (document, textSource, logger, (string)rr.Reference, reportResult);
			}

			throw new ArgumentException ($"Cannot create collector for resolve result", nameof (rr));
		}
	}

	class MSBuildItemReferenceCollector : MSBuildReferenceCollector
	{
		public MSBuildItemReferenceCollector (MSBuildDocument document, ITextSource textSource, ILogger logger, string itemName, Action<(int Offset, int Length, ReferenceUsage Usage)> reportResult)
			: base (document, textSource, logger, itemName, reportResult) { }

		protected override void VisitResolvedElement (XElement element, MSBuildElementSyntax resolved)
		{
			if ((resolved.SyntaxKind == MSBuildSyntaxKind.Item || resolved.SyntaxKind == MSBuildSyntaxKind.ItemDefinition) && IsMatch (element.Name.Name)) {
				AddResult (element.NameOffset, element.Name.Name.Length, ReferenceUsage.Write);
			}
			base.VisitResolvedElement (element, resolved);
		}

		protected override void VisitResolvedAttribute (XElement element, XAttribute attribute, MSBuildElementSyntax resolvedElement, MSBuildAttributeSyntax resolvedAttribute)
		{
			if (resolvedAttribute.ValueKind == MSBuildValueKind.ItemName.AsLiteral () && IsMatch (attribute.Value)) {
				AddResult (attribute.Span.Start, attribute.Name.Name.Length, ReferenceUsage.Write);
			}
			base.VisitResolvedAttribute (element, attribute, resolvedElement, resolvedAttribute);
		}

		protected override void VisitValueExpression (
			XElement element, XAttribute attribute,
			MSBuildElementSyntax resolvedElement, MSBuildAttributeSyntax resolvedAttribute,
			ITypedSymbol valueType, MSBuildValueKind kind, ExpressionNode node)
		{
			foreach (var n in node.WithAllDescendants ()) {
				switch (n) {
				case ExpressionItemName ei:
					if (IsMatch (ei.Name)) {
						AddResult (ei.Offset, ei.Name.Length, ReferenceUsage.Read);
					}
					break;
				case ExpressionMetadata em:
					if (em.IsQualified && IsMatch (em.ItemName)) {
						AddResult (em.ItemNameOffset, em.ItemName.Length, ReferenceUsage.Read);
					}
					break;
				}
			}
		}
	}

	class MSBuildPropertyReferenceCollector : MSBuildReferenceCollector
	{
		public MSBuildPropertyReferenceCollector (MSBuildDocument document, ITextSource textSource, ILogger logger, string propertyName, Action<(int Offset, int Length, ReferenceUsage Usage)> reportResult)
			: base (document, textSource, logger, propertyName, reportResult) { }


		protected override void VisitResolvedElement (XElement element, MSBuildElementSyntax resolved)
		{
			if ((resolved.SyntaxKind == MSBuildSyntaxKind.Property) && IsMatch (element.Name.Name)) {
				AddResult (element.NameOffset, element.Name.Name.Length, ReferenceUsage.Write);
			}
			base.VisitResolvedElement (element, resolved);
		}

		protected override void VisitResolvedAttribute (XElement element, XAttribute attribute, MSBuildElementSyntax resolvedElement, MSBuildAttributeSyntax resolvedAttribute)
		{
			if (resolvedAttribute.ValueKind == MSBuildValueKind.PropertyName.AsLiteral () && IsMatch (attribute.Value)) {
				AddResult (attribute.Span.Start, attribute.Name.Name.Length, ReferenceUsage.Write);
			}
			base.VisitResolvedAttribute (element, attribute, resolvedElement, resolvedAttribute);
		}

		protected override void VisitValueExpression (
			XElement element, XAttribute attribute,
			MSBuildElementSyntax resolvedElement, MSBuildAttributeSyntax resolvedAttribute,
			ITypedSymbol valueType, MSBuildValueKind kind, ExpressionNode node)
		{
			foreach (var n in node.WithAllDescendants ()) {
				switch (n) {
				case ExpressionPropertyName ep:
					if (IsMatch (ep.Name)) {
						AddResult (ep.Offset, ep.Length, ReferenceUsage.Read);
					}
					break;
				}
			}
		}
	}

	class MSBuildTaskReferenceCollector : MSBuildReferenceCollector
	{
		public MSBuildTaskReferenceCollector (MSBuildDocument document, ITextSource textSource, ILogger logger, string taskName, Action<(int Offset, int Length, ReferenceUsage Usage)> reportResult)
			: base (document, textSource, logger, taskName, reportResult) { }

		protected override void VisitResolvedElement (XElement element, MSBuildElementSyntax resolved)
		{
			switch (resolved.SyntaxKind) {
			case MSBuildSyntaxKind.Task:
				if (IsMatch (element.Name.Name)) {
					AddResult (element.NameOffset, element.Name.Name.Length, ReferenceUsage.Read);
				}
				break;
			case MSBuildSyntaxKind.UsingTask:
				var nameAtt = element.Attributes.Get ("TaskName", true);
				if (nameAtt != null && !string.IsNullOrEmpty (nameAtt.Value)) {
					var nameIdx = nameAtt.Value.LastIndexOf ('.') + 1;
					string name = nameIdx > 0 ? nameAtt.Value.Substring (nameIdx) : nameAtt.Value;
					if (IsMatch (name)) {
						AddResult (nameAtt.ValueOffset + nameIdx, name.Length, ReferenceUsage.Declaration);
					}
				}
				break;
			}
			base.VisitResolvedElement (element, resolved);
		}
	}

	class MSBuildMetadataReferenceCollector : MSBuildReferenceCollector
	{
		readonly string itemName;

		public MSBuildMetadataReferenceCollector (MSBuildDocument document, ITextSource textSource, ILogger logger, string itemName, string metadataName, Action<(int Offset, int Length, ReferenceUsage Usage)> reportResult)
			: base (document, textSource, logger, metadataName, reportResult)
		{
			this.itemName = itemName;
		}

		protected override void VisitResolvedElement (XElement element, MSBuildElementSyntax resolved)
		{
			if (resolved.SyntaxKind == MSBuildSyntaxKind.Metadata && IsMatch (element.Name.Name) && IsItemNameMatch (element.ParentElement.Name.Name)) {
				AddResult (element.NameOffset, element.Name.Name.Length, ReferenceUsage.Write);
			}
			base.VisitResolvedElement (element, resolved);
		}

		protected override void VisitResolvedAttribute (XElement element, XAttribute attribute, MSBuildElementSyntax resolvedElement, MSBuildAttributeSyntax resolvedAttribute)
		{
			if (resolvedAttribute.AbstractKind == MSBuildSyntaxKind.Metadata && IsMatch (attribute.Name.Name) && IsItemNameMatch (element.Name.Name)) {
				AddResult (attribute.Span.Start, attribute.Name.Name.Length, ReferenceUsage.Write);
			}
			base.VisitResolvedAttribute (element, attribute, resolvedElement, resolvedAttribute);
		}

		// null doc means offsets will not be correct
		internal static ExpressionNode GetIncludeExpression (XElement itemElement)
		{
			var include = itemElement.Attributes
				.FirstOrDefault (e => string.Equals (e.Name.Name, "Include", StringComparison.OrdinalIgnoreCase));

			if (include == null || string.IsNullOrWhiteSpace (include.Value)) {
				return null;
			}
			return ExpressionParser.Parse (
				include.Value,
				ExpressionOptions.ItemsMetadataAndLists,
				include.Span.Start);
		}

		protected override void VisitValueExpression (
			XElement element, XAttribute attribute,
			MSBuildElementSyntax resolvedElement, MSBuildAttributeSyntax resolvedAttribute,
			ITypedSymbol valueType, MSBuildValueKind kind, ExpressionNode node)
		{
			//these are things like <Foo Include="@(Bar)" RemoveMetadata="SomeBarMetadata" />
			if (kind.IsKindOrListOfKind (MSBuildValueKind.MetadataName)) {
				var expr = GetIncludeExpression (element);
				if (expr != null && expr
					.WithAllDescendants ()
					.OfType<ExpressionItemName> ()
					.Any (n => IsItemNameMatch (n.ItemName))
				) {
					switch (node) {
					case ListExpression list:
						foreach (var c in list.Nodes) {
							if (c is ExpressionText l) {
								CheckMatch (l);
								break;
							}
						}
						break;
					case ExpressionText lit:
						CheckMatch (lit);
						break;
					}
				}

				void CheckMatch (ExpressionText t)
				{
					if (IsPureMatchIgnoringWhitespace (t, out int offset, out int length)) {
						AddResult (offset, length, ReferenceUsage.Read);
					}
				}
				return;
			}

			foreach (var n in node.WithAllDescendants ()) {
				switch (n) {
				case ExpressionMetadata em:
					var iname = em.GetItemName ();
					if (iname != null && IsItemNameMatch (iname) && IsMatch (em.MetadataName)) {
						AddResult (em.MetadataNameOffset, em.MetadataName.Length, ReferenceUsage.Read);
					}
					break;
				}
			}
		}

		bool IsItemNameMatch (string name) => string.Equals (name, itemName, StringComparison.OrdinalIgnoreCase);
	}

	class MSBuildTargetReferenceCollector : MSBuildReferenceCollector
	{
		public MSBuildTargetReferenceCollector (MSBuildDocument document, ITextSource textSource, ILogger logger, string targetName, Action<(int Offset, int Length, ReferenceUsage Usage)> reportResult)
			: base (document, textSource, logger, targetName, reportResult) { }

		protected override void VisitValueExpression (
			XElement element, XAttribute attribute,
			MSBuildElementSyntax resolvedElement, MSBuildAttributeSyntax resolvedAttribute,
			ITypedSymbol valueType, MSBuildValueKind kind, ExpressionNode node)
		{
			if (!kind.IsKindOrListOfKind (MSBuildValueKind.TargetName)) {
				return;
			}
			bool isDeclaration = resolvedAttribute?.SyntaxKind == MSBuildSyntaxKind.Target_Name;

			switch (node) {
			case ListExpression list:
				foreach (var c in list.Nodes) {
					if (c is ExpressionText l) {
						CheckMatch (l, isDeclaration);
					}
				}
				break;
			case ExpressionText lit:
				CheckMatch (lit, isDeclaration);
				break;
			}
		}

		void CheckMatch (ExpressionText node, bool isDeclaration)
		{
			if (IsPureMatchIgnoringWhitespace (node, out int offset, out int length)) {
				AddResult (offset, length, isDeclaration ? ReferenceUsage.Declaration : ReferenceUsage.Read);
			}
		}
	}

	class MSBuildTargetDefinitionCollector : MSBuildReferenceCollector
	{
		public MSBuildTargetDefinitionCollector (MSBuildDocument document, ITextSource textSource, ILogger logger, string targetName, Action<(int Offset, int Length, ReferenceUsage Usage)> reportResult)
			: base (document, textSource, logger, targetName, reportResult) { }

		protected override void VisitResolvedElement (XElement element, MSBuildElementSyntax resolved)
		{
			if (resolved.SyntaxKind == MSBuildSyntaxKind.Target) {
				var nameAtt = element.Attributes.Get ("Name", true);
				if (nameAtt != null && IsMatch (nameAtt.Value)) {
					AddResult (nameAtt.ValueSpan.Start, nameAtt.ValueSpan.Length, ReferenceUsage.Declaration);
				}
			}
			base.VisitResolvedElement (element, resolved);
		}
	}

	class MSBuildStaticPropertyFunctionReferenceCollector : MSBuildReferenceCollector
	{
		readonly string className;

		public MSBuildStaticPropertyFunctionReferenceCollector (MSBuildDocument document, ITextSource textSource, ILogger logger, string className, string functionName, Action<(int Offset, int Length, ReferenceUsage Usage)> reportResult)
			: base (document, textSource, logger, MSBuildPropertyFunctionReferenceCollector.StripGetPrefix (functionName), reportResult)
		{
			this.className = className;
		}

		protected override void VisitValueExpression (
			XElement element, XAttribute attribute,
			MSBuildElementSyntax resolvedElement, MSBuildAttributeSyntax resolvedAttribute,
			ITypedSymbol valueType, MSBuildValueKind kind, ExpressionNode node)
		{
			foreach (var n in node.WithAllDescendants ()) {
				switch (n) {
				case ExpressionFunctionName func:
					if (func.Parent is ExpressionPropertyFunctionInvocation inv) {
						string baseName = MSBuildPropertyFunctionReferenceCollector.StripGetPrefix (func.Name);
						if (IsMatch (baseName) && inv.Target is ExpressionClassReference cn && cn.Name == className) {
							AddResult (func.Offset, func.Length, ReferenceUsage.Read);
						}
					}
					break;
				}
			}
		}
	}

	class MSBuildPropertyFunctionReferenceCollector : MSBuildReferenceCollector
	{
		//ensure that props and function-style props are equivalent e.g get_Now() and Now
		//this is kinda hacky, we should really check whether the get variant is a
		//method and the non-get variant is a property
		internal static string StripGetPrefix (string name)
		{
			if (name.StartsWith ("get_", StringComparison.Ordinal) && name.Length > 4) {
				return name.Substring (4);
			}
			return name;
		}

		readonly MSBuildValueKind valueKind;
		readonly IFunctionTypeProvider functionTypeProvider;

		public MSBuildPropertyFunctionReferenceCollector (
			MSBuildDocument document, ITextSource textSource, ILogger logger,
			MSBuildValueKind valueKind, string functionName, IFunctionTypeProvider functionTypeProvider,
			Action<(int Offset, int Length, ReferenceUsage Usage)> reportResult)
			: base (document, textSource, logger, StripGetPrefix(functionName), reportResult)
		{
			if (valueKind == MSBuildValueKind.Unknown) {
				valueKind = MSBuildValueKind.String;
			}
			this.valueKind = valueKind;
			this.functionTypeProvider = functionTypeProvider;
		}

		protected override void VisitValueExpression (
			XElement element, XAttribute attribute,
			MSBuildElementSyntax resolvedElement, MSBuildAttributeSyntax resolvedAttribute,
			ITypedSymbol valueType, MSBuildValueKind kind, ExpressionNode node)
		{
			foreach (var n in node.WithAllDescendants ()) {
				switch (n) {
				case ExpressionFunctionName func:
					if (func.Parent is ExpressionPropertyFunctionInvocation inv) {
						string baseName = StripGetPrefix (func.Name);
						if (IsMatch (baseName)) {
							//TODO: should we be fuzzy here and accept "unknown"?
							var resolvedKind = functionTypeProvider.ResolveType (inv);
							if (resolvedKind == MSBuildValueKind.Unknown) {
								resolvedKind = MSBuildValueKind.String;
							}
							if (resolvedKind == valueKind) {
								AddResult (func.Offset, func.Length, ReferenceUsage.Read);
							}
						}
					}
					break;
				}
			}
		}
	}

	class MSBuildItemFunctionReferenceCollector : MSBuildReferenceCollector
	{
		public MSBuildItemFunctionReferenceCollector (MSBuildDocument document, ITextSource textSource, ILogger logger, string functionName, Action<(int Offset, int Length, ReferenceUsage Usage)> reportResult)
			: base (document, textSource, logger, functionName, reportResult) { }

		protected override void VisitValueExpression (
			XElement element, XAttribute attribute,
			MSBuildElementSyntax resolvedElement, MSBuildAttributeSyntax resolvedAttribute,
			ITypedSymbol valueType, MSBuildValueKind kind, ExpressionNode node)
		{
			foreach (var n in node.WithAllDescendants ()) {
				switch (n) {
				case ExpressionFunctionName func:
					if (func.Parent is ExpressionItemFunctionInvocation inv && IsMatch (func.Name)) {
						AddResult (func.Offset, func.Length, ReferenceUsage.Read);
					}
					break;
				}
			}
		}
	}

	class MSBuildClassReferenceCollector : MSBuildReferenceCollector
	{
		public MSBuildClassReferenceCollector (MSBuildDocument document, ITextSource textSource, ILogger logger, string className, Action<(int Offset, int Length, ReferenceUsage Usage)> reportResult)
			: base (document, textSource, logger, className, reportResult) { }

		protected override void VisitValueExpression (
			XElement element, XAttribute attribute,
			MSBuildElementSyntax resolvedElement, MSBuildAttributeSyntax resolvedAttribute,
			ITypedSymbol valueType, MSBuildValueKind kind, ExpressionNode node)
		{
			foreach (var n in node.WithAllDescendants ()) {
				switch (n) {
				case ExpressionClassReference classRef:
					if (IsMatch (classRef.Name) && classRef.Parent is ExpressionPropertyFunctionInvocation) {
						AddResult (classRef.Offset, classRef.Length, ReferenceUsage.Read);
					}
					break;
				}
			}
		}
	}

	class MSBuildEnumReferenceCollector : MSBuildReferenceCollector
	{
		public MSBuildEnumReferenceCollector (MSBuildDocument document, ITextSource textSource, ILogger logger, string enumName, Action<(int Offset, int Length, ReferenceUsage Usage)> reportResult)
			: base (document, textSource, logger, enumName, reportResult) { }

		protected override void VisitValueExpression (
			XElement element, XAttribute attribute,
			MSBuildElementSyntax resolvedElement, MSBuildAttributeSyntax resolvedAttribute,
			ITypedSymbol valueType, MSBuildValueKind kind, ExpressionNode node)
		{
			foreach (var n in node.WithAllDescendants ()) {
				switch (n) {
				case ExpressionClassReference classRef:
					if (IsMatch (classRef.Name) && classRef.Parent is ExpressionArgumentList) {
						AddResult (classRef.Offset, classRef.Length, ReferenceUsage.Read);
					}
					break;
				}
			}
		}
	}
}