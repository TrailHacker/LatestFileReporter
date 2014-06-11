namespace LatestFileReporter.Interfaces
{
	public interface IFileSystem
	{
		string DestinationsDirectoryPath { get; set; }
		string SourceDirectoryPath { get; set; }
		string SearchFileExtension { get; set; }
		string LogFileDirectoryPath { get; set; }
		string LogFileExtension { get; set; }
	}
}