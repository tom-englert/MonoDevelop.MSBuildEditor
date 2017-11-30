﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonoDevelop.MSBuildEditor.Evaluation;
using MonoDevelop.MSBuildEditor.Language;
using System.Text;

namespace MonoDevelop.MSBuildEditor.Schema
{
	static class MSBuildCompletionExtensions
	{
		public static IEnumerable<BaseInfo> GetAttributeCompletions (this MSBuildResolveResult rr, IEnumerable<IMSBuildSchema> schemas, MSBuildToolsVersion tv)
		{
			bool isInTarget = false;
			if (rr.LanguageElement.Kind == MSBuildKind.Item) {
				isInTarget = rr.LanguageElement.IsInTarget (rr.XElement);
			}

			foreach (var att in rr.LanguageElement.Attributes) {
				if (!att.IsAbstract) {
					if (rr.LanguageElement.Kind == MSBuildKind.Item) {
						if (isInTarget) {
							if (att.Name == "Update") {
								continue;
							}
						} else {
							if (att.Name == "KeepMetadata" || att.Name == "RemoveMetadata" || att.Name == "KeepDuplicates") {
								continue;
							}
						}
					}
					yield return att;
				}
			}


			if (rr.LanguageElement.Kind == MSBuildKind.Item && tv.IsAtLeast (MSBuildToolsVersion.V15_0)) {
				foreach (var item in schemas.GetMetadata (rr.ElementName, false)) {
					yield return item;
				}
			}

			if (rr.LanguageElement.Kind == MSBuildKind.Task) {
				foreach (var parameter in schemas.GetTaskParameters (rr.ElementName)) {
					yield return parameter;

				}
			}
		}

		public static bool IsInTarget (this MSBuildLanguageElement resolvedElement, Xml.Dom.XElement element)
		{
			switch (resolvedElement.Kind) {
			case MSBuildKind.Metadata:
				element = element?.ParentElement ();
				goto case MSBuildKind.Item;
			case MSBuildKind.Property:
			case MSBuildKind.Item:
				element = element?.ParentElement ();
				goto case MSBuildKind.ItemGroup;
			case MSBuildKind.ItemGroup:
			case MSBuildKind.PropertyGroup:
				var name = element?.ParentElement ()?.Name.Name;
				return string.Equals (name, "Target", StringComparison.OrdinalIgnoreCase);
			}
			return false;
		}

		static IEnumerable<BaseInfo> GetAbstractAttributes (this IEnumerable<IMSBuildSchema> schemas, MSBuildKind kind, string elementName)
		{
			switch (kind) {
			case MSBuildKind.Item:
				return schemas.GetItems ();
			case MSBuildKind.Task:
				return schemas.GetTasks ();
			case MSBuildKind.Property:
				return schemas.GetProperties (false);
			case MSBuildKind.Metadata:
				return schemas.GetMetadata (elementName, false);
			}
			return null;
		}

		public static IEnumerable<BaseInfo> GetElementCompletions (this MSBuildResolveResult rr, IEnumerable<IMSBuildSchema> schemas)
		{
			if (rr?.LanguageElement == null) {
				yield return MSBuildLanguageElement.Get ("Project");
				yield break;
			}

			if (rr.LanguageElement.Children == null) {
				yield break;
			}

			foreach (var c in rr.LanguageElement.Children) {
				if (c.IsAbstract) {
					var abstractChildren = GetAbstractChildren (schemas, rr.LanguageElement.AbstractChild.Kind, rr.ElementName);
					if (abstractChildren != null) {
						foreach (var child in abstractChildren) {
							yield return child;
						}
					}
				} else {
					yield return c;
				}
			}
		}

		static IEnumerable<BaseInfo> GetAbstractChildren (this IEnumerable<IMSBuildSchema> schemas, MSBuildKind kind, string elementName)
		{
			switch (kind) {
			case MSBuildKind.Item:
				return schemas.GetItems ();
			case MSBuildKind.Task:
				return schemas.GetTasks ();
			case MSBuildKind.Property:
				return schemas.GetProperties (false);
			case MSBuildKind.Metadata:
				return schemas.GetMetadata (elementName, false);
			}
			return null;
		}

