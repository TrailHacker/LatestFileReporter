namespace LatestFileReporter.Interfaces
{
	public interface IBatchRunner
	{
		string BatchFileDirectoryPath { get; set; }
		int AttemptedRunCounter { get; set; }
		int Run(string filePath);
	}
}