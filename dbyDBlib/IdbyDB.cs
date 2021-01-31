using System;
using System.Collections.Generic;
using System.Data;

using Microsoft.Data.SqlClient;


namespace Debugyou.SQLServer.DataClient
{
	interface IdbyDB
	{
		string ConnectionString { get; }
		Tuple<int?,DataSet> Procedure(string procedurename, List<SqlParameter> parameters);

		object ScalarFunction(string functionname, List<SqlParameter> parameters);
		object ScalarFunction(string functionname);

		DataTable TableFunction(string functionname, List<string> fields, List<SqlParameter> parameters);
		DataTable TableFunction(string functionname, List<string> fields);
	}
}
