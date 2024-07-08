using System;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

namespace Number_Calculator
{
    [TestFixture("Chrome")]
    [TestFixture("Edge")]
    // [TestFixture("Firefox")]
    [TestFixture("Brave")]
    [TestFixture("Opera")]
    public class NumberCalculatorTests
    {
        private readonly string browser;
        private IWebDriver driver;

        // Parameterless constructor
        public NumberCalculatorTests()
        {
            // This constructor can be used by NUnit to instantiate the test fixture.
            //  May leave it empty or add initialization logic if needed.
        }

        // Constructor with a parameter for the browser
        public NumberCalculatorTests(string browser) : this()
        {
            this.browser = browser;
        }

        [SetUp]
        public void Setup()
        {
            var Url = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com/number-calculator/";
            driver = GetWebDriver(browser);
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl(Url);

            // Wait until the page is fully loaded
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
        }

        [TearDown]
        public void TearDown()
        {
            if (driver != null)
            {
                driver.Quit();
                driver.Dispose();
                driver = null;
            }
        }

        private IWebDriver GetWebDriver(string browser)
        {
            switch (browser)
            {
                case "Chrome":
                    var chromeOptions = new ChromeOptions();
                    chromeOptions.AddArgument("--headless");
                    return new ChromeDriver(chromeOptions);

                case "Edge":
                    var edgeOptions = new EdgeOptions();
                    edgeOptions.AddArgument("--headless");
                    return new EdgeDriver(edgeOptions);

                case "Firefox":
                    var geckoDriverPath = GetDriverPath("geckodriver");
                    var firefoxBinaryPath = GetDriverPath("firefox");

                    var firefoxOptions = new FirefoxOptions();
                    firefoxOptions.AddArgument("--headless");
                    firefoxOptions.BinaryLocation = firefoxBinaryPath;

                    var firefoxService = FirefoxDriverService.CreateDefaultService(geckoDriverPath);
                    firefoxService.Host = "::1"; // Use IPv6 loopback address

                    // Increase command timeout
                    var commandTimeout = TimeSpan.FromMinutes(3);
                    
                    return new FirefoxDriver(firefoxService, firefoxOptions);

                    // var firefoxOptions = new FirefoxOptions();
                    // firefoxOptions.AddArgument("--headless");

                    // // Specify the path to the Firefox binary if needed
                    // firefoxOptions.BinaryLocation = "/usr/bin/firefox";

                    // var firefoxService = FirefoxDriverService.CreateDefaultService();
                    // firefoxService.Host = "::1"; // Use IPv6 loopback address

                    // return new FirefoxDriver(firefoxService, firefoxOptions, commandTimeout);

                case "Brave":
                    var braveDriverPath = GetDriverPath("brave");
                    var braveOptions = new ChromeOptions();
                    braveOptions.BinaryLocation = braveDriverPath;
                    braveOptions.AddArgument("--headless");
                    return new ChromeDriver(braveOptions);

                case "Opera":
                    var operaDriverPath = GetDriverPath("opera");
                    var operaOptions = new ChromeOptions();
                    operaOptions.BinaryLocation = operaDriverPath;
                    operaOptions.AddArgument("--headless");
                    return new ChromeDriver(operaOptions);

                default:
                    throw new ArgumentException($"Browser not supported: {browser}");
            }
        }

        private string GetDriverPath(string driverFileName)
        {
            var driversDirectory = Environment.GetEnvironmentVariable("DRIVERS_PATH") ?? "./WebDrivers";
            return System.IO.Path.Combine(driversDirectory, driverFileName);
        }
        
        [Test, Order(1), Category("InputValidation")]
        public void ValidateFieldsAcceptOnlyNumbers()
        {
            driver.FindElement(By.Id("number1")).SendKeys("5");
            var dropdown = driver.FindElement(By.Id("operation"));
            dropdown.FindElement(By.XPath("//option[. = '+ (sum)']")).Click();

            driver.FindElement(By.Id("number2")).SendKeys("10");
            driver.FindElement(By.Id("calcButton")).Click();
            Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("15"));
        }

