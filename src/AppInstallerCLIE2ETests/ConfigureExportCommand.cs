// -----------------------------------------------------------------------------
// <copyright file="ConfigureExportCommand.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. Licensed under the MIT License.
// </copyright>
// -----------------------------------------------------------------------------

namespace AppInstallerCLIE2ETests
{
    using System.IO;
    using AppInstallerCLIE2ETests.Helpers;
    using NUnit.Framework;
    using NUnit.Framework.Internal;

    /// <summary>
    /// `Configure export` command tests.
    /// </summary>
    public class ConfigureExportCommand
    {
        private const string Command = "configure export";
        private const string ShowCommand = "configure show";

        private string previousPathValue = string.Empty;

        /// <summary>
        /// Set up.
        /// </summary>
        [OneTimeSetUp]
        public void BaseSetup()
        {
            TestCommon.SetupTestSource(false);
            var installDir = TestCommon.GetRandomTestDir();
            TestCommon.RunAICLICommand("install", $"AppInstallerTest.TestPackageExport -v 1.0.0.0 --silent -l {installDir}");
            this.previousPathValue = System.Environment.GetEnvironmentVariable("PATH");
            System.Environment.SetEnvironmentVariable("PATH", this.previousPathValue + ";" + installDir);
            DSCv3ResourceTestBase.EnsureTestResourcePresence();
        }

        /// <summary>
        /// Tear down.
        /// </summary>
        [OneTimeTearDown]
        public void BaseTeardown()
        {
            TestCommon.RunAICLICommand("uninstall", "AppInstallerTest.TestPackageExport");
            TestCommon.TearDownTestSource();
            if (!string.IsNullOrEmpty(this.previousPathValue))
            {
                System.Environment.SetEnvironmentVariable("PATH", this.previousPathValue);
            }
        }

        /// <summary>
        /// Export a specific package.
        /// </summary>
        [Test]
        public void ExportTestPackage()
        {
            var exportDir = TestCommon.GetRandomTestDir();
            var exportFile = Path.Combine(exportDir, "exported.yml");
            var result = TestCommon.RunAICLICommand(Command, $"--package-id AppInstallerTest.TestPackageExport -o {exportFile}");
            Assert.AreEqual(Constants.ErrorCode.S_OK, result.ExitCode);
            Assert.True(File.Exists(exportFile));

            // Check exported file is readable and validate content
            var showResult = TestCommon.RunAICLICommand(ShowCommand, $"-f {exportFile}");
            Assert.AreEqual(Constants.ErrorCode.S_OK, showResult.ExitCode);
            Assert.True(showResult.StdOut.Contains("Microsoft.WinGet.Dev/Source"));
            Assert.True(showResult.StdOut.Contains($"[{Constants.TestSourceName}_{Constants.TestSourceType}]"));
            Assert.True(showResult.StdOut.Contains($"type: {Constants.TestSourceType}"));
            Assert.True(showResult.StdOut.Contains($"argument: {Constants.TestSourceUrl}"));
            Assert.True(showResult.StdOut.Contains($"name: {Constants.TestSourceName}"));

            Assert.True(showResult.StdOut.Contains("Microsoft.WinGet.Dev/Package"));
            Assert.True(showResult.StdOut.Contains($"[{Constants.TestSourceName}_AppInstallerTest.TestPackageExport]"));
            Assert.True(showResult.StdOut.Contains($"Dependencies: {Constants.TestSourceName}_{Constants.TestSourceType}"));
            Assert.True(showResult.StdOut.Contains("id: AppInstallerTest.TestPackageExport"));
            Assert.True(showResult.StdOut.Contains($"source: {Constants.TestSourceName}"));
        }

