// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MonoDevelop.MSBuild.Analysis;
using MonoDevelop.MSBuild.Language.Expressions;
using MonoDevelop.MSBuild.Schema;

namespace MonoDevelop.MSBuild.Language
{
	class CoreDiagnostics
	{
		public const string EmptySdkAttribute_Id = nameof(EmptySdkAttribute);
		public static readonly MSBuildDiagnosticDescriptor EmptySdkAttribute = new (
			EmptySdkAttribute_Id,
			"Empty SDK attribute",
			null,
			MSBuildDiagnosticSeverity.Error
		);

		public const string InvalidSdkAttribute_Id = nameof(InvalidSdkAttribute);
		public static readonly MSBuildDiagnosticDescriptor InvalidSdkAttribute = new (
			InvalidSdkAttribute_Id,
			"Invalid SDK attribute",
			"The SDK attribute '{0}' has an invalid format",
			MSBuildDiagnosticSeverity.Error
		);

		public const string SdkNotFound_Id = nameof(SdkNotFound);
		public static readonly MSBuildDiagnosticDescriptor SdkNotFound = new (
			SdkNotFound_Id,
			"SDK not found",
			"The SDK '{0}' was not found",
			MSBuildDiagnosticSeverity.Error
		);

		public const string InternalError_Id = nameof(InternalError);
		public static readonly MSBuildDiagnosticDescriptor InternalError = new (
			InternalError_Id,
			"Internal error",
			"An internal error occurred: {0}",
			MSBuildDiagnosticSeverity.Error
		);

		public const string UnresolvedImportConditioned_Id = nameof (UnresolvedImportConditioned);
		public static readonly MSBuildDiagnosticDescriptor UnresolvedImportConditioned = new (
			UnresolvedImportConditioned_Id,
			"Could not resolve conditioned import",
			"The conditioned import '{0}' could not be resolved",
			MSBuildDiagnosticSeverity.Warning
		);

		public const string UnresolvedImport_Id = nameof (UnresolvedImport);
		public static readonly MSBuildDiagnosticDescriptor UnresolvedImport = new (
			UnresolvedImport_Id,
			"Could not resolve import",
			"The import '{0}' could not be resolved",
			MSBuildDiagnosticSeverity.Error
		);

		public const string UnresolvedSdkImportConditioned_Id = nameof (UnresolvedSdkImportConditioned);
		public static readonly MSBuildDiagnosticDescriptor UnresolvedSdkImportConditioned = new (
			UnresolvedSdkImportConditioned_Id,
			"Could not resolve conditioned SDK import",
			"The conditioned import '{0}' could not be resolved from SDK path '{1}'",
			MSBuildDiagnosticSeverity.Warning
		);

		public const string UnresolvedSdkImport_Id = nameof (UnresolvedSdkImport);
		public static readonly MSBuildDiagnosticDescriptor UnresolvedSdkImport = new (
			UnresolvedSdkImport_Id,
			"Could not resolve SDK import",
			"The import '{0}' could not be resolved from SDK path '{1}'",
			MSBuildDiagnosticSeverity.Error
		);

		public const string UnresolvedSdk_Id = nameof(UnresolvedSdk);
		public static readonly MSBuildDiagnosticDescriptor UnresolvedSdk = new (
			UnresolvedSdk_Id,
			"Could not resolve SDK",
			"The SDK '{0}' could not be resolved",
			MSBuildDiagnosticSeverity.Warning
		);

		public const string UnknownElement_Id = nameof(UnknownElement);
		public static readonly MSBuildDiagnosticDescriptor UnknownElement = new (
			UnknownElement_Id,
			"Unknown element",
			"The element '{0}' is not valid at this location",
			MSBuildDiagnosticSeverity.Error
		);

		public const string UnknownAttribute_Id = nameof(UnknownAttribute);
		public static readonly MSBuildDiagnosticDescriptor UnknownAttribute = new (
			UnknownAttribute_Id,
			"Unknown attribute",
			"The attribute '{0}' is not valid at this location",
			MSBuildDiagnosticSeverity.Error
		);

