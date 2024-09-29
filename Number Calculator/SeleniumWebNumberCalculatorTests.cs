using System;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;

namespace Number_Calculator
{
    [TestFixture("Chrome")]
    //[TestFixture("Edge")]
    //[TestFixture("Firefox")]
    //[TestFixture("Brave")]
    //[TestFixture("Opera")]
    //[Parallelizable(ParallelScope.All)]
    public class NumberCalculatorTests
    {
        private readonly string browser;
        private IWebDriver? driver;

        // ExtentReports variables
        private static AventStack.ExtentReports.ExtentReports? extent;
        private AventStack.ExtentReports.ExtentTest? test;


        // Parameterless constructor
        // public NumberCalculatorTests()
        // {
        // This constructor can be used by NUnit to instantiate the test fixture.
        //  May leave it empty or add initialization logic if needed.
        // }

        // Constructor with a parameter for the browser
        public NumberCalculatorTests(string browser)
        {
            this.browser = browser ?? throw new ArgumentNullException(nameof(browser));
        }

        [OneTimeSetUp]
        public void SetupReport()
        {
            extent = new AventStack.ExtentReports.ExtentReports();
            var htmlReporter = new ExtentHtmlReporter(Path.Combine(Directory.GetCurrentDirectory(), "TestResults.html"));
            htmlReporter.Config.Theme = AventStack.ExtentReports.Reporter.Configuration.Theme.Dark;
            htmlReporter.Config.DocumentTitle = "Number Calculator Test Report";
            htmlReporter.Config.ReportName = "Selenium Test Results";
            extent.AttachReporter(htmlReporter);
        }

        [SetUp]
        public void Setup()
        {
            test = extent?.CreateTest(TestContext.CurrentContext.Test.Name);

            var Url = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com/number-calculator/";
            driver = GetWebDriver(browser);
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl(Url);

            // Wait until the page is fully loaded
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
        }

        [OneTimeTearDown]
        public void TearDownReport()
        {
            extent?.Flush();
        }

        [TearDown]
        public void TearDown()
        {
            var status = TestContext.CurrentContext.Result.Outcome.Status;
            var stacktrace = TestContext.CurrentContext.Result.StackTrace;
            var errorMessage = TestContext.CurrentContext.Result.Message;

            if (status == NUnit.Framework.Interfaces.TestStatus.Failed)
            {
                test?.Fail($"Test failed with message: {errorMessage}");
                test?.Fail($"Stack Trace: {stacktrace}");

                // Capture screenshot on failure
                string screenshot = CaptureScreenshot();
                test?.AddScreenCaptureFromPath(screenshot);
            }
            else if (status == NUnit.Framework.Interfaces.TestStatus.Passed)
            {
                test?.Pass("Test passed");
            }

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

                    // var firefoxOptions = new FirefoxOptions();
                    // firefoxOptions.AddArgument("--headless");

                    // // Specify the path to the Firefox binary if needed
                    // firefoxOptions.BinaryLocation = "/usr/bin/firefox";

                    // var firefoxService = FirefoxDriverService.CreateDefaultService();
                    // firefoxService.Host = "::1"; // Use IPv6 loopback address

                    // Increase command timeout
                    // var commandTimeout = TimeSpan.FromMinutes(3);
                    
                    // return new FirefoxDriver(firefoxService, firefoxOptions, commandTimeout);
                    
                    return new FirefoxDriver(firefoxService, firefoxOptions);

                case "Brave":
                    var braveDriverPath = GetDriverPath("brave");
                    var braveOptions = new ChromeOptions();
                    braveOptions.BinaryLocation = braveDriverPath;
                    braveOptions.AddArgument("--headless");
                    return new ChromeDriver(braveOptions);

                case "Opera":
                    var operaBinaryPath = GetDriverPath("opera.exe");
                    var chromeOptionsForOpera = new ChromeOptions();
                    chromeOptionsForOpera.BinaryLocation = operaBinaryPath;
                    chromeOptionsForOpera.AddArgument("--headless");
                    return new ChromeDriver(chromeOptionsForOpera);

                default:
                    throw new ArgumentException($"Browser not supported: {browser}");
            }
        }

