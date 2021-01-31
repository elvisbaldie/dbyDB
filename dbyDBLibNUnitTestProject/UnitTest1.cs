using NUnit.Framework;
using System;
using Microsoft.Data.SqlClient;
using Debugyou.SQLServer.DataClient;


namespace dbyDBLibNUnitTestProject
{
	/// <summary>
	/// 
	///		Tests
	/// 
	/// </summary>
	public class Tests
	{
		//	**LOOKOUT!!** This test suite assumes access to the AdventureWorks database
		string m_connectionstring = null;

		[SetUp]
		public void Setup()
		{
			//	Here we simply build and set our connection string, that's all.
			//	One of the tests below will check the connection
			SqlConnectionStringBuilder scsb = new SqlConnectionStringBuilder();
			scsb.DataSource = "(local)";
			scsb.InitialCatalog = "SQLforApplicationsProgrammers";
			scsb.IntegratedSecurity = true;
			m_connectionstring = scsb.ConnectionString;
		}

		[Test]
		public void EmptyConnectionStringTest()
		{
			//	This test checks to see what would happen if we tried to run with a null connection string
			string mynullstring = null;

			string getdate = "getdate";

			//	All we want to do is find out what time it is
			dbyDB mycaller = new dbyDB(mynullstring);

			//	Valid sql with an empty connection string
			var ex = Assert.Throws<ArgumentException>(() => mycaller.ScalarFunction(getdate));

			Assert.AreEqual("Connection string parameter cannot be null, empty or whitespace (Parameter 'm_connectionstring')", ex.Message);
			Assert.AreEqual(nameof(m_connectionstring), ex.ParamName);
		}

		/// <summary>
		/// Call getdate and check that what comes back is really a datetime
		/// </summary>
		[Test]
		public void ScalarFunctionNoParametersFirstTest()
		{
			string getdate = "getdate";

			//	All we want to do is find out what time it is
			dbyDB mycaller = new dbyDB(m_connectionstring);

			var now = mycaller.ScalarFunction(getdate);

			Assert.IsNotNull(now);

		}

		/// <summary>
		/// Call getdate() and check that what comes back is a datetime. 
		/// </summary>
		[Test]
		public void ScalarFunctionNoParametersSecondTest()
		{
			string getdate = "getdate()";

			//	All we want to do is find out what time it is
			dbyDB mycaller = new dbyDB(m_connectionstring);

			var now = mycaller.ScalarFunction(getdate);

		}
	}
}