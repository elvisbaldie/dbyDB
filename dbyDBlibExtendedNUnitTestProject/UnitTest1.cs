
using System;
using Microsoft.Data.SqlClient;
using dbyDBlib;
using NUnit.Framework;

namespace dbyDBlibExtendedNUnitTestProject
{
	public class Tests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void EmptyQueryTest()
		{
			//	This test checks to see what happens if we try to run with a null connection string
			string mynullstring = null;

			//	All we want to do is find out what time it is
			dbyDBcallerExtended mycaller = new dbyDBcaller(m_connectionstring);

			//	Valid sql with an empty query string
			var ex = Assert.Throws<ArgumentException>(() => mycaller.query(mynullstring));

			Assert.AreEqual("sql (Parameter 'sql parameter cannot be null, empty or whitespace')", ex.Message);
		}
	}
}