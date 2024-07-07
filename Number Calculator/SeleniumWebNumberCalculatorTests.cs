using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

namespace Number_Calculator
{
    // [TestFixture("Chrome")]
    // [TestFixture("Edge")]
    // [TestFixture("Firefox")]
    // [TestFixture("Brave")]
    // [TestFixture("Opera")]
    // [Parallelizable(ParallelScope.All)]

    public class NumberCalculatorTests
    {
        private readonly string browser;
        public IWebDriver driver;

        public NumberCalculatorTests(string browser)
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
            driver.Quit();
            driver.Dispose();
        }

        private IWebDriver GetWebDriver(string browser)
        {
            switch (browser)
            {
                case "Chrome":
                    var chromeOptions = new ChromeOptions();
                    chromeOptions.AddArgument("--headless=new");
                    return new ChromeDriver(chromeOptions);
                    // return new ChromeDriver();

                case "Edge":
                    var edgeOptions = new EdgeOptions();
                    edgeOptions.AddArgument("--headless");
                    return new EdgeDriver(edgeOptions);
                    // return new EdgeDriver();

                case "Firefox":
                    // Specify the absolute path to geckodriver.exe
                    var geckoDriverPath = @"C:\WebDrivers\geckodriver.exe";
                    // Specify the path to the Firefox binary
                    var firefoxBinaryPath = @"C:\Program Files (x86)\Mozilla Firefox\firefox.exe";

                    var firefoxOptions = new FirefoxOptions();
                    firefoxOptions.AddArgument("--headless");
                    firefoxOptions.BinaryLocation = firefoxBinaryPath;

                    var firefoxService = FirefoxDriverService.CreateDefaultService(geckoDriverPath);
                    firefoxService.Host = "::1"; // Use IPv6 loopback address

                    return new FirefoxDriver(firefoxService, firefoxOptions);

                case "Brave":
                    var braveOptions = new ChromeOptions();
                    braveOptions.BinaryLocation = @"C:\Program Files\BraveSoftware\Brave-Browser\Application\brave.exe";
                    braveOptions.AddArgument("--headless=new");
                    return new ChromeDriver(braveOptions);
                    // return new ChromeDriver();

                case "Opera":
                    var operaOptions = new ChromeOptions();
                    operaOptions.BinaryLocation = @"C:\Users\123\AppData\Local\Programs\Opera\launcher.exe";
                    operaOptions.AddArgument("--no-first-run");
                    operaOptions.AddArgument("--disable-popup-blocking");
                    operaOptions.AddArgument("--disable-notifications");
                    operaOptions.AddArgument("--disable-gpu");
                    operaOptions.AddArgument("--no-sandbox");
                    operaOptions.AddArgument("--disable-infobars");
                    operaOptions.AddArgument("--disable-extensions");
                    operaOptions.AddArgument("--disable-dev-shm-usage");
                    operaOptions.AddArgument("--disable-software-rasterizer");
                    operaOptions.AddArgument("--mute-audio");
                    operaOptions.AddArgument("--headless");

                    // Use ChromeDriver with OperaOptions
                    var operaDriverPath = @"C:\WebDrivers\operadriver.exe";
                    var operaService = ChromeDriverService.CreateDefaultService(operaDriverPath);
                    return new ChromeDriver(operaService, operaOptions);

                default:
                    throw new ArgumentException($"Browser not supported: {browser}");
            }
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