        [Test, Order(2), Category("InputValidation")]
        public void ValidateFieldsDontAcceptTextAndSymbols()
        {
            driver.FindElement(By.Id("number1")).SendKeys("~~@%");
            var dropdown = driver.FindElement(By.Id("operation"));
            
            dropdown.FindElement(By.XPath("//option[. = '+ (sum)']")).Click();
            driver.FindElement(By.Id("number2")).SendKeys("cxbzfdb");
            
            driver.FindElement(By.Id("calcButton")).Click();
            Assert.That(driver.FindElement(By.CssSelector("i")).Text, Is.EqualTo("invalid input"));
        }

        [Test, Order(3), Category("InputValidation")]
        public void ValidateField2CantBeEmpty()
        {
            driver.FindElement(By.Id("number1")).SendKeys("13");

            var dropdown = driver.FindElement(By.Id("operation"));
            dropdown.FindElement(By.XPath("//option[. = '- (subtract)']")).Click();
            
            driver.FindElement(By.Id("number2")).SendKeys(string.Empty);
            driver.FindElement(By.Id("calcButton")).Click();
            Assert.That(driver.FindElement(By.CssSelector("i")).Text, Is.EqualTo("invalid input"));
        }

        [Test, Order(4), Category("InputValidation")]
        public void ValidateField1CantBeEmpty()
        {
            driver.FindElement(By.Id("number1")).SendKeys("13");
            
            var dropdown = driver.FindElement(By.Id("operation"));
            dropdown.FindElement(By.XPath("//option[. = '- (subtract)']")).Click();

            driver.FindElement(By.Id("number2")).SendKeys(string.Empty);
            driver.FindElement(By.Id("calcButton")).Click();
            Assert.That(driver.FindElement(By.CssSelector("i")).Text, Is.EqualTo("invalid input"));
        }

        [Test, Order(5), Category("InputValidation")]
        public void ValidateNumbersFieldsCantBeEmpty()
        {
            driver.FindElement(By.Id("number1")).SendKeys(string.Empty);
            
            var dropdown = driver.FindElement(By.Id("operation"));
            dropdown.FindElement(By.XPath("//option[. = '+ (sum)']")).Click();

            driver.FindElement(By.Id("number2")).SendKeys(string.Empty);
            driver.FindElement(By.Id("calcButton")).Click();
            Assert.That(driver.FindElement(By.CssSelector("i")).Text, Is.EqualTo("invalid input"));
        }

        [Test, Order(6), Category("InputValidation")]
        public void ValidateFieldOperationCantBeEmpty()
        {
            driver.FindElement(By.Id("number1")).SendKeys("16");
            driver.FindElement(By.Id("number2")).SendKeys("23");
            driver.FindElement(By.Id("calcButton")).Click();
            Assert.That(driver.FindElement(By.CssSelector("i")).Text, Is.EqualTo("invalid operation"));
        }

        [Test, Order(7), Category("OperationSelection")]
        public void ValidateOptionSumIsChoosen()
        {
            var dropdown = driver.FindElement(By.Id("operation"));
            dropdown.FindElement(By.XPath("//option[. = '+ (sum)']")).Click();
            
            string value = driver.FindElement(By.Id("operation")).GetAttribute("value");
            Assert.That(value, Is.EqualTo("+"));
        }

        [Test, Order(8), Category("OperationSelection")]
        public void ValidateOptionSubtractIsChoosen()
        {
            var dropdown = driver.FindElement(By.Id("operation"));
            dropdown.FindElement(By.XPath("//option[. = '- (subtract)']")).Click();
            string value = driver.FindElement(By.Id("operation")).GetAttribute("value");
            Assert.That(value, Is.EqualTo("-"));
        }