        private string GetDriverPath(string driverFileName)
        {
            var driversDirectory = Environment.GetEnvironmentVariable("DRIVERS_PATH") ?? "./WebDrivers";
            return System.IO.Path.Combine(driversDirectory, driverFileName);
        }

        private string CaptureScreenshot()
        {
            string screenshotPath = $"screenshot_{DateTime.Now:yyyyMMddHHmmss}.png";
            ((ITakesScreenshot)driver)?.GetScreenshot().SaveAsFile(screenshotPath);
            return screenshotPath;
        }

        [Test, Order(1), Category("InputValidation")]
        public void ValidateFieldsAcceptOnlyNumbers()
        {
            try
            {
                driver?.FindElement(By.Id("number1")).SendKeys("5");
                var dropdown = driver?.FindElement(By.Id("operation"));
                dropdown?.FindElement(By.XPath("//option[. = '+ (sum)']")).Click();

                driver?.FindElement(By.Id("number2")).SendKeys("10");
                driver?.FindElement(By.Id("calcButton")).Click();

                string result = driver?.FindElement(By.CssSelector("pre")).Text ?? string.Empty;
                Assert.That(result, Is.EqualTo("15"));

                test?.Info($"Calculation result: {result}");
            }
            catch (Exception ex)
            {
                test?.Fail($"Test failed with exception: {ex.Message}");
                throw;
            }
        }

        //[Test, Order(2), Category("InputValidation")]
        //public void ValidateFieldsDontAcceptTextAndSymbols()
        //{
        //    driver.FindElement(By.Id("number1")).SendKeys("~~@%");
        //    var dropdown = driver.FindElement(By.Id("operation"));
            
        //    dropdown.FindElement(By.XPath("//option[. = '+ (sum)']")).Click();
        //    driver.FindElement(By.Id("number2")).SendKeys("cxbzfdb");
            
        //    driver.FindElement(By.Id("calcButton")).Click();
        //    Assert.That(driver.FindElement(By.CssSelector("i")).Text, Is.EqualTo("invalid input"));
        //}

        //[Test, Order(3), Category("InputValidation")]
        //public void ValidateField2CantBeEmpty()
        //{
        //    _test = _extent.CreateTest("InputValidation");

        //    try
        //    {
        //        driver.FindElement(By.Id("number1")).SendKeys("13");

        //        var dropdown = driver.FindElement(By.Id("operation"));
        //        dropdown.FindElement(By.XPath("//option[. = '- (subtract)']")).Click();
            
        //        driver.FindElement(By.Id("number2")).SendKeys(string.Empty);
        //        driver.FindElement(By.Id("calcButton")).Click();
        //        Assert.That(driver.FindElement(By.CssSelector("i")).Text, Is.EqualTo("invalid input"));
        //    }
        //    catch (Exception ex)
        //    {
        //        _test.Fail("Test failed: " + ex.Message);
        //        throw; // Rethrow to ensure NUnit captures the failure
        //    }
        //}

        //[Test, Order(4), Category("InputValidation")]
        //public void ValidateField1CantBeEmpty()
        //{
        //    driver.FindElement(By.Id("number1")).SendKeys("13");
            
        //    var dropdown = driver.FindElement(By.Id("operation"));
        //    dropdown.FindElement(By.XPath("//option[. = '- (subtract)']")).Click();

        //    driver.FindElement(By.Id("number2")).SendKeys(string.Empty);
        //    driver.FindElement(By.Id("calcButton")).Click();
        //    Assert.That(driver.FindElement(By.CssSelector("i")).Text, Is.EqualTo("invalid input"));
        //}

        //[Test, Order(5), Category("InputValidation")]
        //public void ValidateNumbersFieldsCantBeEmpty()
        //{
        //    driver.FindElement(By.Id("number1")).SendKeys(string.Empty);
            
