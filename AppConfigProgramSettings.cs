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

			FromEmailAddress = appSettings["fromEmailAddress"];
			ToEmailAddresses = appSettings["toEmailAddresses"].Split(';');
			MaxFailCountBeforeFailing = Convert.ToInt32(appSettings["MaxCountOfOutdatedFilesBeforeFailing"] ?? "3");

			AttemptedRunCounter = Convert.ToInt32(appSettings["BatchRunnerAttempts"] ?? "3");
			DestinationFileDirectoryPath = appSettings["destinationDirectory"];
			SourceFileDirectoryPath = appSettings["sourceDirectory"];
			BatchFileDirectoryPath = ConfigurationManager.AppSettings["batchDirectory"];
			LogFileDirectoryPath = appSettings["logsDirectory"];
			SearchFileExtension = appSettings["searchFileExtension"] ?? ".cub";
			LogFileExtension = appSettings["logFileExtension"] ?? ".log";
		}

		public string FromEmailAddress { get; private set; }
		public string[] ToEmailAddresses { get; private set; }

		public string DestinationFileDirectoryPath { get; private set; }
		public string SourceFileDirectoryPath { get; private set; }
		public string LogFileDirectoryPath { get; private set; }
		public string BatchFileDirectoryPath { get; private set; }
		public string SearchFileExtension { get; private set; }
		public string LogFileExtension { get; private set; }
		public int MaxFailCountBeforeFailing { get; private set; }
		public int AttemptedRunCounter { get; private set; }
	}
}