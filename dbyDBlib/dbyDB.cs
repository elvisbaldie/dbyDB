using System;
using System.Text;
using System.Collections.Generic;
using System.Data;

using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace Debugyou.SQLServer.DataClient
{

 /*
 * 
 *  dbyDB
 *  
 *  (c) debugyou 2020
 *  
 *  An object of this class calls database objects - procedure, function, table function
 *  
 *  This is a very basic set of example code, written as part of 'SQL Tips for Application Developers'
 *  You will note that the code shown here does not contain sophisticated error-handling, nor does it deal with important
 *  issues such as transaction-handling or permissioning. It's free, so you can't complain - caveat developor, you might say.
 *  
 *  You may notice that there are two private 'query' methods in this class. There are private quite deliberately, the reasoning being that 
 *  we prefer that developers should avoid raw sql and keep their SQL code encapsulated wherever possible. However, if you're reading this then 
 *  you're probably a developer, which means you're easily smart enough to write your own raw sql query function anyway. So there was no point 
 *  in trying to prevent you from doing so. Nevertheless, please try to work through the interface if you can. Good luck!
 * 
 */
    public class dbyDB : IdbyDB, IdbyDBAsync
    {
        public dbyDB(string connectionstring)  { m_connectionstring = connectionstring; }

        /// <summary>
        /// ConnectionString is read-only
        /// </summary>
		public string ConnectionString { get { return m_connectionstring; } }
        protected readonly string m_connectionstring = null;

        /// 
        ///     Procedure
        ///     
        ///     (c) debugyou ltd, 2021
        /// 
        /// <summary>run a stored procedure</summary>
        /// <param name="procedurename">name of the store procedure to be run</param>
        /// <param name="parameters">List of SqlParameters to be supplied to the stored procedure</param>
        /// <param name="results">DataSet containing results selected by the stored procedure that we called</param>
        /// <returns>Whatever integer value was retrieved by the stored proc</returns>
        public Tuple<int?,DataSet> Procedure(string procedurename, List<SqlParameter> parameters)
        {
            //  At this point we may consider some sanity checks on the input parameters
            if (string.IsNullOrWhiteSpace(m_connectionstring)) throw new ArgumentException( "Connection string parameter cannot be null, empty or whitespace",nameof(m_connectionstring));
            if (string.IsNullOrWhiteSpace(procedurename)) throw new ArgumentException("Procedure name cannot be null, empty or whitespace", nameof(procedurename));

            //  Default return value for a stored procedure is zero, but it can be null
            int? returnvalue = null;

            //  We assign a new and empty dataset to the output parameter
            DataSet results = new DataSet();

            //  Now we get on with the query
            using (SqlConnection connection = new SqlConnection(m_connectionstring))
            {
                // Create SqlCommand and identify it as a stored procedure.
                using (SqlCommand sqlCommand = new SqlCommand(procedurename, connection))
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    //  Add the parameters into the SqlCommand. While doing this, check to see if there is a return value parameter in the list
                    SqlParameter preturn = null;

                    bool wereWePassedAReturnParameter = false;

                    if (parameters != null)
                    {
                        //  Now we go through the passed-in parameter list and add them to our sql command
                        foreach (SqlParameter p in parameters)
                        {
                            sqlCommand.Parameters.Add(p);

                            //  What we are doing here is checking to see whether or not the caller has specified 
                            //  a parameter to hold the return value. Because if they have not, we'll add it.
                            if (p.Direction == ParameterDirection.ReturnValue)
                            {
                                preturn = p;
                                wereWePassedAReturnParameter = true;
                            }
                        }
                    }

                    if (preturn==null)
                    {
                        //  The caller did not specify a return value, so we add one. The reason for all this parameter copying is that we do not 
                        //  want to return to the caller with a list of parameters that's not what was supplied to us (other than populating output
                        //  parameters). Some part of me says that I should just add in the return parameter anyway, but another part says not.
                        preturn = new SqlParameter();
                        preturn.Direction = ParameterDirection.ReturnValue;
                        sqlCommand.Parameters.Add(preturn);
                    }

                    try
                    {
                        connection.Open();

                        using (SqlDataAdapter adapter = new SqlDataAdapter())
                        {
                            adapter.SelectCommand = sqlCommand;

                            //  First we fill the result set...
                            adapter.Fill(results);

                            //  Now we extract the return value and return the parameters
                            if (parameters != null)
                            {
                                parameters.Clear();
                                foreach (SqlParameter p in sqlCommand.Parameters)
                                {
                                    if (p.Direction == ParameterDirection.ReturnValue)
                                    {
                                        //  We've found a return value parameter
                                        returnvalue = (int?)p.Value;
                                        if (wereWePassedAReturnParameter)
										{
                                            //  If we were passed a return parameter in the first place then 
                                            //  now is the time to give it back. Otherwise not.
                                            parameters.Add(p);
                                        }
                                    }
                                    else
                                    {
                                        //  For other parameters, we also pass them out into the list.
                                        //  Why doe we do this? Well, some of them may be output parameters
                                        parameters.Add(p);
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }

            //  We return the return value along with the results
            return Tuple.Create(returnvalue, results);
        }

        /// 
        ///     ProcedureAsync
        ///     
        ///     (c) debugyou ltd, 2021
        /// 
        /// <summary>run a stored procedure</summary>
        /// <param name="procedurename">name of the store procedure to be run</param>
        /// <param name="parameters">List of SqlParameters to be supplied to the stored procedure</param>
        /// <param name="results">DataSet containing results selected by the stored procedure that we called</param>
        /// <returns>Whatever integer value was retrieved by the stored proc</returns>
        public async Task<Tuple<int?,DataSet>> ProcedureAsync(string procedurename, List<SqlParameter> parameters)
        {
            //  Let us do our sanity checks as early as possible so that we can throw exceptions in the main thread 
            if (string.IsNullOrWhiteSpace(m_connectionstring)) throw new ArgumentException( "Connection string parameter cannot be null, empty or whitespace",nameof(m_connectionstring));
            if (string.IsNullOrWhiteSpace(procedurename)) throw new ArgumentException("Procedure name cannot be null, empty or whitespace", nameof(procedurename));

            var returnvalue = await Task.Run(()=>Procedure(procedurename, parameters));

            //  We return the return value. Note that this defaults to zero, but it can under some circumstances be null.
            return returnvalue;
        }



        /// 
        ///     ScalarFunction
        /// 
        ///     (c) debugyou ltd, 2021
        /// 
        /// <summary>
        ///     Call a scalar function and return the result
        /// </summary>
        /// 
        /// <param name="functionname">The name of the scalar function we're calling</param>
        /// <param name="parameters">List of Sql Parameters we're passing to the function</param>
        /// 
        /// <returns>
        ///     Whatever the function retrieves from the database
        /// </returns>
        /// 
        public object ScalarFunction(string functionname, List<SqlParameter> parameters)
		{
            //  Before we start, sanity-check the input parameters and the connection string. The parameter list can be empty or null
            if (string.IsNullOrWhiteSpace(m_connectionstring)) throw new ArgumentException("Connection string parameter cannot be null, empty or whitespace", nameof(m_connectionstring));
            if (string.IsNullOrWhiteSpace(functionname)) throw new ArgumentException("function name cannot be null, empty or whitespace", nameof(functionname));

            object result = null;

            //  Here is where we build the query that selects the function called with the parameters supplied.
            //  **ATTENTION!!** As for the table function call, there could really be some better validation around all this, it does allow the user to basically execute anything at all.
            StringBuilder functionquery = new StringBuilder();
            functionquery.AppendFormat("select {0}({1})", ScalarFunctionNameCleaner(functionname), List2SqlParameterNames(parameters));

            using (SqlConnection connection = new SqlConnection(m_connectionstring))
            {
                // Create SqlCommand and identify it as a stored procedure.
                using (SqlCommand sqlCommand = new SqlCommand(functionquery.ToString(), connection))
                {
                    sqlCommand.CommandType = CommandType.Text;

                    SqlParameter preturn = null;

                    //  Add the parameters into the SqlCommand. While doing this, check to see if there is a return value parameter in the list,
                    //  and if there is no such return parameter, add it. The reason for this is so as to make sure there is something for the scalar 
                    //  function to return.
                    if (parameters != null)
                    {
                        //  Now we go through the passed-in parameter list and add them to our sql command
                        foreach (SqlParameter p in parameters)
                        {
                            sqlCommand.Parameters.Add(p);

                            //  What we are doing here is checking to see whether or not the caller has specified 
                            //  a parameter to hold the return value. Because if they have not, we'll add it.
                            if (p.Direction == ParameterDirection.ReturnValue)
                            {
                                preturn = p;
                            }
                        }
                    }

                    if (preturn == null)
                    {
                        //  The caller did not specify a return value, so we add one
                        preturn = new SqlParameter();
                        preturn.Direction = ParameterDirection.ReturnValue;
                        sqlCommand.Parameters.Add(preturn);
                    }

                    try
                    {
                        //  Open the connection and call the function
                        connection.Open();
                        result = sqlCommand.ExecuteScalar();
                    }

                    finally
                    {
                        connection.Close();
                    }
                }
            }
            return result;
		}

        /// 
        ///     ScalarFunctionAsync
        /// 
        ///     (c) debugyou ltd, 2021
        /// 
        /// <summary>
        ///     Call a scalar function and return the result
        /// </summary>
        /// 
        /// <param name="functionname">The name of the scalar function we're calling</param>
        /// <param name="parameters">List of Sql Parameters we're passing to the function</param>
        /// 
        /// <returns>
        ///     Whatever the function retrieves from the database
        /// </returns>
        /// 
        public async Task<object> ScalarFunctionAsync(string functionname, List<SqlParameter> parameters)
        {
            //  Before we start, sanity-check the input parameters and the connection string. The parameter list can be empty or null
            if (string.IsNullOrWhiteSpace(m_connectionstring)) throw new ArgumentException( "Connection string parameter cannot be null, empty or whitespace",nameof(m_connectionstring));
            if (string.IsNullOrWhiteSpace(functionname)) throw new ArgumentException("function name cannot be null, empty or whitespace", nameof(functionname));

            object result = await Task.Run(() => ScalarFunctionAsync(functionname, parameters));

            return result;
        }

        /// 
        ///     ScalarFunction
        /// 
        ///     (c) debugyou ltd, 2021
        /// 
        /// <summary>
        /// 
        ///     As above, but this one is a call to a scalar function that takes no parameters.
        /// 
        /// </summary>
        /// <param name="functionname">
        ///     name of the scalar function we are calling
        /// </param>
        /// <returns>
        ///    Whatever the function retrieves from the database
        /// </returns>
        public object ScalarFunction(string functionname)
        {
            //  This is in two lines simply for purposes of debugging
            object result = ScalarFunction(functionname, null);
            return result;
        }

        public async Task<object> ScalarFunctionAsync(string functionname)
        {
            //  This is in two lines simply for purposes of debugging
            var result = await Task.Run(()=>ScalarFunction (functionname, null));
            return result;
        }

        ///
        ///     TableFunction
        /// 
        ///     (c) debugyou ltd, 2021
        /// 
        /// <summary>
        /// 
        ///     Call a table function and return the result
        /// 
        /// </summary>
        /// <param name="functionname">
        ///     Name of the table function 
        /// </param>
        /// <param name="parameters">
        ///     Parameters supplied to the function
        /// </param>
        /// <param name="fields">
        ///     The list of fields selected by the caller
        /// </param>
        /// <returns>
        ///     A DataTable containing the fields requested
        /// </returns>
        public DataTable TableFunction(string functionname, List<string> fields, List<SqlParameter> parameters)
        {
            //  Before we start, sanity-check the input parameters and the connection string. The parameter list can be empty or null. The list of fields must not be empty
            if (string.IsNullOrWhiteSpace(m_connectionstring)) throw new ArgumentException( "Connection string parameter cannot be null, empty or whitespace",nameof(m_connectionstring));
            if (string.IsNullOrWhiteSpace(functionname)) throw new ArgumentException("function name cannot be null, empty or whitespace", nameof(functionname));
            if (fields==null || fields.Count==0) throw new ArgumentException("list of fields to be selected cannot be null or empty", nameof(functionname));

            //  Here is where we build the query that selects what the user wanted from the function called with the parameters supplied.
            //  **ATTENTION!!** There could really be some better validation around all this, it does allow the user to basically execute anything at all.
            StringBuilder functionquery = new StringBuilder();
            functionquery.AppendFormat("select {0} from {1}({2})", List2CommaSeparatedString(fields), functionname, List2SqlParameterNames(parameters));
      
            //  Now we run our query and retrieve the results.
            DataSet result = query(functionquery.ToString(),parameters);

            //  This is a table function, so we deliver a table.
            DataTable returnvalue = (result!=null && result.Tables.Count>0 ? result.Tables[0] : null);

            return result.Tables[0];
        }

        public async Task<DataTable> TableFunctionAsync(string functionname, List<string> fields, List<SqlParameter> parameters)
        {
            var result = await Task.Run(() => TableFunction(functionname, fields, parameters));
            return result;
        }

        /// 
        ///     TableFunction
        /// 
        ///     (c) debugyou ltd, 2021
        /// 
        /// <summary>
        /// 
        ///     Call a table function and return the result
        /// 
        /// </summary>
        /// <param name="functionname">
        ///     Name of the table function 
        /// </param>
        /// <param name="fields">
        ///     The list of fields selected by the caller
        /// </param>
        /// <returns>
        ///     A DataTable containing the fields requested
        /// </returns>
        public DataTable TableFunction(string functionname, List<string> fields)
        {
            DataTable result = TableFunction(functionname, fields, null);
            return result;
        }


        public async Task<DataTable> TableFunctionAsync(string functionname, List<string> fields)
        {
            var result = await Task.Run(() => TableFunction(functionname, fields));
            return result;
        }
        /// 
        ///     query
        ///     
        ///     (c) debugyou ltd, 2021
        /// 
        /// <summary> Here's a method whose name is self-explanatory. It runs a query, which can if necessary be a parameterised query</summary>
        /// <param name="sql">the sql to be executed in this query</param>
        /// <param name="parameters"></param>
        /// <returns>All results of the query are returned as a DataSet</returns>
        private DataSet query(string sql, List<SqlParameter> parameters)
        {
            //  sanity-check the input parameters
            if (string.IsNullOrWhiteSpace(m_connectionstring)) throw new ArgumentException( "Connection string parameter cannot be null, empty or whitespace",nameof(m_connectionstring));
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentException("sql parameter cannot be null, empty or whitespace", nameof(sql));

            DataSet results = null;

            using (SqlConnection connection = new SqlConnection(m_connectionstring))
            {
                // Create SqlCommand and identify it as a stored procedure.
                using (SqlCommand sqlCommand = new SqlCommand(sql, connection))
                {
                    sqlCommand.CommandType = CommandType.Text;

                    if (parameters != null)
                    {
                        //  Now we go through the passed-in parameter list and add them to our sql command
                        foreach (SqlParameter p in parameters)
                        {
                            sqlCommand.Parameters.Add(p);
                        }
                    }

                    try
                    {
                        connection.Open();

                        using (SqlDataAdapter adapter = new SqlDataAdapter())
                        {
                            adapter.SelectCommand = sqlCommand;

                            //  Fill the result set, and we're done
                            adapter.Fill(results);
                        }
                    }

                    finally
                    {
                        connection.Close();
                    }
                }
            }
            return results;
        }

        /// 
        ///     query
        /// 
        ///     (c) debugyou ltd, 2021
        /// 
        /// <summary>Run an sql query that takes no parameters</summary>
        /// <param name="sql">the sql to be executed in this query</param>
        /// <returns>Results of the query as a DataSet</returns>
        ///
        private DataSet query(string sql)
        {
            DataSet results = query(sql, null);
            return results;
        }

        //  ----------------------------------------------------------------------------------------------------------------------------------------------
        //
        //  Helper methods
        //
        //  Here are a few methods to help carry out various pieces of validation or transformation.
        //
        //  ----------------------------------------------------------------------------------------------------------------------------------------------

        /// 
        ///     List2CommaSeparatedString
        ///     
        ///     (c) debugyou ltd, 2021
        /// 
        /// <summary> 
        ///     Transform a list of strings into a comma-separated string. The purpose of this is to build the list of fields to be selected in the function call.
        /// </summary>
        /// <param name="fields"></param>
        /// <returns>A comma-separated list of field names</returns>
        private string List2CommaSeparatedString(List<string> fields)
		{
            string result = null;

            if (fields!=null && fields.Count>0)
			{
                StringBuilder sb = new StringBuilder();

                int fieldcount = 0;
                foreach (string myfield in fields)
				{
                    if (!string.IsNullOrEmpty(myfield))
                    {
                        if (fieldcount>0)
                        {
                            sb.AppendFormat(",{0}", myfield);
                        }
                        else
                        {
                            //  Just the first field does not need a comma in front of it
                            sb.Append(myfield);
                        }
                        fieldcount++;
                    }
                }

                result = sb.ToString();
			}
            return result;
		}

        /// 
        ///     List2CommaSeparatedString
        ///     
        ///     (c) debugyou ltd, 2021
        /// 
        /// <summary> 
        ///     Transform a list of objects into a comma-separated string of the names of those parameters. The purpose of this is to build the list of fields to be selected in the function call.
        /// </summary>
        /// <param name="fields"></param>
        /// <returns>A comma-separated list of sql parameter names</returns>
        private string List2CommaSeparatedString<T>(List<T> parameters)
        {
            string result = null;

            if (parameters != null && parameters.Count > 0)
            {
                StringBuilder sb = new StringBuilder();

                int parametercount = 0;
                foreach (T myparameter in parameters)
                {
                    if (myparameter!=null)
                    {
                        if (parametercount > 0)
                        {
                            sb.AppendFormat(",{0}", myparameter.ToString());
                        }
                        else
                        {
                            //  Just the first field does not need a comma in front of it
                            sb.Append(myparameter.ToString());
                        }
                        parametercount++;
                    }
                }

                result = sb.ToString();
            }
            return result;
        }
        /// <summary>
        /// Deliver a comma-separated list of parameter names for use in a scalar or table function call
        /// </summary>
        /// <param name="parameters">The sql parameters we wish to pass in to the function call</param>
        /// <returns></returns>
        private string List2SqlParameterNames(List<SqlParameter> parameters)
        {
            string result = null;

            if (parameters != null && parameters.Count > 0)
            {
                if (parameters != null && parameters.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();

                    int parametercount = 0;
                    foreach (SqlParameter myparameter in parameters)
                    {
                        if (myparameter != null)
                        {
                            if (parametercount > 0)
                            {
                                sb.AppendFormat(",{0}", myparameter.ParameterName.ToString());
                            }
                            else
                            {
                                //  Just the first field does not need a comma in front of it
                                sb.Append(myparameter.ParameterName.ToString());
                            }
                            parametercount++;
                        }
                    }

                    result = sb.ToString();
                }
            }
            return result;
        }

        /// 
        ///     ScalarFunctionNameCleaner
        /// 
        /// <summary>A quick syntax check on the name of a scalar function</summary>
        /// <param name="name">Name of the function as passed in by the caller</param>
        /// <returns>Name of the function as we intend to use it</returns>
        private string ScalarFunctionNameCleaner(string name)
		{
            string result = null;

            //  We are hoping that the user will not pass in a blank function name
            if (!string.IsNullOrWhiteSpace(name))
			{
                //  First chop off your leading and trailing spaces
                result = name.Trim();
                if (result.EndsWith("()"))
				{
                    //  Next we will excuse the poor user any trailing brackets that may have been left on the 
                    //  end of the function name
                    result = result.Substring(0, result.Length - 2);
				}
			}
            return result;
		}
    }
}
