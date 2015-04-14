using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using LatestFileReporter.Interfaces;

namespace LatestFileReporter
{
	public class ProgramDefinition : IProgramDefinition
	{
		private IProgramSettings Settings { get; set; }

		public ProgramDefinition(IProgramSettings settings)
		{
			Settings = settings;
		}

		public IEnumerable<IFileInfo> GetFilesToProcess()
		{
			var ignoreList = Settings.IgnoreFileList;
			var directory = new DirectoryInfo(Settings.SourceFileDirectoryPath);
			return from fileInfo in directory.GetFiles("*" + Settings.SearchFileExtension)
				where !ignoreList.Contains(fileInfo.Name, StringComparer.InvariantCultureIgnoreCase)
				select (IFileInfo) new FileWrapper(fileInfo);
		}

		public bool DoesLogFileIndicateCommonError(string fileName)
		{
			var logFile = BuildPath(Settings.LogFileDirectoryPath, fileName, Settings.LogFileExtension);
			var tail = ReadEndTokens(logFile, 10, Encoding.UTF8, "\n");
			return tail.Contains("Unable to create binary file");
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

			// the copy was successful
			return true;
		}

		public bool RunBatchFile(string fileName)
		{
			var path = BuildPath(Settings.BatchFileDirectoryPath, fileName, ".bat");
			if (!File.Exists(path))
				throw new FileNotFoundException("Unable to find batch file", path);

			return ExecuteCommand(path) == 0;
		}

		public void SendMessage(IFileInfo[] files)
		{
			using (var message = CreateMailMessage(files))
			using (var client = CreateSmtpClient())
				client.Send(message);
		}


		private MailMessage CreateMailMessage(IFileInfo[] outdatedFiles)
		{
			var message = new MailMessage(string.Join(";", Settings.ToEmailAddresses), Settings.FromEmailAddress);
			if (outdatedFiles.Any())
			{
				var friendlyMessage = string.Format("There are {0} files that were not copied: ", outdatedFiles.Count());
				var body = new StringBuilder();
				Console.WriteLine(friendlyMessage);
				body.Append(friendlyMessage);
				foreach (var file in outdatedFiles)
				{
					Console.WriteLine("\t" + file);
					body.Append("\t" + file.Name);
				}
				message.Subject = "Wake Up!";
				message.Body = body.ToString();
			}
			else
			{
				Console.WriteLine("All files are up to date!");
				message.Subject = "Keep sleeping... All is Good.";
				message.Body = "All files are up to date!";
			}

			return message;
		}

		private static SmtpClient CreateSmtpClient()
		{
			var smtpUserName = Environment.GetEnvironmentVariable("SmtpUserName", EnvironmentVariableTarget.User);
			if (string.IsNullOrEmpty(smtpUserName))
				throw new ApplicationException("Expecting a User environment variable named 'SmtpUserName'");

			var smtpPassword = Environment.GetEnvironmentVariable("SmtpPassword", EnvironmentVariableTarget.User);
			if (string.IsNullOrEmpty(smtpPassword))
				throw new ApplicationException("Expecting a User environment variable named 'SmtpPassword'");

			SmtpDeliveryMethod deliveryMethod;
			var appSettings = ConfigurationManager.AppSettings;
			if (!Enum.TryParse(appSettings["smtpDeliveryMethod"] ?? string.Empty, true, out deliveryMethod))
				deliveryMethod = SmtpDeliveryMethod.Network;

			var client = new SmtpClient
			{
				Port = Convert.ToInt32(appSettings["smtpPort"]),
				Host = appSettings["smtpHost"],
				EnableSsl = Convert.ToBoolean(appSettings["smtpEnableSsl"]),
				UseDefaultCredentials = Convert.ToBoolean(appSettings["smtpUseDefaultCreds"]),
				Credentials = new NetworkCredential(smtpUserName, smtpPassword),
				DeliveryMethod = deliveryMethod
			};
			return client;
		}

		private static string BuildPath(string directory, string fileName, string extension)
		{
			var temp = Path.Combine(directory, fileName);
			var name = Path.GetFileNameWithoutExtension(temp);
			return Path.Combine(directory, name + extension);
		}

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

		// NOTE: code taken from http://stackoverflow.com/a/5519517/84406
		// ******************************************************************
		private static int ExecuteCommand(string command)
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

			// return the result
			return exitCode;
		}

	}
}