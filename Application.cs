using System;
using System.IO;
using System.Linq;

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

		public FileSystemInfo[] GetOutdatedFiles()
		{
			var expectedDate = DateTime.Now.Date;
			var directory = new DirectoryInfo(Definition.SourceDirectoryPath);

			return (from file in directory.GetFileSystemInfos(Definition.SearchPattern)
				orderby file.LastWriteTime descending
				where file.LastWriteTime < expectedDate
				select file).ToArray();
		}

		public bool CopySourceFile(string fileName)
		{
			throw new System.NotImplementedException();
		}

		public bool RunBatchFile(string fileName)
		{
			throw new System.NotImplementedException();
		}

		public bool KeepGoing(int runCount)
		{
			return runCount < BatchRunner.AttemptedRunCounter;
		}

		public void SendMessage(FileSystemInfo[] files)
		{
			var message = MessageFactory.Create(files);
			Mailer.Send(message);
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