using System;
using System.Linq;
using LatestFileReporter.Interfaces;
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
			var app = new Mock<IApplication>();
			var files = new[]
			{
				CreateFile("test.cub", DateTime.Now.AddDays(-1).Date)
			};
			app.Setup(a => a.GetOutdatedFiles()).Returns(files);
			app.Setup(a => a.CopySourceFile(It.Is<string>(s => s == "test.cub"))).Returns(false);
			app.Setup(a => a.KeepGoing(It.Is<int>(i => i == 1))).Returns(false);

			var program = new Program();
			var result = program.Run(app.Object);

			app.Verify(a => a.RunBatchFile(It.Is<string>(s => s == "test.cub")), Times.Once());
			app.Verify(a => a.SendMessage(It.Is<IFileInfo[]>(f => f.Count() == 1)));
			Assert.AreEqual(1, result);

		}

		private static IFileInfo CreateFile(string fileName, DateTime date)
		{
			var safeName = fileName;
			var safeDate = date;
			var file = new Mock<IFileInfo>();
			file.Setup(f => f.Name).Returns(safeName);
			file.Setup(f => f.LastWriteTime).Returns(safeDate);
			return file.Object;
		}

	}
	// ReSharper restore InconsistentNaming
}
