# Vector Art Kit Web Client

## Description
An Angular 2 web client for Vector Art Kit.

## Architecture
### Security
#### Csrf protection
- If anti-forgery cookies are not available when app component initializes, a request is sent to retrieve them.
- Until the cookies arrive, post requests are paused. 
- When an account is logged into, fresh cookies are sent by the api.
- When an account is logged out of, fresh cookies are sent by the api.
- If a request fails anti-forgery validation, the api sends fresh cookies and the request is retried.

Note that cross origin resource sharing (CORS) must be setup so as to prevent other domains from accessing the request token
cookie.
#### WithCredentials
All XmlHttpRequests are sent with withCredentials set to true. This means the application cookie is always attched if it is
available. This is necessary for authentication.
## Style
In general, this project follows the styles recommended by Angular 2 (https://angular.io/docs/ts/latest/guide/style-guide.html).
The following are styles specific to this project.
### Naming
#### Components
Verb or Noun. For example log-in or account-details. Names should be verbose and relevant to users. 
### Disposal
Any type that utilizes observables, subjects, event emitters or any other persistent types must have a dispose function that releases resources.
Dispose should be called on these objects in ngOnDestroy of the component that creates these objects. In addition, ngOnDestroy must
itself release resources that the component owns.
### Error Handling
Angular2  throws errors where necessary. These errors are caught by its ErrorHandler 
class and printed to the console. This project needs some way to catch errors globally and to navigate
to ErrorComponent on errors.

## Testing Methodology

### Stubs
To keep tests DRY, stubs must be created for commonly used services and components. To facilitate 
easy importing, stubs must:
- Be located in src/testing.
- Have file names ending in ".stub".  
- Export class names that follow the format Stub\<service/component name\>.

Note that stubs are only necessary for injected types. Most types can just be spied on.

### Specs

#### Component Specs
Component specs must test the following:
- Every function.
- All bindings.
- Rendering that utilizes control structures (ngIf, ngSelect etc).
  - Why?
    - Control structures utilize non strongly typed expressions. Also, multiple levels of nested control structures
      can be hard to verify through visual inspection.
Style tips:
- Avoid calling detectChanges unecessarily to minimize test durations.

#### Style Tips For All Specs
- Only test 1 outcome of a binary control flow statement. Consider the following if statement: if condition x is true, 
  operation y occurs. It can be inferred that when condition x is false, y cannot possibly occur. Therefore it is only
  necessary to test either for when x is true or when x is false.

