using System.Collections.Generic;

namespace LatestFileReporter.Interfaces
{
	public interface IProgramDefinition
	{
		IEnumerable<IFileInfo> GetFilesToProcess();

		bool CopySourceFile(string fileName);
		bool RunBatchFile(string fileName);
		void SendMessage(IFileInfo[] files);
		bool DoesLogFileIndicateCommonError(string fileName);
	}
}