        //    var dropdown = driver.FindElement(By.Id("operation"));
        //    dropdown.FindElement(By.XPath("//option[. = '+ (sum)']")).Click();

        //    driver.FindElement(By.Id("number2")).SendKeys(string.Empty);
        //    driver.FindElement(By.Id("calcButton")).Click();
        //    Assert.That(driver.FindElement(By.CssSelector("i")).Text, Is.EqualTo("invalid input"));
        //}

        //[Test, Order(6), Category("InputValidation")]
        //public void ValidateFieldOperationCantBeEmpty()
        //{
        //    driver.FindElement(By.Id("number1")).SendKeys("16");
        //    driver.FindElement(By.Id("number2")).SendKeys("23");
        //    driver.FindElement(By.Id("calcButton")).Click();
        //    Assert.That(driver.FindElement(By.CssSelector("i")).Text, Is.EqualTo("invalid operation"));
        //}

        //[Test, Order(7), Category("OperationSelection")]
        //public void ValidateOptionSumIsChoosen()
        //{
        //    var dropdown = driver.FindElement(By.Id("operation"));
        //    dropdown.FindElement(By.XPath("//option[. = '+ (sum)']")).Click();
            
        //    string value = driver.FindElement(By.Id("operation")).GetAttribute("value");
        //    Assert.That(value, Is.EqualTo("+"));
        //}

        //[Test, Order(8), Category("OperationSelection")]
        //public void ValidateOptionSubtractIsChoosen()
        //{
        //    var dropdown = driver.FindElement(By.Id("operation"));
        //    dropdown.FindElement(By.XPath("//option[. = '- (subtract)']")).Click();
        //    string value = driver.FindElement(By.Id("operation")).GetAttribute("value");
        //    Assert.That(value, Is.EqualTo("-"));
        //}

        //[Test, Order(9), Category("OperationSelection")]
        //public void ValidateOptionMultiplyIsChoosen()
        //{
        //    var dropdown = driver.FindElement(By.Id("operation"));
        //    dropdown.FindElement(By.XPath("//option[. = '* (multiply)']")).Click();
        //    string value = driver.FindElement(By.Id("operation")).GetAttribute("value");
        //    Assert.That(value, Is.EqualTo("*"));
        //}

        //[Test, Order(10), Category("OperationSelection")]
        //public void ValidateOptionDivideIsChoosen()
        //{
        //    var dropdown = driver.FindElement(By.Id("operation"));
        //    dropdown.FindElement(By.XPath("//option[. = '/ (divide)']")).Click();
            
        //    string value = driver.FindElement(By.Id("operation")).GetAttribute("value");
        //    Assert.That(value, Is.EqualTo("/"));
        //}

        //[Test, Order(11), Category("CalculationsTests")]
        //public void AssertCorrectAddition()
        //{
        //    driver.FindElement(By.Id("number1")).SendKeys("12");
        //    {
        //        string value = driver.FindElement(By.Id("number1")).GetAttribute("value");
        //        Assert.That(value, Is.EqualTo("12"));
        //    }

        //    var dropdown = driver.FindElement(By.Id("operation"));
        //    dropdown.FindElement(By.XPath("//option[. = '+ (sum)']")).Click();

        //    {
        //        string value = driver.FindElement(By.Id("operation")).GetAttribute("value");
        //        Assert.That(value, Is.EqualTo("+"));
        //    }
            
        //    driver.FindElement(By.Id("number2")).SendKeys("13");
            
        //    {
        //        string value = driver.FindElement(By.Id("number2")).GetAttribute("value");
        //        Assert.That(value, Is.EqualTo("13"));
        //    }

        //    driver.FindElement(By.Id("calcButton")).Click();
        //    Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("25"));
        //}

        //[Test, Order(12), Category("CalculationsTests")]
        //public void AssertCorrectSubtraction()
        //{
        //    driver.FindElement(By.Id("number1")).SendKeys("17");
        //    {
        //        string value = driver.FindElement(By.Id("number1")).GetAttribute("value");
        //        Assert.That(value, Is.EqualTo("17"));
        //    }
            