		public const string MissingRequiredAttribute_Id = nameof(MissingRequiredAttribute);
		public static readonly MSBuildDiagnosticDescriptor MissingRequiredAttribute = new (
			MissingRequiredAttribute_Id,
			"Missing required attribute",
			"The element '{0}' is missing the required attribute '{1}'",
			MSBuildDiagnosticSeverity.Error
		);

		public const string UnexpectedText_Id = nameof(UnexpectedText);
		public static readonly MSBuildDiagnosticDescriptor UnexpectedText = new (
			UnexpectedText_Id,
			"Unexpected text content",
			"The element '{0}' does not permit text content",
			MSBuildDiagnosticSeverity.Error
		);

		public const string RequiredAttributeEmpty_Id = nameof(RequiredAttributeEmpty);
		public static readonly MSBuildDiagnosticDescriptor RequiredAttributeEmpty = new (
			RequiredAttributeEmpty_Id,
			"Empty required attribute",
			"The required attribute '{0}' has an empty value",
			MSBuildDiagnosticSeverity.Error
		);

		public const string AttributeEmpty_Id = nameof(AttributeEmpty);
		public static readonly MSBuildDiagnosticDescriptor AttributeEmpty = new (
			AttributeEmpty_Id,
			"Empty attribute",
			"The attribute '{0}' has an empty value",
			MSBuildDiagnosticSeverity.Warning
		);

		public const string Deprecated_Id = nameof(Deprecated);
		public static readonly MSBuildDiagnosticDescriptor Deprecated = new (
			Deprecated_Id,
			"Deprecated {0}",
			"The {0} '{1}' is deprecated",
			MSBuildDiagnosticSeverity.Warning
		);

		public const string DeprecatedWithMessage_Id = nameof(DeprecatedWithMessage);
		public static readonly MSBuildDiagnosticDescriptor DeprecatedWithMessage = new (
			DeprecatedWithMessage_Id,
			"Deprecated {0}",
			"The {0} '{1}' is deprecated: {2}",
			MSBuildDiagnosticSeverity.Warning
		);

		public const string TaskNotDefined_Id = nameof(TaskNotDefined);
		public static readonly MSBuildDiagnosticDescriptor TaskNotDefined = new (
			TaskNotDefined_Id,
			"Task not defined",
			"The task '{0}' is not defined",
			MSBuildDiagnosticSeverity.Error
		);

		public const string UnknownTaskParameter_Id = nameof(UnknownTaskParameter);
		public static readonly MSBuildDiagnosticDescriptor UnknownTaskParameter = new (
			UnknownTaskParameter_Id,
			"Unknown task parameter",
			"Unknown parameter '{1}' on task '{1}'",
			MSBuildDiagnosticSeverity.Error
		);

		public const string EmptyRequiredTaskParameter_Id = nameof(EmptyRequiredTaskParameter);
		public static readonly MSBuildDiagnosticDescriptor EmptyRequiredTaskParameter = new (
			EmptyRequiredTaskParameter_Id,
			"Empty required task parameter",
			"Required parameter '{1}' on task '{1}' is empty",
			MSBuildDiagnosticSeverity.Error
		);

		public const string MissingRequiredTaskParameter_Id = nameof(MissingRequiredTaskParameter);
		public static readonly MSBuildDiagnosticDescriptor MissingRequiredTaskParameter = new (
			MissingRequiredTaskParameter_Id,
			"Missing task parameter",
			"Task '{0}' is missing required parameter '{1}'",
			MSBuildDiagnosticSeverity.Error
		);

		public const string NonOutputTaskParameter_Id = nameof(NonOutputTaskParameter);
		public static readonly MSBuildDiagnosticDescriptor NonOutputTaskParameter = new (
			NonOutputTaskParameter_Id,
			"Incorrect parameter usage",
			"Parameter '{1}' on task '{0}' cannot be used an an output parameter",
			MSBuildDiagnosticSeverity.Error
		);

		public const string NoTargets_Id = nameof(NoTargets);
		public static readonly MSBuildDiagnosticDescriptor NoTargets = new (
			NoTargets_Id,
			"No targets in project",
			"Project does not define or import any targets",
			MSBuildDiagnosticSeverity.Error
		);

