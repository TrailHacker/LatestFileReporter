using System;
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
	}
}