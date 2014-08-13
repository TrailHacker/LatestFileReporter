using System;
using System.IO;
using System.Linq;
using System.Text;
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

		public Application(IFileSystem definition, params TextWriter[] writers)
		{
			_writers = writers ?? new TextWriter[0];

			Definition = definition;
			Mailer = new Mailer();
			BatchRunner = new BatchFileRunner();
			MessageFactory = new MessageFactory();
		}

		public IFileInfo[] GetOutdatedFiles()
		{
			var directory = new DirectoryInfo(Definition.SourceFileDirectoryPath);
			var files = (from fileInfo in directory.GetFileSystemInfos("*" + Definition.SearchFileExtension)
						 let file = (IFileInfo) new FileWrapper(fileInfo)
						 where !HasProcessedToday(file)
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
			var sourceFilePath = BuildPath(Definition.SourceFileDirectoryPath, fileName, Definition.SearchFileExtension);
			var destinationFilePath = BuildPath(Definition.DestinationFileDirectoryPath, fileName, Definition.SearchFileExtension);

			// if the file doesn't exist, or it exists, but hasn't been processed today, return false
			if (!File.Exists(sourceFilePath) || !HasProcessedToday(sourceFilePath))
				return false;

			// otherwise, the file exists and was processed today, so copy it to the destination path
			File.Copy(sourceFilePath, destinationFilePath, true);
			WriteLine("Copied file from source directory: {0}", fileName);

			// the copy was successful
			return true;
		}

		public bool RunBatchFile(string fileName)
		{
			var path = BuildPath(BatchRunner.BatchFileDirectoryPath, fileName, ".bat");
			if (!File.Exists(path))
			{
				WriteLine("Batch file does note exist: {0}", path);
				return false;
			}

			WriteLine("Started batch file: [{0}]", path);
			var exitCode = BatchRunner.Run(path);
			WriteLine("Batch file exited with '{0}'", exitCode);
			return exitCode == 0;
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

		public bool DoesLogFileIndicateCommonError(string fileName)
		{
			var logFile = BuildPath(Definition.LogFileDirectoryPath, fileName, Definition.LogFileExtension);
			var tail = ReadEndTokens(logFile, 10, Encoding.UTF8, "\n");
			return tail.Contains("Unable to create binary file");
		}

		private string BuildPath(string directory, string fileName, string extension)
		{
			var temp = Path.Combine(directory, fileName);
			var name = Path.GetFileNameWithoutExtension(temp);
			return Path.Combine(directory, name + extension);
		}

		// *******************************************************************
		// NOTE: code taken from http://stackoverflow.com/a/398512/84406
		// *******************************************************************
		private static string ReadEndTokens(string path, Int64 numberOfTokens, Encoding encoding, string tokenSeparator)
		{

			var sizeOfChar = encoding.GetByteCount("\n");
			var buffer = encoding.GetBytes(tokenSeparator);


			using (var fs = new FileStream(path, FileMode.Open))
			{
				Int64 tokenCount = 0;
				var endPosition = fs.Length / sizeOfChar;

				for (Int64 position = sizeOfChar; position < endPosition; position += sizeOfChar)
				{
					fs.Seek(-position, SeekOrigin.End);
					fs.Read(buffer, 0, buffer.Length);

					if (encoding.GetString(buffer) != tokenSeparator) 
						continue;

					tokenCount++;
					if (tokenCount != numberOfTokens) 
						continue;

					var returnBuffer = new byte[fs.Length - fs.Position];
					fs.Read(returnBuffer, 0, returnBuffer.Length);
					return encoding.GetString(returnBuffer);
				}

				// handle case where number of tokens in file is less than numberOfTokens
				fs.Seek(0, SeekOrigin.Begin);
				buffer = new byte[fs.Length];
				fs.Read(buffer, 0, buffer.Length);
				return encoding.GetString(buffer);
			}
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