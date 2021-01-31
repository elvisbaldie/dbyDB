using NUnit.Framework;
using System;
using System.Data;
using System.Collections.Generic;

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

			//	Now we check the details of the exception
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

			//	Here's where we call the getdate function
			var now = mycaller.ScalarFunction(getdate);

			Assert.IsNotNull(now);

			//	Better parse the results of our efforts so as to check that it's really a datetime
			DateTime nowdt;
			Assert.IsTrue(DateTime.TryParse(now.ToString(), out nowdt));
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
			Assert.IsNotNull(now);

			//	Better parse the results of our efforts so as to check that it's really a datetime
			DateTime nowdt;
			Assert.IsTrue(DateTime.TryParse(now.ToString(), out nowdt));

		}

		/// <summary>
		/// Call DivideByZero and check that it throws an exception
		/// </summary>
		[Test]
		public void ScalarFunctionThrowDivideByZeroTest()
		{
			string divbyzero = "[dbydbtest].[DivideByZero]";

			dbyDB mycaller = new dbyDB(m_connectionstring);

			//	This is just a call to a scalar function that returns 1/0. 
			var ex = Assert.Throws<SqlException>(() => mycaller.ScalarFunction(divbyzero));

			//	**LOOKOUT!!** Note that the class library itself does the bare minimum here. Rather than translate all (or any)
			//	of the SqlExceptions into corresponding non-sql exceptions, we leave you to do that.
			//	Now we check the details of the exception
			Assert.AreEqual("Divide by zero error encountered.", ex.Message);
		}

		/// <summary>
		/// Test just to see how the call behaves if we run a stored procedure that returns nothing
		/// </summary>
		[Test]
		public void StoredProcedureDoNothingTest()
		{
			string donothing = "dbydbtest.DoNothing";

			//	All we want to do is find out what time it is
			dbyDB mycaller = new dbyDB(m_connectionstring);

			Tuple<int?, DataSet> result = mycaller.StoredProcedure(donothing);
			Assert.IsNotNull(result);

			//	The default return value is zero
			Assert.AreEqual(result.Item1, 0);
		}

		/// <summary>
		/// Test just to see that the library works with the return value correctly
		/// </summary>
		[Test]
		public void StoredProcedureReturnNumberTest()
		{
			string returnnumber = "dbydbtest.ReturnNumber";

			//	Build a parameter to hold the expected return value
			SqlParameter parameter = new SqlParameter("@parameter1", 12345);
			List<SqlParameter> parameterlist = new List<SqlParameter>();
			parameterlist.Add(parameter);

			//	All we want to do is find out what time it is
			dbyDB mycaller = new dbyDB(m_connectionstring);

			//	The stored procedure is expected to return an empty data set 
			Tuple<int?, DataSet> result = mycaller.StoredProcedure(returnnumber, parameterlist);
			Assert.IsNotNull(result);

			//	We expect to find our return value, 12345, in Item1, where the method puts whatever was returned.
			Assert.AreEqual(result.Item1, 12345);
		}

		/// <summary>
		/// This calls a stored procedure that returns a single row. In this row, each column has a different data type.
		/// Essentially this is a work in progress that just checks to see how the library works with each data type. 
		/// 
		/// This routine is essentially a whole battery of tests, but we're basically checking the same thing - can our 
		/// stored procedure call be trusted to give us back data types as we expect?
		/// </summary>
		[Test]
		public void StoredProcedureGetAllDataTypesTest()
		{
			string selectall = "dbydbtest.GetAllDataTypes";

			//	All we want to do is find out what time it is
			dbyDB mycaller = new dbyDB(m_connectionstring);

			//	The stored procedure is expected to return a dataset containing one table which has a single row
			Tuple<int?, DataSet> result = mycaller.StoredProcedure(selectall);
			Assert.IsNotNull(result);

			//	Return value is zero
			Assert.AreEqual(result.Item1, 0);

			//	check that the dataset is not null, has one table
			DataSet myset = result.Item2;
			Assert.IsNotNull(myset);
			Assert.AreEqual(1, myset.Tables.Count);

			//	Check that the datatable is not null, has one row
			DataTable mytable = myset.Tables[0];
			Assert.IsNotNull(mytable);
			Assert.AreEqual(1, mytable.Rows.Count);

			//	Extract the row
			DataRow myrow = mytable.Rows[0];
			Assert.IsNotNull(myrow);

			//	The GetAllDataTypes stored procedure returns columns named after datatypes according to a fairly consistent naming convention
			//	So MyBigInt contains a BigInt, MyInt contains an Int, MyBit = Bit. Also MyVarchar50 = varchar(50) and so on.

			//
			//	First let's check that the columns returned are what we expect, just so we don't get any unexpected exceptions. This is more
			//	of a check that the stored proc is in sync with the C#, rather than a test of the stored procedure,
			//
			int myint_idx = mytable.Columns.IndexOf("MyInt");
			Assert.IsTrue(myint_idx >= 0, string.Format("MyInt column not found in result set"));

			int mybigint_idx = mytable.Columns.IndexOf("MyBigInt");
			Assert.IsTrue(mybigint_idx >= 0, string.Format("MyBigInt column not found in result set"));

			int mytinyint_idx = mytable.Columns.IndexOf("MyTinyInt");
			Assert.IsTrue(mytinyint_idx >= 0, string.Format("MyTinyInt column not found in result set"));

			int mybit_idx = mytable.Columns.IndexOf("MyBit");
			Assert.IsTrue(mybit_idx >= 0, string.Format("MyBit column not found in result set"));

			int mydatetime_idx = mytable.Columns.IndexOf("MyDateTime");
			Assert.IsTrue(mydatetime_idx >= 0, string.Format("MyDateTime column not found in result set"));

			int mydate_idx = mytable.Columns.IndexOf("MyDate");
			Assert.IsTrue(mydate_idx >= 0, string.Format("MyDate column not found in result set"));

			int mychar_idx = mytable.Columns.IndexOf("MyChar");
			Assert.IsTrue(mychar_idx >= 0, string.Format("MyChar column not found in result set"));

			int mychar7_idx = mytable.Columns.IndexOf("MyChar7");
			Assert.IsTrue(mychar7_idx >= 0, string.Format("MyChar7 column not found in result set"));

			int myvarchar50_idx = mytable.Columns.IndexOf("MyVarChar50");
			Assert.IsTrue(myvarchar50_idx >= 0, string.Format("MyVarChar50 column not found in result set"));

			int myvarcharmax_idx = mytable.Columns.IndexOf("MyVarCharMax");
			Assert.IsTrue(myvarcharmax_idx >= 0, string.Format("MyVarCharMax column not found in result set"));


			//	Now we check the data types of the columns returned, starting with our various integers
			Assert.AreEqual(typeof(int), mytable.Columns[myint_idx].DataType);
			Assert.AreEqual(typeof(long), mytable.Columns[mybigint_idx].DataType);
			Assert.AreEqual(typeof(byte), mytable.Columns[mytinyint_idx].DataType);
			Assert.AreEqual(typeof(bool), mytable.Columns[mybit_idx].DataType);

			//	Date and DateTime are retrieved as DateTime
			Assert.AreEqual(typeof(DateTime), mytable.Columns[mydatetime_idx].DataType);
			Assert.AreEqual(typeof(DateTime), mytable.Columns[mydate_idx].DataType);

			//	All of the character types come back as string
			Assert.AreEqual(typeof(string), mytable.Columns[mychar_idx].DataType);
			Assert.AreEqual(typeof(string), mytable.Columns[mychar7_idx].DataType);
			Assert.AreEqual(typeof(string), mytable.Columns[myvarchar50_idx].DataType);
			Assert.AreEqual(typeof(string), mytable.Columns[myvarcharmax_idx].DataType);

			//
			//	Having got to the end of the type checking, we then verify that the values returned are correct
			//	dbydbtest.GetAllDataTypes returns a specific set of hard-coded values, so we are able to compare what comes back with 
			//	what we expect to come back.
			//
			const int MYINT = 32767;
			const long MYBIGINT = 2150123456;
			const byte MYTINYINT = 254;
			const bool MYBIT = false;

			DateTime MYDATETIME = new DateTime(1974,2,17,12,34,56); //	17 Feb 1974 12:34:56
			DateTime MYDATE = new DateTime(1980,01,3); // '03 Jan 1980'

			string MYCHAR = "z";
			string MYCHAR7 = "abc    ";	//	**NOTE** The char(7) field in the database is a fixed length seven character field
			string MYVARCHAR50 = "The quick brown fox jumps over the lazy dog";

			//	Integer
			int myint;
			bool parsedint = int.TryParse(myrow[myint_idx].ToString(), out myint);
			Assert.IsTrue(parsedint);
			Assert.AreEqual(MYINT, myint);

			//	Big Integer, or long
			long mybigint;
			bool parsedlong = long.TryParse(myrow[mybigint_idx].ToString(), out mybigint);
			Assert.IsTrue(parsedlong);
			Assert.AreEqual(MYBIGINT, mybigint);

			//	Tiny Integer, or byte
			byte mytinyint;
			bool parsedbyte = byte.TryParse(myrow[mytinyint_idx].ToString(), out mytinyint);
			Assert.IsTrue(parsedbyte);
			Assert.AreEqual(MYTINYINT, mytinyint);

			//	Bit, or bool
			bool mybit;
			bool parsedbit = bool.TryParse(myrow[mybit_idx].ToString(), out mybit);
			Assert.IsTrue(parsedbit);
			Assert.AreEqual(MYBIT, mybit);

			//	DateTime
			DateTime mydatetime;
			bool parseddatetime = DateTime.TryParse(myrow[mydatetime_idx].ToString(), out mydatetime);
			Assert.IsTrue(parseddatetime);
			Assert.AreEqual(MYDATETIME, mydatetime);

			//	Date (that is to say, DateTime)
			DateTime mydate;
			bool parseddate = DateTime.TryParse(myrow[mydate_idx].ToString(), out mydate);
			Assert.IsTrue(parseddate);
			Assert.AreEqual(MYDATE, mydate);

			//	Char or Varchar of any description comes through as string
			string mychar = myrow[mychar_idx].ToString();
			Assert.AreEqual(MYCHAR, mychar);

			string mychar7 = myrow[mychar7_idx].ToString();
			Assert.AreEqual(MYCHAR7, mychar7);

			string myvarchar50 = myrow[myvarchar50_idx].ToString();
			Assert.AreEqual(MYVARCHAR50, myvarchar50);

			//	In this case the varchar(max) actually contains null in the database
			string myvarcharmax = myrow[myvarcharmax_idx].ToString();
			Assert.IsTrue(string.IsNullOrEmpty(myvarcharmax));
		}

		/// <summary>
		/// Test the stored procedure that waits for a set number of seconds. This is not an especially precise test, given the imprecise nature
		/// of working with a database server. This test should pass sufficiently often to make it a real test
		/// </summary>
		[Test]
		public void StoredProcedureWaitForSecondsTest()
		{
			string waitfortest = "dbydbtest.WaitForSeconds";

			//	All we want to do is find out what time it is
			dbyDB mycaller = new dbyDB(m_connectionstring);

			const int SECONDSTOWAIT = 10;

			//	Build a parameter to hold the input value, which is the number of seconds to wait
			SqlParameter parameter = new SqlParameter("@seconds", SECONDSTOWAIT);
			List<SqlParameter> parameterlist = new List<SqlParameter>();
			parameterlist.Add(parameter);

			//
			//	Right, so here is the rather wobbly basis of our test. We ask the time now and also after we finish, compare the two.
			//	Might be sensible to build in a bit of tolerance in order to avoid unnecessary and irritating failure. It goes without 
			//	saying that this test will normally fail when stepping through in debug
			//
			DateTime start = DateTime.Now;

			//	The stored procedure is expected to return a dataset containing one table which has a single row
			Tuple<int?, DataSet> result = mycaller.StoredProcedure(waitfortest, parameterlist);

			//	Before we do anything at all, what time is it?
			DateTime end = DateTime.Now;

			//	The usual considerations apply to what comes back, although they aren't at all important
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.Item1);
			Assert.AreEqual(0, result.Item1);

			//	Now we find out how many seconds the call took
			TimeSpan ts = end.Subtract(start);
			Assert.AreEqual(SECONDSTOWAIT, ts.Seconds);
		}

		/// <summary>
		/// Test of what happens if we call a stored procedure that throws a divide by zero. This is just the stored procedure version of the 
		/// earlier test of the scalar function that also divides by zero.
		/// </summary>
		[Test]
		public void StoredProcedureThrowDivideByZeroErrorTest()
		{
			string divbyzerotest = "dbydbtest.ThrowDivideByZeroError";

			//	All we want to do is find out what time it is
			dbyDB mycaller = new dbyDB(m_connectionstring);

			//	The stored procedure is expected to throw a divide by zero exception
			var ex = Assert.Throws<SqlException>(() => mycaller.StoredProcedure(divbyzerotest));
			Assert.AreEqual("Divide by zero error encountered.", ex.Message);
		}
	}
}