namespace LatestFileReporter.Interfaces
{
	public interface IApplication
	{
		IFileInfo[] GetOutdatedFiles();
		bool CopySourceFile(string fileName);
		bool RunBatchFile(string fileName);
		bool KeepGoing(int runCount);
		void SendMessage(IFileInfo[] files);
		void ReportError(string message);

	}
}