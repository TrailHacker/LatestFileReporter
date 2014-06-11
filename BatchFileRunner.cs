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
			var appSettings = ConfigurationManager.AppSettings;
			BatchFileDirectoryPath = appSettings["batchDirectory"];
			AttemptedRunCounter = 3;
		}

		public string BatchFileDirectoryPath { get; set; }
		public int AttemptedRunCounter { get; set; }
		public static void ExecuteCommand(string command)
		{
			var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
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
		}

	}
}