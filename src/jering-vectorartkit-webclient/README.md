# Vector Art Kit Web Client

## Description
An Angular 2 web client for Vector Art Kit.

## Architecture
This project follows the architectural style recommended by Angular 2.

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