        //    var dropdown = driver.FindElement(By.Id("operation"));
        //    dropdown.FindElement(By.XPath("//option[. = '- (subtract)']")).Click();

        //    {
        //        string value = driver.FindElement(By.Id("operation")).GetAttribute("value");
        //        Assert.That(value, Is.EqualTo("-"));
        //    }

        //    driver.FindElement(By.Id("number2")).SendKeys("10");

        //    {
        //        string value = driver.FindElement(By.Id("number2")).GetAttribute("value");
        //        Assert.That(value, Is.EqualTo("10"));
        //    }

        //    driver.FindElement(By.Id("calcButton")).Click();
        //    Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("7"));
        //}

        //[Test, Order(13), Category("CalculationsTests")]
        //public void AssertCorrectMultiplication()
        //{
        //    driver.FindElement(By.Id("number1")).SendKeys("14");
        //    {
        //        string value = driver.FindElement(By.Id("number1")).GetAttribute("value");
        //        Assert.That(value, Is.EqualTo("14"));
        //    }
            
        //    var dropdown = driver.FindElement(By.Id("operation"));
        //    dropdown.FindElement(By.XPath("//option[. = '* (multiply)']")).Click();

        //    {
        //        string value = driver.FindElement(By.Id("operation")).GetAttribute("value");
        //        Assert.That(value, Is.EqualTo("*"));
        //    }
            
        //    driver.FindElement(By.Id("number2")).SendKeys("8");
            
        //    {
        //        string value = driver.FindElement(By.Id("number2")).GetAttribute("value");
        //        Assert.That(value, Is.EqualTo("8"));
        //    }

        //    driver.FindElement(By.Id("calcButton")).Click();
        //    Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("112"));
        //}

        //[Test, Order(14), Category("CalculationsTests")]
        //public void AssertCorrectDivision()
        //{
        //    driver.FindElement(By.Id("number1")).SendKeys("112");
        //    {
        //        string value = driver.FindElement(By.Id("number1")).GetAttribute("value");
        //        Assert.That(value, Is.EqualTo("112"));
        //    }
            
        //    var dropdown = driver.FindElement(By.Id("operation"));
        //    dropdown.FindElement(By.XPath("//option[. = '/ (divide)']")).Click();

        //    {
        //        string value = driver.FindElement(By.Id("operation")).GetAttribute("value");
        //        Assert.That(value, Is.EqualTo("/"));
        //    }

        //    driver.FindElement(By.Id("number2")).SendKeys("8");

        //    {
        //        string value = driver.FindElement(By.Id("number2")).GetAttribute("value");
        //        Assert.That(value, Is.EqualTo("8"));
        //    }

        //    driver.FindElement(By.Id("calcButton")).Click();
        //    Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("14"));
        //}

        //[Test, Order(15), Category("ResetFunctionalityTests")]
        //public void ResetField1()
        //{
        //    driver.FindElement(By.Id("number1")).SendKeys("12");
        //    {
        //        string value = driver.FindElement(By.Id("number1")).GetAttribute("value");
        //        Assert.That(value, Is.EqualTo("12"));
        //    }
            
        //    driver.FindElement(By.Id("resetButton")).Click();
        //    {
        //        string value = driver.FindElement(By.Id("number1")).GetAttribute("value");
        //        Assert.That(value, Is.EqualTo(string.Empty));
        //    }
        //}

        //[Test, Order(16), Category("ResetFunctionalityTests")]
        //public void ResetField2()
        //{
        //    driver.FindElement(By.Id("number2")).SendKeys("15");
        //    {
        //        string value = driver.FindElement(By.Id("number2")).GetAttribute("value");
        //        Assert.That(value, Is.EqualTo("15"));
        //    }
            
        //    driver.FindElement(By.Id("resetButton")).Click();
        //    {
        //        string value = driver.FindElement(By.Id("number2")).GetAttribute("value");
        //        Assert.That(value, Is.EqualTo(string.Empty));
        //    }
        //}

