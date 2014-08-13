namespace LatestFileReporter.Interfaces
{
	public interface IProgramSettings
	{
		string FromEmailAddress { get; }
		string[] ToEmailAddresses { get; }
		string DestinationFileDirectoryPath { get; }
		string SourceFileDirectoryPath { get; }
		string BatchFileDirectoryPath { get; }
		string SearchFileExtension { get; }
		string LogFileDirectoryPath { get; }
		string LogFileExtension { get; }
		int MaxFailCountBeforeFailing { get; }
		int AttemptedRunCounter { get; }
	}
}