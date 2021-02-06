/********************************************************************************************************************************

	SQL Tips for Application Developers	

	Author				:	elvis baldie, debugyou ltd
	
	Copyright (c) debugyou ltd, January 2021

	Released under Gnu General Public License 3.0

	script				:	install.1.0.0.sql

	Purpose				:	To install all sql objects connected with the dbyDB test programs (version 1.0.0). 
							These five stored procedures are used by the dbyDB test project. In this script we
							assume that whoever is running it has permissions to create a schema, some stored
							procedures and a function.

						Procedures:

							dbydbtest.DoNothing
							dbydbtest.GetAllDataTypes
							dbydbtest.ReturnNumber
							dbydbtest.ThrowDivideByZeroError
							dbydbtest.WaitForSeconds

						Functions:

							dbydbtest.DivideByZero

						Schema:
							dbydbtest



*********************************************************************************************************************************/

--	**NOTE!** It goes without saying, but edit this if you happened to put all of your dbydbtest objects in a database other
--	than SQLforApplicationsProgrammers then you should edit this line
use SQLforApplicationsProgrammers
go


--
--	This is a slightly underhand way to create the dbydbtest schema, but it's apparently the only way we can check if it exists
--	already. 
--
if not exists (select null from sys.schemas where [name] = N'dbydbtest' )
    exec('create schema [dbydbtest]');
go


--
--	Now we create all of the procedures used by the test program for version 1.0.0
--

/********************************************************************************************************************************

	SQL Tips for Application Developers	

	Author				:	elvis baldie, debugyou ltd
	
	Copyright (c) debugyou ltd, January 2021

	Released under Gnu General Public License 3.0

	Stored Procedure	:	dbydbtest.DoNothing

	Purpose				:	To serve as an example of a simple stored procedure that returns nothing and selects nothing

	Example				:	exec dbydbtest.DoNothing

	What's going on		:	Well, nothing is going on. This stored procedure exists purely for the purpose of 
							testing the dbydbtest sql server client library

*********************************************************************************************************************************/
create OR alter procedure [dbydbtest].[DoNothing]
as
begin
	set nocount on;
end
go

IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbydbtest', N'PROCEDURE',N'DoNothing', NULL,NULL))
	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Stored procedure to do nothing' , @level0type=N'SCHEMA',@level0name=N'dbydbtest', @level1type=N'PROCEDURE',@level1name=N'DoNothing'
ELSE
BEGIN
	EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Stored procedure to do nothing' , @level0type=N'SCHEMA',@level0name=N'dbydbtest', @level1type=N'PROCEDURE',@level1name=N'DoNothing'
END
GO


/********************************************************************************************************************************

	SQL Tips for Application Developers	

	Author				:	elvis baldie, debugyou ltd
	
	Copyright (c) debugyou ltd, January 2021

	Released under Gnu General Public License 3.0

	Stored Procedure	:	dbydbtest.GetAllDataTypes

	Purpose				:	To serve as an example of a simple stored procedure that returns one column for every data type 
							that I think can be processed. It's a work in progress.

	Example				:	exec dbydbtest.GetAllDataTypes

	What's going on		:	We create a temporary table that has many different column types, and then we populate a couple of
							rows with constant values. This is all 'hard-coded' so that the test program knows what to expect.

*********************************************************************************************************************************/
CREATE OR ALTER PROCEDURE [dbydbtest].[GetAllDataTypes]
AS
BEGIN
	SET NOCOUNT ON;

	declare @testtable table
	(
		MyInt			int				null,
		MyBigInt		bigint			null,

		MyTinyInt		tinyint			null,
		MyBit			bit				null,

		MyDateTime		Datetime		null,
		MyDate			Date			null,

		MyChar			char			null,
		MyChar7			char(7)			null,
		MyVarchar50		varchar(50)		null,
		MyVarcharMax	varchar(max)	null
	)

	insert into @testtable(MyInt, MyBigInt, MyTinyInt, MyBit, MyDateTime, MyDate, MyChar, MyChar7, MyVarchar50, MyVarcharMax)
		values(32767, 2150123456, 254, 0, '17 Feb 1974 12:34:56', '03 Jan 1980', 'z', 'abc', 'The quick brown fox jumps over the lazy dog', null)

	--	Now we select everything, which should make one column for each datatype. We
	--	will see what emerges from the dbydbtest call to this procedure!
	select MyBigInt, MyInt, MyTinyInt, MyBit, MyDateTime, MyDate, MyChar, MyChar7, MyVarchar50, MyVarcharMax from @testtable

END
GO

IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbydbtest', N'PROCEDURE',N'GetAllDataTypes', NULL,NULL))
	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Stored procedure to select a row of different sql data types in order to check what is returned' , @level0type=N'SCHEMA',@level0name=N'dbydbtest', @level1type=N'PROCEDURE',@level1name=N'GetAllDataTypes'
ELSE
BEGIN
	EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Stored procedure to select a row of different sql data types in order to check what is returned' , @level0type=N'SCHEMA',@level0name=N'dbydbtest', @level1type=N'PROCEDURE',@level1name=N'GetAllDataTypes'