        //[Test, Order(17), Category("ResetFunctionalityTests")]
        //public void ResetOperationField()
        //{
        //    var dropdown = driver.FindElement(By.Id("operation"));
        //    dropdown.FindElement(By.XPath("//option[. = '* (multiply)']")).Click();

        //    {
        //        string value = driver.FindElement(By.Id("operation")).GetAttribute("value");
        //        Assert.That(value, Is.EqualTo("*"));
        //    }
            
        //    driver.FindElement(By.Id("resetButton")).Click();
        //    {
        //        string value = driver.FindElement(By.Id("operation")).GetAttribute("value");
        //        Assert.That(value, Is.EqualTo("-- select an operation --"));
        //    }
        //}

        //[Test, Order(18), Category("ResetFunctionalityTests")]
        //public void ResetAllFields()
        //{
        //    driver.FindElement(By.Id("number1")).SendKeys("13");
        //    {
        //        string value = driver.FindElement(By.Id("number1")).GetAttribute("value");
        //        Assert.That(value, Is.EqualTo("13"));
        //    }
            
        //    var dropdown = driver.FindElement(By.Id("operation"));
        //    dropdown.FindElement(By.XPath("//option[. = '* (multiply)']")).Click();

        //    {
        //        string value = driver.FindElement(By.Id("operation")).GetAttribute("value");
        //        Assert.That(value, Is.EqualTo("*"));
        //    }
            
        //    driver.FindElement(By.Id("number2")).SendKeys("17");
        //    {
        //        string value = driver.FindElement(By.Id("number2")).GetAttribute("value");
        //        Assert.That(value, Is.EqualTo("17"));
        //    }
           
        //    driver.FindElement(By.Id("resetButton")).Click();
        //    {
        //        string value = driver.FindElement(By.Id("number1")).GetAttribute("value");
        //        Assert.That(value, Is.EqualTo(string.Empty));
        //    }
        //    {
        //        string value = driver.FindElement(By.Id("operation")).GetAttribute("value");
        //        Assert.That(value, Is.EqualTo("-- select an operation --"));
        //    }
        //    {
        //        string value = driver.FindElement(By.Id("number2")).GetAttribute("value");
        //        Assert.That(value, Is.EqualTo(string.Empty));
        //    }
        //}

        //[Test, Order(19), Category("EdgeCasesTests")]
        //public void AddSpacesInNumbersFields()
        //{
        //    driver.FindElement(By.Id("number1")).SendKeys("   9   ");
            
        //    var dropdown = driver.FindElement(By.Id("operation"));
        //    dropdown.FindElement(By.XPath("//option[. = '/ (divide)']")).Click();

        //    driver.FindElement(By.Id("number2")).SendKeys("   3    ");
        //    driver.FindElement(By.Id("calcButton")).Click();
        //    Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("3"));
        //    driver.FindElement(By.Id("resetButton")).Click();
        //}

        //[Test, Order(20), Category("EdgeCasesTests")]
        //public void OperationsWithInfinity()
        //{
        //    driver.FindElement(By.Id("number1")).SendKeys("Infinity");
        //    {
        //        var dropdown = driver.FindElement(By.Id("operation"));
        //        dropdown.FindElement(By.XPath("//option[. = '+ (sum)']")).Click();
        //    }
        //    driver.FindElement(By.Id("number2")).SendKeys("4");
        //    driver.FindElement(By.Id("calcButton")).Click();
            
        //    Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("Infinity"));
        //    driver.FindElement(By.Id("resetButton")).Click();
            
        //    driver.FindElement(By.Id("number1")).SendKeys("Infinity");
        //    {
        //        var dropdown = driver.FindElement(By.Id("operation"));
        //        dropdown.FindElement(By.XPath("//option[. = '+ (sum)']")).Click();
        //    }
        //    driver.FindElement(By.Id("number2")).SendKeys("Infinity");
        //    driver.FindElement(By.Id("calcButton")).Click();
            
