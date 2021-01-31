using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Data.SqlClient;

namespace SQLServerClientLibraryNUnitTestProject
{
	public class UnitTestAsync
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
			scsb.InitialCatalog = "AdventureWorks";
			scsb.IntegratedSecurity = true;
			m_connectionstring = scsb.ConnectionString;
		}

		/// <summary>
		/// All this test does 
		/// </summary>
		[Test]
		public void SleepTest()
		{

		}
	}
}
