using System;

namespace LatestFileReporter.Interfaces
{
	public interface IFileInfo
	{
		DateTime LastWriteTime { get; }
		string Name { get; }
	}
}