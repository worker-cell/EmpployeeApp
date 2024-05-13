CREATE TABLE [dbo].[Employees]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [Payroll_Number] NVARCHAR(10) NOT NULL, 
    [Forenames] NVARCHAR(50) NOT NULL, 
    [Surname] NVARCHAR(50) NOT NULL, 
    [Date_of_Birth] DATE NOT NULL, 
    [Telephone] NVARCHAR(20) NOT NULL, 
    [Mobile] NVARCHAR(20) NOT NULL, 
    [Address] NVARCHAR(50) NOT NULL, 
    [Address_2] NVARCHAR(50) NOT NULL, 
    [Postcode] NVARCHAR(20) NOT NULL, 
    [EMail_Home] NVARCHAR(320) NOT NULL, 
    [Start_Date] DATE NOT NULL
)
