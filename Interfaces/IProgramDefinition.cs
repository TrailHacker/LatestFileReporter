namespace LatestFileReporter.Interfaces
{
	public interface IProgramDefinition
	{
		bool CopySourceFile(string fileName);
		bool RunBatchFile(string fileName);
		bool DoesLogFileIndicateCommonError(string fileName);
	}
}