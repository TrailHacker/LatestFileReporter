namespace LatestFileReporter
{
	public interface IBatchRunner
	{
		string BatchFileDirectoryPath { get; set; }
		int AttemptedRunCounter { get; set; }
	}
}