        /// <summary>
        /// Export a specific package with related configuration.
        /// </summary>
        [Test]
        public void ExportTestPackageWithPackageSettings()
        {
            var exportDir = TestCommon.GetRandomTestDir();
            var exportFile = Path.Combine(exportDir, "exported.yml");
            var result = TestCommon.RunAICLICommand(Command, $"--package-id AppInstallerTest.TestPackageExport --module AppInstallerTest --resource TestResource -o {exportFile}");
            Assert.AreEqual(Constants.ErrorCode.S_OK, result.ExitCode);
            Assert.True(File.Exists(exportFile));

            // Check exported file is readable and validate content
            var showResult = TestCommon.RunAICLICommand(ShowCommand, $"-f {exportFile}");
            Assert.AreEqual(Constants.ErrorCode.S_OK, showResult.ExitCode);
            Assert.True(showResult.StdOut.Contains("Microsoft.WinGet.Dev/Source"));
            Assert.True(showResult.StdOut.Contains($"[{Constants.TestSourceName}_{Constants.TestSourceType}]"));
            Assert.True(showResult.StdOut.Contains($"type: {Constants.TestSourceType}"));
            Assert.True(showResult.StdOut.Contains($"argument: {Constants.TestSourceUrl}"));
            Assert.True(showResult.StdOut.Contains($"name: {Constants.TestSourceName}"));

            Assert.True(showResult.StdOut.Contains("Microsoft.WinGet.Dev/Package"));
            Assert.True(showResult.StdOut.Contains($"[{Constants.TestSourceName}_AppInstallerTest.TestPackageExport]"));
            Assert.True(showResult.StdOut.Contains($"Dependencies: {Constants.TestSourceName}_{Constants.TestSourceType}"));
            Assert.True(showResult.StdOut.Contains("id: AppInstallerTest.TestPackageExport"));
            Assert.True(showResult.StdOut.Contains($"source: {Constants.TestSourceName}"));

            Assert.True(showResult.StdOut.Contains("AppInstallerTest/TestResource"));
            Assert.True(showResult.StdOut.Contains($"Dependencies: {Constants.TestSourceName}_AppInstallerTest.TestPackageExport"));
            Assert.True(showResult.StdOut.Contains("data: TestData"));
        }

        /// <summary>
        /// Export a specific package with version.
        /// </summary>
        [Test]
        public void ExportTestPackageWithVersion()
        {
            var exportDir = TestCommon.GetRandomTestDir();
            var exportFile = Path.Combine(exportDir, "exported.yml");
            var result = TestCommon.RunAICLICommand(Command, $"--package-id AppInstallerTest.TestPackageExport --include-versions -o {exportFile}");
            Assert.AreEqual(Constants.ErrorCode.S_OK, result.ExitCode);
            Assert.True(File.Exists(exportFile));

            // Check exported file is readable and validate content
            var showResult = TestCommon.RunAICLICommand(ShowCommand, $"-f {exportFile}");
            Assert.AreEqual(Constants.ErrorCode.S_OK, showResult.ExitCode);
            Assert.True(showResult.StdOut.Contains("Microsoft.WinGet.Dev/Source"));
            Assert.True(showResult.StdOut.Contains($"[{Constants.TestSourceName}_{Constants.TestSourceType}]"));
            Assert.True(showResult.StdOut.Contains($"type: {Constants.TestSourceType}"));
            Assert.True(showResult.StdOut.Contains($"argument: {Constants.TestSourceUrl}"));
            Assert.True(showResult.StdOut.Contains($"name: {Constants.TestSourceName}"));

            Assert.True(showResult.StdOut.Contains("Microsoft.WinGet.Dev/Package"));
            Assert.True(showResult.StdOut.Contains($"[{Constants.TestSourceName}_AppInstallerTest.TestPackageExport]"));
            Assert.True(showResult.StdOut.Contains($"Dependencies: {Constants.TestSourceName}_{Constants.TestSourceType}"));
            Assert.True(showResult.StdOut.Contains("id: AppInstallerTest.TestPackageExport"));
            Assert.True(showResult.StdOut.Contains($"source: {Constants.TestSourceName}"));
            Assert.True(showResult.StdOut.Contains("version: 1.0.0.0"));
        }