        //    Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("Infinity"));
        //    driver.FindElement(By.Id("resetButton")).Click();
            
        //    driver.FindElement(By.Id("number1")).SendKeys("5");
        //    {
        //        var dropdown = driver.FindElement(By.Id("operation"));
        //        dropdown.FindElement(By.XPath("//option[. = '+ (sum)']")).Click();
        //    }
        //    driver.FindElement(By.Id("number2")).SendKeys("Infinity");
        //    driver.FindElement(By.Id("calcButton")).Click();
        //    driver.FindElement(By.Id("result")).Click();
            
        //    Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("Infinity"));
        //    driver.FindElement(By.Id("resetButton")).Click();
            
        //    driver.FindElement(By.Id("number1")).SendKeys("Infinity");
        //    {
        //        var dropdown = driver.FindElement(By.Id("operation"));
        //        dropdown.FindElement(By.XPath("//option[. = '- (subtract)']")).Click();
        //    }
        //    driver.FindElement(By.Id("number2")).SendKeys("3");
        //    driver.FindElement(By.Id("calcButton")).Click();
            
        //    Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("Infinity"));
        //    driver.FindElement(By.Id("resetButton")).Click();
            
        //    driver.FindElement(By.Id("number1")).SendKeys("3");
        //    {
        //        var dropdown = driver.FindElement(By.Id("operation"));
        //        dropdown.FindElement(By.XPath("//option[. = '- (subtract)']")).Click();
        //    }
        //    driver.FindElement(By.Id("number2")).SendKeys("Infinity");
        //    driver.FindElement(By.Id("calcButton")).Click();
        //    driver.FindElement(By.Id("result")).Click();
            
        //    Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("-Infinity"));
        //    driver.FindElement(By.Id("resetButton")).Click();
            
        //    driver.FindElement(By.Id("number1")).SendKeys("Infinity");
        //    {
        //        var dropdown = driver.FindElement(By.Id("operation"));
        //        dropdown.FindElement(By.XPath("//option[. = '- (subtract)']")).Click();
        //    }
        //    driver.FindElement(By.Id("number2")).SendKeys("Infinity");
        //    driver.FindElement(By.Id("calcButton")).Click();
            
        //    Assert.That(driver.FindElement(By.CssSelector("i")).Text, Is.EqualTo("invalid calculation"));
        //    driver.FindElement(By.Id("resetButton")).Click();
            
        //    driver.FindElement(By.Id("number1")).SendKeys("Infinity");
        //    {
        //        var dropdown = driver.FindElement(By.Id("operation"));
        //        dropdown.FindElement(By.XPath("//option[. = '* (multiply)']")).Click();
        //    }
        //    driver.FindElement(By.Id("number2")).SendKeys("5");
        //    driver.FindElement(By.Id("calcButton")).Click();
            
        //    Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("Infinity"));
        //    driver.FindElement(By.Id("resetButton")).Click();
            
        //    driver.FindElement(By.Id("number1")).SendKeys("5");
        //    {
        //        var dropdown = driver.FindElement(By.Id("operation"));
        //        dropdown.FindElement(By.XPath("//option[. = '* (multiply)']")).Click();
        //    }
        //    driver.FindElement(By.Id("number2")).SendKeys("Infinity");
        //    driver.FindElement(By.Id("calcButton")).Click();
            
        //    Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("Infinity"));
        //    driver.FindElement(By.Id("resetButton")).Click();
            
        //    driver.FindElement(By.Id("number1")).SendKeys("Infinity");
        //    {
        //        var dropdown = driver.FindElement(By.Id("operation"));
        //        dropdown.FindElement(By.XPath("//option[. = '* (multiply)']")).Click();
        //    }
        //    driver.FindElement(By.Id("number2")).SendKeys("Infinity");
        //    driver.FindElement(By.Id("calcButton")).Click();
            
        //    Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("Infinity"));
        //    driver.FindElement(By.Id("resetButton")).Click();
            
