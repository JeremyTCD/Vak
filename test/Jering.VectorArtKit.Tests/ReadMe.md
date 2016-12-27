- Selenium.Webdriver.ChromeDriver does not work for .net core project file structure yet.
- Selenium.Webdriver does not target .net core yet.
- Once these are updated, delete manually added ChromeDriver.exe and rebuild to ensure that Selenium.Webdriver.ChromeDriver is working. 
  Also, set target framework in project.json to netcoreapp1.1
    - Emails should then be tested more thoroughly using string resources from the web api.
- Also this project's symbols cant seem to be loaded dynamically for tests, the only difference is its file structure due to targetting net45. 
  After updating to netcoreapp1.0 this should be checked.