		public const string OtherwiseMustBeLastInChoose_Id = nameof(OtherwiseMustBeLastInChoose);
		public static readonly MSBuildDiagnosticDescriptor OtherwiseMustBeLastInChoose = new (
			OtherwiseMustBeLastInChoose_Id,
			"Otherwise must be last choice",
			"'Otherwise' must be the last choice in a 'Choose'",
			MSBuildDiagnosticSeverity.Error
		);

		public const string OnErrorMustBeLastInTarget_Id = nameof(OnErrorMustBeLastInTarget);
		public static readonly MSBuildDiagnosticDescriptor OnErrorMustBeLastInTarget = new (
			OnErrorMustBeLastInTarget_Id,
			"OnError must be last element in target",
			"In a target, OnError may only be followed by other OnError elements",
			MSBuildDiagnosticSeverity.Error
		);

		public const string OutputMustHavePropertyOrItemName_Id = nameof(OutputMustHavePropertyOrItemName);
		public static readonly MSBuildDiagnosticDescriptor OutputMustHavePropertyOrItemName = new (
			OutputMustHavePropertyOrItemName_Id,
			"Output must have PropertyName or TaskName",
			"Task Output element must specify a PropertyName or a TaskName",
			MSBuildDiagnosticSeverity.Error
		);

		public const string UsingTaskMustHaveAssembly_Id = nameof(UsingTaskMustHaveAssembly);
		public static readonly MSBuildDiagnosticDescriptor UsingTaskMustHaveAssembly = new (
			UsingTaskMustHaveAssembly_Id,
			"UsingTask requires assembly",
			"UsingTask must have AssemblyName or AssemblyFile attribute",
			MSBuildDiagnosticSeverity.Error
		);

		public const string TaskFactoryCannotHaveAssemblyName_Id = nameof(TaskFactoryCannotHaveAssemblyName);
		public static readonly MSBuildDiagnosticDescriptor TaskFactoryCannotHaveAssemblyName = new (
			TaskFactoryCannotHaveAssemblyName_Id,
			"TaskFactory cannot have AssemblyName",
			"TaskFactory is not compatible with AssemblyName attribute on UsingTask. Use AssemblyFile instead.",
			MSBuildDiagnosticSeverity.Error
		);

		public const string TaskFactoryMustHaveAssemblyFile_Id = nameof(TaskFactoryMustHaveAssemblyFile);
		public static readonly MSBuildDiagnosticDescriptor TaskFactoryMustHaveAssemblyFile = new (
			TaskFactoryMustHaveAssemblyFile_Id,
			"TaskFactory must have AssemblyFile",
			"TaskFactory requires AssemblyFile attribute on UsingTask",
			MSBuildDiagnosticSeverity.Error
		);

		public const string TaskFactoryMustHaveOneAssemblyOnly_Id = nameof(TaskFactoryMustHaveOneAssemblyOnly);
		public static readonly MSBuildDiagnosticDescriptor TaskFactoryMustHaveOneAssemblyOnly = new (
			TaskFactoryMustHaveOneAssemblyOnly_Id,
			"UsingTask can only have one assembly",
			"UsingTask may not have both AssemblyName and AssemblyFile attributes",
			MSBuildDiagnosticSeverity.Error
		);

		public const string OneParameterGroup_Id = nameof(OneParameterGroup);
		public static readonly MSBuildDiagnosticDescriptor OneParameterGroup = new (
			OneParameterGroup_Id,
			"One ParameterGroup per UsingTask",
			"Each UsingTask may only have a single ParameterGroup",
			MSBuildDiagnosticSeverity.Error
		);

		public const string OneTaskBody_Id = nameof(OneTaskBody);
		public static readonly MSBuildDiagnosticDescriptor OneTaskBody = new (
			OneTaskBody_Id,
			"One Task body per UsingTask",
			"Each UsingTask may only have one Task element",
			MSBuildDiagnosticSeverity.Error
		);

		public const string TaskBodyMustHaveFactory_Id = nameof(TaskBodyMustHaveFactory);
		public static readonly MSBuildDiagnosticDescriptor TaskBodyMustHaveFactory = new (
			TaskBodyMustHaveFactory_Id,
			"Task body must have factory",
			"UsingTask without TaskFactory attribute cannot have Task element",
			MSBuildDiagnosticSeverity.Error
		);