        //    driver.FindElement(By.Id("number1")).SendKeys("Infinity");
        //    {
        //        var dropdown = driver.FindElement(By.Id("operation"));
        //        dropdown.FindElement(By.XPath("//option[. = '/ (divide)']")).Click();
        //    }
        //    driver.FindElement(By.Id("number2")).SendKeys("3");
        //    driver.FindElement(By.Id("calcButton")).Click();
            
        //    Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("Infinity"));
        //    driver.FindElement(By.Id("resetButton")).Click();
            
        //    driver.FindElement(By.Id("number1")).SendKeys("3");
        //    {
        //        var dropdown = driver.FindElement(By.Id("operation"));
        //        dropdown.FindElement(By.XPath("//option[. = '/ (divide)']")).Click();
        //    }
        //    driver.FindElement(By.Id("number2")).SendKeys("Infinity");
        //    driver.FindElement(By.Id("calcButton")).Click();
            
        //    Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("0"));
        //    driver.FindElement(By.Id("resetButton")).Click();
            
        //    driver.FindElement(By.Id("number1")).SendKeys("Infinity");
        //    {
        //        var dropdown = driver.FindElement(By.Id("operation"));
        //        dropdown.FindElement(By.XPath("//option[. = '/ (divide)']")).Click();
        //    }
        //    driver.FindElement(By.Id("number2")).SendKeys("Infinity");
        //    driver.FindElement(By.Id("calcButton")).Click();
            
        //    Assert.That(driver.FindElement(By.CssSelector("i")).Text, Is.EqualTo("invalid calculation"));
        //    driver.FindElement(By.Id("resetButton")).Click();
        //}

        //[Test, Order(21), Category("EdgeCasesTests")]
        //public void NegativeNumbersOperations()
        //{
        //    driver.FindElement(By.Id("number1")).SendKeys("-9");
        //    {
        //        var dropdown = driver.FindElement(By.Id("operation"));
        //        dropdown.FindElement(By.XPath("//option[. = '/ (divide)']")).Click();
        //    }
        //    driver.FindElement(By.Id("number2")).SendKeys("-3");
        //    driver.FindElement(By.Id("calcButton")).Click();
            
        //    Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("3"));
        //    driver.FindElement(By.Id("resetButton")).Click();
            
        //    driver.FindElement(By.Id("number1")).SendKeys("-6");
        //    {
        //        var dropdown = driver.FindElement(By.Id("operation"));
        //        dropdown.FindElement(By.XPath("//option[. = '+ (sum)']")).Click();
        //    }
        //    driver.FindElement(By.Id("number2")).SendKeys("-4");
        //    driver.FindElement(By.Id("calcButton")).Click();
            
        //    Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("-10"));
        //    driver.FindElement(By.Id("resetButton")).Click();
            
        //    driver.FindElement(By.Id("number1")).SendKeys("-13");
        //    {
        //        var dropdown = driver.FindElement(By.Id("operation"));
        //        dropdown.FindElement(By.XPath("//option[. = '* (multiply)']")).Click();
        //    }
        //    driver.FindElement(By.Id("number2")).SendKeys("-2");
        //    driver.FindElement(By.Id("calcButton")).Click();
            
        //    Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("26"));
        //    driver.FindElement(By.Id("resetButton")).Click();
            
        //    driver.FindElement(By.Id("number1")).SendKeys("-13");
        //    {
        //        var dropdown = driver.FindElement(By.Id("operation"));
        //        dropdown.FindElement(By.XPath("//option[. = '* (multiply)']")).Click();
        //    }
        //    driver.FindElement(By.Id("number2")).SendKeys("2");
        //    driver.FindElement(By.Id("calcButton")).Click();
            
        //    Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("-26"));
        //    driver.FindElement(By.Id("resetButton")).Click();
            
        //    driver.FindElement(By.Id("number1")).SendKeys("6");
        //    {
        //        var dropdown = driver.FindElement(By.Id("operation"));
        //        dropdown.FindElement(By.XPath("//option[. = '/ (divide)']")).Click();
        //    }
        //    driver.FindElement(By.Id("number2")).SendKeys("-3");
        //    driver.FindElement(By.Id("calcButton")).Click();
            
