using System;
using System.Linq;
using LatestFileReporter.Interfaces;
using log4net;

namespace LatestFileReporter
{
	sealed class Program
	{
		private readonly ILog _logger;

		public IProgramSettings Settings { get; set; }
		public IProgramDefinition Definition { get; set; }

		#region Static Methods

		private static int Main()
		{
			var settings = new AppConfig();
			var program = new Program
			{
				Settings = settings,
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
			_logger = LogManager.GetLogger(typeof(Program));
		}

		public int Run()
		{

			int result;

			try
			{
				var files = GetOutdatedFiles();
				var attempt = 0;
				var keepGoing = true;

				while (files.Any() && keepGoing)
				{
					foreach (var file in files)
					{
						if (!DoesLogFileIndicateCommonError(file.Name))
							continue;

						if (CopySourceFile(file.Name))
							continue;

						RunBatchFile(file.Name);

					}

					attempt++;
					files = GetOutdatedFiles();
					keepGoing = KeepGoing(attempt);

				}

				_logger.Info("Sending email!");
				SendMessage(files);
				result = files.Count();

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

		#region Private Methods

		private IFileInfo[] GetOutdatedFiles()
		{
			var files = (from file in Definition.GetFilesAsQueryable()
				where !HasProcessedToday(file)
				select file).ToArray();

			switch (files.Length)
			{
				case 0:
					_logger.Info("All files are up to date!");
					break;
				case 1:
					_logger.Info("There is 1 file out of date.");
					break;
				default:
					_logger.WarnFormat("There are {0} files that didn't process today.", files.Count());
					break;
			}

			if (files.Length > Settings.MaxCountOfOutdatedFilesBeforeFailing)
				throw new TooManyFailingCubesException(files);

			return files;
		}

		private bool DoesLogFileIndicateCommonError(string filePath)
		{
			if (Definition.DoesLogFileIndicateCommonError(filePath))
				return true;

			_logger.Info("Log file does NOT report a common error. Reading next file...");
			return false;
		}

		private bool CopySourceFile(string fileName)
		{
			if (!Definition.CopySourceFile(fileName))
				return false;

			_logger.InfoFormat("Copied file from source directory: {0}", fileName);
			return true;
		}

		private bool RunBatchFile(string fileName)
		{
			_logger.InfoFormat("Starting batch file to rebuild [{0}]", fileName);

			var result = Definition.RunBatchFile(fileName);
			_logger.Info("Finished processing batch file");
			return result;
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
			Definition.SendMessage(files);
		}


		#endregion
	}
}
