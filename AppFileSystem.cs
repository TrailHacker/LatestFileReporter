using System;
using System.Configuration;
using LatestFileReporter.Interfaces;

namespace LatestFileReporter
{
	public class AppFileSystem : IFileSystem
	{
		public AppFileSystem()
		{
			var appSettings = ConfigurationManager.AppSettings;

			DestinationFileDirectoryPath = appSettings["destinationDirectory"];
			SourceFileDirectoryPath = appSettings["sourceDirectory"];
			SearchFileExtension = appSettings["searchFileExtension"];
			LogFileDirectoryPath = appSettings["logsDirectory"];
			LogFileExtension = appSettings["logFileExtension"];
			MaxFailCountBeforeFailing = Convert.ToInt32(appSettings["MaxCountOfOutdatedFilesBeforeFailing"]);
		}

		public string DestinationFileDirectoryPath { get; set; }
		public string SourceFileDirectoryPath { get; set; }
		public string LogFileDirectoryPath { get; set; }
		public string SearchFileExtension { get; set; }
		public string LogFileExtension { get; set; }
		public int MaxFailCountBeforeFailing { get; set; }
	}
}