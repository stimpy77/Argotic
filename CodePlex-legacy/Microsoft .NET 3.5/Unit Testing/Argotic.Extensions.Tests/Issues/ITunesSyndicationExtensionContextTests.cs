using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Argotic.Extensions.Core;

namespace Argotic.Extensions.Tests.Issues
{
    /// <summary>
    /// Summary description for ITunesSyndicationExtensionContextTest
    /// </summary>
    [TestClass]
    public class ITunesSyndicationExtensionContextTests
    {
        public ITunesSyndicationExtensionContextTests()
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

        /// <summary>
        /// Work item 13840
        /// </summary>
        [TestMethod]
        public void iTunesDurationParseError13840()
        {
            TimeSpan tspan = ITunesSyndicationExtensionContext_Accessor.ParseDuration("03:00");
            Assert.AreEqual(0, tspan.Hours, "Assertion on hours failed");
            Assert.AreEqual(3, tspan.Minutes, "Assertion on minutes failed");
            Assert.AreEqual(0, tspan.Seconds, "Assertion on seconds failed");
        }
    }
}
