using System;
using System.IO;
using System.Text;
using Nesquick;
using Nesquick.Tests;
using NUnit.Framework;

namespace Tests
{
    public class CPUTests
    {
        private const string NESTES_CART = "../../../nestest/nestest.nes";
        private const string NESTES_LOG = "../../../nestest/nestestv2.log";
        public NestestConsole NestestConsole { get; private set; }

        [SetUp]
        public void SetUp() => NestestConsole = new NestestConsole(new Cartridge(NESTES_CART));

        [Test]
        public void Nestest()
        {
            NestestConsole.Run();

            string[] nestestLog = File.ReadAllLines(NESTES_LOG);

            var log = NestestConsole.Log.ToString().Split(Environment.NewLine);

            for (int i = 0; i < log.Length; i++)
            {
                Assert.AreEqual(nestestLog[i], log[i], "Previous line: " + log[(i > 0 ? i - 1 : 0)]);
            }
        }
    }
}