        /// <summary>
        /// Export all.
        /// </summary>
        [Test]
        public void ExportAll()
        {
            var exportDir = TestCommon.GetRandomTestDir();
            var exportFile = Path.Combine(exportDir, "exported.yml");
            var result = TestCommon.RunAICLICommand(Command, $"--all -o {exportFile}", timeOut: 1200000);
            Assert.AreEqual(Constants.ErrorCode.S_OK, result.ExitCode);
            Assert.True(File.Exists(exportFile));

            // Check exported file is readable and validate content
            var showResult = TestCommon.RunAICLICommand(ShowCommand, $"-f {exportFile}", timeOut: 1200000);
            Assert.AreEqual(Constants.ErrorCode.S_OK, showResult.ExitCode);

            Assert.True(showResult.StdOut.Contains("Microsoft.PowerShell"));

            Assert.True(showResult.StdOut.Contains("Microsoft.WinGet.Dev/UserSettingsFile"));
            Assert.True(showResult.StdOut.Contains("Microsoft.WinGet.Dev/AdminSettings"));
            Assert.True(showResult.StdOut.Contains("Microsoft.Windows.Settings/WindowsSettings"));

            Assert.True(showResult.StdOut.Contains("Microsoft.WinGet.Dev/Source"));
            Assert.True(showResult.StdOut.Contains($"[{Constants.TestSourceName}_{Constants.TestSourceType}]"));
            Assert.True(showResult.StdOut.Contains($"type: {Constants.TestSourceType}"));
            Assert.True(showResult.StdOut.Contains($"argument: {Constants.TestSourceUrl}"));
            Assert.True(showResult.StdOut.Contains($"name: {Constants.TestSourceName}"));

            Assert.True(showResult.StdOut.Contains("Microsoft.WinGet.Dev/Package"));
            Assert.True(showResult.StdOut.Contains($"[{Constants.TestSourceName}_AppInstallerTest.TestPackageExport]"));
            Assert.True(showResult.StdOut.Contains($"Dependencies: {Constants.TestSourceName}_{Constants.TestSourceType}"));
            Assert.True(showResult.StdOut.Contains("id: AppInstallerTest.TestPackageExport"));
            Assert.True(showResult.StdOut.Contains($"source: {Constants.TestSourceName}"));

            Assert.True(showResult.StdOut.Contains("AppInstallerTest/TestResource"));
            Assert.True(showResult.StdOut.Contains($"Dependencies: {Constants.TestSourceName}_AppInstallerTest.TestPackageExport"));
            Assert.True(showResult.StdOut.Contains("data: TestData"));
        }

        /// <summary>
        /// Export a specific package that's not installed.
        /// </summary>
        [Test]
        public void ExportFailedWithNotFoundPackage()
        {
            var exportDir = TestCommon.GetRandomTestDir();
            var exportFile = Path.Combine(exportDir, "exported.yml");
            var result = TestCommon.RunAICLICommand(Command, $"--package-id NotFound.NotFound -o {exportFile}");
            Assert.AreEqual(Constants.ErrorCode.ERROR_NO_APPLICATIONS_FOUND, result.ExitCode);
        }

        /// <summary>
        /// Export all with specific package id.
        /// </summary>
        [Test]
        public void ExportFailedWithAllAndSpecificPackage()
        {
            var exportDir = TestCommon.GetRandomTestDir();
            var exportFile = Path.Combine(exportDir, "exported.yml");
            var result = TestCommon.RunAICLICommand(Command, $"--all --package-id AppInstallerTest.TestPackageExport -o {exportFile}");
            Assert.AreEqual(Constants.ErrorCode.ERROR_INVALID_CL_ARGUMENTS, result.ExitCode);
        }
    }
}
