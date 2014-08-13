using System;
using System.Linq;
using LatestFileReporter.Interfaces;
using Moq;
using NUnit.Framework;

namespace LatestFileReporter.Tests
{
	[TestFixture]
	public class ApplicationTests
	{
		[Test]
		public void get_oudated_files_returns_all_files_where_last_write_time_not_today()
		{
			var app = new Application(Mock.Of<IProgramSettings>());
			var files = new[]
			{
				Factory.CreateFile("bad1.cub", DateTime.Now.AddDays(-1).Date),
				Factory.CreateFile("bad2.cub", DateTime.Now.AddDays(-1).Date),
				Factory.CreateFile("good.cub", DateTime.Now.Date)
			};
			var count = files.Where(app.HasProcessedToday).Count();
			Assert.AreEqual(2, count);
		}
	}
}