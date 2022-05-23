// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using AWS.Deploy.Common;
using AWS.Deploy.Common.IO;

namespace AWS.Deploy.Orchestration.Utilities
{
    public interface IZipFileManager
    {
        Task CreateFromDirectory(string sourceDirectoryName, string destinationArchiveFileName);
    }

    public class ZipFileManager : IZipFileManager
    {
        private readonly ICommandLineWrapper _commandLineWrapper;
        private readonly IFileManager _fileManager;

        public ZipFileManager(ICommandLineWrapper commandLineWrapper, IFileManager fileManager)
        {
            _commandLineWrapper = commandLineWrapper;
            _fileManager = fileManager;
        }

        public async Task CreateFromDirectory(string sourceDirectoryName, string destinationArchiveFileName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ZipFile.CreateFromDirectory(sourceDirectoryName, destinationArchiveFileName);
            }
            else
            {
                await BuildZipForLinux(sourceDirectoryName, destinationArchiveFileName);
            }
        }

        /// <summary>
        /// Use the native zip utility if it exist which will maintain linux/osx file permissions.
        /// </summary>
        private async Task BuildZipForLinux(string sourceDirectoryName, string destinationArchiveFileName)
        {
            var zipCLI = FindExecutableInPath("zip");

            if (string.IsNullOrEmpty(zipCLI))
                throw new FailedToCreateZipFileException(DeployToolErrorCode.FailedToFindZipUtility, "Failed to find the \"zip\" utility program in path. This program is required to maintain Linux file permissions in the zip archive.", null);

            var args = new StringBuilder($"\"{destinationArchiveFileName}\"");

            var allFiles = GetFilesToIncludeInArchive(sourceDirectoryName);
            foreach (var kvp in allFiles)
            {
                args.AppendFormat(" \"{0}\"", kvp.Key);
            }

            var command = $"{zipCLI} {args}";
            var result = await _commandLineWrapper.TryRunWithResult(command, sourceDirectoryName);
            if (result.ExitCode != 0)
            {
                var errorMessage = "We were unable to create a zip archive of the packaged application.";
                if (!string.IsNullOrEmpty(result.StandardError))
                    errorMessage = $"We were unable to create a zip archive of the packaged application due to the following reason:{Environment.NewLine}{result.StandardError}";

                errorMessage += $"{Environment.NewLine}Normally this indicates a problem running the \"zip\" utility. Make sure that application is installed and available in your PATH.";
                throw new FailedToCreateZipFileException(DeployToolErrorCode.ZipUtilityFailedToZip, errorMessage, result.ExitCode);
            }
        }

        /// <summary>
        /// Get the list of files from the publish folder that should be added to the zip archive.
        /// This will skip all files in the runtimes folder because they have already been flatten to the root.
        /// </summary>
        private IDictionary<string, string> GetFilesToIncludeInArchive(string publishLocation)
        {
            var includedFiles = new Dictionary<string, string>();
            var allFiles = Directory.GetFiles(publishLocation, "*.*", SearchOption.AllDirectories);
            foreach (var file in allFiles)
            {
                var relativePath = Path.GetRelativePath(publishLocation, file);

                includedFiles[relativePath] = file;
            }

            return includedFiles;
        }

        /// <summary>
        /// A collection of known paths for common utilities that are usually not found in the path
        /// </summary>
        private readonly IDictionary<string, string> KNOWN_LOCATIONS = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"zip", @"/usr/bin/zip" }
        };

        /// <summary>
        /// Search the path environment variable for the command given.
        /// </summary>
        /// <param name="command">The command to search for in the path</param>
        /// <returns>The full path to the command if found otherwise it will return null</returns>
        private string? FindExecutableInPath(string command)
        {
            if (_fileManager.Exists(command))
                return Path.GetFullPath(command);

            Func<string, string> quoteRemover = x =>
            {
                if (x.StartsWith("\""))
                    x = x.Substring(1);
                if (x.EndsWith("\""))
                    x = x.Substring(0, x.Length - 1);
                return x;
            };

            var envPath = Environment.GetEnvironmentVariable("PATH") ?? "";
            foreach (var path in envPath.Split(Path.PathSeparator))
            {
                try
                {
                    var fullPath = Path.Combine(quoteRemover(path), command);
                    if (_fileManager.Exists(fullPath))
                        return fullPath;
                }
                catch (Exception)
                {
                    // Catch exceptions and continue if there are invalid characters in the user's path.
                }
            }

            if (KNOWN_LOCATIONS.ContainsKey(command) && _fileManager.Exists(KNOWN_LOCATIONS[command]))
                return KNOWN_LOCATIONS[command];

            return null;
        }
    }
}
