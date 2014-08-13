namespace LatestFileReporter.Interfaces
{
	public interface IProgramSettings
	{
		string DestinationFileDirectoryPath { get; }
		string SourceFileDirectoryPath { get; }
		string SearchFileExtension { get; }
		string LogFileDirectoryPath { get; }
		string LogFileExtension { get; }
		int MaxFailCountBeforeFailing { get; }
	}
}