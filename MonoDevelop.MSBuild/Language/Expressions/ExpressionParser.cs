// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace MonoDevelop.MSBuild.Language.Expressions
{
	static partial class ExpressionParser
	{
		public static ExpressionNode Parse (string expression, ExpressionOptions options = ExpressionOptions.None, int baseOffset = 0)
		{
			return Parse (expression, 0, expression.Length - 1, options, baseOffset);
		}

		public static ExpressionNode Parse (string buffer, int startOffset, int endOffset, ExpressionOptions options, int baseOffset)
		{
			return Parse (buffer, startOffset, endOffset, options, baseOffset, out _);
		}

		static ExpressionNode Parse (string buffer, int startOffset, int endOffset, ExpressionOptions options, int baseOffset, out bool hasError)
		{
			List<ExpressionNode> splitList = null;
			char listSeparator = ';';
			var nodes = new List<ExpressionNode> ();

			int lastNodeEnd = startOffset;
			int offset = startOffset;
			while (offset <= endOffset) {
				char c = buffer [offset];

				//consume entities simply so the semicolon doesn't mess with list parsing
				//we don't need the value and the base XML editor will handle errors
				if (c == '&') {
					offset++;
					//FIXME: use proper entity name logic. this will do for now.
					ReadName (buffer, ref offset, endOffset);
					if (offset > endOffset) {
						break;
					}
					if (buffer[offset] == ';') {
						offset++;
						continue;
					}
					c = buffer [offset];
				}

				if ((options.HasFlag (ExpressionOptions.Lists) && c == ';') || (c == ',' && options.HasFlag (ExpressionOptions.CommaLists))) {
					CaptureLiteral (offset, nodes.Count == 0);
					if (splitList == null) {
						splitList = new List<ExpressionNode> ();
					}
					FlushNodesToSplitList (offset, c, out hasError);
					offset++;
					lastNodeEnd = offset;
					continue;
				}

				int possibleLiteralEndOffset = offset;

				ExpressionNode node;
				switch (c) {
				case '@':
					if (!TryConsumeParen ()) {
						offset++;
						continue;
					}
					if (options.HasFlag (ExpressionOptions.Items)) {
						node = ParseItem (buffer, ref offset, endOffset, baseOffset, out hasError);
					} else {
						node = new ExpressionError (baseOffset + offset, ExpressionErrorKind.ItemsDisallowed, out hasError);
					}
					break;
				case '$':
					if (!TryConsumeParen ()) {
						offset++;
						continue;
					}
					node = ParseProperty (buffer, ref offset, endOffset, baseOffset, out hasError);
					break;
				case '%':
					if (!TryConsumeParen ()) {
						offset++;
						continue;
					}
					if (options.HasFlag (ExpressionOptions.Metadata)) {
						node = ParseMetadata (buffer, ref offset, endOffset, baseOffset, out hasError);
					} else {
						node = new ExpressionError (baseOffset + offset, ExpressionErrorKind.MetadataDisallowed, out hasError);
					}
					break;
				default:
					offset++;
					continue;
				}

				CaptureLiteral (possibleLiteralEndOffset, false);
				lastNodeEnd = offset;

				nodes.Add (node);
				if (hasError) {
					//short circuit out without capturing the rest as text, since it's not useful
					return CreateResult (offset, out hasError);
				}

				bool TryConsumeParen ()
				{
					if (offset < endOffset && buffer[offset+1] == '(') {
						offset++;
						offset++;
						return true;
					}
					return false;
				}
			}

			CaptureLiteral (endOffset + 1, nodes.Count == 0);
			return CreateResult (endOffset, out hasError);

			void CaptureLiteral (int toOffset, bool isPure)
			{
				if (toOffset > lastNodeEnd) {
					string s = buffer.Substring (lastNodeEnd, toOffset - lastNodeEnd);
					nodes.Add (new ExpressionText (baseOffset + lastNodeEnd, s, isPure));
				}
			}

			void FlushNodesToSplitList (int offset, char sep, out bool hasError)
			{
				hasError = false;
				listSeparator = sep;
				if (nodes.Count == 0) {
					splitList.Add (new ExpressionError (baseOffset + offset, ExpressionErrorKind.EmptyListEntry, out hasError));
				} else if (nodes.Count == 1) {
					splitList.Add (nodes [0]);
					nodes.Clear ();
				} else {
					var start = nodes [0].Offset;
					var l = nodes [nodes.Count - 1].End - start;
					splitList.Add (new ConcatExpression (start, l, nodes.ToArray ()));
					nodes.Clear ();
				}
			}

			ExpressionNode CreateResult (int offset, out bool hasError)
			{
				hasError = false;
				if (splitList != null) {
					FlushNodesToSplitList (offset, default, out hasError);
				}
				if (splitList != null) {
					return new ListExpression (baseOffset + startOffset, endOffset - startOffset + 1, listSeparator, splitList.ToArray ());
				}
				if (nodes.Count == 0) {
					return new ExpressionText (baseOffset + startOffset, "", true);
				}
				if (nodes.Count == 1) {
					return nodes [0];
				}
				return new ConcatExpression (baseOffset + startOffset, endOffset - startOffset + 1, nodes.ToArray ());
			}
		}

		static ExpressionNode ParseItem (string buffer, ref int offset, int endOffset, int baseOffset, out bool hasError)
		{
			int start = offset - 2;

			ConsumeWhitespace (ref offset);

			string name = ReadName (buffer, ref offset, endOffset);
			if (name == null) {
				return new ExpressionError (baseOffset + offset, ExpressionErrorKind.ExpectingItemName, out hasError);
			}

			ExpressionNode itemRef = new ExpressionItemName (baseOffset + offset - name.Length, name);

			while (offset <= endOffset) {
				ConsumeWhitespace (ref offset);

				if (offset <= endOffset && buffer[offset] == ')') {
					hasError = false;
					offset++;
					return new ExpressionItem (baseOffset + start, offset - start, itemRef);
				}

				ConsumeWhitespace (ref offset);

				if (offset > endOffset || buffer[offset] != '-') {
					return new IncompleteExpressionError (
						baseOffset + offset, offset > endOffset,
						itemRef is ExpressionItemTransform ? ExpressionErrorKind.ExpectingRightParen : ExpressionErrorKind.ExpectingRightParenOrDash,
						new ExpressionItem (baseOffset + start, offset - start + 1, itemRef),
						out hasError
					);
				}

				offset++;
				if (offset > endOffset || buffer[offset] != '>') {
					return new IncompleteExpressionError (
						baseOffset + offset, offset > endOffset, ExpressionErrorKind.ExpectingRightAngleBracket,
						new ExpressionItem (baseOffset + start, offset - start + 1, itemRef),
						out hasError
					);
				}

				offset++;
				ConsumeWhitespace (ref offset);

				if (offset > endOffset || !(buffer[offset] == '\'' || char.IsLetter (buffer[offset]))) {
					return new IncompleteExpressionError (
						baseOffset + offset, offset > endOffset, ExpressionErrorKind.ExpectingMethodOrTransform,
						new ExpressionItem (baseOffset + start, offset - start + 1, itemRef),
						out hasError
					);
				}

				char ch = buffer[offset];
				if (ch == '\'' || ch == '`' || ch == '"') {
					itemRef = ParseItemTransform (buffer, ref offset, endOffset, baseOffset, itemRef, out hasError);
				} else {
					itemRef = ParseItemFunction (buffer, ref offset, endOffset, baseOffset, itemRef, out hasError);
				}

				if (hasError) {
					return new ExpressionItem (baseOffset + start, itemRef.End - start - baseOffset, itemRef);
				}
			}

			return new IncompleteExpressionError (
				baseOffset + offset, offset > endOffset,
				itemRef is ExpressionItemTransform? ExpressionErrorKind.ExpectingRightParen : ExpressionErrorKind.ExpectingRightParenOrDash,
				new ExpressionItem (baseOffset + start, offset - start, itemRef),
				out hasError
			);

			void ConsumeWhitespace (ref int o)
			{
				while (o <= endOffset && buffer [o] == ' ') {
					o++;
				}
			}
		}

		static ExpressionNode ParseItemTransform (string buffer, ref int offset, int endOffset, int baseOffset, ExpressionNode target, out bool hasError)
		{
			char terminator = buffer [offset];

			var expr = ReadArgumentString (terminator, buffer, ref offset, endOffset, baseOffset, out hasError);
			if (hasError) {
				return new ExpressionItemTransform (target.Offset, offset - target.Offset + baseOffset, target, expr, null);
			}

			int preSpaceOffset = offset;
			ConsumeSpace (buffer, ref offset, endOffset);

			if (offset > endOffset || buffer [offset] != ',') {
				return new ExpressionItemTransform (target.Offset, preSpaceOffset - target.Offset + baseOffset, target, expr, null);
			}
			offset++;

			ConsumeSpace (buffer, ref offset, endOffset);

			bool foundCustomSeparator = false;
			if (offset <= endOffset) {
				terminator = buffer [offset];
				if (terminator == '\'' || terminator == '"' || terminator == '`') {
					foundCustomSeparator = true;
				}
			}

			if (!foundCustomSeparator) {
				return new IncompleteExpressionError (
					offset,
					offset > endOffset,
					ExpressionErrorKind.ExpectingValue,
					new ExpressionItemTransform (target.Offset, preSpaceOffset - target.Offset + baseOffset, target, expr, null),
					out hasError
				);
			}

			ExpressionNode sepExpr = ReadArgumentString (terminator, buffer, ref offset, endOffset, baseOffset, out hasError);

			return new ExpressionItemTransform (target.Offset, offset - target.Offset + baseOffset, target, expr, sepExpr);
		}

		static ExpressionNode ParseItemFunction (string buffer, ref int offset, int endOffset, int baseOffset, ExpressionNode target, out bool hasError)
		{
			int nameOffset = offset;
			var funcNameStr = ReadName (buffer, ref offset, endOffset);
			if (funcNameStr == null) {
				return new IncompleteExpressionError (
					baseOffset + offset,
					offset > endOffset,
					ExpressionErrorKind.ExpectingMethodName,
					new ExpressionItemFunctionInvocation (target.Offset, (offset + baseOffset) - target.Offset, target, null, null),
					out hasError
				);
			}
			var funcName = new ExpressionFunctionName (nameOffset + baseOffset, funcNameStr);

			ConsumeSpace (buffer, ref offset, endOffset);

			if (offset > endOffset || buffer [offset] != '(') {
				return new IncompleteExpressionError (
					baseOffset + offset,
					offset > endOffset,
					ExpressionErrorKind.ExpectingLeftParen,
					new ExpressionItemFunctionInvocation (target.Offset, (offset + baseOffset) - target.Offset, target, funcName, null),
					out hasError
				);
			}

			var args = ParseFunctionArgumentList (buffer, ref offset, endOffset, baseOffset, out hasError);

			return new ExpressionItemFunctionInvocation (target.Offset, (offset + baseOffset) - target.Offset, target, funcName, args);
		}

		static string TryReadAlphaName (string buffer, ref int offset, int endOffset)
		{
			if (offset > endOffset) {
				return null;
			}

			int start = offset;
			char ch = buffer[offset];
			if (!char.IsLetter (ch)) {
				return null;
			}
			offset++;
			while (offset <= endOffset) {
				ch = buffer[offset];
				if (!char.IsLetter (ch)) {
					break;
				}
				offset++;
			}
			return buffer.Substring (start, offset - start);
		}

		static string ReadName (string buffer, ref int offset, int endOffset)
		{
			if (offset > endOffset) {
				return null;
			}

			int start = offset;
			char ch = buffer [offset];
			if (!char.IsLetter (ch) && ch != '_') {
				return null;
			}
			offset++;
			while (offset <= endOffset) {
				ch = buffer [offset];
				if (!char.IsLetterOrDigit (ch) && ch != '_') {
					break;
				}
				offset++;
			}
			return buffer.Substring (start, offset - start);
		}

		enum ClassRefParseState
		{
			Initial,
			Component,
			ExpectingPeriod,
			ExpectingComponent
		}

		static ExpressionNode ReadClassReference (string buffer, ref int offset, int endOffset, int baseOffset, out bool hasError)
		{
			int start = offset;
			int lastCharOffset = start;

			var sb = new StringBuilder ();
			ClassRefParseState state = ClassRefParseState.Initial;

			while (offset <= endOffset) {
				char ch = buffer [offset];

				switch (state) {
				case ClassRefParseState.Initial:
					if (char.IsLetter (ch) || ch == '_') {
						sb.Append (ch);
						lastCharOffset = offset;
						state = ClassRefParseState.Component;
						break;
					}
					return new ExpressionError (baseOffset + offset, ExpressionErrorKind.ExpectingClassName, out hasError);

				case ClassRefParseState.Component:
					if (char.IsLetterOrDigit (ch) || ch == '_') {
						sb.Append (ch);
						lastCharOffset = offset;
						break;
					}
					if (ch == '.') {
						sb.Append (ch);
						lastCharOffset = offset;
						state = ClassRefParseState.ExpectingComponent;
						break;
					}
					if (char.IsWhiteSpace (ch)) {
						state = ClassRefParseState.ExpectingPeriod;
						break;
					}
					hasError = false;
					return new ExpressionClassReference (baseOffset + start, sb.ToString ());

				case ClassRefParseState.ExpectingComponent:
					if (char.IsLetter (ch) || ch == '_') {
						sb.Append (ch);
						lastCharOffset = offset;
						state = ClassRefParseState.Component;
						break;
					}
					if (char.IsWhiteSpace (ch)) {
						break;
					}
					return new ExpressionError (baseOffset + offset, ExpressionErrorKind.ExpectingClassNameComponent, out hasError);

				case ClassRefParseState.ExpectingPeriod:
					if (char.IsWhiteSpace (ch)) {
						break;
					}
					if (ch == '.') {
						sb.Append (ch);
						lastCharOffset = offset;
						state = ClassRefParseState.ExpectingComponent;
						break;
					}
					hasError = false;
					return new ExpressionClassReference (baseOffset + start, sb.ToString ());
				}
				offset++;
			}

			switch (state) {
			case ClassRefParseState.Initial:
				return new ExpressionError (offset, true, ExpressionErrorKind.ExpectingClassName, out hasError);
			case ClassRefParseState.ExpectingComponent:
				return new IncompleteExpressionError (offset, true, ExpressionErrorKind.ExpectingClassNameComponent,
					new ExpressionClassReference (baseOffset + start, sb.ToString ()),
					out hasError
				);
			}

			hasError = false;
			return new ExpressionClassReference (baseOffset + start, sb.ToString ());
		}

		static void ConsumeSpace (string buffer, ref int offset, int endOffset)
		{
			while (offset <= endOffset) {
				var ch = buffer [offset];
				if (ch != ' ') {
					return;
				}
				offset++;
			}
		}

		//attempts to read int or double token. assumes offset is already at first character
		static ExpressionNode ReadArgumentNumber (string buffer, ref int offset, int endOffset, int baseOffset, out bool hasError)
		{
			int start = offset;
			bool foundPeriod = false;
			while (offset <= endOffset) {
				char ch = buffer [offset];
				switch (ch) {
				case '.':
					if (foundPeriod) {
						offset++;
						break;
					}
					foundPeriod = true;
					goto case '0';
				case ',': case ')': case ' ': case ']': {
						return Result (ref offset, out hasError);
					}
				case '0': case '1': case '2': case '3': case '4': case '5': case '6': case '7': case '8': case '9':
					offset++;
					continue;
				default:
					return new ExpressionError (start + baseOffset, ExpressionErrorKind.CouldNotParseNumber, out hasError);
				}
			}
			if (foundPeriod && (offset - start) == 1) {
				return new ExpressionError (start + baseOffset, ExpressionErrorKind.IncompleteValue, out hasError);
			}
			return Result (ref offset, out hasError);

			ExpressionNode Result (ref int offset, out bool hasError)
			{
				string str = buffer.Substring (start, offset - start);
				if (foundPeriod) {
					if (double.TryParse (str, out double result)) {
						hasError = false;
						return new ExpressionArgumentFloat (start + baseOffset, str.Length, result);
					}
				} else {
					if (long.TryParse (str, out long result)) {
						hasError = false;
						return new ExpressionArgumentInt (start + baseOffset, str.Length, result);
					}
				}

				return new ExpressionError (start + baseOffset, ExpressionErrorKind.CouldNotParseNumber, out hasError);
			}
		}

		static ExpressionNode ParseProperty (string buffer, ref int offset, int endOffset, int baseOffset, out bool hasError)
		{
			int start = offset - 2;

			ConsumeSpace (buffer, ref offset, endOffset);

			ExpressionNode propRef;

			if (offset <= endOffset && buffer [offset] == '[') {
				propRef = ParsePropertyStaticFunction (offset, buffer, ref offset, endOffset, baseOffset, out hasError);
				if (hasError) {
					return new ExpressionProperty (baseOffset + start, offset - start, propRef);
				}
			} else {
				string name = ReadName (buffer, ref offset, endOffset);
				if (name == null) {
					return new ExpressionError (baseOffset + offset, ExpressionErrorKind.ExpectingPropertyName, out hasError);
				}

				propRef = new ExpressionPropertyName (baseOffset + offset - name.Length, name);
			}

			ConsumeSpace (buffer, ref offset, endOffset);

			if (offset <= endOffset
				&& buffer [offset] == ':'
				&& propRef is ExpressionPropertyName regStr
				&& string.Equals ("registry", regStr.Name, StringComparison.OrdinalIgnoreCase)
				) {
				offset++;
				ConsumeSpace (buffer, ref offset, endOffset);
				int regStart = offset;
				while (offset <= endOffset) {
					char ch = buffer [offset];

					switch (ch) {
					case '\\':
					case '@':
					case '_':
					case '.':
						offset++;
						continue;
					case ' ':
						ConsumeSpace (buffer, ref offset, endOffset);
						goto case ')';
					case ')':
						string value = buffer.Substring (regStart, offset - regStart);
						propRef = new ExpressionPropertyRegistryValue (propRef.Offset, offset - propRef.Offset + baseOffset, value);
						break;
					default:
						if (char.IsLetterOrDigit (ch)) {
							goto case '.';
						}
						string v = buffer.Substring (regStart, offset - regStart);
						propRef = new ExpressionPropertyRegistryValue (propRef.Offset, offset - propRef.Offset + baseOffset, v);
						// as the current char is not ')', this will turn into an error
						break;
					}
					break;
				}
			} else {
				char c;
				while (offset <= endOffset && ((c = buffer[offset]) == '.' || c == '[')) {
					propRef = c == '.'
						? ParsePropertyInstanceFunction (buffer, ref offset, endOffset, baseOffset, propRef, out hasError)
						: ParsePropertyIndexer (buffer, ref offset, endOffset, baseOffset, propRef, out hasError);
					if (hasError) {
						return new ExpressionProperty (baseOffset + start, offset - start, propRef);
					}
					ConsumeSpace (buffer, ref offset, endOffset);
				}
			}

			if (offset > endOffset || buffer [offset] != ')') {
				return new IncompleteExpressionError (
					baseOffset + offset, offset > endOffset,
					ExpressionErrorKind.ExpectingRightParenOrPeriod,
					new ExpressionProperty (baseOffset + start, offset - start, propRef),
					out hasError
				);
			}

			hasError = false;
			offset++;
			return new ExpressionProperty (baseOffset + start, offset - start, propRef);
		}

		static ExpressionNode ParsePropertyStaticFunction (int start, string buffer, ref int offset, int endOffset, int baseOffset, out bool hasError)
		{
			offset++;
			ConsumeSpace (buffer, ref offset, endOffset);

			var classRef = ReadClassReference (buffer, ref offset, endOffset, baseOffset, out hasError);
			if (hasError) {
				return new ExpressionPropertyFunctionInvocation (baseOffset + start, offset - start, classRef, null, null);
			}

			if (offset + 2 > endOffset || buffer [offset] != ']' || buffer [offset+1] != ':' || buffer [offset+2] != ':') {
				bool eofCausedError = offset + 2 > endOffset
					&& (offset > endOffset || buffer[offset] == ']')
					&& (offset+1 > endOffset || buffer[offset+1] == ':')
					&& (offset+2 > endOffset || buffer[offset+2] == ':');

				return new IncompleteExpressionError (
					baseOffset + offset,
					eofCausedError,
					ExpressionErrorKind.ExpectingBracketColonColon,
					new ExpressionPropertyFunctionInvocation (baseOffset + start, offset - start, classRef, null, null),
					out hasError
				); ;
			}
			offset += 3;

			ConsumeSpace (buffer, ref offset, endOffset);

			int nameOffset = offset;
			var funcNameStr = ReadName (buffer, ref offset, endOffset);
			if (funcNameStr == null) {
				return new IncompleteExpressionError (
					baseOffset + offset,
					offset > endOffset,
					ExpressionErrorKind.ExpectingMethodName,
					new ExpressionPropertyFunctionInvocation (baseOffset + start, offset - start, classRef, null, null),
					out hasError
				);
			}
			var funcName = new ExpressionFunctionName (baseOffset + nameOffset, funcNameStr);

			ConsumeSpace (buffer, ref offset, endOffset);

			if (offset > endOffset) {
				return new IncompleteExpressionError (
					baseOffset + offset,
					offset > endOffset,
					ExpressionErrorKind.IncompleteProperty,
					new ExpressionPropertyFunctionInvocation (baseOffset + start, offset - start, classRef, funcName, null),
					out hasError
				);
			}

			//arguments are optional, it could be a property
			ExpressionNode args = null;
			if (buffer[offset] == '(') {
				args = ParseFunctionArgumentList (buffer, ref offset, endOffset, baseOffset, out hasError);
			}

			return new ExpressionPropertyFunctionInvocation (baseOffset + start, offset - start, classRef, funcName, args);
		}

		static ExpressionNode ParsePropertyInstanceFunction (string buffer, ref int offset, int endOffset, int baseOffset, ExpressionNode target, out bool hasError)
		{
			offset++;

			ConsumeSpace (buffer, ref offset, endOffset);

			int nameOffset = offset;
			var funcNameStr = ReadName (buffer, ref offset, endOffset);
			if (funcNameStr == null) {
				return new IncompleteExpressionError (
					baseOffset + offset,
					offset > endOffset,
					ExpressionErrorKind.ExpectingMethodName,
					new ExpressionPropertyFunctionInvocation (target.Offset, offset + baseOffset - target.Offset, target, null, null),
					out hasError
				);
			}
			var funcName = new ExpressionFunctionName (baseOffset + nameOffset, funcNameStr);

			ConsumeSpace (buffer, ref offset, endOffset);

			if (offset > endOffset) {
				return new IncompleteExpressionError (
					baseOffset + offset,
					offset > endOffset,
					ExpressionErrorKind.IncompleteProperty,
					new ExpressionPropertyFunctionInvocation (target.Offset, offset + baseOffset - target.Offset, target, funcName, null),
					out hasError
				);
			}

			//arguments are optional, it could be a property
			ExpressionNode args = null;
			if (buffer[offset] == '(') {
				args = ParseFunctionArgumentList (buffer, ref offset, endOffset, baseOffset, out hasError);
			} else {
				hasError = false;
			}

			return new ExpressionPropertyFunctionInvocation (target.Offset, offset + baseOffset - target.Offset, target, funcName, args);
		}

		static ExpressionNode ParsePropertyIndexer (string buffer, ref int offset, int endOffset, int baseOffset, ExpressionNode target, out bool hasError)
		{
			var args = ParseFunctionArgumentList (buffer, ref offset, endOffset, baseOffset, out hasError);
			return new ExpressionPropertyFunctionInvocation (target.Offset, offset + baseOffset - target.Offset, target, null, args);
		}

		static ExpressionNode ParseFunctionArgumentList (string buffer, ref int offset, int endOffset, int baseOffset, out bool hasError)
		{
			hasError = false;

			bool isIndexer = buffer [offset] == '[';
			char terminator = isIndexer ? ']' : ')';

			int start = offset;

			offset++;
			ConsumeSpace (buffer, ref offset, endOffset);

			var values = new List<ExpressionNode> ();
			bool first = true;

			while (true) {
				if (offset <= endOffset && buffer [offset] == terminator) {
					offset++;
					break;
				}

				bool foundComma = false;
				if (!first && offset <= endOffset && buffer [offset] == ',') {
					foundComma = true;
					offset++;
					ConsumeSpace (buffer, ref offset, endOffset);
				}

				if (offset > endOffset || (!first && !foundComma)) {
					return new IncompleteExpressionError (
						baseOffset + offset,
						offset > endOffset,
						first ? ExpressionErrorKind.ExpectingRightParenOrValue
						: (foundComma
							? ExpressionErrorKind.ExpectingValue
							: ExpressionErrorKind.ExpectingRightParenOrComma),
						new ExpressionArgumentList (baseOffset + start, offset - start, values),
						out hasError
					);
				}

				ExpressionNode arg = ParseFunctionArgument (first, buffer, ref offset, endOffset, baseOffset, out hasError);
				if (arg != null) {
					values.Add (arg);
				}
				if (hasError) {
					break;
				}

				ConsumeSpace (buffer, ref offset, endOffset);

				first = false;
			}

			return new ExpressionArgumentList (baseOffset + start, offset - start, values);
		}

		//expects to be advanced to valid first char of argument
		static ExpressionNode ParseFunctionArgument (bool wasFirst, string buffer, ref int offset, int endOffset, int baseOffset, out bool hasError)
		{
			int start = offset;

			var ch = buffer [start];
			if (ch == '.' || char.IsDigit (ch)) {
				return ReadArgumentNumber (buffer, ref offset, endOffset, baseOffset, out hasError);
			}

			if (ch == '"' || ch == '\'' || ch == '`') {
				return ReadArgumentString (ch, buffer, ref offset, endOffset, baseOffset, out hasError);
			}

			bool wasEOF = false;

			if (ch == '$' || ch == '%') {
				if (offset < endOffset) {
					if (buffer[offset + 1] == '(') {
						offset += 2;
						ExpressionNode node = ch == '$'
							? ParseProperty (buffer, ref offset, endOffset, baseOffset, out hasError)
							: ParseMetadata (buffer, ref offset, endOffset, baseOffset, out hasError);
						return node;
					}
				} else {
					wasEOF = true;
				}
			}
			else if (char.IsLetter (ch)) {
				var crNode = ReadClassReference (buffer, ref offset, endOffset, baseOffset, out hasError);
				if (crNode is ExpressionClassReference classRef && bool.TryParse (classRef.Name, out bool boolVal)) {
					hasError = false;
					return new ExpressionArgumentBool (classRef.Offset, boolVal);
				}
				return crNode;
			}

			//the token didn't start with any character we expected
			//consume the character anyway so the completion trigger can use it
			offset++;
			var expr = new ExpressionText (baseOffset + start, buffer.Substring (start, offset - start), true);
			var kind = wasFirst ? ExpressionErrorKind.ExpectingRightParenOrValue : ExpressionErrorKind.ExpectingValue;
			return new IncompleteExpressionError (baseOffset + start, wasEOF, kind, expr, out hasError);
		}

		static ExpressionNode ReadArgumentString (char terminator, string buffer, ref int offset, int endOffset, int baseOffset, out bool hasError)
		{
			int start = offset;
			offset++;
			while (offset <= endOffset) {
				char ch = buffer [offset];
				if (ch == terminator) {
					offset++;
					var subExpr = Parse (buffer, start + 1, offset - 2, ExpressionOptions.ItemsAndMetadata, baseOffset, out hasError);
					return new QuotedExpression (start + baseOffset, offset - start, terminator, subExpr);
				}
				offset++;
			}

			return new IncompleteExpressionError (
				baseOffset + offset,
				true,
				ExpressionErrorKind.IncompleteString,
				new QuotedExpression (
					start + baseOffset,
					offset - start,
					terminator, Parse (buffer, start + 1, endOffset, ExpressionOptions.ItemsAndMetadata, baseOffset, out _)),
				out hasError);
		}

		static ExpressionNode ParseMetadata (string buffer, ref int offset, int endOffset, int baseOffset, out bool hasError)
		{
			int start = offset - 2;

			ConsumeSpace (buffer, ref offset, endOffset);

			string name = ReadName (buffer, ref offset, endOffset);
			if (name == null) {
				return new ExpressionError (baseOffset + offset, ExpressionErrorKind.ExpectingMetadataOrItemName, out hasError);
			}

			ConsumeSpace (buffer, ref offset, endOffset);

			if (offset <= endOffset && buffer [offset] == ')') {
				hasError = false;
				offset++;
				return new ExpressionMetadata (baseOffset + start, offset - start, null, name);
			}

			if (offset > endOffset || (buffer [offset] != '.' && buffer [offset] != '-')) {
				return new IncompleteExpressionError (
					baseOffset + offset, offset > endOffset, ExpressionErrorKind.ExpectingRightParenOrPeriod,
					new ExpressionMetadata (baseOffset + start, offset - start + 1, name, null),
					out hasError
				);
			}

			//TODO: add warning for nonstandard format
			bool isNonStandard = false;
			if (buffer[offset] == '-') {
				offset++;
				isNonStandard = true;
				if (offset > endOffset || buffer[offset] != '>') {
					return new IncompleteExpressionError (
						baseOffset + offset, offset > endOffset, ExpressionErrorKind.ExpectingRightAngleBracket,
						new ExpressionMetadata (baseOffset + start, offset - start + 1, name, null, isNonStandard),
						out hasError
					);
				}
			}

			offset++;

			ConsumeSpace (buffer, ref offset, endOffset);

			string metadataName = ReadName (buffer, ref offset, endOffset);
			if (metadataName == null) {
				return new IncompleteExpressionError (
					baseOffset + offset, offset > endOffset, ExpressionErrorKind.ExpectingMetadataName,
					new ExpressionMetadata (baseOffset + start, offset - start + 1, name, null, isNonStandard),
					out hasError
				);
			}

			ConsumeSpace (buffer, ref offset, endOffset);

			if (offset > endOffset || buffer [offset] != ')') {
				return new IncompleteExpressionError (
					baseOffset + offset, offset > endOffset, ExpressionErrorKind.ExpectingRightParen,
					new ExpressionMetadata (baseOffset + start, offset - start + 1, name, metadataName, isNonStandard),
					out hasError
				);
			}

			hasError = false;
			offset++;
			return new ExpressionMetadata (baseOffset + start, offset - start, name, metadataName, isNonStandard);
		}
	}
}
