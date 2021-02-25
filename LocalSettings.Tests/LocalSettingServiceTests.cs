using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace LocalSettings.Tests
{
    public class LocalSettingServiceTests
    {
        private readonly List<FileInfo> _filesToDeleteAtTheEnd = new List<FileInfo>();

        private DirectoryInfo _workDirectory;

        private DirectoryInfo _testDataFolder;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            foreach (var fileInfo in _filesToDeleteAtTheEnd)
            {
                fileInfo.Refresh();
                if (fileInfo.Exists)
                {
                    fileInfo.Delete();
                }
            }
        }

        [SetUp]
        public void Setup()
        {
            _workDirectory = new DirectoryInfo(TestContext.CurrentContext.WorkDirectory);
            _testDataFolder = new DirectoryInfo(Path.Combine(_workDirectory.FullName, "TestData"));
        }

        [Test]
        public void VerifyComplexFileWrite()
        {
            Assert.That(_testDataFolder.Exists, $"Precondition: Test data folder {_testDataFolder.FullName} has to exist.");

            var referenceFile = Path.Combine(_testDataFolder.FullName, "ComplextTestData.yaml");
            Assert.That(File.Exists(referenceFile), "Missing reference data file");

            var targetFile = new FileInfo(Path.Combine(_testDataFolder.FullName, $"WriteTest_{Guid.NewGuid()}.yaml"));
            _filesToDeleteAtTheEnd.Add(targetFile);

            var target = new LocalSettingService();
            target.Initialize(targetFile, SettingWriteMode.OnChange);
            target.SetComplexValue("my_complex_key", new ComplexTestClass());

            var referenceFileContent = File.ReadAllText(referenceFile);
            var createdFileContent = File.ReadAllText(targetFile.FullName);
            Assert.That(createdFileContent, Is.EqualTo(referenceFileContent));
        }

        [Test]
        public void VerifySimpleKeyValue()
        {
            Assert.That(_testDataFolder.Exists, $"Precondition: Test data folder {_testDataFolder.FullName} has to exist.");

            var referenceFile = Path.Combine(_testDataFolder.FullName, "SimpleTestData.yaml");
            Assert.That(File.Exists(referenceFile), "Missing reference data file");
           
            var referenceFileAfterModifications = Path.Combine(_testDataFolder.FullName, "SimpleTestData_afterModifications.yaml");
            Assert.That(File.Exists(referenceFileAfterModifications), "Missing reference data file (after modifications)");

            var targetFile = new FileInfo(Path.Combine(_testDataFolder.FullName, $"SimpleTestData_{Guid.NewGuid()}.yaml"));
            _filesToDeleteAtTheEnd.Add(targetFile);
            File.Copy(referenceFile, targetFile.FullName, true);

            var target = new LocalSettingService();
            target.Initialize(targetFile, SettingWriteMode.OnChange);

            // Read a couple of values
            const string key1 = "string_value_1";
            const string keyInt = "int_value";
            const string keyDecimal = "decimal_value";
            const string keyDateTime = "datetime_value";

            var referenceDateTime = new DateTime(2020, 12, 24, 08, 12, 20);

            Assert.That(target.Get(key1), Is.EqualTo("My fancy string"));
            Assert.That(target.GetInt(keyInt), Is.EqualTo(42));
            Assert.That(target.GetDecimal(keyDecimal), Is.EqualTo(42.43));
            Assert.That(target.GetDateTime(keyDateTime), Is.EqualTo(referenceDateTime));
            
            // now modify them
            target.Set(key1, "new, updated value");
            target.Set(keyInt, 4242);
            target.Set(keyDecimal, 100.20m);
            target.Set(keyDateTime, new DateTime(2018, 11, 14, 09, 12, 20));

            // and finally check the expected new yaml content
            var referenceFileContent = File.ReadAllText(referenceFileAfterModifications);
            var updatedFileContent = File.ReadAllText(targetFile.FullName);
            Assert.That(updatedFileContent, Is.EqualTo(referenceFileContent));

        }
    }
}