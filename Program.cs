using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Linq;
using LatestFileReporter.Interfaces;

namespace LatestFileReporter
{
	sealed class Program
	{
		public IProgramSettings Settings { get; set; }
		public IProgramDefinition Definition { get; set; }
		private static TextWriter[] _writers; 

		static int Main()
		{
			var settings = new AppConfigProgramSettings();
			_writers = new[] {Console.Out};

			var program = new Program
			{
				Settings = settings,
				Definition = new ProgramDefinition(settings)
			};
			return program.Run();

		}

		/*
		 * var files = GetOutdatedFiles();
		 * var keepGoing = true;
		 * var attempts = 0;
		 * 
		 * while (files.Any() && keepGoing)
		 * {
		 *	foreach(var file in files)
		 *	{
		 *		if (!DoesLogFileIndicateCommonError(file))
		 *			continue;
		 *		
		 *		if (CopySourceFile(file))
		 *			continue;
		 *			
		 *		if (RunBatchFile(file))
		 *			continue;
		 *			
		 *		WriteLine("Batch file failed: " + file.Name);
		 *	}
		 *	
		 *	attempts++;
		 *	files = GetOutdatedFiles();
		 *	keepGoing = KeepGoing(attempts);
		 *	
		 * }
		 * 
		 * SendMessage(files, attempts);
		 * result = files.Length;
		 * 
		 */
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
						if (attempt > 0 && !DoesLogFileIndicateCommonError(file.Name))
							continue;

						if (!CopySourceFile(file.Name))
							RunBatchFile(file.Name);
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
				result = -1;
			}
			catch(Exception oops)
			{
				ReportError(oops.StackTrace);
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

			if (files.Length > Settings.MaxFailCountBeforeFailing)
				throw new TooManyFailingCubesException(files);

			return files;
		}

		private bool DoesLogFileIndicateCommonError(string filePath)
		{
			return Definition.DoesLogFileIndicateCommonError(filePath);
		}

		private bool CopySourceFile(string fileName)
		{
			return Definition.CopySourceFile(fileName);
		}

		private bool RunBatchFile(string fileName)
		{
			return Definition.RunBatchFile(fileName);
		}

		private bool KeepGoing(int attempts)
		{
			return Definition.KeepGoing(attempts);
		}

		private void ReportError(string message)
		{
			Definition.ReportError(message);
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

		public static void WriteLine(string format, params object[] args)
		{
			if (args == null)
				throw new ArgumentNullException("args");

			WriteLine(string.Format(format, args));
		}

		public static void WriteLine(string message)
		{
			foreach (var writer in _writers)
				writer.WriteLine(message);
		}

	}
}
