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
			var files = new[]
			{
				Factory.CreateFile("test.cub", DateTime.Now.AddDays(-1).Date)
			};

			var settingsMock = new Mock<IProgramSettings>();
			settingsMock.Setup(d => d.MaxFailCountBeforeFailing).Returns(3);
			settingsMock.Setup(d => d.AttemptedRunCounter).Returns(1);

			var definitionMock = new Mock<IProgramDefinition>();
			definitionMock.Setup(a => a.GetFilesAsQueryable()).Returns(files.AsQueryable());
			definitionMock.Setup(a => a.DoesLogFileIndicateCommonError(It.Is<string>(s => s == "test.cub"))).Returns(true); // only executes when function returns true
			definitionMock.Setup(a => a.CopySourceFile(It.Is<string>(s => s == "test.cub"))).Returns(false); // batch file only executes when copy function returns false

			var program = new Program
			{
				Settings = settingsMock.Object,
				Definition = definitionMock.Object
			};
			var result = program.Run();

			definitionMock.Verify(a => a.RunBatchFile(It.Is<string>(s => s == "test.cub")), Times.Once());
			definitionMock.Verify(a => a.DoesLogFileIndicateCommonError(It.Is<string>(s => s == "test.cub")), Times.Once()); 
			definitionMock.Verify(a => a.SendMessage(It.Is<IFileInfo[]>(f => f.Count() == 1)), Times.Once());
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
