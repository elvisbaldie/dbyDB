using System;
using System.Collections.Generic;
using System.Data;

using Microsoft.Data.SqlClient;


namespace Debugyou.SQLServer.DataClient
{
	interface IdbyDB
	{
		string ConnectionString { get; }
		Tuple<int?,DataSet> StoredProcedure(string procedurename, List<SqlParameter> parameters);

		Tuple<int?, DataSet> StoredProcedure(string procedurename);

		object ScalarFunction(string functionname, List<SqlParameter> parameters);
		object ScalarFunction(string functionname);

		DataTable TableFunction(string functionname, List<string> fields, List<SqlParameter> parameters);
		DataTable TableFunction(string functionname, List<string> fields);
	}
}
