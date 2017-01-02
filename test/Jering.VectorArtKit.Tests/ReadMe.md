# Vector Art Kit Tests

## Description
End to end tests for Vector Art Kit. 

## Scope
The goal of these tests is to plug holes in unit and integration tests for constituent projects.
The following features are already covered by tests for constituent projects:
- Entire web api
  - Tested using integration tests.
- Web client binding, rendering, utility functions and asynchronous logic
  - Tested using jasmine specs.

The following are features that must be tested using end to end tests:
- Web client routing
  - Router configuration cannot be tested using jasmine specs. Router configuration refers to declarations made in
    router.ts files. Specifically: 
    - Guards
    - Path-component relationships

## Notes
- Selenium.Webdriver.ChromeDriver does not work for .net core project file structure yet.
- Selenium.Webdriver does not target .net core yet.
- Once these are updated, delete manually added ChromeDriver.exe and rebuild to ensure that Selenium.Webdriver.ChromeDriver is working. 
  Also, set target framework in project.json to netcoreapp1.1
    - Emails should then be tested more thoroughly using string resources from the web api.
- Also this project's symbols cant seem to be loaded dynamically for tests, the only difference is its file structure due to targetting net45. 
  After updating to netcoreapp1.0 this should be checked.