using System;
using System.Configuration;
using System.Diagnostics;
using LatestFileReporter.Interfaces;

namespace LatestFileReporter
{
	public class BatchFileRunner : IBatchRunner
	{
		public BatchFileRunner()
		{
			BatchFileDirectoryPath = ConfigurationManager.AppSettings["batchDirectory"];
			AttemptedRunCounter = 3;
		}

		public string BatchFileDirectoryPath { get; set; }
		public int AttemptedRunCounter { get; set; }

		// ******************************************************************
		// NOTE: code taken from http://stackoverflow.com/a/5519517/84406
		// ******************************************************************
		public int Run(string filePath)
		{
			var processInfo = new ProcessStartInfo("cmd.exe", "/c " + filePath)
			{
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true
			};

			// *** Redirect the output ***
			var process = Process.Start(processInfo);
			if (process == null)
				throw new InvalidOperationException("Process is null");

			process.WaitForExit();

			// *** Read the streams ***
			var output = process.StandardOutput.ReadToEnd();
			var error = process.StandardError.ReadToEnd();
			var exitCode = process.ExitCode;

			Console.WriteLine("output>>" + (String.IsNullOrEmpty(output) ? "(none)" : output));
			Console.WriteLine("error>>" + (String.IsNullOrEmpty(error) ? "(none)" : error));
			Console.WriteLine("ExitCode: " + exitCode, "ExecuteCommand");
			process.Close();

			// return the result
			return exitCode;
		}

	}
}