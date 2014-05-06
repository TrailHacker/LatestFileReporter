using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace LatestFileReporter
{
	// ReSharper disable InconsistentNaming
	[TestFixture]
	public class ProgramTests
	{
		[SetUp]
		public void SetupMailer()
		{
		}

		[Test]
		public void program_is_not_null()
		{
			var program = new Program();
			Assert.IsNotNull(program);
		}

		[Test]
		public void one_modified_file_reports_failure()
		{

		}

	}
	// ReSharper restore InconsistentNaming
}