		public static IReadOnlyList<BaseInfo> GetValueCompletions (
			MSBuildValueKind kind, MSBuildRootDocument doc)
		{
			var simple = kind.GetSimpleValues (true);
			if (simple != null) {
				return simple;
			}

			switch (kind) {
			case MSBuildValueKind.TargetName:
				return doc.GetTargets ().ToList ();
			case MSBuildValueKind.PropertyName:
				return doc.GetProperties (true).ToList ();
			case MSBuildValueKind.ItemName:
				return doc.GetItems ().ToList ();
			case MSBuildValueKind.TargetFramework:
				return FrameworkInfoProvider.Instance.GetFrameworksWithShortNames ().ToList ();
			case MSBuildValueKind.TargetFrameworkIdentifier:
				return FrameworkInfoProvider.Instance.GetFrameworkIdentifiers ().ToList ();
			case MSBuildValueKind.TargetFrameworkVersion:
				return doc.Frameworks.SelectMany (
					tfm => FrameworkInfoProvider.Instance.GetFrameworkVersions (tfm.Identifier)
				).ToList ();
			case MSBuildValueKind.TargetFrameworkProfile:
				return doc.Frameworks.SelectMany (
					tfm => FrameworkInfoProvider.Instance.GetFrameworkProfiles (tfm.Identifier, tfm.Version)
				).ToList ();
			}

			var fileCompletions = GetFilenameCompletions (kind, doc, null, 0);
			if (fileCompletions != null) {
				return fileCompletions;
			}

			return null;
		}

		public static IReadOnlyList<BaseInfo> GetFilenameCompletions (
			MSBuildValueKind kind, MSBuildRootDocument doc,
			ExpressionNode triggerExpression, int triggerLength)
		{
			bool includeFiles = false;
			switch (kind) {
			case MSBuildValueKind.File:
			case MSBuildValueKind.ProjectFile:
				includeFiles = true;
				break;
			case MSBuildValueKind.FileOrFolder:
				includeFiles = true;
				break;
			case MSBuildValueKind.Folder:
			case MSBuildValueKind.FolderWithSlash:
				break;
			default:
				return null;
			}

			string basePath = EvaluateExpressionAsPath (triggerExpression, doc, triggerLength + 1);
			return basePath == null? null : GetPathCompletions (doc.Filename, basePath, includeFiles);
		}

		public static string EvaluateExpressionAsPath (ExpressionNode expression, MSBuildRootDocument doc, int skipEndChars = 0)
		{
			if (expression == null) {
				return null;
			}

			if (expression is ExpressionLiteral lit) {
				var path = lit.Value.Substring (0, lit.Value.Length - skipEndChars);
				//FIXME handle encoding
				return Projects.MSBuild.MSBuildProjectService.FromMSBuildPath (Path.GetDirectoryName (doc.Filename), path);
			}

			if (!(expression is Expression expr)) {
				return null;
			}

			//FIXME evaluate directly without the MSBuildEvaluationContext
			var sb = new StringBuilder ();
			for (int i = 0; i < expr.Nodes.Count; i++) {
				var node = expr.Nodes [i];
				if (node is ExpressionLiteral l) {
					var val = l.Value;
					if (i == expr.Nodes.Count - 1) {
						val = val.Substring (0, val.Length - skipEndChars);
					}
					sb.Append (val);
				} else if (node is ExpressionProperty p) {
					sb.Append ($"$({p.Name})");
				} else {
					return null;
				}
			}

			var evalCtx = MSBuildEvaluationContext.Create (
				doc.ToolsVersion, doc.RuntimeInformation, doc.Filename, doc.Filename
			);
			return evalCtx.EvaluatePath (sb.ToString (), Path.GetDirectoryName (doc.Filename));
		}

		static IReadOnlyList<BaseInfo> GetPathCompletions (string projectPath, string completionBasePath, bool includeFiles)
		{
			var projectBaseDir = Path.GetDirectoryName (projectPath);
			if (completionBasePath == null) {
				completionBasePath = projectBaseDir;
			} else {
				completionBasePath = Path.GetFullPath (Path.Combine (projectBaseDir, completionBasePath));
			}

			var infos = new List<BaseInfo> ();
			foreach (var e in Directory.GetDirectories (completionBasePath)) {
				var name = Path.GetFileName (e);
				infos.Add (new FileOrFolderInfo (name, true, e));
			}

			if (includeFiles) {
				foreach (var e in Directory.GetFiles (completionBasePath)) {
					var name = Path.GetFileName (e);
					infos.Add (new FileOrFolderInfo (name, false, e));
				}
			}
			return infos;
		}

