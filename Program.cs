using System;
using System.IO;
using System.Linq;

namespace LatestFileReporter
{
	sealed class Program
	{

		private bool _showHelp;
		public IFileSystem Definition { get; set; }
		public IBatchRunner BatchRunner { get; set; }
		public IMessageFactory MessageFactory { get; set; }
		public IMailer Mailer { get; set; }

		public Program()
		{
			_showHelp = false;
			Definition = new AppFileSystem();
			Mailer = new Mailer();
			BatchRunner = new BatchFileRunner();
			MessageFactory = new MessageFactory();
		}

		public int Run()
		{

			_showHelp = false;
			var result = 0;

			try
			{
				var expectedDate = DateTime.Now.Date;
				var directory = new DirectoryInfo(Definition.SourceDirectoryPath);

				var files = (from file in directory.GetFileSystemInfos(Definition.SearchPattern)
					orderby file.LastWriteTime descending
					where file.LastWriteTime < expectedDate
					select file).ToArray();

				var message = MessageFactory.Create(files);
				Mailer.Send(message);

			}
			catch(Exception oops)
			{
				Console.WriteLine(oops.StackTrace);
				result = -1;
			}

			return result;

		}

		static int Main(string[] args)
		{
			var program = new Program();
			ParseArgs(program, args);
			return program.Run();
		}

		private static void ParseArgs(Program program, string[] args)
		{
			if (args.Length == 0)
				return;

			program.Definition.DestinationsDirectoryPath = args[0];

			// do more complex processing here!

		}

	}
}
