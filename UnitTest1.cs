using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace AppiumDesktopCalculatorFreeAutomation
{
    public class Tests
    {
        private const string appiumServer = "http://127.0.0.1:4723/wd/hub";
        // Get-StartApps
        private const string appLocation = "DigitalchemyLLC.CalculatorFree_q7s52g45wnx0g!App";
        private WindowsDriver<WindowsElement> driver;
        private AppiumOptions appiumOptions;
        private WindowsElement clearButton => driver.FindElementByAccessibilityId("ClearButton");
        private WindowsElement inputField => driver.FindElementByAccessibilityId("OperationScrollView");

        [OneTimeSetUp]
        public void Setup()
        {
            this.appiumOptions = new AppiumOptions() { PlatformName = "Windows" };
            appiumOptions.AddAdditionalCapability("app", appLocation);
            this.driver = new WindowsDriver<WindowsElement>(new Uri(appiumServer), appiumOptions);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        [OneTimeTearDown]
        public void CloseApp()
        {
            clearButton.Click();
            driver.Dispose();
        }

        [Test]
        public void TestAppName()
        {
            var appName = driver.FindElementByAccessibilityId("TitleBar");
            Assert.That(appName.Text, Is.EqualTo("Calculator Free"));
        }

        [TestCase("5","+","5","10")]
        [TestCase("5.25","+","5","10.25")]
        [TestCase("12345","+","12345", "24,690")]
        [TestCase("5", "-", "5", "0")]
        [TestCase("5.25", "-", "5", "0.25")]
        [TestCase("5", "*", "5", "25")]
        [TestCase("5.25", "*", "5", "26.25")]
        [TestCase("5555", "*", "1234", "6,854,870")]
        [TestCase("5", "/", "5", "1")]
        [TestCase("5.25", "/", "5", "1.05")]
        [TestCase("555", "/", "12345", "0.044957472")]
        public void Test_CalculationsPosituveNums_WithPositiveResult(string num1, string operation, string num2, string expectedResult)
        {
            inputField.SendKeys(num1);
            inputField.SendKeys(operation);
            inputField.SendKeys(num2);
            inputField.SendKeys(Keys.Enter);
            var res = driver.FindElement(By.XPath("//Text[@AutomationId='NormalNumber'][last()]"));
            var resultActual = Regex.Match(res.Text, @"[0-9]*((\.?)(([0-9]*)?)?)+((,?)(([0-9]*)?)?)+").Value; 
            Assert.That(resultActual, Is.EqualTo(expectedResult));
            clearButton.Click();
        }


        [TestCase("555", "-", "1235", "-680")]
        [TestCase("10", "-", "100", "-90")]
        [TestCase("1000000", "-", "100000000", "-99,000,000")]
        public void Test_CalculationsPosituveNums_WithNegativeResult(string num1, string operation, string num2, string expectedResult)
        {
            inputField.SendKeys(num1);
            inputField.SendKeys(operation);
            inputField.SendKeys(num2);
            inputField.SendKeys(Keys.Enter);
            var minus = driver.FindElement(By.XPath("//Text[@ClassName='TextBlock'][@Name='-']"));
            var res = driver.FindElement(By.XPath("//Text[@AutomationId='NormalNumber'][last()]"));
            var resultNum = Regex.Match(res.Text, @"[0-9]*((\.?)(([0-9]*)?)?)+((,?)(([0-9]*)?)?)+").Value;
            var resultActual = minus.Text+resultNum;
            Assert.That(resultActual.ToString, Is.EqualTo(expectedResult));
            clearButton.Click();
        }

        [Test]
        public void Test_DivideByZero()
        {
            inputField.SendKeys("5");
            inputField.SendKeys("/");
            inputField.SendKeys("0");
            inputField.SendKeys(Keys.Enter);
            var res = driver.FindElement(By.XPath("//Text[@ClassName='TextBlock'][@Name='Error']"));
            Assert.True(res.Displayed);
            clearButton.Click();
        }

        [TestCase("5", "0.05")]
        [TestCase("8", "0.08")]
        [TestCase("80", "0.8")]
        [TestCase("800", "8")]
        public void Test_Percent(string num, string result)
        {
            inputField.SendKeys(num);
            inputField.SendKeys("%");
            inputField.SendKeys(Keys.Enter);
            var res = driver.FindElement(By.XPath("//Text[@AutomationId='NormalNumber'][last()]"));
            var resultActual = Regex.Match(res.Text, @"[0-9]*((\.?)(([0-9]*)?)?)+((,?)(([0-9]*)?)?)+").Value;
            Assert.That(resultActual, Is.EqualTo(result));
            clearButton.Click();
        }
    }
}