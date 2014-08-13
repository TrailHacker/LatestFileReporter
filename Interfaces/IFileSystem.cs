namespace LatestFileReporter.Interfaces
{
	public interface IFileSystem
	{
		string DestinationFileDirectoryPath { get; set; }
		string SourceFileDirectoryPath { get; set; }
		string SearchFileExtension { get; set; }
		string LogFileDirectoryPath { get; set; }
		string LogFileExtension { get; set; }
	}
}