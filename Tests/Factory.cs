using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LatestFileReporter.Interfaces;
using Moq;

namespace LatestFileReporter.Tests
{
	public static class Factory
	{
		public static IFileInfo CreateFile(string fileName, DateTime date)
		{
			var safeName = fileName;
			var safeDate = date;
			var file = new Mock<IFileInfo>();
			file.Setup(f => f.Name).Returns(safeName);
			file.Setup(f => f.LastWriteTime).Returns(safeDate);
			return file.Object;
		}

		public static IProgramSettings CreateSettings(int attemptCount = 1, int maxUnproccessedFilesBeforeExceptionCount = 3)
		{
			var safeAttemptCount = attemptCount;
			var safeMaxBlahCount = maxUnproccessedFilesBeforeExceptionCount;

			var settingsMock = new Mock<IProgramSettings>();
			settingsMock.Setup(d => d.MaxCountOfOutdatedFilesBeforeFailing).Returns(safeMaxBlahCount);
			settingsMock.Setup(d => d.AttemptedRunCounter).Returns(safeAttemptCount);
			return settingsMock.Object;
		}

		public static IProgramDefinition CreateDefinition(IEnumerable<IFileInfo> files, 
			out Mock<IProgramDefinition> mock,
			Expression<Func<string, bool>> expressionToPassLogCheck = null,
			Expression<Func<string, bool>> expressionToPassCopyCheck = null,
			Expression<Func<string, bool>> expressionToPassBatchRun = null)
		{
			var definitionMock = new Mock<IProgramDefinition>();
			if (expressionToPassLogCheck == null)
				expressionToPassLogCheck = s => true;
			if (expressionToPassCopyCheck == null)
				expressionToPassCopyCheck = s => true;
			if (expressionToPassBatchRun == null)
				expressionToPassBatchRun = s => true;

			//definitionMock.Setup(a => a.GetFilesToProcess()).Returns(files.AsQueryable());
			definitionMock.Setup(a => a.DoesLogFileIndicateCommonError(It.Is(expressionToPassLogCheck))).Returns(true); // only executes batch when function returns true
			definitionMock.Setup(a => a.CopySourceFile(It.Is(expressionToPassCopyCheck))).Returns(true); // batch file only executes when copy function returns false
			definitionMock.Setup(a => a.RunBatchFile(It.Is(expressionToPassBatchRun))).Returns(true); // batch file only executes when copy function returns false

			mock = definitionMock;
			return definitionMock.Object;
		}
	}
}