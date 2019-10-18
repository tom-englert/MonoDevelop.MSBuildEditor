// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MonoDevelop.MSBuild.Language.Expressions
{
	abstract class ExpressionItemNode : ExpressionNode
	{
		protected ExpressionItemNode (int offset, int length) : base (offset, length)
		{
		}

		public abstract string ItemName { get; }
		public abstract int ItemNameOffset { get; }
	}

	class ExpressionItemName : ExpressionItemNode
	{
		public string Name { get; }

		public ExpressionItemName (int offset, string name) : base (offset, name.Length)
		{
			Name = name;
		}

		public override string ItemName => Name;
		public override int ItemNameOffset => Offset;

		public override ExpressionNodeKind NodeKind => ExpressionNodeKind.ItemName;
	}

	class ExpressionItemFunctionInvocation : ExpressionItemNode
	{
		public ExpressionNode Target { get; }
		public ExpressionFunctionName Function { get; }

		public override string ItemName => (Target as ExpressionItemNode)?.ItemName;
		public override int ItemNameOffset => (Target as ExpressionItemNode).ItemNameOffset;

		public ExpressionNode Arguments { get; }

		public ExpressionItemFunctionInvocation (int offset, int length, ExpressionNode target, ExpressionFunctionName methodName, ExpressionNode arguments)
			: base (offset, length)
		{
			Target = target;
			target.SetParent (this);
			Function = methodName;
			methodName?.SetParent (this);
			Arguments = arguments;
			arguments?.SetParent (this);
		}

		public override ExpressionNodeKind NodeKind => ExpressionNodeKind.ItemFunctionInvocation;
	}

	class ExpressionItemTransform : ExpressionItemNode
	{
		public ExpressionNode Target { get; }
		public ExpressionNode Transform { get; }
		public ExpressionNode Separator { get; }

		public override string ItemName => (Target as ExpressionItemNode)?.ItemName;
		public override int ItemNameOffset => (Target as ExpressionItemNode).ItemNameOffset;

		public ExpressionItemTransform (int offset, int length, ExpressionNode target, ExpressionNode transform, ExpressionNode separator)
			: base (offset, length)
		{
			Target = target;
			target.SetParent (this);
			Transform = transform;
			transform?.SetParent (this);
			Separator = separator;
			separator?.SetParent (this);
		}

		public override ExpressionNodeKind NodeKind => ExpressionNodeKind.ItemTransform;
	}
}
