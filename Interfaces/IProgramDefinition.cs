using System.Linq;

namespace LatestFileReporter.Interfaces
{
	public interface IProgramDefinition
	{
		IQueryable<IFileInfo> GetFilesAsQueryable();

		string GetSourceFile(string fileName);
		string GetDestinationFile(string fileName);

		bool CopySourceFile(string fileName);
		bool RunBatchFile(string fileName);
		bool KeepGoing(int runCount);
		void SendMessage(IFileInfo[] files);
		void ReportError(string message);
		bool DoesLogFileIndicateCommonError(string fileName);
	}
}