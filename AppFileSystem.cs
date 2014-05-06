using System.Configuration;

namespace LatestFileReporter
{
	public class AppFileSystem : IFileSystem
	{
		public AppFileSystem()
		{
			var appSettings = ConfigurationManager.AppSettings;
			SearchFileExtension = appSettings["searchFileExtension"];
			LogFileExtension = appSettings["logFileExtension"];
			DestinationsDirectoryPath = appSettings["destinationDirectory"];
			SourceDirectoryPath = appSettings["sourceDirectory"];
			
		}
		public string DestinationsDirectoryPath { get; set; }
		public string SourceDirectoryPath { get; set; }
		public string SearchFileExtension { get; set; }
		public string LogFileDirectoryPath { get; set; }
		public string LogFileExtension { get; set; }
	}
}