        [Test, Order(9), Category("OperationSelection")]
        public void ValidateOptionMultiplyIsChoosen()
        {
            var dropdown = driver.FindElement(By.Id("operation"));
            dropdown.FindElement(By.XPath("//option[. = '* (multiply)']")).Click();
            string value = driver.FindElement(By.Id("operation")).GetAttribute("value");
            Assert.That(value, Is.EqualTo("*"));
        }

        [Test, Order(10), Category("OperationSelection")]
        public void ValidateOptionDivideIsChoosen()
        {
            var dropdown = driver.FindElement(By.Id("operation"));
            dropdown.FindElement(By.XPath("//option[. = '/ (divide)']")).Click();
            
            string value = driver.FindElement(By.Id("operation")).GetAttribute("value");
            Assert.That(value, Is.EqualTo("/"));
        }

        [Test, Order(11), Category("CalculationsTests")]
        public void AssertCorrectAddition()
        {
            driver.FindElement(By.Id("number1")).SendKeys("12");
            {
                string value = driver.FindElement(By.Id("number1")).GetAttribute("value");
                Assert.That(value, Is.EqualTo("12"));
            }

            var dropdown = driver.FindElement(By.Id("operation"));
            dropdown.FindElement(By.XPath("//option[. = '+ (sum)']")).Click();

            {
                string value = driver.FindElement(By.Id("operation")).GetAttribute("value");
                Assert.That(value, Is.EqualTo("+"));
            }
            
            driver.FindElement(By.Id("number2")).SendKeys("13");
            
            {
                string value = driver.FindElement(By.Id("number2")).GetAttribute("value");
                Assert.That(value, Is.EqualTo("13"));
            }

            driver.FindElement(By.Id("calcButton")).Click();
            Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("25"));
        }

        [Test, Order(12), Category("CalculationsTests")]
        public void AssertCorrectSubtraction()
        {
            driver.FindElement(By.Id("number1")).SendKeys("17");
            {
                string value = driver.FindElement(By.Id("number1")).GetAttribute("value");
                Assert.That(value, Is.EqualTo("17"));
            }
            
            var dropdown = driver.FindElement(By.Id("operation"));
            dropdown.FindElement(By.XPath("//option[. = '- (subtract)']")).Click();

            {
                string value = driver.FindElement(By.Id("operation")).GetAttribute("value");
                Assert.That(value, Is.EqualTo("-"));
            }

            driver.FindElement(By.Id("number2")).SendKeys("10");

            {
                string value = driver.FindElement(By.Id("number2")).GetAttribute("value");
                Assert.That(value, Is.EqualTo("10"));
            }

            driver.FindElement(By.Id("calcButton")).Click();
            Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("7"));
        }

        [Test, Order(13), Category("CalculationsTests")]
        public void AssertCorrectMultiplication()
        {
            driver.FindElement(By.Id("number1")).SendKeys("14");
            {
                string value = driver.FindElement(By.Id("number1")).GetAttribute("value");
                Assert.That(value, Is.EqualTo("14"));
            }
            
            var dropdown = driver.FindElement(By.Id("operation"));
            dropdown.FindElement(By.XPath("//option[. = '* (multiply)']")).Click();

            {
                string value = driver.FindElement(By.Id("operation")).GetAttribute("value");
                Assert.That(value, Is.EqualTo("*"));
            }
            
            driver.FindElement(By.Id("number2")).SendKeys("8");
            
            {
                string value = driver.FindElement(By.Id("number2")).GetAttribute("value");
                Assert.That(value, Is.EqualTo("8"));
            }

            driver.FindElement(By.Id("calcButton")).Click();
            Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("112"));
        }

        [Test, Order(14), Category("CalculationsTests")]
        public void AssertCorrectDivision()
        {
            driver.FindElement(By.Id("number1")).SendKeys("112");
            {
                string value = driver.FindElement(By.Id("number1")).GetAttribute("value");
                Assert.That(value, Is.EqualTo("112"));
            }
            
            var dropdown = driver.FindElement(By.Id("operation"));
            dropdown.FindElement(By.XPath("//option[. = '/ (divide)']")).Click();

            {
                string value = driver.FindElement(By.Id("operation")).GetAttribute("value");
                Assert.That(value, Is.EqualTo("/"));
            }

            driver.FindElement(By.Id("number2")).SendKeys("8");

            {
                string value = driver.FindElement(By.Id("number2")).GetAttribute("value");
                Assert.That(value, Is.EqualTo("8"));
            }

            driver.FindElement(By.Id("calcButton")).Click();
            Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("14"));
        }

        [Test, Order(15), Category("ResetFunctionalityTests")]
        public void ResetField1()
        {
            driver.FindElement(By.Id("number1")).SendKeys("12");
            {
                string value = driver.FindElement(By.Id("number1")).GetAttribute("value");
                Assert.That(value, Is.EqualTo("12"));
            }
            
            driver.FindElement(By.Id("resetButton")).Click();
            {
                string value = driver.FindElement(By.Id("number1")).GetAttribute("value");
                Assert.That(value, Is.EqualTo(string.Empty));
            }
        }

        [Test, Order(16), Category("ResetFunctionalityTests")]
        public void ResetField2()
        {
            driver.FindElement(By.Id("number2")).SendKeys("15");
            {
                string value = driver.FindElement(By.Id("number2")).GetAttribute("value");
                Assert.That(value, Is.EqualTo("15"));
            }
            
            driver.FindElement(By.Id("resetButton")).Click();
            {
                string value = driver.FindElement(By.Id("number2")).GetAttribute("value");
                Assert.That(value, Is.EqualTo(string.Empty));
            }
        }

        [Test, Order(17), Category("ResetFunctionalityTests")]
        public void ResetOperationField()
        {
            var dropdown = driver.FindElement(By.Id("operation"));
            dropdown.FindElement(By.XPath("//option[. = '* (multiply)']")).Click();

            {
                string value = driver.FindElement(By.Id("operation")).GetAttribute("value");
                Assert.That(value, Is.EqualTo("*"));
            }
            
            driver.FindElement(By.Id("resetButton")).Click();
            {
                string value = driver.FindElement(By.Id("operation")).GetAttribute("value");
                Assert.That(value, Is.EqualTo("-- select an operation --"));
            }
        }

        [Test, Order(18), Category("ResetFunctionalityTests")]
        public void ResetAllFields()
        {
            driver.FindElement(By.Id("number1")).SendKeys("13");
            {
                string value = driver.FindElement(By.Id("number1")).GetAttribute("value");
                Assert.That(value, Is.EqualTo("13"));
            }
            
            var dropdown = driver.FindElement(By.Id("operation"));
            dropdown.FindElement(By.XPath("//option[. = '* (multiply)']")).Click();

            {
                string value = driver.FindElement(By.Id("operation")).GetAttribute("value");
                Assert.That(value, Is.EqualTo("*"));
            }
            
            driver.FindElement(By.Id("number2")).SendKeys("17");
            {
                string value = driver.FindElement(By.Id("number2")).GetAttribute("value");
                Assert.That(value, Is.EqualTo("17"));
            }
           
            driver.FindElement(By.Id("resetButton")).Click();
            {
                string value = driver.FindElement(By.Id("number1")).GetAttribute("value");
                Assert.That(value, Is.EqualTo(string.Empty));
            }
            {
                string value = driver.FindElement(By.Id("operation")).GetAttribute("value");
                Assert.That(value, Is.EqualTo("-- select an operation --"));
            }
            {
                string value = driver.FindElement(By.Id("number2")).GetAttribute("value");
                Assert.That(value, Is.EqualTo(string.Empty));
            }
        }

        [Test, Order(19), Category("EdgeCasesTests")]
        public void AddSpacesInNumbersFields()
        {
            driver.FindElement(By.Id("number1")).SendKeys("   9   ");
            
            var dropdown = driver.FindElement(By.Id("operation"));
            dropdown.FindElement(By.XPath("//option[. = '/ (divide)']")).Click();

            driver.FindElement(By.Id("number2")).SendKeys("   3    ");
            driver.FindElement(By.Id("calcButton")).Click();
            Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("3"));
            driver.FindElement(By.Id("resetButton")).Click();
        }

        [Test, Order(20), Category("EdgeCasesTests")]
        public void OperationsWithInfinity()
        {
            driver.FindElement(By.Id("number1")).SendKeys("Infinity");
            {
                var dropdown = driver.FindElement(By.Id("operation"));
                dropdown.FindElement(By.XPath("//option[. = '+ (sum)']")).Click();
            }
            driver.FindElement(By.Id("number2")).SendKeys("4");
            driver.FindElement(By.Id("calcButton")).Click();
            
            Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("Infinity"));
            driver.FindElement(By.Id("resetButton")).Click();
            
            driver.FindElement(By.Id("number1")).SendKeys("Infinity");
            {
                var dropdown = driver.FindElement(By.Id("operation"));
                dropdown.FindElement(By.XPath("//option[. = '+ (sum)']")).Click();
            }
            driver.FindElement(By.Id("number2")).SendKeys("Infinity");
            driver.FindElement(By.Id("calcButton")).Click();
            
            Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("Infinity"));
            driver.FindElement(By.Id("resetButton")).Click();
            
            driver.FindElement(By.Id("number1")).SendKeys("5");
            {
                var dropdown = driver.FindElement(By.Id("operation"));
                dropdown.FindElement(By.XPath("//option[. = '+ (sum)']")).Click();
            }
            driver.FindElement(By.Id("number2")).SendKeys("Infinity");
            driver.FindElement(By.Id("calcButton")).Click();
            driver.FindElement(By.Id("result")).Click();
            
            Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("Infinity"));
            driver.FindElement(By.Id("resetButton")).Click();
            
            driver.FindElement(By.Id("number1")).SendKeys("Infinity");
            {
                var dropdown = driver.FindElement(By.Id("operation"));
                dropdown.FindElement(By.XPath("//option[. = '- (subtract)']")).Click();
            }
            driver.FindElement(By.Id("number2")).SendKeys("3");
            driver.FindElement(By.Id("calcButton")).Click();
            
            Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("Infinity"));
            driver.FindElement(By.Id("resetButton")).Click();
            
            driver.FindElement(By.Id("number1")).SendKeys("3");
            {
                var dropdown = driver.FindElement(By.Id("operation"));
                dropdown.FindElement(By.XPath("//option[. = '- (subtract)']")).Click();
            }
            driver.FindElement(By.Id("number2")).SendKeys("Infinity");
            driver.FindElement(By.Id("calcButton")).Click();
            driver.FindElement(By.Id("result")).Click();
            
            Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("-Infinity"));
            driver.FindElement(By.Id("resetButton")).Click();
            
            driver.FindElement(By.Id("number1")).SendKeys("Infinity");
            {
                var dropdown = driver.FindElement(By.Id("operation"));
                dropdown.FindElement(By.XPath("//option[. = '- (subtract)']")).Click();
            }
            driver.FindElement(By.Id("number2")).SendKeys("Infinity");
            driver.FindElement(By.Id("calcButton")).Click();
            
            Assert.That(driver.FindElement(By.CssSelector("i")).Text, Is.EqualTo("invalid calculation"));
            driver.FindElement(By.Id("resetButton")).Click();
            
            driver.FindElement(By.Id("number1")).SendKeys("Infinity");
            {
                var dropdown = driver.FindElement(By.Id("operation"));
                dropdown.FindElement(By.XPath("//option[. = '* (multiply)']")).Click();
            }
            driver.FindElement(By.Id("number2")).SendKeys("5");
            driver.FindElement(By.Id("calcButton")).Click();
            
            Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("Infinity"));
            driver.FindElement(By.Id("resetButton")).Click();
            
            driver.FindElement(By.Id("number1")).SendKeys("5");
            {
                var dropdown = driver.FindElement(By.Id("operation"));
                dropdown.FindElement(By.XPath("//option[. = '* (multiply)']")).Click();
            }
            driver.FindElement(By.Id("number2")).SendKeys("Infinity");
            driver.FindElement(By.Id("calcButton")).Click();
            
            Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("Infinity"));
            driver.FindElement(By.Id("resetButton")).Click();
            
            driver.FindElement(By.Id("number1")).SendKeys("Infinity");
            {
                var dropdown = driver.FindElement(By.Id("operation"));
                dropdown.FindElement(By.XPath("//option[. = '* (multiply)']")).Click();
            }
            driver.FindElement(By.Id("number2")).SendKeys("Infinity");
            driver.FindElement(By.Id("calcButton")).Click();
            
            Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("Infinity"));
            driver.FindElement(By.Id("resetButton")).Click();
            
            driver.FindElement(By.Id("number1")).SendKeys("Infinity");
            {
                var dropdown = driver.FindElement(By.Id("operation"));
                dropdown.FindElement(By.XPath("//option[. = '/ (divide)']")).Click();
            }
            driver.FindElement(By.Id("number2")).SendKeys("3");
            driver.FindElement(By.Id("calcButton")).Click();
            
            Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("Infinity"));
            driver.FindElement(By.Id("resetButton")).Click();
            
            driver.FindElement(By.Id("number1")).SendKeys("3");
            {
                var dropdown = driver.FindElement(By.Id("operation"));
                dropdown.FindElement(By.XPath("//option[. = '/ (divide)']")).Click();
            }
            driver.FindElement(By.Id("number2")).SendKeys("Infinity");
            driver.FindElement(By.Id("calcButton")).Click();
            
            Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("0"));
            driver.FindElement(By.Id("resetButton")).Click();
            
            driver.FindElement(By.Id("number1")).SendKeys("Infinity");
            {
                var dropdown = driver.FindElement(By.Id("operation"));
                dropdown.FindElement(By.XPath("//option[. = '/ (divide)']")).Click();
            }
            driver.FindElement(By.Id("number2")).SendKeys("Infinity");
            driver.FindElement(By.Id("calcButton")).Click();
            
            Assert.That(driver.FindElement(By.CssSelector("i")).Text, Is.EqualTo("invalid calculation"));
            driver.FindElement(By.Id("resetButton")).Click();
        }

        [Test, Order(21), Category("EdgeCasesTests")]
        public void NegativeNumbersOperations()
        {
            driver.FindElement(By.Id("number1")).SendKeys("-9");
            {
                var dropdown = driver.FindElement(By.Id("operation"));
                dropdown.FindElement(By.XPath("//option[. = '/ (divide)']")).Click();
            }
            driver.FindElement(By.Id("number2")).SendKeys("-3");
            driver.FindElement(By.Id("calcButton")).Click();
            
            Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("3"));
            driver.FindElement(By.Id("resetButton")).Click();
            
            driver.FindElement(By.Id("number1")).SendKeys("-6");
            {
                var dropdown = driver.FindElement(By.Id("operation"));
                dropdown.FindElement(By.XPath("//option[. = '+ (sum)']")).Click();
            }
            driver.FindElement(By.Id("number2")).SendKeys("-4");
            driver.FindElement(By.Id("calcButton")).Click();
            
            Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("-10"));
            driver.FindElement(By.Id("resetButton")).Click();
            
            driver.FindElement(By.Id("number1")).SendKeys("-13");
            {
                var dropdown = driver.FindElement(By.Id("operation"));
                dropdown.FindElement(By.XPath("//option[. = '* (multiply)']")).Click();
            }
            driver.FindElement(By.Id("number2")).SendKeys("-2");
            driver.FindElement(By.Id("calcButton")).Click();
            
            Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("26"));
            driver.FindElement(By.Id("resetButton")).Click();
            
            driver.FindElement(By.Id("number1")).SendKeys("-13");
            {
                var dropdown = driver.FindElement(By.Id("operation"));
                dropdown.FindElement(By.XPath("//option[. = '* (multiply)']")).Click();
            }
            driver.FindElement(By.Id("number2")).SendKeys("2");
            driver.FindElement(By.Id("calcButton")).Click();
            
            Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("-26"));
            driver.FindElement(By.Id("resetButton")).Click();
            
            driver.FindElement(By.Id("number1")).SendKeys("6");
            {
                var dropdown = driver.FindElement(By.Id("operation"));
                dropdown.FindElement(By.XPath("//option[. = '/ (divide)']")).Click();
            }
            driver.FindElement(By.Id("number2")).SendKeys("-3");
            driver.FindElement(By.Id("calcButton")).Click();
            
            Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("-2"));
            driver.FindElement(By.Id("resetButton")).Click();
        }

        [Test, Order(22), Category("EdgeCasesTests")]
        public void DivideByZero()
        {
            driver.FindElement(By.Id("number1")).SendKeys("9");
            {
                var dropdown = driver.FindElement(By.Id("operation"));
                dropdown.FindElement(By.XPath("//option[. = '/ (divide)']")).Click();
            }
            driver.FindElement(By.Id("number2")).SendKeys("0");
            driver.FindElement(By.Id("calcButton")).Click();
            
            Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("Infinity"));
            driver.FindElement(By.Id("resetButton")).Click();
            
            driver.FindElement(By.Id("number1")).SendKeys("0");
            {
                var dropdown = driver.FindElement(By.Id("operation"));
                dropdown.FindElement(By.XPath("//option[. = '/ (divide)']")).Click();
            }
            driver.FindElement(By.Id("number2")).SendKeys("5");
            driver.FindElement(By.Id("calcButton")).Click();
            
            Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("0"));
        }

        [Test, Order(23), Category("EdgeCasesTests")]
        public void DecimalNumbersOperations()
        {
            driver.FindElement(By.Id("number1")).SendKeys("1.8");
            {
                var dropdown = driver.FindElement(By.Id("operation"));
                dropdown.FindElement(By.XPath("//option[. = '- (subtract)']")).Click();
            }
            driver.FindElement(By.Id("number2")).SendKeys("0.3");
            driver.FindElement(By.Id("calcButton")).Click();
            
            Assert.That(driver.FindElement(By.CssSelector("h4")).Text, Is.EqualTo("Result:"));
            Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("1.5"));
            driver.FindElement(By.Id("resetButton")).Click();
            
            driver.FindElement(By.Id("number1")).SendKeys("5.7");
            {
                var dropdown = driver.FindElement(By.Id("operation"));
                dropdown.FindElement(By.XPath("//option[. = '+ (sum)']")).Click();
            }
            driver.FindElement(By.Id("number2")).SendKeys("1.5");
            driver.FindElement(By.Id("calcButton")).Click();
            
            Assert.That(driver.FindElement(By.CssSelector("h4")).Text, Is.EqualTo("Result:"));
            Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("7.2"));
            driver.FindElement(By.Id("resetButton")).Click();
            
            driver.FindElement(By.Id("number1")).SendKeys("14.9");
            {
                var dropdown = driver.FindElement(By.Id("operation"));
                dropdown.FindElement(By.XPath("//option[. = '* (multiply)']")).Click();
            }
            driver.FindElement(By.Id("number2")).SendKeys("0.0000056");
            driver.FindElement(By.Id("calcButton")).Click();
            
            Assert.That(driver.FindElement(By.CssSelector("h4")).Text, Is.EqualTo("Result:"));
            Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("0.00008344"));
            driver.FindElement(By.Id("resetButton")).Click();
            
            driver.FindElement(By.Id("number1")).SendKeys("900.9");
            {
                var dropdown = driver.FindElement(By.Id("operation"));
                dropdown.FindElement(By.XPath("//option[. = '/ (divide)']")).Click();
            }
            driver.FindElement(By.Id("number2")).SendKeys("3.003");
            driver.FindElement(By.Id("calcButton")).Click();
            
            Assert.That(driver.FindElement(By.CssSelector("h4")).Text, Is.EqualTo("Result:"));
            Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("300"));
        }
    }
}
