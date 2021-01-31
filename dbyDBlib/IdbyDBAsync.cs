using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;

namespace Debugyou.SQLServer.DataClient
{
	interface IdbyDBAsync
	{
		string ConnectionString { get; }

		Task<Tuple<int?, DataSet>> StoredProcedureAsync(string procedurename, List<SqlParameter> parameters);

		Task<Tuple<int?, DataSet>> StoredProcedureAsync(string procedurename);

		Task<object> ScalarFunctionAsync(string functionname, List<SqlParameter> parameters);
		Task<object> ScalarFunctionAsync(string functionname);

		Task<DataTable> TableFunctionAsync(string functionname, List<string> fields, List<SqlParameter> parameters);
		Task<DataTable> TableFunctionAsync(string functionname, List<string> fields);
	}
}
