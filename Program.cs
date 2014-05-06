using System;
using System.IO;
using System.Linq;

namespace LatestFileReporter
{
	sealed class Program
	{
		static int Main()
		{
			var app = new Application(Console.Out);
			var program = new Program();
			return program.Run(app);
		}

		public int Run(IApplication app)
		{

			int result;

			try
			{
				var files = app.GetOutdatedFiles();

				var attempt = 0;
				var keepGoing = true;
				while (files.Any() && keepGoing)
				{
					foreach (var file in files)
					{
						if (!app.CopySourceFile(file.Name))
							app.RunBatchFile(file.Name);
					}

					attempt++;
					files = app.GetOutdatedFiles();
					keepGoing = app.KeepGoing(attempt);

				}

				app.SendMessage(files);
				result = files.Count();

			}
			catch(Exception oops)
			{
				Console.WriteLine(oops.StackTrace);
				result = -1;
			}

			return result;

		}

	}
}
