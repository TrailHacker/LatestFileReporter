using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LatestFileReporter.Interfaces;

namespace LatestFileReporter
{
	public class ProgramDefinition : IProgramDefinition
	{

		public IProgramSettings Settings { get; set; }
		public IBatchRunner BatchRunner { get; set; }
		public IMessageFactory MessageFactory { get; set; }
		public IMailer Mailer { get; set; }

		public ProgramDefinition(IProgramSettings settings)
		{
			Settings = settings;
			Mailer = new Mailer();
			BatchRunner = new BatchFileRunner();
			MessageFactory = new MessageFactory();
		}

		public IQueryable<IFileInfo> GetFilesAsQueryable()
		{
			var directory = new DirectoryInfo(Settings.SourceFileDirectoryPath);
			return (from fileInfo in directory.GetFileSystemInfos("*" + Settings.SearchFileExtension)
				select (IFileInfo) new FileWrapper(fileInfo)).AsQueryable();
		}

		public string GetSourceFile(string fileName)
		{
			return BuildPath(Settings.SourceFileDirectoryPath, fileName, Settings.SearchFileExtension);
		}

		public string GetDestinationFile(string fileName)
		{
			return BuildPath(Settings.DestinationFileDirectoryPath, fileName, Settings.SearchFileExtension);
		}

		public bool CopySourceFile(string fileName)
		{
			var sourceFilePath = BuildPath(Settings.SourceFileDirectoryPath, fileName, Settings.SearchFileExtension);
			var destinationFilePath = BuildPath(Settings.DestinationFileDirectoryPath, fileName, Settings.SearchFileExtension);

			// if the file doesn't exist, or it exists, but hasn't been processed today, return false
			if (!File.Exists(sourceFilePath) || !Program.HasProcessedToday(sourceFilePath))
				return false;

			// otherwise, the file exists and was processed today, so copy it to the destination path
			File.Copy(sourceFilePath, destinationFilePath, true);
			Program.WriteLine("Copied file from source directory: {0}", fileName);

			// the copy was successful
			return true;
		}

		public bool RunBatchFile(string fileName)
		{
			var path = BuildPath(BatchRunner.BatchFileDirectoryPath, fileName, ".bat");
			if (!File.Exists(path))
			{
				Program.WriteLine("Batch file does note exist: {0}", path);
				return false;
			}

			Program.WriteLine("Started batch file: [{0}]", path);
			var exitCode = BatchRunner.Run(path);
			Program.WriteLine("Batch file exited with '{0}'", exitCode);
			return exitCode == 0;
		}

		public bool KeepGoing(int runCount)
		{
			Program.WriteLine("Attempt {0} of {1}.", runCount, BatchRunner.AttemptedRunCounter);
			return runCount < BatchRunner.AttemptedRunCounter;
		}

		public void SendMessage(IFileInfo[] files)
		{
			var message = MessageFactory.Create(files);
			Mailer.Send(message);
		}

		public void ReportError(string message)
		{
			Program.WriteLine(message);
		}

		public bool DoesLogFileIndicateCommonError(string fileName)
		{
			var logFile = BuildPath(Settings.LogFileDirectoryPath, fileName, Settings.LogFileExtension);
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

	}
}