END
GO


/********************************************************************************************************************************

	SQL Tips for Application Developers	

	Author				:	elvis baldie, debugyou ltd
	
	Copyright (c) debugyou ltd, January 2021

	Released under Gnu General Public License 3.0

	Stored Procedure	:	dbydbtest.ReturnNumber

	Purpose				:	To serve as an example of a simple stored procedure that returns a value other than the default 0

	Example				:	
							declare @result int = 70
							exec dbydbtest.ReturnNumber @result
							print 'result was ' + convert(char(10),@result)

	What's going on		:	Return whatever number was supplied as an input parameter. This stored procedure exists purely 
							for the purpose of testing the dbydbtest sql server client library. We are checking that the call
							passes return values back correctly.

*********************************************************************************************************************************/
CREATE OR ALTER PROCEDURE [dbydbtest].[ReturnNumber]
	@parameter1	int 
AS
BEGIN
	SET NOCOUNT ON;
	return @parameter1
END
GO

IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbydbtest', N'PROCEDURE',N'ReturnNumber', NULL,NULL))
	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Stored procedure to return whatever is passed to it in parameter 1' , @level0type=N'SCHEMA',@level0name=N'dbydbtest', @level1type=N'PROCEDURE',@level1name=N'ReturnNumber'
ELSE
BEGIN
	EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Stored procedure to return whatever is passed to it in parameter 1' , @level0type=N'SCHEMA',@level0name=N'dbydbtest', @level1type=N'PROCEDURE',@level1name=N'ReturnNumber'
END
GO


/********************************************************************************************************************************

	SQL Tips for Application Developers	

	Author				:	elvis baldie, debugyou ltd
	
	Copyright (c) debugyou ltd, January 2021

	Released under Gnu General Public License 3.0

	Stored Procedure	:	dbydbtest.ThrowDivideByZeroError

	Purpose				:	To serve as an example of a simple stored procedure that throws an exception

	Example				:	exec dbydbtest.ThrowDivideByZeroError

	What's going on		:	We cut to the chase and just throw that divide by zero right away.			

*********************************************************************************************************************************/
CREATE OR ALTER PROCEDURE [dbydbtest].[ThrowDivideByZeroError]
AS
BEGIN
    SET NOCOUNT ON;

	SELECT 1/0 as [Oh No!]
END;
GO

IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbydbtest', N'PROCEDURE',N'ThrowDivideByZeroError', NULL,NULL))
	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Stored procedure to throw a divide by zero error' , @level0type=N'SCHEMA',@level0name=N'dbydbtest', @level1type=N'PROCEDURE',@level1name=N'ThrowDivideByZeroError'
ELSE
BEGIN
	EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Stored procedure to throw a divide by zero error' , @level0type=N'SCHEMA',@level0name=N'dbydbtest', @level1type=N'PROCEDURE',@level1name=N'ThrowDivideByZeroError'
END
GO

/********************************************************************************************************************************

	SQL Tips for Application Developers	

	Author				:	elvis baldie, debugyou ltd
	
	Copyright (c) debugyou ltd, January 2021

	Released under Gnu General Public License 3.0

	Stored Procedure	:	dbydbtest.WaitForSeconds

	Purpose				:	This is a stored procedure that waits for a specified number of seconds. I know it's somewhat crude, 
							but that's okay. This procedure only exists on order to help with testing dbyDB sql library, so it 
							doesn't need to be exact.

	Example				:	
							declare @before datetime = getdate()
							declare @delay int = 6
							exec dbydbtest.WaitForSeconds @delay
							declare @after datetime = getdate()
							print 'stored proc took ' + rtrim(convert(char(6),datediff(second,@before,@after))) + ' seconds' 
							

	What's going on		:	Return whatever number was supplied as an input parameter. This stored procedure exists purely 
							for the purpose of testing the dbydbtest sql server client library. We are checking that the call
							passes return values back correctly.

*********************************************************************************************************************************/
CREATE OR ALTER PROCEDURE [dbydbtest].[WaitForSeconds]
	@parameter1	int 
AS
BEGIN
	SET NOCOUNT ON;

	--	The actual value of this datetime does not matter, because WaitForSeconds only takes notice of the time component
	declare @startdate datetime = DATEADD(SECOND, @parameter1,'01 Jan 1900')

	WaitFor delay @startdate

	return 0
END
GO

IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbydbtest', N'PROCEDURE',N'WaitForSeconds', NULL,NULL))
	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Stored procedure to wait for a number of seconds specifed in parameter1' , @level0type=N'SCHEMA',@level0name=N'dbydbtest', @level1type=N'PROCEDURE',@level1name=N'WaitForSeconds'
ELSE
BEGIN
	EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Stored procedure to wait for a number of seconds specifed in parameter1' , @level0type=N'SCHEMA',@level0name=N'dbydbtest', @level1type=N'PROCEDURE',@level1name=N'WaitForSeconds'
END
GO



