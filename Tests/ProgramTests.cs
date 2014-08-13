using System;
using System.Linq;
using LatestFileReporter.Interfaces;
using Moq;
using NUnit.Framework;

namespace LatestFileReporter.Tests
{
	// ReSharper disable InconsistentNaming
	[TestFixture]
	public class ProgramTests
	{
		[Test]
		public void program_is_not_null()
		{
			var program = new Program();
			Assert.IsNotNull(program);
		}

		[Test]
		public void get_oudated_files_returns_all_files_where_last_write_time_not_today()
		{
			var files = new[]
			{
				Factory.CreateFile("bad1.cub", DateTime.Now.AddDays(-10).Date),
				Factory.CreateFile("bad2.cub", DateTime.Now.AddDays(-1).Date),
				Factory.CreateFile("good.cub", DateTime.Now.Date)
			};
			var count = files.Where(Program.HasProcessedToday).Count();
			Assert.AreEqual(2, count);
		}

		[Test]
		public void unprocessed_file_triggers_batch_file()
		{
			var mock = new Mock<IProgramDefinition>();
			var files = new[]
			{
				Factory.CreateFile("test.cub", DateTime.Now.AddDays(-1).Date)
			};
			var defMock = new Mock<IProgramSettings>();
			defMock.Setup(d => d.MaxFailCountBeforeFailing).Returns(3);

			// BUG: Need to figure out how to get the max fail count 
			// IDEA: This belongs on the 'Program'...

			mock.Setup(a => a.GetFilesAsQueryable()).Returns(files.AsQueryable());
			mock.Setup(a => a.DoesLogFileIndicateCommonError(It.Is<string>(s => s == "test.cub"))).Returns(true); // only executes when function returns true
			mock.Setup(a => a.CopySourceFile(It.Is<string>(s => s == "test.cub"))).Returns(false); // batch file only executes when copy function returns false
			mock.Setup(a => a.KeepGoing(It.Is<int>(i => i == 1))).Returns(false); // short circuit to one iteration

			var program = new Program
			{
				Settings = defMock.Object,
				Definition = mock.Object
			};
			var result = program.Run();

			mock.Verify(a => a.RunBatchFile(It.Is<string>(s => s == "test.cub")), Times.Once());
			mock.Verify(a => a.DoesLogFileIndicateCommonError(It.Is<string>(s => s == "test.cub")), Times.Once()); 
			mock.Verify(a => a.SendMessage(It.Is<IFileInfo[]>(f => f.Count() == 1)), Times.Once());
			Assert.AreEqual(1, result);

		}

		[Test]
		public void more_than_three_failing_tests_sends_critical_email()
		{
			var app = new Mock<IProgramDefinition>();
			var files = new[]
			{
				Factory.CreateFile("bad1.cub", DateTime.Now.AddDays(-1).Date),
				Factory.CreateFile("bad2.cub", DateTime.Now.AddDays(-1).Date),
				Factory.CreateFile("good.cub", DateTime.Now.Date)
			};
		}

		[Test]
		public void no_outdated_files_sends_keep_sleeping_email()
		{
			
		}

		[Test]
		public void successful_run_reports_keep_sleeping_email()
		{
			
		}

		[Test]
		public void failed_run_reports_wake_up_email()
		{
			
		}

	}
	// ReSharper restore InconsistentNaming
}