        //    Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("-2"));
        //    driver.FindElement(By.Id("resetButton")).Click();
        //}

        //[Test, Order(22), Category("EdgeCasesTests")]
        //public void DivideByZero()
        //{
        //    driver.FindElement(By.Id("number1")).SendKeys("9");
        //    {
        //        var dropdown = driver.FindElement(By.Id("operation"));
        //        dropdown.FindElement(By.XPath("//option[. = '/ (divide)']")).Click();
        //    }
        //    driver.FindElement(By.Id("number2")).SendKeys("0");
        //    driver.FindElement(By.Id("calcButton")).Click();
            
        //    Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("Infinity"));
        //    driver.FindElement(By.Id("resetButton")).Click();
            
        //    driver.FindElement(By.Id("number1")).SendKeys("0");
        //    {
        //        var dropdown = driver.FindElement(By.Id("operation"));
        //        dropdown.FindElement(By.XPath("//option[. = '/ (divide)']")).Click();
        //    }
        //    driver.FindElement(By.Id("number2")).SendKeys("5");
        //    driver.FindElement(By.Id("calcButton")).Click();
            
        //    Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("0"));
        //}

        //[Test, Order(23), Category("EdgeCasesTests")]
        //public void DecimalNumbersOperations()
        //{
        //    driver.FindElement(By.Id("number1")).SendKeys("1.8");
        //    {
        //        var dropdown = driver.FindElement(By.Id("operation"));
        //        dropdown.FindElement(By.XPath("//option[. = '- (subtract)']")).Click();
        //    }
        //    driver.FindElement(By.Id("number2")).SendKeys("0.3");
        //    driver.FindElement(By.Id("calcButton")).Click();
            
        //    Assert.That(driver.FindElement(By.CssSelector("h4")).Text, Is.EqualTo("Result:"));
        //    Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("1.5"));
        //    driver.FindElement(By.Id("resetButton")).Click();
            
        //    driver.FindElement(By.Id("number1")).SendKeys("5.7");
        //    {
        //        var dropdown = driver.FindElement(By.Id("operation"));
        //        dropdown.FindElement(By.XPath("//option[. = '+ (sum)']")).Click();
        //    }
        //    driver.FindElement(By.Id("number2")).SendKeys("1.5");
        //    driver.FindElement(By.Id("calcButton")).Click();
            
        //    Assert.That(driver.FindElement(By.CssSelector("h4")).Text, Is.EqualTo("Result:"));
        //    Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("7.2"));
        //    driver.FindElement(By.Id("resetButton")).Click();
            
        //    driver.FindElement(By.Id("number1")).SendKeys("14.9");
        //    {
        //        var dropdown = driver.FindElement(By.Id("operation"));
        //        dropdown.FindElement(By.XPath("//option[. = '* (multiply)']")).Click();
        //    }
        //    driver.FindElement(By.Id("number2")).SendKeys("0.0000056");
        //    driver.FindElement(By.Id("calcButton")).Click();
            
        //    Assert.That(driver.FindElement(By.CssSelector("h4")).Text, Is.EqualTo("Result:"));
        //    Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("0.00008344"));
        //    driver.FindElement(By.Id("resetButton")).Click();
            
        //    driver.FindElement(By.Id("number1")).SendKeys("900.9");
        //    {
        //        var dropdown = driver.FindElement(By.Id("operation"));
        //        dropdown.FindElement(By.XPath("//option[. = '/ (divide)']")).Click();
        //    }
        //    driver.FindElement(By.Id("number2")).SendKeys("3.003");
        //    driver.FindElement(By.Id("calcButton")).Click();
            
        //    Assert.That(driver.FindElement(By.CssSelector("h4")).Text, Is.EqualTo("Result:"));
        //    Assert.That(driver.FindElement(By.CssSelector("pre")).Text, Is.EqualTo("300"));
        //}
    }
}
