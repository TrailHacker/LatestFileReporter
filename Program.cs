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
			var pattern = ConfigurationManager.AppSettings["pattern"];
			var expectedDate = DateTime.Now.Date;
			var directory = new DirectoryInfo(path);
			var files = (from file in directory.GetFileSystemInfos(pattern)
				orderby file.LastWriteTime descending
				where file.LastWriteTime < expectedDate
				select file).ToArray();

			var emails = ConfigurationManager.AppSettings["emails"];
			var from = ConfigurationManager.AppSettings["from"];
			var message = new MailMessage(from, emails);
			if (!files.Any())
			{
				Console.WriteLine("No files modified");
				message.Subject = "All is Good!";
				message.Body = "Keep sleeping...";
			}
			else
			{
				Console.WriteLine("There were " + files.Count() + " files modified today: ");
				foreach(var file in files)
					Console.WriteLine("\t" + file);

				var body = new StringBuilder();
				body.Append(string.Format("There are {0} files that are out of date!", files.Count()));
				foreach(var file in files)
					body.Append("\t" + file.Name);
				message.Subject = "Wake Up!";
				message.Body = body.ToString();
			}

			var smtpUserName = Environment.GetEnvironmentVariable("SmtpUserName", EnvironmentVariableTarget.User);
			var smtpPassword = Environment.GetEnvironmentVariable("SmtpPassword", EnvironmentVariableTarget.User);
			var smtp = new SmtpClient
			{
				Port = 587,
				Host = "smtp.gmail.com",
				EnableSsl = true,
				UseDefaultCredentials = false,
				Credentials = new NetworkCredential(smtpUserName, smtpPassword),
				DeliveryMethod = SmtpDeliveryMethod.Network
			};

			smtp.Send(message);
		}

	}
}
