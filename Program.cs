using System;
using System.IO;
using System.Linq;

namespace LatestFileReporter
{
	sealed class Program
	{

		public int Run()
		{

			int result;
			var app = new Application(Console.Out);

			try
			{
				var files = app.GetOutdatedFiles();

				var attempt = 0;
				var stop = false;
				while (files.Any() || stop)
				{
					foreach (var file in files)
					{
						if (!app.CopySourceFile(file.Name))
							app.RunBatchFile(file.Name);
					}

					attempt++;
					files = app.GetOutdatedFiles();
					stop = !app.KeepGoing(attempt);

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

		static int Main()
		{
			var program = new Program();
			return program.Run();
		}

	}
}
