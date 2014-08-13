using System;
using System.Configuration;
using LatestFileReporter.Interfaces;

namespace LatestFileReporter
{
	public class AppConfigProgramSettings : IProgramSettings
	{
		public AppConfigProgramSettings()
		{
			var appSettings = ConfigurationManager.AppSettings;

			DestinationFileDirectoryPath = appSettings["destinationDirectory"];
			SourceFileDirectoryPath = appSettings["sourceDirectory"];
			SearchFileExtension = appSettings["searchFileExtension"];
			LogFileDirectoryPath = appSettings["logsDirectory"];
			LogFileExtension = appSettings["logFileExtension"];
			MaxFailCountBeforeFailing = Convert.ToInt32(appSettings["MaxCountOfOutdatedFilesBeforeFailing"]);
		}

		public string DestinationFileDirectoryPath { get; private set; }
		public string SourceFileDirectoryPath { get; private set; }
		public string LogFileDirectoryPath { get; private set; }
		public string SearchFileExtension { get; private set; }
		public string LogFileExtension { get; private set; }
		public int MaxFailCountBeforeFailing { get; private set; }
	}
}