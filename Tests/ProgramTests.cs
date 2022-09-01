using Microsoft.VisualStudio.TestTools.UnitTesting;
using TelegramBot;

namespace Tests
{
    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        public void IsDateTest()
        {
            string date = "12.12.2050";
            bool expected = true;

            Program qwe = new Program();
            bool actual = Program.IsDate(date);

            Assert.AreEqual(expected, actual);
        }
    }
}
