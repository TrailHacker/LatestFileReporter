using System;
using System.Configuration;
using System.Linq;
using LatestFileReporter.Interfaces;

namespace LatestFileReporter
{
	sealed class Program
	{
		private static AppFileSystem _definition;

		static int Main()
		{
			_definition = new AppFileSystem();

			var app = new Application(_definition, Console.Out);
			var program = new Program();
			return program.Run(app);

		}

		public int Run(IApplication app)
		{

			int result;

			try
			{
				var files = app.GetOutdatedFiles();
				if (files.Length > _definition.MaxFailCountBeforeFailing)
					throw new TooManyFailingCubesException(files);

				var attempt = 0;
				var keepGoing = true;
				while (files.Any() && keepGoing)
				{
					foreach (var file in files)
					{
						// ensure that the log file reports a certain error...
						if (attempt > 0 && !app.DoesLogFileIndicateCommonError(file.Name))
							continue;

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
			catch (TooManyFailingCubesException majorOops)
			{
				app.ReportError(string.Format("{0}: \nOutdated Files: {1}", majorOops.Message,
					majorOops.FailingFiles.Select(f => string.Format("{0} ({1})", f.Name, f.LastWriteTime))));
				result = -1;
			}
			catch(Exception oops)
			{
				app.ReportError(oops.StackTrace);
				result = -9;
			}

			return result;

		}

	}
}
