using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using GitPowerShell.Util;

namespace GitPowerShellTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class FileSystemUtilTests
    {
        public FileSystemUtilTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestMakeRelative()
        {
            Assert.AreEqual(@"File.txt", FileSystemUtil.MakeRelative(@"C:\Temp\Folder\File.txt", @"C:\Temp\Folder"));

            /* Test canonicalization */
            Assert.AreEqual(@"File.txt", FileSystemUtil.MakeRelative(@"C:/Temp/Folder/File.txt", @"C:\Temp\Folder"));
            Assert.AreEqual(@"File.txt", FileSystemUtil.MakeRelative(@"C:/Temp/Folder/File.txt", @"C:\\Temp\\Folder"));
            
            /* Test relativizing against children */
            Assert.AreEqual(@"Subfolder\File.txt", FileSystemUtil.MakeRelative(@"C:\Temp\Folder\Subfolder\File.txt", @"C:\Temp\Folder"));
            Assert.AreEqual(@"Subfolder\A\B\C\D\E\File.txt", FileSystemUtil.MakeRelative(@"C:\Temp\Folder\Subfolder\A\B\C\D\E\File.txt", @"C:\Temp\Folder"));
            Assert.AreEqual(@"Subfolder\A\B\C\D\E\File.txt", FileSystemUtil.MakeRelative(@"C:\Temp\Folder\Subfolder\A\B\C\D\E\File.txt", @"C:\Temp\Folder\"));

            /* Test relativizing against parents */
            Assert.AreEqual(@"..\File.txt", FileSystemUtil.MakeRelative(@"C:\Temp\Folder\File.txt", @"C:\Temp\Folder\Subfolder"));
            Assert.AreEqual(@"..\..\File.txt", FileSystemUtil.MakeRelative(@"C:\Temp\Folder\File.txt", @"C:\Temp\Folder\Subfolder\Subsubfolder"));
            Assert.AreEqual(@"..\..\File.txt", FileSystemUtil.MakeRelative(@"C:\Temp\Folder\File.txt", @"C:\Temp\Folder\Subfolder\Subsubfolder\"));
            Assert.AreEqual(@"..\..\..\..\..\..\File.txt", FileSystemUtil.MakeRelative(@"C:\Temp\Folder\File.txt", @"C:\Temp\Folder\Subfolder\A\B\C\D\E"));

            /* Ensure uses folders (not substrings */
            Assert.AreEqual(@"..\Folder\File.txt", FileSystemUtil.MakeRelative(@"C:\Temp\Folder\File.txt", @"C:\Temp\Fo"));

            /* Children of parents */
            Assert.AreEqual(@"..\..\..\..\..\..\Foo\Bar\Foobar\File.txt", FileSystemUtil.MakeRelative(@"C:\Temp\Folder\Foo\Bar\Foobar\File.txt", @"C:\Temp\Folder\Subfolder\A\B\C\D\E"));

            /* Uncommon roots */
            Assert.AreEqual(@"C:\Temp\Folder\File.txt", FileSystemUtil.MakeRelative(@"C:\Temp\Folder\File.txt", @"D:\Foo\Bar"));

            /* Self */
            Assert.AreEqual(@".", FileSystemUtil.MakeRelative(@"C:\Temp\Folder", @"C:\Temp\Folder"));
        }
    }
}