		public const string ParameterGroupMustHaveFactory_Id = nameof(ParameterGroupMustHaveFactory);
		public static readonly MSBuildDiagnosticDescriptor ParameterGroupMustHaveFactory = new (
			ParameterGroupMustHaveFactory_Id,
			"ParameterGroup must have factory",
			"UsingTask without TaskFactory attribute cannot have ParameterGroup element",
			MSBuildDiagnosticSeverity.Error
		);

		public const string TaskFactoryMustHaveBody_Id = nameof(TaskFactoryMustHaveBody);
		public static readonly MSBuildDiagnosticDescriptor TaskFactoryMustHaveBody = new (
			TaskFactoryMustHaveBody_Id,
			"TaskFactory must have body",
			"UsingTask with TaskFactory attribute must have Task element",
			MSBuildDiagnosticSeverity.Error
		);

		public const string UnknownTaskFactory_Id = nameof(UnknownTaskFactory);
		public static readonly MSBuildDiagnosticDescriptor UnknownTaskFactory = new (
			UnknownTaskFactory_Id,
			"Unknown task factory",
			"The task factory '{0}' is not known",
			MSBuildDiagnosticSeverity.Warning
		);

		public const string EmptyTaskFactory_Id = nameof(EmptyTaskFactory);
		public static readonly MSBuildDiagnosticDescriptor EmptyTaskFactory = new (
			EmptyTaskFactory_Id,
			"Empty task factory",
			"TaskFactory attribute is empty",
			MSBuildDiagnosticSeverity.Error
		);

		public const string RoslynCodeTaskFactoryRequiresCodeElement_Id = nameof(RoslynCodeTaskFactoryRequiresCodeElement);
		public static readonly MSBuildDiagnosticDescriptor RoslynCodeTaskFactoryRequiresCodeElement = new (
			RoslynCodeTaskFactoryRequiresCodeElement_Id,
			"RoslynCodeTaskFactory requires Code element",
			"RoslynCodeTaskFactory requires Code element in Task body",
			MSBuildDiagnosticSeverity.Error
		);

		public const string RoslynCodeTaskFactoryWithClassIgnoresParameterGroup_Id = nameof(RoslynCodeTaskFactoryWithClassIgnoresParameterGroup);
		public static readonly MSBuildDiagnosticDescriptor RoslynCodeTaskFactoryWithClassIgnoresParameterGroup = new (
			RoslynCodeTaskFactoryWithClassIgnoresParameterGroup_Id,
			"Empty task factory",
			"TaskFactory attribute is empty",
			MSBuildDiagnosticSeverity.Error);

		public const string UnexpectedList_Id = nameof(UnexpectedList);
		public static readonly MSBuildDiagnosticDescriptor UnexpectedList = new (
			UnexpectedList_Id,
			"Unexpected list in value",
			"The {0} '{1}' does not permit lists",
			MSBuildDiagnosticSeverity.Warning);

		public const string UnexpectedExpression_Id = nameof(UnexpectedExpression);
		public static readonly MSBuildDiagnosticDescriptor UnexpectedExpression = new (
			UnexpectedExpression_Id,
			"Unexpected expression in value",
			"The {0} '{1}' does not permit expressions",
			MSBuildDiagnosticSeverity.Warning);

		public const string ImportVersionRequiresSdk_Id = nameof(ImportVersionRequiresSdk);
		public static readonly MSBuildDiagnosticDescriptor ImportVersionRequiresSdk = new (
			ImportVersionRequiresSdk_Id,
			"Import Version requires Sdk",
			"Import may only have a Version attribute if it has an Sdk attribute",
			MSBuildDiagnosticSeverity.Error);

		public const string ImportMinVersionRequiresSdk_Id = nameof(ImportMinVersionRequiresSdk);
		public static readonly MSBuildDiagnosticDescriptor ImportMinVersionRequiresSdk = new (
			ImportMinVersionRequiresSdk_Id,
			"Import MinVersion requires Sdk",
			"Import may only have a MinVersion attribute if it has an Sdk attribute",
			MSBuildDiagnosticSeverity.Warning);

		public const string UnknownValue_Id = nameof(UnknownValue);
		public static readonly MSBuildDiagnosticDescriptor UnknownValue = new (
			UnknownValue_Id,
			"{1} has unknown value",
			"{0} '{1}' has unknown value '{2}'",
			MSBuildDiagnosticSeverity.Error);

