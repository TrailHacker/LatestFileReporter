using System.IO;

namespace LatestFileReporter
{
	public interface IApplication
	{
		FileSystemInfo[] GetOutdatedFiles();
		bool CopySourceFile(string fileName);
		bool RunBatchFile(string fileName);
		bool KeepGoing(int runCount);
		void SendMessage(FileSystemInfo[] files);

	}
}