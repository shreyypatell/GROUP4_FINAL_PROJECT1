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

        private void Login()
        {

            // Navigate to the login page
            driver.Navigate().GoToUrl("https://myanimelist.net/login.php");

            // Wait until the page is fully loaded
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            // Enter username
            IWebElement usernameField = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("loginUserName")));
            usernameField.SendKeys(testUserName);

            // Enter password
            IWebElement passwordField = driver.FindElement(By.Id("login-password"));
            passwordField.SendKeys(testPassword);

            // Scroll the login button into view
            IWebElement loginButton = driver.FindElement(By.CssSelector("input.btn-form-submit[type='submit']"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", loginButton);
            loginButton.Click();

            // Wait until redirected to the home page
            wait.Until(driver => driver.Url.Contains("myanimelist.net"));
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            wait.Until(driver => driver.FindElement(By.CssSelector("a.header-profile-link")).Displayed);

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
        public void UA06Logout_LoggedInUser_SuccessfulLogout()
        {
            Login();

            // Find and click the profile dropdown
            IWebElement profileDropdown = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id=\"header-menu\"]/div[8]/a")));

            // Ensure no other elements are obstructing the clickable element
            wait.Until(ExpectedConditions.ElementToBeClickable(profileDropdown));

            // Click the profile dropdown
            profileDropdown.Click();


            // Wait for the logout button to be clickable and click it
            IWebElement logoutButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("form[action='https://myanimelist.net/logout.php'] a")));
            logoutButton.Click();

            // Wait for redirection to the home page after logout
            wait.Until(driver => driver.Url.Equals("https://myanimelist.net/"));

            // Assert that the URL is the home page
            ClassicAssert.AreEqual("https://myanimelist.net/", driver.Url);
        }

        // Anime/Manga Searching and Browsing

        [Test]
        public void AS01BasicSearchFunctionality_SearchTerm_SearchResultsDisplayed()
        {
            // Navigate to the MyAnimeList homepage
            driver.Navigate().GoToUrl("https://myanimelist.net/");

            // Wait until the search bar is fully loaded
            wait.Until(driver => driver.FindElement(By.Id("topSearchText")).Displayed);

            // Enter the search term "Naruto"
            IWebElement searchTextBox = driver.FindElement(By.Id("topSearchText"));
            searchTextBox.SendKeys("Naruto");

            // Submit the search form
            IWebElement searchButton = driver.FindElement(By.Id("topSearchButon"));
            searchButton.Click();

            // Wait until the search results page is loaded
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            // Verify the search results header contains "Search Results for \"Naruto\""
            IWebElement searchResultsHeader = wait.Until(driver => driver.FindElement(By.CssSelector("div.result-header.mb12")));
            StringAssert.Contains("Search Results for \"Naruto\"", searchResultsHeader.Text, "The search results header does not contain 'Naruto'.");

        }

        [Test]
        public void AS02AnimeAdvancedSearchWithFilters_ValidFilters_FilteredResultsDisplayed()
        {
            // Navigate to the anime search page
            driver.Navigate().GoToUrl("https://myanimelist.net/anime.php");

            // Wait for the page to fully load
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            // Perform a basic search
            IWebElement searchBox = driver.FindElement(By.Id("q"));
            searchBox.SendKeys("Naruto");

            // Click on the Advanced Search link
            IWebElement advancedSearchLink = driver.FindElement(By.CssSelector("a[data-ga-click-type='list-add-anime-advanced-window']"));

            // Scroll the advanced search link into view
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", advancedSearchLink);

            // Ensure no other elements are obstructing the clickable element
            wait.Until(ExpectedConditions.ElementToBeClickable(advancedSearchLink));

            // Click the advanced search 
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", advancedSearchLink);

            // Apply filters 
            SelectElement typeFilter = new SelectElement(driver.FindElement(By.Id("filterByType")));
            typeFilter.SelectByValue("1"); // TV

            SelectElement scoreFilter = new SelectElement(driver.FindElement(By.Id("score")));
            scoreFilter.SelectByValue("8"); // Very Good

            SelectElement statusFilter = new SelectElement(driver.FindElement(By.Id("status")));
            statusFilter.SelectByValue("2"); // Finished Airing

            SelectElement ratedFilter = new SelectElement(driver.FindElement(By.Id("r")));
            ratedFilter.SelectByValue("3"); // PG-13 - Teens 13 or older

            // Click the search button
            IWebElement searchButton = driver.FindElement(By.CssSelector("input[data-ga-click-type='list-add-anime-advanced-search']"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", searchButton);
            searchButton.Click();

            // Wait for the search results to load
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            // Verify the search results contain "Naruto"
            bool isNarutoPresent = driver.FindElements(By.CssSelector("a[href*='/anime/20/Naruto']")).Count > 0;

            // Assert that "Naruto" is present in the search results
            ClassicAssert.IsTrue(isNarutoPresent, "\"Naruto\" is not present in the search results.");


        }


    }

}
