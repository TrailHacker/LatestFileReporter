using System.Linq;

namespace LatestFileReporter.Interfaces
{
	public interface IProgramDefinition
	{
		IQueryable<IFileInfo> GetFilesAsQueryable();

		bool CopySourceFile(string fileName);
		bool RunBatchFile(string fileName);
		void SendMessage(IFileInfo[] files);
		bool DoesLogFileIndicateCommonError(string fileName);
	}
}