using System;
using System.IO;
using System.Linq;
using LatestFileReporter.Interfaces;

namespace LatestFileReporter
{
	sealed class Program
	{
		public IProgramSettings Settings { get; set; }
		public IProgramDefinition Definition { get; set; }

		private readonly TextWriter[] _writers; 

		static int Main()
		{
			var settings = new AppConfigProgramSettings();

			var program = new Program
			{
				Settings = settings,
				Definition = new ProgramDefinition(settings)
			};
			return program.Run();

		}

		public Program()
		{
			_writers = new[] {Console.Out};
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
						// ensure that the log file reports a certain error...
						if (!DoesLogFileIndicateCommonError(file.Name))
							continue;

						if (CopySourceFile(file.Name))
							continue;

						if (RunBatchFile(file.Name))
							continue;

						WriteLine("Batch file failed!");
					}

					attempt++;
					files = GetOutdatedFiles();
					keepGoing = KeepGoing(attempt);

				}

				SendMessage(files);
				result = files.Count();

			}
			catch (TooManyFailingCubesException majorOops)
			{
				ReportError(string.Format("{0}: \nOutdated Files: {1}", majorOops.Message,
					majorOops.FailingFiles.Select(f => string.Format("{0} ({1})", f.Name, f.LastWriteTime))));

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

		private IFileInfo[] GetOutdatedFiles()
		{
			var files = (from file in Definition.GetFilesAsQueryable()
				where !HasProcessedToday(file)
				select file).ToArray();

			switch (files.Length)
			{
				case 0:
					WriteLine("All files are up to date!");
					break;
				case 1:
					WriteLine("There is 1 file out of date.");
					break;
				default:
					WriteLine("There are {0} files that didn't process today.", files.Count());
					break;
			}

			if (files.Length > Settings.MaxCountOfOutdatedFilesBeforeFailing)
				throw new TooManyFailingCubesException(files);

			return files;
		}

		private bool DoesLogFileIndicateCommonError(string filePath)
		{
			return Definition.DoesLogFileIndicateCommonError(filePath);
		}

		private bool CopySourceFile(string fileName)
		{
			if (!Definition.CopySourceFile(fileName)) 
				return false;

			WriteLine("Copied file from source directory: {0}", fileName);
			return true;
		}

		private bool RunBatchFile(string fileName)
		{
			WriteLine("Starting batch file to rebuild [{0}]", fileName);
			var result = Definition.RunBatchFile(fileName);
			WriteLine("Finished processing batch file");
			return result;
		}

		private bool KeepGoing(int attempts)
		{
			WriteLine("Attempt {0} of {1}.", attempts, Settings.AttemptedRunCounter);
			return attempts < Settings.AttemptedRunCounter;
		}

		private void ReportError(string message)
		{
			WriteLine("Error: " + message);
		}

		private void SendMessage(IFileInfo[] files)
		{
			Definition.SendMessage(files);
		}

		public static bool HasProcessedToday(IFileInfo file)
		{
			return !(file.LastWriteTime < DateTime.Now.Date);
		}

		public static bool HasProcessedToday(string filePath)
		{
			return HasProcessedToday(new FileWrapper(filePath));
		}

		private void WriteLine(string format, params object[] args)
		{
			if (args == null)
				throw new ArgumentNullException("args");

			WriteLine(string.Format(format, args));
		}

		private void WriteLine(string message)
		{
			foreach (var writer in _writers)
				writer.WriteLine(message);
		}

	}
}
