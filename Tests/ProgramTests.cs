using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TelegramBot;

namespace Tests
{
    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        public void IsDatePastDate()
        {
            string date = "01.09.2022";
            bool expected = true;

            Program qwe = new Program();
            bool actual = Program.IsDate(date);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void IsDateToDay()
        {
            string date = DateTime.Now.Date.ToString();
            bool expected = true;

            Program qwe = new Program();
            bool actual = Program.IsDate(date);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void IsDateDayErr()
        {
            string date = "32.10.2010";
            bool expected = false;

            Program qwe = new Program();
            bool actual = Program.IsDate(date);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void IsDateDayZero()
        {
            string date = "0.10.2010";
            bool expected = false;

            Program qwe = new Program();
            bool actual = Program.IsDate(date);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void IsDateDayNull()
        {
            string date = ".10.2010";
            bool expected = false;

            Program qwe = new Program();
            bool actual = Program.IsDate(date);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void IsDateDayNegative()
        {
            string date = "-11.10.2010";
            bool expected = false;

            Program qwe = new Program();
            bool actual = Program.IsDate(date);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void IsDateMonthErr()
        {
            string date = "12.15.2010";
            bool expected = false;

            Program qwe = new Program();
            bool actual = Program.IsDate(date);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void IsDateMonthZero()
        {
            string date = "12.0.2010";
            bool expected = false;

            Program qwe = new Program();
            bool actual = Program.IsDate(date);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void IsDateMonthNull()
        {
            string date = "12..2010";
            bool expected = false;

            Program qwe = new Program();
            bool actual = Program.IsDate(date);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void IsDateMonthNegative()
        {
            string date = "12.-11.2010";
            bool expected = false;

            Program qwe = new Program();
            bool actual = Program.IsDate(date);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void IsDateYearZero()
        {
            string date = "10.10.0000";
            bool expected = false;

            Program qwe = new Program();
            bool actual = Program.IsDate(date);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void IsDateYearNull()
        {
            string date = "10.10.";
            bool expected = false;

            Program qwe = new Program();
            bool actual = Program.IsDate(date);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void IsDateYearNegative()
        {
            string date = "10.10.-2020";
            bool expected = false;

            Program qwe = new Program();
            bool actual = Program.IsDate(date);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void IsDateYearErrFuture()
        {
            string date = "10.10.2222";
            bool expected = false;

            Program qwe = new Program();
            bool actual = Program.IsDate(date);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void IsDateYearErrPast()
        {
            string date = "10.10.1890";
            bool expected = false;

            Program qwe = new Program();
            bool actual = Program.IsDate(date);

            Assert.AreEqual(expected, actual);
        }

    }
}
