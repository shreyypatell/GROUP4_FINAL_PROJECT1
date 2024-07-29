using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyAnimeListTests
{
    [TestFixture]
    public class Tests
    {
        private IWebDriver driver;
        private WebDriverWait wait;
        private EmailService emailService;
        private String testEmail,
        testUserName = "test63858142", testPassword = "#Test@123";
        string downloadPath = "C:\\Users\\91886\\Downloads\\";


        [SetUp]
        public async Task Setup()
        {

            // Change user agent to mimic a real browser

            driver = new FirefoxDriver();

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(100));

            emailService = new EmailService();

            testEmail = await emailService.RegisterAndGetEmailAsync();


        }

        [TearDown]
        public void Teardown()
        {
            driver.Quit();
        }

        // User Registration and Authentication
        [Test]
        public async Task UA01UserRegistration_ValidInput_SuccessfulRegistration()
        {
            string testEmail = await emailService.GenerateTemporaryEmailAndGetEmailAsync(testPassword);
            string testUserName = "test" + DateTime.Now.Ticks.ToString().Substring(0, 12); ;

            driver.Navigate().GoToUrl("https://myanimelist.net/register.php");

            // Wait until the page is fully loaded
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            // Enter email
            IWebElement emailField = driver.FindElement(By.Id("loginEmail"));
            emailField.SendKeys(testEmail);

            // Enter username
            IWebElement usernameField = driver.FindElement(By.Name("user_name"));
            usernameField.SendKeys(testUserName);

            // Enter password
            IWebElement passwordField = driver.FindElement(By.Id("password"));
            passwordField.SendKeys(testPassword);

            // Select birthday
            SelectElement monthSelect = new SelectElement(driver.FindElement(By.Name("birthday[month]")));
            monthSelect.SelectByValue("1");  // January

            SelectElement daySelect = new SelectElement(driver.FindElement(By.Name("birthday[day]")));
            daySelect.SelectByValue("1");    // 1st day

            SelectElement yearSelect = new SelectElement(driver.FindElement(By.Name("birthday[year]")));
            yearSelect.SelectByValue("2000"); // Year 2000

            // 'Create Account' button
            IWebElement createAccountButton = driver.FindElement(By.Id("create-account"));

            // Wait until the 'Create Account' button is enabled
            wait.Until(driver => createAccountButton.Enabled);
            createAccountButton.Click();

            // Wait for the success message to appear
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id("ga_goodresult")));

            // Verify that the success message is displayed
            IWebElement successMessage = driver.FindElement(By.Id("ga_goodresult"));
            ClassicAssert.IsTrue(successMessage.Displayed);

        }

        [Test]
        public void UA02Login_ValidCredentials_SuccessfulLogin()
        {
            driver.Navigate().GoToUrl("https://myanimelist.net/login.php");

            // Wait until the page is fully loaded
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            // Enter username
            IWebElement usernameField = driver.FindElement(By.Id("loginUserName"));
            usernameField.SendKeys(testUserName);

            // Enter password
            IWebElement passwordField = driver.FindElement(By.Id("login-password"));
            passwordField.SendKeys(testPassword);

            // Scroll the login button into view
            IWebElement loginButton = driver.FindElement(By.CssSelector("input.btn-form-submit[type='submit']"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", loginButton);
            loginButton.Click();
            // Wait until the URL changes to the home page
            wait.Until(driver => driver.Url.Equals("https://myanimelist.net/"));

            // Wait until the profile link is visible
            wait.Until(driver => driver.FindElement(By.CssSelector("a.header-profile-link")).Displayed);

            // Verify the profile link contains the expected username
            IWebElement profileLink = driver.FindElement(By.CssSelector("a.header-profile-link"));
            ClassicAssert.IsTrue(profileLink.Text.Contains(testUserName), "Profile link does not contain the expected username.");

        }

        [Test]
        public void UA03Login_InvalidCredentials_ErrorDisplayed()
        {
            string invalidTestPassword = "#ME@123Invalid";
            driver.Navigate().GoToUrl("https://myanimelist.net/login.php");

            // Wait until the page is fully loaded
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            // Enter username
            IWebElement usernameField = driver.FindElement(By.Id("loginUserName"));
            usernameField.SendKeys(testUserName);

            // Enter password
            IWebElement passwordField = driver.FindElement(By.Id("login-password"));
            passwordField.SendKeys(invalidTestPassword);

            // Scroll the login button into view
            IWebElement loginButton = driver.FindElement(By.CssSelector("input.btn-form-submit[type='submit']"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", loginButton);

            loginButton.Click();

            // Wait for the error message to be displayed
            wait.Until(driver =>
            {
                IWebElement errorMessageElement = driver.FindElement(By.CssSelector("div.badresult"));
                return errorMessageElement.Displayed && errorMessageElement.Text.Contains("Your username or password is incorrect.");
            });


            // Verify the error message
            string errorMessage = driver.FindElement(By.CssSelector("div.badresult")).Text;
            ClassicAssert.IsTrue(errorMessage.Contains("Your username or password is incorrect."), "Error message is not displayed or is incorrect.");

            // Optionally verify that the URL is still the login page
            ClassicAssert.AreEqual("https://myanimelist.net/login.php", driver.Url);
        }

        [Test]
        public void UA04PasswordReset_ValidInput_SuccessMessage()
        {
            string testUserName = "test638581436666";
            string testEmail = "d5af0476-f87a-4c86-b807-167d390976e1@belgianairways.com";

            driver.Navigate().GoToUrl("https://myanimelist.net/password.php");

            // Wait until the page is fully loaded
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            // Enter username
            IWebElement usernameField = driver.FindElement(By.Name("user_name"));
            usernameField.SendKeys(testUserName);

            // Enter email
            IWebElement emailField = driver.FindElement(By.Name("email"));
            emailField.SendKeys(testEmail);

            // Scroll the request password button into view and click it
            IWebElement requestPasswordButton = driver.FindElement(By.CssSelector("input.btn-form-submit[type='submit']"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", requestPasswordButton);
            requestPasswordButton.Click();

            // Wait for the success message to be visible
            wait.Until(driver =>
            {
                try
                {
                    IWebElement successMessageElement = driver.FindElement(By.CssSelector("div.goodresult"));
                    return successMessageElement.Displayed && successMessageElement.Text.Contains("An e-mail has been successfully sent to");
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
            });

            // Verify the success message
            string successMessage = driver.FindElement(By.CssSelector("div.goodresult")).Text;
            ClassicAssert.IsTrue(successMessage.Contains("An e-mail has been successfully sent to"), "Success message is not displayed or is incorrect.");

            // Optionally, verify the URL remains the same or other final checks
            ClassicAssert.AreEqual("https://myanimelist.net/password.php", driver.Url);
        }

        [Test]
        public void UA05ForgotUserName_ValidEmail_SuccessMessage()
        {
            driver.Navigate().GoToUrl("https://myanimelist.net/password.php?username=1");

            // Wait until the page is fully loaded
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            // Enter email
            IWebElement emailField = driver.FindElement(By.Name("email"));
            emailField.SendKeys(testEmail);

            // Scroll the request password button into view and click it
            IWebElement requestPasswordButton = driver.FindElement(By.CssSelector("input.btn-form-submit[type='submit']"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", requestPasswordButton);
            requestPasswordButton.Click();

            // Wait for the success message to be visible
            wait.Until(driver =>
            {
                try
                {
                    var successMessageElement = driver.FindElement(By.CssSelector("div.goodresult"));
                    return successMessageElement.Displayed && successMessageElement.Text.Contains("An e-mail has been successfully sent.");
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
            });

            // Verify the success message
            var successMessage = driver.FindElement(By.CssSelector("div.goodresult")).Text;
            ClassicAssert.IsTrue(successMessage.Contains("An e-mail has been successfully sent."), "Success message is not displayed or is incorrect.");

            // Optionally, verify the URL remains the same or other final checks
            ClassicAssert.AreEqual("https://myanimelist.net/password.php?username=1", driver.Url);
        }

        [Test]
        public void PM01UpdateProfileInformation_NewBio_Successful()
        {
            Login();

            // Navigate to the profile settings page
            driver.Navigate().GoToUrl("https://myanimelist.net/editprofile.php");

            // Wait until the page is fully loaded
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            // Locate the "About Me" textarea and update its value
            IWebElement aboutMeTextArea = driver.FindElement(By.Id("classic-about-me-textarea"));
            string newBio = "New Bio"; // The new profile information to be entered
            aboutMeTextArea.Clear(); // Clear any existing text
            aboutMeTextArea.SendKeys(newBio); // Enter the new bio

            // Locate the submit button and click it
            IWebElement submitButton = driver.FindElement(By.Name("submit"));
            submitButton.Click();

            // Wait for the profile update confirmation
            wait.Until(driver => driver.FindElement(By.CssSelector("div.goodresult")).Displayed &&
                      driver.FindElement(By.CssSelector("div.goodresult")).Text.Contains("Successfully updated your profile."));


            // Verify that the success message is displayed correctly
            string successMessage = driver.FindElement(By.CssSelector("div.goodresult")).Text;
            ClassicAssert.IsTrue(successMessage.Contains("Successfully updated your profile."), "Success message is not displayed or is incorrect.");

            // Navigate to the profile page to verify the updated bio
            driver.Navigate().GoToUrl("https://myanimelist.net/profile/" + testUserName); // Replace with actual profile URL

            // Locate the updated bio on the profile page using the provided XPath
            IWebElement bioElement = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div[3]/div[2]/div/div[2]/div[1]/div/table/tbody/tr/td/div"));

            // Verify the updated bio
            string displayedBio = bioElement.Text.Trim();
            ClassicAssert.AreEqual("New Bio", displayedBio, "Profile information was not updated successfully.");
        }

        [Test]
        public void PM02UpdateCommentSettings_DisallowComments_Successful()
        {
            Login();

            // Navigate to the profile settings page
            driver.Navigate().GoToUrl("https://myanimelist.net/editprofile.php");

            // Wait until the page is fully loaded
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            // Locate the comment settings dropdown
            IWebElement commentSettingsDropdown = driver.FindElement(By.Name("mem_comments"));

            // Create a SelectElement instance to interact with the dropdown
            var selectElement = new SelectElement(commentSettingsDropdown);

            // Select an option, "Disallow Comments"
            selectElement.SelectByValue("0");

            // Locate and click the submit button
            IWebElement submitButton = driver.FindElement(By.Name("submit"));
            submitButton.Click();

            // Wait for success message
            wait.Until(driver => driver.FindElement(By.CssSelector("div.goodresult")).Displayed &&
                                 driver.FindElement(By.CssSelector("div.goodresult")).Text.Contains("Successfully updated your profile."));

            // Assert success message
            ClassicAssert.IsTrue(driver.FindElement(By.CssSelector("div.goodresult")).Displayed);
            ClassicAssert.IsTrue(driver.FindElement(By.CssSelector("div.goodresult")).Text.Contains("Successfully updated your profile."));
        }

        [Test]
        public void PM03UpdateGender_NonBinary_Successful()
        {
            Login();

            // Navigate to profile settings
            driver.Navigate().GoToUrl("https://myanimelist.net/editprofile.php");

            // Select gender dropdown and update to 'Non-Binary'
            var genderDropdown = new SelectElement(driver.FindElement(By.Name("gender")));
            genderDropdown.SelectByText("Non-Binary");

            // Click submit button
            driver.FindElement(By.Name("submit")).Click();

            // Wait for success message
            wait.Until(driver => driver.FindElement(By.CssSelector("div.goodresult")).Displayed &&
                                 driver.FindElement(By.CssSelector("div.goodresult")).Text.Contains("Successfully updated your profile."));

            // Assert success message
            ClassicAssert.IsTrue(driver.FindElement(By.CssSelector("div.goodresult")).Displayed);
            ClassicAssert.IsTrue(driver.FindElement(By.CssSelector("div.goodresult")).Text.Contains("Successfully updated your profile."));
        }

        [Test]
        public void PM04UpdateBirthday_BirthDay_Successful()
        {
            Login();

            // Navigate to profile settings
            driver.Navigate().GoToUrl("https://myanimelist.net/editprofile.php");

            // Select birthday dropdowns
            var monthDropdown = new SelectElement(driver.FindElement(By.Name("bmonth")));
            monthDropdown.SelectByText("Dec");

            var dayDropdown = new SelectElement(driver.FindElement(By.Name("bday")));
            dayDropdown.SelectByText("25");

            var yearDropdown = new SelectElement(driver.FindElement(By.Name("byear")));
            yearDropdown.SelectByText("2000");

            // Click submit button
            driver.FindElement(By.Name("submit")).Click();

            // Wait for success message
            wait.Until(driver => driver.FindElement(By.CssSelector("div.goodresult")).Displayed &&
                                 driver.FindElement(By.CssSelector("div.goodresult")).Text.Contains("Successfully updated your profile."));

            // Assert success message
            ClassicAssert.IsTrue(driver.FindElement(By.CssSelector("div.goodresult")).Displayed);
            ClassicAssert.IsTrue(driver.FindElement(By.CssSelector("div.goodresult")).Text.Contains("Successfully updated your profile."));
        }

        [Test]
        public void PM05UpdateLocation_NewYorkNY_Successful()
        {
            Login();

            // Navigate to profile settings
            driver.Navigate().GoToUrl("https://myanimelist.net/editprofile.php");

            // Input new location
            IWebElement locationInput = driver.FindElement(By.Name("location"));
            locationInput.Clear();
            locationInput.SendKeys("New York, NY");

            // Click submit button
            driver.FindElement(By.Name("submit")).Click();

            // Wait for success message
            wait.Until(driver => driver.FindElement(By.CssSelector("div.goodresult")).Displayed &&
                                 driver.FindElement(By.CssSelector("div.goodresult")).Text.Contains("Successfully updated your profile."));

            // Assert success message
            ClassicAssert.IsTrue(driver.FindElement(By.CssSelector("div.goodresult")).Displayed);
            ClassicAssert.IsTrue(driver.FindElement(By.CssSelector("div.goodresult")).Text.Contains("Successfully updated your profile."));
        }

        [Test]
        public void PM06UpdateExternalLinks_Links_Successful()
        {
            Login();

            // Navigate to profile settings
            driver.Navigate().GoToUrl("https://myanimelist.net/editprofile.php");

            // Input new external links
            var externalLinksTextarea = driver.FindElement(By.Name("external_links"));
            externalLinksTextarea.Clear();
            externalLinksTextarea.SendKeys("https://github.com/username\nhttps://twitter.com/username");

            // Click submit button
            driver.FindElement(By.Name("submit")).Click();

            // Wait for success message
            wait.Until(driver => driver.FindElement(By.CssSelector("div.goodresult")).Displayed &&
                                 driver.FindElement(By.CssSelector("div.goodresult")).Text.Contains("Successfully updated your profile."));

            // Assert success message
            ClassicAssert.IsTrue(driver.FindElement(By.CssSelector("div.goodresult")).Displayed);
            ClassicAssert.IsTrue(driver.FindElement(By.CssSelector("div.goodresult")).Text.Contains("Successfully updated your profile."));
        }
    }

}
