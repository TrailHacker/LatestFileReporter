using System.Configuration;

namespace LatestFileReporter
{
	public class AppFileSystem : IFileSystem
	{
		public AppFileSystem()
		{
			var appSettings = ConfigurationManager.AppSettings;
			SearchPattern = appSettings["searchPattern"];
			LogFileExtension = appSettings["logFileExtension"];
			DestinationsDirectoryPath = appSettings["destinationDirectory"];
			SourceDirectoryPath = appSettings["sourceDirectory"];
			
		}
		public string DestinationsDirectoryPath { get; set; }
		public string SourceDirectoryPath { get; set; }
		public string SearchPattern { get; set; }
		public string LogFileDirectoryPath { get; set; }
		public string LogFileExtension { get; set; }
	}
}