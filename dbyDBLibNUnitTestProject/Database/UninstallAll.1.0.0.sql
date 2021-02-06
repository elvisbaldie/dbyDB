/********************************************************************************************************************************

	SQL Tips for Application Developers	

	Author				:	elvis baldie, debugyou ltd
	
	Copyright (c) debugyou ltd, January 2021

	Released under Gnu General Public License 3.0

	script				:	uninstall.1.0.0.sql

	Purpose				:	To uninstall all sql objects connected with the dbyDB test programs (version 1.0.0). 
							This amounts to a handful of stored procedures, one function and a schema. That's it. 
							All of these are database objects whose raison d'etre is to serve as part of the test
							rig for the dbyDB SQL Server DataClient library.

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
--	Now we drop all of the procedures used by the test program for version 1.0.0
--
if exists (select * from dbo.sysobjects where id = object_id(N'[dbydbtest].[DoNothing]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
begin 
	drop procedure dbydbtest.DoNothing
end
go

if exists (select * from dbo.sysobjects where id = object_id(N'[dbydbtest].[GetAllDataTypes]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
begin 
	drop procedure dbydbtest.GetAllDataTypes
end
go

if exists (select * from dbo.sysobjects where id = object_id(N'[dbydbtest].[ThrowDivideByZeroError]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
begin 
	drop procedure dbydbtest.ThrowDivideByZeroError
end
go

if exists (select * from dbo.sysobjects where id = object_id(N'[dbydbtest].[WaitForSeconds]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
begin 
	drop procedure dbydbtest.WaitForSeconds
end
go

if exists (select * from dbo.sysobjects where id = object_id(N'[dbydbtest].[ReturnNumber]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
begin 
	drop procedure dbydbtest.ReturnNumber
end
go

if exists (select * from dbo.sysobjects where id = object_id(N'[dbydbtest].[DivideByZero]') and OBJECTPROPERTY(id, N'IsScalarFunction') = 1)
begin 
	drop function dbydbtest.DivideByZero
end
go

--
--	Finally drop the schema itself 
--
if exists (select null from information_schema.schemata where schema_name = 'dbydbtest' )
begin
	drop schema [dbydbtest]
end
go