		public const string HasDefaultValue_Id = nameof(HasDefaultValue);
		public static readonly MSBuildDiagnosticDescriptor HasDefaultValue = new (
			HasDefaultValue_Id,
			"{1} has default value",
			"{0} '{1}' has default value '{2}'",
			MSBuildDiagnosticSeverity.Warning);

		public const string InvalidGuid_Id = nameof(InvalidGuid);
		public static readonly MSBuildDiagnosticDescriptor InvalidGuid = new (
			InvalidGuid_Id,
			"Invalid GUID format",
			"The value '{0}' is not a valid GUID format",
			MSBuildDiagnosticSeverity.Error);

		public const string InvalidInteger_Id = nameof(InvalidInteger);
		public static readonly MSBuildDiagnosticDescriptor InvalidInteger = new (
			InvalidInteger_Id,
			"Invalid integer",
			"The value '{0}' is not a valid integer",
			MSBuildDiagnosticSeverity.Error);

		public const string InvalidBool_Id = nameof(InvalidBool);
		public static readonly MSBuildDiagnosticDescriptor InvalidBool = new (
			InvalidBool_Id,
			"Invalid bool",
			"The value '{0}' is not a valid bool",
			MSBuildDiagnosticSeverity.Error);

		public const string InvalidLcid_Id = nameof(InvalidLcid);
		public static readonly MSBuildDiagnosticDescriptor InvalidLcid = new (
			InvalidLcid_Id,
			"Invalid LCID",
			"The value '{0}' is not a valid LCID integer",
			MSBuildDiagnosticSeverity.Error);

		public const string UnknownLcid_Id = nameof(UnknownLcid);
		public static readonly MSBuildDiagnosticDescriptor UnknownLcid = new (
			UnknownLcid_Id,
			"Unknown LCID",
			"The value '{0}' is not a known LCID",
			MSBuildDiagnosticSeverity.Warning);

		public const string InvalidCulture_Id = nameof(InvalidCulture);
		public static readonly MSBuildDiagnosticDescriptor InvalidCulture = new (
			InvalidCulture_Id,
			"Invalid culture name",
			"The value '{0}' is not a valid culture name",
			MSBuildDiagnosticSeverity.Error);

		public const string UnknownCulture_Id = nameof(UnknownCulture);
		public static readonly MSBuildDiagnosticDescriptor UnknownCulture = new (
			UnknownCulture_Id,
			"Unknown culture name",
			"The value '{0}' is not a known culture name",
			MSBuildDiagnosticSeverity.Warning);

		public const string InvalidUrl_Id = nameof(InvalidUrl);
		public static readonly MSBuildDiagnosticDescriptor InvalidUrl = new (
			InvalidUrl_Id,
			"Invalid URL",
			"The value '{0}' is not a valid URL",
			MSBuildDiagnosticSeverity.Error);

		public const string InvalidVersion_Id = nameof(InvalidVersion);
		public static readonly MSBuildDiagnosticDescriptor InvalidVersion = new (
			InvalidVersion_Id,
			"Invalid version format",
			"The value '{0}' is not a valid version format",
			MSBuildDiagnosticSeverity.Error);

		public const string UnknownTargetFramework_Id = nameof(UnknownTargetFramework);
		public static readonly MSBuildDiagnosticDescriptor UnknownTargetFramework = new (
			UnknownTargetFramework_Id,
			"Unknown target framework",
			"The value '{0}' is not a known target framework short name",
			MSBuildDiagnosticSeverity.Warning);

		public const string UnknownTargetFrameworkIdentifier_Id = nameof(UnknownTargetFrameworkIdentifier);
		public static readonly MSBuildDiagnosticDescriptor UnknownTargetFrameworkIdentifier = new (
			UnknownTargetFrameworkIdentifier_Id,
			"Unknown target framework identifier",
			"The value '{0}' is not a known target framework identifier",
			MSBuildDiagnosticSeverity.Warning);