		public static BaseInfo GetResolvedReference (this MSBuildResolveResult rr, MSBuildRootDocument doc)
		{
			switch (rr.ReferenceKind) {
			case MSBuildReferenceKind.Item:
				return doc.GetItem ((string)rr.Reference);
			case MSBuildReferenceKind.Metadata:
				var meta = (Tuple<string, string>)rr.Reference;
				return doc.GetMetadata (meta.Item1, meta.Item2, true);
			case MSBuildReferenceKind.Property:
				return doc.GetProperty ((string)rr.Reference);
			case MSBuildReferenceKind.Task:
				return doc.GetTask ((string)rr.Reference);
			case MSBuildReferenceKind.Target:
				return doc.GetTarget ((string)rr.Reference);
			case MSBuildReferenceKind.Keyword:
				var attName = rr.AttributeName;
				if (attName != null) {
					var att = rr.LanguageElement.GetAttribute (attName);
					if (att != null && !att.IsAbstract) {
						return att;
					}
				} else {
					if (!rr.LanguageElement.IsAbstract) {
						return rr.LanguageElement;
					}
				}
				break;
			case MSBuildReferenceKind.KnownValue:
				return (ValueInfo)rr.Reference;
			case MSBuildReferenceKind.TargetFramework:
				var fx = (FrameworkReference)rr.Reference;
				return FrameworkInfoProvider.Instance.GetBestInfo (fx, doc.Frameworks);
			}
			return null;
		}

		public static ValueInfo GetElementOrAttributeValueInfo (this MSBuildResolveResult rr, IEnumerable<IMSBuildSchema> schemas)
		{
			if (rr.LanguageElement == null) {
				return null;
			}

			if (rr.AttributeName != null) {
				return schemas.GetAttributeInfo (rr.LanguageAttribute, rr.ElementName, rr.AttributeName);
			}

			return schemas.GetElementInfo (rr.LanguageElement, rr.ParentName, rr.ElementName);
		}

		public static MSBuildValueKind InferValueKindIfUnknown (ValueInfo variable)
		{
			var kind = InferUnknownKind (variable);

			if (variable.ValueSeparators != null) {
				if (variable.ValueSeparators.Contains (';')) {
					kind |= MSBuildValueKind.List;
				}
				if (variable.ValueSeparators.Contains (',')) {
					kind |= MSBuildValueKind.CommaList;
				}
			}

			return kind;
		}

		static MSBuildValueKind InferUnknownKind (ValueInfo variable)
		{
			if (variable.ValueKind != MSBuildValueKind.Unknown) {
				return variable.ValueKind;
			}

			if (variable is MSBuildLanguageAttribute att) {
				switch (att.Name) {
				case "Include":
				case "Exclude":
				case "Remove":
				case "Update":
					return MSBuildValueKind.File.List ();
				}
			}

			if (variable is PropertyInfo || variable is MetadataInfo) {
				if (StartsWith ("Enable")
					|| StartsWith ("Disable")
					|| StartsWith ("Require")
					|| StartsWith ("Use")
					|| StartsWith ("Allow")
					|| EndsWith ("Enabled")
					|| EndsWith ("Disabled")
					|| EndsWith ("Required")) {
					return MSBuildValueKind.Bool;
				}
				if (EndsWith ("DependsOn")) {
					return MSBuildValueKind.TargetName.List ();
				}
				if (EndsWith ("Path")) {
					return MSBuildValueKind.FileOrFolder;
				}
				if (EndsWith ("Paths")) {
					return MSBuildValueKind.FileOrFolder.List ();
				}
				if (EndsWith ("Directory")
					|| EndsWith ("Dir")) {
					return MSBuildValueKind.Folder;
				}
				if (EndsWith ("File")) {
					return MSBuildValueKind.File;
				}
				if (EndsWith ("FileName")) {
					return MSBuildValueKind.Filename;
				}
				if (EndsWith ("Url")) {
					return MSBuildValueKind.Url;
				}
				if (EndsWith ("Ext")) {
					return MSBuildValueKind.Extension;
				}
				if (EndsWith ("Guid")) {
					return MSBuildValueKind.Guid;
				}
				if (EndsWith ("Directories") || EndsWith ("Dirs")) {
					return MSBuildValueKind.Folder.List ();
				}
				if (EndsWith ("Files")) {
					return MSBuildValueKind.File.List ();
				}
			}

			return MSBuildValueKind.Unknown;

			bool StartsWith (string prefix) => variable.Name.StartsWith (prefix, StringComparison.OrdinalIgnoreCase)
			                                           && variable.Name.Length > prefix.Length
			                                           && char.IsUpper (variable.Name[prefix.Length]);
			bool EndsWith (string suffix) => variable.Name.EndsWith (suffix, StringComparison.OrdinalIgnoreCase);
		}
	}
}
