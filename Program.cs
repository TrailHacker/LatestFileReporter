using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LatestFileReporter.Interfaces;
using log4net;
using log4net.Config;

namespace LatestFileReporter
{
	sealed class Program
	{
		private readonly ILog _logger;

		public IFileExtractor Extractor { get; set; }
		public IProgramSettings Settings { get; set; }
		public IProgramDefinition Definition { get; set; }

		#region Static Methods

		private static int Main()
		{
			var settings = new AppConfig();
			var program = new Program
			{
				Settings = settings,
				Extractor = new FileExtractor(settings),
				Definition = new ProgramDefinition(settings)
			};
			return program.Run();
		}

		public static bool HasProcessedToday(string filePath)
		{
			return HasProcessedToday(new FileWrapper(filePath));
		}

		public static bool HasProcessedToday(IFileInfo file)
		{
			return !(file.LastWriteTime < DateTime.Now.Date);
		}


		#endregion

		public Program()
		{
			XmlConfigurator.ConfigureAndWatch(new FileInfo("logger.config"));
			_logger = LogManager.GetLogger(typeof(Program));
		}

		public int Run()
		{

			int result;

			try
			{
				_logger.Info("Beginning the file checker logic");

				var filesToProcess = Extractor.GetFilesToProcess().ToArray();
				var outdatedFiles = GetOutdatedFiles(filesToProcess);
				var emptyFiles = GetEmptyFiles(filesToProcess);
				if (emptyFiles.Any())
					_logger.ErrorFormat("{0} file(s) are empty", emptyFiles.Count());

				var attempt = 0;
				var keepGoing = true;

				while (outdatedFiles.Any() && keepGoing)
				{
					_logger.WarnFormat("There are {0} files to process", outdatedFiles.Count());
					_logger.InfoFormat("Attempt #{0}", attempt + 1);

					foreach (var file in outdatedFiles)
						ProcessFile(file);

					attempt++;

					_logger.Info("Updating list of files to analyze");
					filesToProcess = Extractor.GetFilesToProcess().ToArray();
					outdatedFiles = GetOutdatedFiles(filesToProcess);
					keepGoing = KeepGoing(attempt);

				}

				_logger.Info("Sending email!");
				SendMessage(outdatedFiles);
				result = outdatedFiles.Count();

			}
			catch (TooManyFailingCubesException majorOops)
			{
				var failingFiles = majorOops.FailingFiles.Select(f => string.Format("{0} ({1})", f.Name, f.LastWriteTime));
				_logger.ErrorFormat("{0}: \nOutdated Files: {1}", majorOops.Message, string.Join(", ", failingFiles));

				SendMessage(majorOops.FailingFiles);
				result = -1;
			}
			catch(Exception oops)
			{
				ReportError(oops.StackTrace);
				SendMessage(new IFileInfo[0]);
				result = -9;
			}

			return result;

		}

		private void ProcessFile(IFileInfo file)
		{
			if (!Definition.DoesLogFileIndicateCommonError(file.Name))
			{
				_logger.InfoFormat("File does not have common error: {0}", file.Name);
				return;
			}

			if (Definition.CopySourceFile(file.Name))
			{
				_logger.InfoFormat("Copied file from source directory: {0}", file.Name);
				return;
			}

			_logger.InfoFormat("Starting batch file for '{0}'...", file.Name);
			var result = Definition.RunBatchFile(file.Name);
			_logger.InfoFormat("Finished batch file! (Result = {0})", result);

		}

		#region Private Methods

		private IFileInfo[] GetOutdatedFiles(IEnumerable<IFileInfo> filesToAnalyze)
		{
			var files = (from file in filesToAnalyze
				where !HasProcessedToday(file)
				select file).ToArray();

			switch (files.Length)
			{
				case 0:
					_logger.Info("All files are up to date!");
					break;
				case 1:
					_logger.Warn("There is 1 file out of date.");
					break;
				default:
					_logger.WarnFormat("There are {0} files that didn't process today.", files.Count());
					break;
			}

			if (files.Length > Settings.MaxCountOfOutdatedFilesBeforeFailing)
				throw new TooManyFailingCubesException(files);

			return files;
		}

		private IFileInfo[] GetEmptyFiles(IEnumerable<IFileInfo> filesToAnalyze)
		{
			var files = (from file in filesToAnalyze
				where file.FileSize <= GetEmptyFileThreshold(file)
				select file).ToArray();

			switch (files.Length)
			{
				case 0:
					_logger.Info("All files are up to date!");
					break;
				case 1:
					_logger.Warn("There is 1 file out of date.");
					break;
				default:
					_logger.WarnFormat("There are {0} files that didn't process today.", files.Count());
					break;
			}

			if (files.Length > Settings.MaxCountOfOutdatedFilesBeforeFailing)
				throw new TooManyFailingCubesException(files);

			return files;
		}

		private long GetEmptyFileThreshold(IFileInfo file)
		{
			var path = Settings.SourceFileDirectoryPath;
			var extension = Settings.FileSizeExtensionToCompare;
			if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(extension))
				return 0L;

			var filePath = Path.Combine(path, file.Name, extension);
			var sourceFile = new FileInfo(filePath);
			return sourceFile.Length;
		}

		private bool KeepGoing(int attempts)
		{
			_logger.InfoFormat("Attempt {0} of {1}.", attempts, Settings.AttemptedRunCounter);
			return attempts < Settings.AttemptedRunCounter;
		}

		private void ReportError(string message)
		{
			_logger.Error(message);
		}

		private void SendMessage(IFileInfo[] files)
		{
			var emailer = new Emailer(Settings, files);
			emailer.SendMessage();
		}


		#endregion
	}
}