		public const string UnknownTargetFrameworkVersion_Id = nameof(UnknownTargetFrameworkVersion);
		public static readonly MSBuildDiagnosticDescriptor UnknownTargetFrameworkVersion = new (
			UnknownTargetFrameworkVersion_Id,
			"Unknown target framework version",
			"The value '{0}' is not a known version for target framework '{1}'",
			MSBuildDiagnosticSeverity.Warning);

		public const string UnknownTargetFrameworkProfile_Id = nameof(UnknownTargetFrameworkProfile);
		public static readonly MSBuildDiagnosticDescriptor UnknownTargetFrameworkProfile = new (
			UnknownTargetFrameworkProfile_Id,
			"Unknown target framework profile",
			"The value '{0}' is not a known profile for target framework '{1},Version={2}'",
			MSBuildDiagnosticSeverity.Warning);

		public const string ItemAttributeNotValidInTarget_Id = nameof(ItemAttributeNotValidInTarget);
		public static readonly MSBuildDiagnosticDescriptor ItemAttributeNotValidInTarget = new (
			ItemAttributeNotValidInTarget_Id,
			"{0} not valid in targets",
			"The item attribute '{0}' is not valid in a target",
			MSBuildDiagnosticSeverity.Error);

		public const string ItemAttributeOnlyValidInTarget_Id = nameof(ItemAttributeOnlyValidInTarget);
		public static readonly MSBuildDiagnosticDescriptor ItemAttributeOnlyValidInTarget = new (
			ItemAttributeOnlyValidInTarget_Id,
			"{0} not valid outside targets",
			"The item attribute '{0}' is not valid outside targets",
			MSBuildDiagnosticSeverity.Error);

		public const string ItemMustHaveInclude_Id = nameof(ItemMustHaveInclude);
		public static readonly MSBuildDiagnosticDescriptor ItemMustHaveInclude = new (
			ItemMustHaveInclude_Id,
			"Item has no Include, Update or Remove attribute",
			"Items outside targets must have Include, Update or Remove attribute",
			MSBuildDiagnosticSeverity.Error);

		public static (MSBuildDiagnosticDescriptor, object[]) GetExpressionError (ExpressionError error, ITypedSymbol info)
		{
			(MSBuildDiagnosticDescriptor, object[]) Return (MSBuildDiagnosticDescriptor desc, params object[] args) => (desc, args);
			return error.Kind switch
			{
				ExpressionErrorKind.MetadataDisallowed => Return (MetadataDisallowed, DescriptionFormatter.GetKindNoun (info), info.Name),
				ExpressionErrorKind.EmptyListEntry => Return (EmptyListValue),
				ExpressionErrorKind.ExpectingItemName => Return (ExpectingItemName),
				ExpressionErrorKind.ExpectingRightParen => Return (ExpectingChar, ')'),
				ExpressionErrorKind.ExpectingRightParenOrPeriod => Return (ExpectingCharOrChar, ')', '.'),
				ExpressionErrorKind.ExpectingPropertyName => Return (ExpectingPropertyName),
				ExpressionErrorKind.ExpectingMetadataName => Return (ExpectingMetadataName),
				ExpressionErrorKind.ExpectingMetadataOrItemName => Return (ExpectingMetadataOrItemName),
				ExpressionErrorKind.ExpectingRightAngleBracket => Return (ExpectingChar, '>'),
				ExpressionErrorKind.ExpectingRightParenOrDash => Return (ExpectingCharOrChar, ')', '-'),
				ExpressionErrorKind.ItemsDisallowed => Return (ItemsDisallowed, DescriptionFormatter.GetKindNoun (info), info.Name),
				ExpressionErrorKind.ExpectingMethodOrTransform => Return (ExpectingFunctionOrTransform),
				ExpressionErrorKind.ExpectingMethodName => Return (ExpectingFunctionName),
				ExpressionErrorKind.ExpectingLeftParen => Return (ExpectingChar, '('),
				ExpressionErrorKind.ExpectingRightParenOrComma => Return (ExpectingCharOrChar, ')', ','),
				ExpressionErrorKind.ExpectingRightParenOrValue => Return (ExpectingRightParenOrValue),
				ExpressionErrorKind.ExpectingValue => Return (ExpectingValue),
				ExpressionErrorKind.CouldNotParseNumber => Return (CouldNotParseNumber),
				ExpressionErrorKind.IncompleteValue => Return (IncompleteValue),
				ExpressionErrorKind.ExpectingBracketColonColon => Return (ExpectingChar, "]::"),
				ExpressionErrorKind.ExpectingClassName => Return (ExpectingClassName),
				ExpressionErrorKind.ExpectingClassNameComponent => Return (IncompleteClassName),
				ExpressionErrorKind.IncompleteString => Return (IncompleteString),
				ExpressionErrorKind.IncompleteProperty => Return (IncompleteProperty),
				_ => throw new System.Exception ($"Unhandled ExpressionErrorKind '{error.Kind}'")
			};
		}

