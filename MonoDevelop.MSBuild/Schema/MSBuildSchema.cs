// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Assembly = System.Reflection.Assembly;

using MonoDevelop.MSBuild.Language;
using MonoDevelop.MSBuild.Language.Typesystem;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MonoDevelop.MSBuild.Schema
{
	partial class MSBuildSchema : IMSBuildSchema, IEnumerable<ISymbol>
	{
		public Dictionary<string, PropertyInfo> Properties { get; } = new Dictionary<string, PropertyInfo> (StringComparer.OrdinalIgnoreCase);
		public Dictionary<string, ItemInfo> Items { get; } = new Dictionary<string, ItemInfo> (StringComparer.OrdinalIgnoreCase);
		public Dictionary<string, TaskInfo> Tasks { get; } = new Dictionary<string, TaskInfo> (StringComparer.OrdinalIgnoreCase);
		public Dictionary<string, TargetInfo> Targets { get; } = new Dictionary<string, TargetInfo> (StringComparer.OrdinalIgnoreCase);
		public List<string> IntelliSenseImports { get; } = new List<string> ();

		public static MSBuildSchema Load (TextReader reader, out IList<MSBuildSchemaLoadError> errors, string origin)
		{
			var schema = new MSBuildSchema ();
			schema.LoadInternal (reader, out errors, origin);
			return schema;
		}

		public static MSBuildSchema LoadResourceFromCallingAssembly (string resourceId, out IList<MSBuildSchemaLoadError> loadErrors)
		{
			var asm = Assembly.GetCallingAssembly ();
			using var stream = asm.GetManifestResourceStream (resourceId);
			if (stream == null) {
				throw new ArgumentException ($"Did not find resource stream '{resourceId}'");
			}
			using var sr = new StreamReader (stream);
			return Load (sr, out loadErrors, $"{asm.Location}/{resourceId}");
		}

		void LoadInternal (TextReader reader, out IList<MSBuildSchemaLoadError> loadErrors, string origin)
		{
			var state = new SchemaLoadState (origin);

			JObject doc;
			using (var jr = new JsonTextReader (reader)) {
				doc = JObject.Load (jr);
			}

			JObject properties = null;
			JObject items = null;
			JObject targets = null;
			JArray intellisenseImports = null;
			JArray metadataGroups = null;
			JObject customTypes = null;

			// we don't process the values in the switch, as we need a particular ordering
			foreach (var kv in doc) {
				switch (kv.Key) {
				case "properties":
					properties = (JObject)kv.Value;
					break;
				case "items":
					items = (JObject)kv.Value;
					break;
				case "targets":
					targets = (JObject)kv.Value;
					break;
				case "license":
				case "$schema":
					break;
				case "intellisenseImports":
					intellisenseImports = (JArray)kv.Value;
					break;
				case "metadata":
					metadataGroups = (JArray)kv.Value;
					break;
				case "types":
					customTypes = (JObject)kv.Value;
					break;
				default:
					state.AddWarning (kv.Value ?? doc, $"Unknown property");
					break;
				}
			}

			if (intellisenseImports != null) {
				LoadIntelliSenseImports (intellisenseImports);
			}
			// customTypes must come before properties, items and metadataGroups
			// as they may use the declared custom types
			if (customTypes != null) {
				state.LoadCustomTypes (customTypes);
			}
			if (properties != null) {
				foreach ((var name, var prop) in state.ReadProperties(properties)) {
					Properties.Add (name, prop);
				}
			}
			if (items != null) {
				foreach ((var name, var item) in state.ReadItems (items)) {
					Items.Add (name, item);
				}
			}
			// metadataGroups must come after items, as it may apply metadata to existing items
			if (metadataGroups != null) {
				foreach (var metaGroupObj in metadataGroups) {
					(var metaGroup, var appliesTo) = state.ReadMetadataGroup ((JObject)metaGroupObj);
					foreach (var itemName in appliesTo) {
						if (!Items.TryGetValue (itemName, out ItemInfo item)) {
							item = new ItemInfo (itemName, null);
							Items.Add (itemName, item);
						}
						metaGroup.ApplyToItem (item);
					}
				}
			}
			if (targets != null) {
				LoadTargets (targets, state);
			}

			loadErrors = (IList< MSBuildSchemaLoadError>)state.Errors ?? Array.Empty<MSBuildSchemaLoadError>();
		}

		public bool IsPrivate (string name)
		{
			// everything in a schema is public
			return false;
		}

		void LoadTargets (JObject items, SchemaLoadState state)
		{
			foreach (var kv in items) {
				var name = kv.Key;
				var desc = (string)((JValue)kv.Value).Value;
				Targets.Add (name, new TargetInfo (name, desc));
			}
		}

		void LoadIntelliSenseImports (JArray intelliSenseImports)
		{
			foreach (var import in intelliSenseImports) {
				string val = (string)((JValue)import).Value;
				if (!string.IsNullOrEmpty (val)) {
					IntelliSenseImports.Add (val);
				}
			}
		}

		IEnumerator<ISymbol> IEnumerable<ISymbol>.GetEnumerator ()
		{
			foreach (var item in Items.Values) {
				yield return item;
			}
			foreach (var prop in Properties.Values) {
				yield return prop;
			}
			foreach (var task in Tasks.Values) {
				yield return task;
			}
			foreach (var target in Targets.Values) {
				yield return target;
			}
		}

		IEnumerator IEnumerable.GetEnumerator () => ((IEnumerable<ISymbol>)this).GetEnumerator ();

		public void Add (ISymbol info)
		{
			switch (info) {
			case ItemInfo item:
				Items.Add (item.Name, item);
				break;
			case PropertyInfo prop:
				Properties.Add (prop.Name, prop);
				break;
			case TaskInfo task:
				Tasks.Add (task.Name, task);
				break;
			case TargetInfo target:
				Targets.Add (target.Name, target);
				break;
			default:
				throw new ArgumentException ($"Only items, properties, tasks and targets are allowed");
			}
		}
	}
}
