CREATE SCHEMA [Accounts]
    AUTHORIZATION [dbo];

GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Views and stored procedures that provide access for Accounts', @level0type = N'SCHEMA', @level0name = N'Accounts';

