# Accounts Database EfCore Interface

## Description
Abstraction for interacting with the accounts database using entity framework core.

## Why EfCore?
### Change Detection
A core feature of EfCore is its change detection system. This system enables write operations to 
ignore unchanged columns, thereby reducing data transfer magnitude and write speed. 
It is possible to write plain sql or sprocs for every write operation, for example a sproc that
sets an account's password, password last changed date and security stamp at once. While far more 
performant, writing such sprocs is time consuming and not worthwhile for simple queries.
### Concurrency
If a row has a column with the Sql Server type RowVersion, EfCore automatically enforces optimistic 
concurrency control. Application server side concurrency control is necessary since token/password 
checks are not easily done on the database. 
