using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LatestFileReporter.Interfaces;

namespace LatestFileReporter
{
	public class Application : IApplication
	{
		private readonly TextWriter[] _writers;
		public IFileSystem Definition { get; set; }
		public IBatchRunner BatchRunner { get; set; }
		public IMessageFactory MessageFactory { get; set; }
		public IMailer Mailer { get; set; }

		public Application(params TextWriter[] writers)
		{
			_writers = writers ?? new TextWriter[0];

			Definition = new AppFileSystem();
			Mailer = new Mailer();
			BatchRunner = new BatchFileRunner();
			MessageFactory = new MessageFactory();
		}

		public IFileInfo[] GetOutdatedFiles()
		{
			var directory = new DirectoryInfo(Definition.SourceDirectoryPath);
			var files = (from fileInfo in directory.GetFileSystemInfos("*" + Definition.SearchFileExtension)
						 let file = new FileWrapper(fileInfo)
				where HasProcessedToday(file)
				select file).ToArray();

			switch (files.Count())
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

			return files;
		}

		private bool HasProcessedToday(IFileInfo file)
		{
			return !(file.LastWriteTime < DateTime.Now.Date);
		}

		private bool HasProcessedToday(string filePath)
		{
			return HasProcessedToday(new FileWrapper(filePath));
		}

		public bool CopySourceFile(string fileName)
		{
			var sourceFilePath = Path.Combine(Definition.SourceDirectoryPath, fileName);
			var destinationFilePath = Path.Combine(Definition.DestinationsDirectoryPath, sourceFilePath);
			if (!File.Exists(sourceFilePath) || !HasProcessedToday(sourceFilePath))
				return false;

			File.Copy(sourceFilePath, destinationFilePath);
			WriteLine("Copied file from source directory: {0}", fileName);
			return true;
		}

		public bool RunBatchFile(string fileName)
		{
			// TODO: http://stackoverflow.com/a/5519517/84406
			var path = Path.Combine(BatchRunner.BatchFileDirectoryPath, fileName);
			if (!File.Exists(path))
			{
				WriteLine("Batch file does note exist: {0}", path);
				return false;
			}

			var info = new ProcessStartInfo(path);
			var process = Process.Start(info);
			WriteLine("Started batch file: [{0}]", path);
			if (process == null)
			{
				WriteLine("Batch process is null");
				return false;
			}

			process.WaitForExit();
			WriteLine("Batch file exited with '{0}'", process.ExitCode);
			return process.ExitCode == 0;
		}

		public bool KeepGoing(int runCount)
		{
			WriteLine("Attempt {0} of {1}.", runCount, BatchRunner.AttemptedRunCounter);
			return runCount < BatchRunner.AttemptedRunCounter;
		}

		public void SendMessage(IFileInfo[] files)
		{
			var message = MessageFactory.Create(files);
			Mailer.Send(message);
		}

		public void ReportError(string message)
		{
			WriteLine(message);
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