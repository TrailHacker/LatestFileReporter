using System.Collections.Generic;
using LatestFileReporter.Interfaces;

namespace LatestFileReporter
{
	public interface IFileExtractor {
		IEnumerable<IFileInfo> GetFilesToProcess();
	}
}