		public const string MetadataDisallowed_Id = nameof(MetadataDisallowed);
		public static readonly MSBuildDiagnosticDescriptor MetadataDisallowed = new (
			MetadataDisallowed_Id,
			"Metadata not permitted",
			"{0} '{1}' does not permit metadata",
			MSBuildDiagnosticSeverity.Error);

		public const string ItemsDisallowed_Id = nameof(ItemsDisallowed);
		public static readonly MSBuildDiagnosticDescriptor ItemsDisallowed = new (
			ItemsDisallowed_Id,
			"Items not permitted",
			"{0} '{1}' does not permit items",
			MSBuildDiagnosticSeverity.Error);

		public const string EmptyListValue_Id = nameof(EmptyListValue);
		public static readonly MSBuildDiagnosticDescriptor EmptyListValue = new (
			EmptyListValue_Id,
			"Empty list value",
			MSBuildDiagnosticSeverity.Error);

		public const string ExpectingItemName_Id = nameof(ExpectingItemName);
		public static readonly MSBuildDiagnosticDescriptor ExpectingItemName = new (
			ExpectingItemName_Id,
			"Expecting item name",
			MSBuildDiagnosticSeverity.Error);

		public const string ExpectingChar_Id = nameof(ExpectingChar);
		public static readonly MSBuildDiagnosticDescriptor ExpectingChar = new (
			ExpectingChar_Id,
			"Expecting '{0}'",
			MSBuildDiagnosticSeverity.Error);

		public const string ExpectingCharOrChar_Id = nameof(ExpectingCharOrChar);
		public static readonly MSBuildDiagnosticDescriptor ExpectingCharOrChar = new (
			ExpectingCharOrChar_Id,
			"Expecting '{0}' or {1}",
			MSBuildDiagnosticSeverity.Error);

		public static readonly MSBuildDiagnosticDescriptor ExpectingPropertyName= new (
			nameof (ExpectingPropertyName),
			"Expecting property name",
			MSBuildDiagnosticSeverity.Error);

		public const string ExpectingMetadataName_Id = nameof(ExpectingMetadataName);
		public static readonly MSBuildDiagnosticDescriptor ExpectingMetadataName = new (
			ExpectingMetadataName_Id,
			"Expecting metadata name",
			MSBuildDiagnosticSeverity.Error);

		public const string ExpectingMetadataOrItemName_Id = nameof(ExpectingMetadataOrItemName);
		public static readonly MSBuildDiagnosticDescriptor ExpectingMetadataOrItemName = new (
			ExpectingMetadataOrItemName_Id,
			"Expecting metadata or item name",
			MSBuildDiagnosticSeverity.Error);

		public const string ExpectingFunctionName_Id = nameof(ExpectingFunctionName);
		public static readonly MSBuildDiagnosticDescriptor ExpectingFunctionName = new (
			ExpectingFunctionName_Id,
			"Expecting function name",
			MSBuildDiagnosticSeverity.Error);

		public const string ExpectingValue_Id = nameof(ExpectingValue);
		public static readonly MSBuildDiagnosticDescriptor ExpectingValue = new (
			ExpectingValue_Id,
			"Expecting value",
			MSBuildDiagnosticSeverity.Error);

		public const string ExpectingFunctionOrTransform_Id = nameof(ExpectingFunctionOrTransform);
		public static readonly MSBuildDiagnosticDescriptor ExpectingFunctionOrTransform = new (
			ExpectingFunctionOrTransform_Id,
			"Expecting item function or transform",
			MSBuildDiagnosticSeverity.Error);

