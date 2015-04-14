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

			var count = files.Count(f => !Program.HasProcessedToday(f));
			Assert.AreEqual(2, count);
		}

		[Test]
		public void unprocessed_file_triggers_batch_file()
		{
			Mock<IProgramDefinition> mock;
			var settings = Factory.CreateSettings();
			var files = new[] { Factory.CreateFile("test.cub", DateTime.Now.AddDays(-1).Date) };
			var definition = Factory.CreateDefinition(files, out mock, s => s == "test.cub", s => false);

			var program = new Program { Settings = settings, Definition = definition };
			var result = program.Run();

			mock.Verify(a => a.DoesLogFileIndicateCommonError(It.Is<string>(s => s == "test.cub")), Times.Once()); 
			mock.Verify(a => a.CopySourceFile(It.Is<string>(s => s == "test.cub")), Times.Once());
			mock.Verify(a => a.RunBatchFile(It.Is<string>(s => s == "test.cub")), Times.Once());
			// mock.Verify(a => a.SendMessage(It.Is<IFileInfo[]>(f => f.Count() == 1)), Times.Once());
			Assert.AreEqual(1, result);

		}

		[Test]
		public void batch_file_does_not_run_when_copy_was_successful()
		{
			Mock<IProgramDefinition> mock;
			var settings = Factory.CreateSettings();
			var files = new[] { Factory.CreateFile("test.cub", DateTime.Now.AddDays(-1).Date) };
			var definition = Factory.CreateDefinition(files, out mock, s => s == "test.cub");

			var program = new Program { Settings = settings, Definition = definition };
			var result = program.Run();

			mock.Verify(a => a.DoesLogFileIndicateCommonError(It.Is<string>(s => s == "test.cub")), Times.Once()); 
			mock.Verify(a => a.CopySourceFile(It.Is<string>(s => s == "test.cub")), Times.Once());
			mock.Verify(a => a.RunBatchFile(It.Is<string>(s => s == "test.cub")), Times.Never());
			// mock.Verify(a => a.SendMessage(It.Is<IFileInfo[]>(f => f.Count() == 1)), Times.Once());
			Assert.AreEqual(1, result);

		}

		[Test]
		public void more_than_three_unprocessed_files_short_circuits_loop()
		{
			Mock<IProgramDefinition> mock;
			var files = new[]
			{
				Factory.CreateFile("bad1.cub", DateTime.Now.AddDays(-1).Date),
				Factory.CreateFile("bad2.cub", DateTime.Now.AddDays(-1).Date),
				Factory.CreateFile("bad3.cub", DateTime.Now.AddDays(-1).Date),
				Factory.CreateFile("bad4.cub", DateTime.Now.AddDays(-1).Date),
				Factory.CreateFile("good.cub", DateTime.Now.Date)
			};
			var settings = Factory.CreateSettings();
			var definition = Factory.CreateDefinition(files, out mock);

			var program = new Program {Settings = settings, Definition = definition};
			var result = program.Run();

			mock.Verify(a => a.DoesLogFileIndicateCommonError(It.Is<string>(s => s.Contains("bad"))), Times.Never()); 
			mock.Verify(a => a.CopySourceFile(It.Is<string>(s => s.Contains("bad"))), Times.Never());
			mock.Verify(a => a.RunBatchFile(It.Is<string>(s => s.Contains("bad"))), Times.Never());
			// mock.Verify(a => a.SendMessage(It.Is<IFileInfo[]>(f => f.Count() == 4)), Times.Once());
			Assert.AreEqual(-1, result);

		}

		[Test]
		public void no_outdated_files_does_not_process_loop()
		{
			Mock<IProgramDefinition> mock;
			var settings = Factory.CreateSettings();
			var files = new[] { Factory.CreateFile("test.cub", DateTime.Now.Date) };
			var definition = Factory.CreateDefinition(files, out mock);

			var program = new Program { Settings = settings, Definition = definition };
			var result = program.Run();

			mock.Verify(a => a.DoesLogFileIndicateCommonError(It.Is<string>(s => s == "test.cub")), Times.Never()); 
			mock.Verify(a => a.CopySourceFile(It.Is<string>(s => s == "test.cub")), Times.Never());
			mock.Verify(a => a.RunBatchFile(It.Is<string>(s => s == "test.cub")), Times.Never());
			// mock.Verify(a => a.SendMessage(It.Is<IFileInfo[]>(f => !f.Any())), Times.Once());
			Assert.AreEqual(0, result);
		}

		[Test, Ignore]
		public void successful_run_reports_keep_sleeping_email()
		{
			Mock<IProgramDefinition> mock;
			var settings = Factory.CreateSettings();
			var files = new[] { Factory.CreateFile("test.cub", DateTime.Now.AddDays(-1).Date) };
			var definition = Factory.CreateDefinition(files, out mock, null, s => false);

			var program = new Program { Settings = settings, Definition = definition };
			var result = program.Run();

			mock.Verify(a => a.CopySourceFile(It.Is<string>(s => s == "test.cub")), Times.Once());
			mock.Verify(a => a.RunBatchFile(It.Is<string>(s => s == "test.cub")), Times.Once());
			mock.Verify(a => a.DoesLogFileIndicateCommonError(It.Is<string>(s => s == "test.cub")), Times.Once()); 
			//mock.Verify(a => a.SendMessage(It.Is<IFileInfo[]>(f => f.Count() == 1)), Times.Once());
			Assert.AreEqual(1, result);
		}

		[Test, Ignore]
		public void failed_run_reports_wake_up_email()
		{
			
		}

	}
	// ReSharper restore InconsistentNaming
}
