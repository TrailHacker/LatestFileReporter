using System.Configuration;

namespace LatestFileReporter
{
	public class BatchFileRunner : IBatchRunner
	{
		public BatchFileRunner()
		{
			var appSettings = ConfigurationManager.AppSettings;
			BatchFileDirectoryPath = appSettings["batchDirectory"];
			AttemptedRunCounter = 3;
		}

		public string BatchFileDirectoryPath { get; set; }
		public int AttemptedRunCounter { get; set; }
	}
}