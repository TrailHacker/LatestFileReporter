using System;
using System.IO;
using System.Linq;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace LatestFileReporter
{
	static class Program
	{
		static void Main(string[] args)
		{
			var path = args.Any() ? args[0] : ConfigurationManager.AppSettings["path"];
			var files = GetUnprocessedCubeFiles(path);
			var message = PrepareEmailMessage();
			if (!files.Any())
				ReportSuccess(message);
			else
				ReportFailure(files, message);

			try
			{
				var smtp = CreateSmtpClient();
				smtp.Send(message);
			}
			catch (Exception error)
			{
				Console.WriteLine(error.Message);
			}
		}

		private static void ReportFailure(FileSystemInfo[] files, MailMessage message)
		{
			Console.WriteLine("There were " + files.Count() + " files modified today: ");
			foreach (var file in files)
				Console.WriteLine("\t" + file);

			var body = new StringBuilder();
			body.Append(string.Format("There are {0} files that are out of date!", files.Count()));
			foreach (var file in files)
				body.Append("\t" + file.Name);
			message.Subject = "Wake Up!";
			message.Body = body.ToString();
		}

		private static void ReportSuccess(MailMessage message)
		{
			Console.WriteLine("No files modified");
			message.Subject = "All is Good!";
			message.Body = "Keep sleeping...";
		}

		private static MailMessage PrepareEmailMessage()
		{
			var emails = ConfigurationManager.AppSettings["emails"];
			var from = ConfigurationManager.AppSettings["from"];
			var message = new MailMessage(@from, emails);
			return message;
		}

		private static FileSystemInfo[] GetUnprocessedCubeFiles(string path)
		{
			var pattern = ConfigurationManager.AppSettings["pattern"];
			var expectedDate = DateTime.Now.Date;
			var directory = new DirectoryInfo(path);
			var files = (from file in directory.GetFileSystemInfos(pattern)
				orderby file.LastWriteTime descending
				where file.LastWriteTime < expectedDate
				select file).ToArray();
			return files;
		}

		private static SmtpClient CreateSmtpClient()
		{
			var smtpUserName = Environment.GetEnvironmentVariable("SmtpUserName", EnvironmentVariableTarget.User);
			if (string.IsNullOrEmpty(smtpUserName))
				throw new ApplicationException("Expecting a User environment variable named 'SmtpUserName'");

			var smtpPassword = Environment.GetEnvironmentVariable("SmtpPassword", EnvironmentVariableTarget.User);
			if (string.IsNullOrEmpty(smtpPassword))
				throw new ApplicationException("Expecting a User environment variable named 'SmtpPassword'");

			var smtp = new SmtpClient
			{
				Port = 587,
				Host = "smtp.gmail.com",
				EnableSsl = true,
				UseDefaultCredentials = false,
				Credentials = new NetworkCredential(smtpUserName, smtpPassword),
				DeliveryMethod = SmtpDeliveryMethod.Network
			};
			return smtp;
		}
	}
}
