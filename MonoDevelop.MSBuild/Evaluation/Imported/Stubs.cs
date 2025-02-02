// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//
// Stubs for dependencies of imported classes to limit the amount of imported files

using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Build.FileSystem;
using NuGet.Frameworks;

using BF = System.Reflection.BindingFlags;

namespace Microsoft.NET.StringTools
{
	internal static class Strings
	{
		internal static string WeakIntern (string v) => v;
	}
}

namespace Microsoft.Build.Shared.FileSystem
{
	internal static class FileSystems
	{
		public static MSBuildFileSystemBase Default = GetDefaultFileSystem ();

		private static MSBuildFileSystemBase GetDefaultFileSystem () => new ConcreteMSBuildFileSystem ();
			/*
		{
			var asm = typeof (MSBuildFileSystemBase).Assembly.GetType ("Microsoft.Build.Shared.FileSystem.FileSystems");
			var prop = asm.GetField ("Default", BF.Static | BF.NonPublic | BF.Public);
			var defaultFilesystem = prop.GetValue (null);
			return new WrappedFileSystem (defaultFilesystem);
		}*/

		class ConcreteMSBuildFileSystem : MSBuildFileSystemBase{}

		// eventually this should get simpler once this commit is generally available (committed 2021/05/21)
		// as it makes MSBuildFileSystemBase nonabstract and routes the default impl to FileSystems.Default
		// https://github.com/dotnet/msbuild/commit/f4533349fb1b702fc2a4b9657d0e85ba3700282b
		sealed class WrappedFileSystem : MSBuildFileSystemBase
		{
			public WrappedFileSystem (object defaultFilesystem)
			{
			}

			public override bool DirectoryExists (string path)
			{
				throw new NotImplementedException ();
			}

			public override IEnumerable<string> EnumerateDirectories (string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
			{
				throw new NotImplementedException ();
			}

			public override IEnumerable<string> EnumerateFiles (string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
			{
				throw new NotImplementedException ();
			}

			public override IEnumerable<string> EnumerateFileSystemEntries (string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
			{
				throw new NotImplementedException ();
			}

			public override bool FileExists (string path)
			{
				throw new NotImplementedException ();
			}

			public override bool FileOrDirectoryExists (string path)
			{
				throw new NotImplementedException ();
			}

			public override FileAttributes GetAttributes (string path)
			{
				throw new NotImplementedException ();
			}

			public override Stream GetFileStream (string path, FileMode mode, FileAccess access, FileShare share)
			{
				throw new NotImplementedException ();
			}

			public override DateTime GetLastWriteTimeUtc (string path)
			{
				throw new NotImplementedException ();
			}

			public override TextReader ReadFile (string path)
			{
				throw new NotImplementedException ();
			}

			public override byte[] ReadFileAllBytes (string path)
			{
				throw new NotImplementedException ();
			}

			public override string ReadFileAllText (string path)
			{
				throw new NotImplementedException ();
			}
		}
	}
}

namespace Microsoft.Build.Evaluation
{
	class NuGetFrameworkWrapper
	{
		internal string GetTargetFrameworkIdentifier (string tfm)
			=> NuGetFramework.Parse (tfm).Framework;

		internal string GetTargetFrameworkVersion (string tfm, int minVersionPartCount)
			=> GetNonZeroVersionParts (NuGetFramework.Parse (tfm).Version, minVersionPartCount);

		internal bool IsCompatible (string target, string candidate)
			=> DefaultCompatibilityProvider.Instance.IsCompatible (
				NuGetFramework.Parse (target),
				NuGetFramework.Parse (candidate)
			);

		internal string GetTargetPlatformIdentifier (string tfm)
			=> NuGetFramework.Parse (tfm).Platform;

		internal string GetTargetPlatformVersion (string tfm, int minVersionPartCount)
			=> GetNonZeroVersionParts (NuGetFramework.Parse (tfm).PlatformVersion, minVersionPartCount);

		// from https://raw.githubusercontent.com/dotnet/msbuild/7434b575d12157ef98aeaad3b86c8f235f551c41/src/Build/Utilities/NuGetFrameworkWrapper.cs
		private string GetNonZeroVersionParts (Version version, int minVersionPartCount)
		{
			var nonZeroVersionParts = version.Revision == 0 ? version.Build == 0 ? version.Minor == 0 ? 1 : 2 : 3 : 4;
			return version.ToString (Math.Max (nonZeroVersionParts, minVersionPartCount));
		}
	}
}

namespace Microsoft.Build.Shared
{
	class BuildEnvironmentHelper
	{
		public static BuildEnvironmentHelper Instance { get; set; }
		public string MSBuildExtensionsPath { get; set; }
		public string VisualStudioInstallRootDirectory { get; set; }
		public string MSBuildToolsDirectory64 { get; set; }
		public string MSBuildSDKsPath { get; set; }
		public string MSBuildToolsDirectory32 { get; set; }
		public string CurrentMSBuildToolsDirectory { get; set; }
		public BuildEnvironmentMode Mode => BuildEnvironmentMode.None;
		public bool RunningInVisualStudio => false;
		public bool RunningTests => false;
	}

	class MSBuildConstants
	{
		public static char[] DirectorySeparatorChar = { System.IO.Path.DirectorySeparatorChar };
		public static char[] SpaceChar = { ' ' };

		public const string CurrentToolsVersion = "Current";
		public const string CurrentProductVersion = "17.0";
		public const string ToolsPath = "MSBuildToolsPath";
		public const string CurrentAssemblyVersion = "15.1.0.0";
	}

	class Traits
	{
		public static Traits Instance { get; } = new Traits ();
		public bool CacheFileExistence => false;
		public bool DebugEngine => false;
		internal EscapeHatches EscapeHatches { get; } = new EscapeHatches ();

	}
	class EscapeHatches
	{
		public bool AlwaysUseContentTimestamp => false;
		public bool DisableLongPaths => false;
		public bool UseSymlinkTimeInsteadOfTargetTime => false;
	}

	class FrameworkLocationHelper
	{
		internal static string programFiles32 = Environment.GetFolderPath (Environment.SpecialFolder.ProgramFilesX86);
	}

	enum BuildEnvironmentMode
	{
		None,
		VisualStudio
	}

	/*
		class NativeMethodsShared
		{
			bool? isWindows;

			public bool IsWindows => isWindows ?? (bool)(isWindows = RuntimeInformation.IsOSPlatform (OSPlatform.Windows));
		}
	*/

	class ChangeWaves
	{
		public static readonly Version Wave17_0 = new Version (17, 0);

		internal static bool AreFeaturesEnabled (Version wave)
		{
			return true;
		}
	}
}