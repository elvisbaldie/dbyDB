using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

using dbyDBlib;

namespace dbyDBlibExtended
{
	public class dbyDBcallerExtended : dbyDBcaller
	{
		public dbyDBcallerExtended(string connectionstring) : base(connectionstring) { }

        /// 
        ///     query
        ///     
        ///     (c) debugyou ltd, 2021
        /// 
        /// <summary> Here's a method whose name is self-explanatory. It runs a query, which can if necessary be a parameterised query</summary>
        /// <param name="sql">the sql to be executed in this query</param>
        /// <param name="parameters"></param>
        /// <returns>All results of the query are returned as a DataSet</returns>
        public DataSet query(string sql, List<SqlParameter> parameters)
        {
            //  sanity-check the input parameters
            if (string.IsNullOrWhiteSpace(m_connectionstring)) throw new ArgumentException(nameof(m_connectionstring), "Connection string parameter cannot be null, empty or whitespace");
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentException(nameof(sql), "sql parameter cannot be null, empty or whitespace");

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
        public DataSet query(string sql)
        {
            DataSet results = query(sql, null);
            return results;
        }
    }
}
