using System;
using System.Collections.Generic;
using System.Configuration;
using LatestFileReporter.Interfaces;

namespace LatestFileReporter
{
	public class AppConfig : IProgramSettings
	{
		public AppConfig()
		{
			var appSettings = ConfigurationManager.AppSettings;

			FromEmailAddress = appSettings["fromEmailAddress"];
			ToEmailAddresses = appSettings["toEmailAddresses"].Split(';');

			MaxCountOfOutdatedFilesBeforeFailing = Convert.ToInt32(appSettings["MaxCountOfOutdatedFilesBeforeFailing"] ?? "3");
			SearchFileExtension = appSettings["searchFileExtension"] ?? ".cub";
			SourceFileDirectoryPath = appSettings["sourceDirectory"];
			DestinationFileDirectoryPath = appSettings["destinationDirectory"];
			BatchFileDirectoryPath = appSettings["batchDirectory"];
			LogFileDirectoryPath = appSettings["logsDirectory"];
			LogFileExtension = appSettings["logFileExtension"] ?? ".log";
			AttemptedRunCounter = Convert.ToInt32(appSettings["BatchRunnerAttempts"] ?? "3");

			var ignoreFileList = appSettings["ignoreFileList"] ?? string.Empty;
			IgnoreFileList = ignoreFileList.Split(';');
			FileSizeExtensionToCompare = appSettings["fileExtensionToCompareSize"];
		}

		public string FromEmailAddress { get; private set; }
		public IEnumerable<string> ToEmailAddresses { get; private set; }

		public string DestinationFileDirectoryPath { get; private set; }
		public string SourceFileDirectoryPath { get; private set; }
		public string LogFileDirectoryPath { get; private set; }
		public string BatchFileDirectoryPath { get; private set; }
		public string SearchFileExtension { get; private set; }
		public string LogFileExtension { get; private set; }
		public int MaxCountOfOutdatedFilesBeforeFailing { get; private set; }
		public int AttemptedRunCounter { get; private set; }
		public IEnumerable<string> IgnoreFileList { get; private set; }
		public string FileSizeExtensionToCompare { get; private set; }
	}
}