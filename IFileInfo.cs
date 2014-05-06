using System;

namespace LatestFileReporter
{
	public interface IFileInfo
	{
		DateTime LastWriteTime { get; }
		string Name { get; }
	}
}