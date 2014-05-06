using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace LatestFileReporter
{
	// ReSharper disable InconsistentNaming
	[TestFixture]
	public class ProgramTests
	{
		[Test]
		public void unmodified_files_report_fail()
		{
			var program = new Program();
			Assert.IsNotNull(program);
		}
	}
	// ReSharper restore InconsistentNaming
}
