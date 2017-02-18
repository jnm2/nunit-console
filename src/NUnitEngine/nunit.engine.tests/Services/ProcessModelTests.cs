using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Engine.Extensibility;
using NUnit.Engine.Internal;
using NUnit.Engine.Runners;
using NUnit.Engine.Services.Tests.Fakes;
using NUnit.Framework;

namespace NUnit.Engine.Services.Tests
{
    [TestFixture]
    public partial class ProcessModelTests
    {
        /// <summary>
        /// Represents a grouping ID, unordered except for <see cref="InProcess"/>. The actual order in which the external processes are created does not matter.
        /// </summary>
        public enum ExpectedGrouping
        {
            InProcess = 0,
            ExternalProcessA = 1,
            ExternalProcessB = 2,
            ExternalProcessC = 3,
            ExternalProcessD = 4,
            ExternalProcessE = 5
        }

        public class PackageFile
        {
            public string Path { get; }
            public ExpectedGrouping ExpectedGrouping { get; }

            public PackageFile(ExpectedGrouping expectedGrouping, string path)
            {
                Path = path;
                ExpectedGrouping = expectedGrouping;
            }
        }

        public sealed class ProjectPackageFile : PackageFile
        {
            public ProcessModel ProjectProcessModel { get; }
            public PackageFile[] ProjectAssemblies { get; }

            public ProjectPackageFile(ExpectedGrouping expectedGrouping, string path, ProcessModel projectProcessModel, params PackageFile[] projectAssemblies) : base(expectedGrouping, path)
            {
                ProjectProcessModel = projectProcessModel;
                ProjectAssemblies = projectAssemblies;
            }
        }


        private static string GetFullPath(string file)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, file);
        }

        
        private const string UniqueFileName1 = "UniqueFileName1.nunit";
        private const string UniqueFileName2 = "UniqueFileName2.nunit";
        private const string UniqueFileName3 = "UniqueFileName3.nunit";
        private const string UniqueFileName4 = "UniqueFileName4.nunit";

        public static IEnumerable<TestCaseData> TestCases => new[]
        {
            new TestCaseData(null, new[]
            {/*
                new ProjectPackageFile(ExpectedGrouping.InProcess, UniqueFileName1, ProcessModel.Default,
                    new PackageFile(ExpectedGrouping.ExternalProcessA, "1.dll"),
                    new PackageFile(ExpectedGrouping.ExternalProcessB, "2.dll")),*/
                /* Waiting till I create a SpyDriverService
                new ProjectPackageFile(ExpectedGrouping.InProcess, UniqueFileName2, ProcessModel.InProcess, 
                    new PackageFile(ExpectedGrouping.InProcess, "3.dll"),
                    new PackageFile(ExpectedGrouping.InProcess, "4.dll")),*/
                new ProjectPackageFile(ExpectedGrouping.ExternalProcessC, UniqueFileName3, ProcessModel.Separate,
                    new PackageFile(ExpectedGrouping.ExternalProcessC, "5.dll"),
                    new PackageFile(ExpectedGrouping.ExternalProcessC, "6.dll")),
                /*new ProjectPackageFile(ExpectedGrouping.InProcess, UniqueFileName4, ProcessModel.Multiple,
                    new PackageFile(ExpectedGrouping.ExternalProcessD, "7.dll"),
                    new PackageFile(ExpectedGrouping.ExternalProcessE, "8.dll"))*/
            })
        };

        [TestCaseSource(nameof(TestCases))]
        public void PackageRunsInCorrectProcess(string cliProcessModel, PackageFile[] packageFiles)
        {
            var services = new ServiceContext();

            var extensionService = new ExtensionService();
            extensionService.InstallExtensionInstance<IProjectLoader>(
                CreateProjectLoader(packageFiles.OfType<ProjectPackageFile>()))
                .AddProperty("FileExtension", ".nunit");
            services.Add(extensionService);

            var spyTestAgency = new SpyTestAgency();
            services.Add(spyTestAgency);

            services.Add(new RuntimeFrameworkService());
            services.Add(new ProjectService());
            services.Add(new DefaultTestRunnerFactory());
            services.Add(new DomainManager());
            services.ServiceManager.StartServices();

            using (var runner = new MasterTestRunner(services, CreateMasterPackage(cliProcessModel, packageFiles)))
            {
                runner.Run(null, TestFilter.Empty);
                ;
            }
        }

        private static TestPackage CreateMasterPackage(string cliProcessModel, IEnumerable<PackageFile> packageFiles)
        {
            var package = new TestPackage((from file in packageFiles select GetFullPath(file.Path)).ToList());
            if (cliProcessModel != null)
                package.AddSetting("ProcessModel", cliProcessModel);
            return package;
        }

        private static IProjectLoader CreateProjectLoader(IEnumerable<ProjectPackageFile> projectPackagesFiles)
        {
            var projectsByPath = new Dictionary<string, IProject>();

            foreach (var projectFile in projectPackagesFiles)
            {
                var path = GetFullPath(projectFile.Path);

                var projectPackage = new TestPackage(path);
                projectPackage.AddSetting("ProcessModel", projectFile.ProjectProcessModel.ToString());
                foreach (var assemblyPath in projectFile.ProjectAssemblies)
                    projectPackage.AddSubPackage(new TestPackage(assemblyPath.Path));

                projectsByPath.Add(path, new StubProject(path, null, projectPackage));
            }

            return new StubProjectLoader(projectsByPath);
        }
    }
}