		public const string ExpectingClassName_Id = nameof(ExpectingClassName);
		public static readonly MSBuildDiagnosticDescriptor ExpectingClassName = new (
			ExpectingClassName_Id,
			"Expecting class name",
			MSBuildDiagnosticSeverity.Error);

		public const string IncompleteClassName_Id = nameof(IncompleteClassName);
		public static readonly MSBuildDiagnosticDescriptor IncompleteClassName = new (
			IncompleteClassName_Id,
			"Incomplete class name",
			MSBuildDiagnosticSeverity.Error);

		public const string IncompleteString_Id = nameof(IncompleteString);
		public static readonly MSBuildDiagnosticDescriptor IncompleteString = new (
			IncompleteString_Id,
			"Incomplete string",
			MSBuildDiagnosticSeverity.Error);

		public const string IncompleteValue_Id = nameof(IncompleteValue);
		public static readonly MSBuildDiagnosticDescriptor IncompleteValue = new (
			IncompleteValue_Id,
			"Incomplete value",
			MSBuildDiagnosticSeverity.Error);

		public const string IncompleteProperty_Id = nameof(IncompleteProperty);
		public static readonly MSBuildDiagnosticDescriptor IncompleteProperty = new (
			IncompleteProperty_Id,
			"Incomplete property",
			MSBuildDiagnosticSeverity.Error);

		public const string CouldNotParseNumber_Id = nameof(CouldNotParseNumber);
		public static readonly MSBuildDiagnosticDescriptor CouldNotParseNumber = new (
			CouldNotParseNumber_Id,
			"Invalid number format",
			MSBuildDiagnosticSeverity.Error);

		public const string ExpectingRightParenOrValue_Id = nameof(ExpectingRightParenOrValue);
		public static readonly MSBuildDiagnosticDescriptor ExpectingRightParenOrValue = new (
			ExpectingRightParenOrValue_Id,
			"Expecting ')' or value",
			MSBuildDiagnosticSeverity.Error);

		public const string UnwrittenItem_Id = nameof(UnwrittenItem);
		public static readonly MSBuildDiagnosticDescriptor UnwrittenItem = new (
			UnwrittenItem_Id,
			"Possible unused or misspelled item",
			"The item '{0}' does not have a value assigned and is not referenced in any imported targets or schemas",
			MSBuildDiagnosticSeverity.Warning);

		public const string UnwrittenProperty_Id = nameof(UnwrittenProperty);
		public static readonly MSBuildDiagnosticDescriptor UnwrittenProperty = new (
			UnwrittenProperty_Id,
			"Possible unused or misspelled property",
			"The property '{0}' does not have a value assigned and is not referenced in any imported targets or schemas",
			MSBuildDiagnosticSeverity.Warning);

		public const string UnwrittenMetadata_Id = nameof(UnwrittenMetadata);
		public static readonly MSBuildDiagnosticDescriptor UnwrittenMetadata = new (
			UnwrittenMetadata_Id,
			"Possible unused or misspelled metadata",
			"The metadata '{0}.{1}' does not have a value assigned and is not referenced in any imported targets or schemas",
			MSBuildDiagnosticSeverity.Warning);

		public const string UnreadItem_Id = nameof(UnreadItem);
		public static readonly MSBuildDiagnosticDescriptor UnreadItem = new (
			UnreadItem_Id,
			"Possible unused or misspelled item",
			"The item '{0}' is not used in this file and is not referenced in any imported targets or schemas",
			MSBuildDiagnosticSeverity.Warning);

		public const string UnreadProperty_Id = nameof(UnreadProperty);
		public static readonly MSBuildDiagnosticDescriptor UnreadProperty = new (
			UnreadProperty_Id,
			"Possible unused or misspelled property",
			"The property '{0}' is not used in this file and is not referenced in any imported targets or schemas",
			MSBuildDiagnosticSeverity.Warning);

		public const string UnreadMetadata_Id = nameof(UnreadMetadata);
		public static readonly MSBuildDiagnosticDescriptor UnreadMetadata = new (
			UnreadMetadata_Id,
			"Possible unused or misspelled metadata",
			"The metadata '{0}.{1}' is not used in this file and is not referenced in any imported targets or schemas",
			MSBuildDiagnosticSeverity.Warning);
	}
}
