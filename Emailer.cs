using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using LatestFileReporter.Interfaces;

namespace LatestFileReporter
{
	public class Emailer : IEmailer
	{
		private readonly IProgramSettings _settings;
		private readonly IFileInfo[] _files;

		public Emailer(IProgramSettings settings, IFileInfo[] files)
		{
			if (settings == null)
				throw new ArgumentNullException("settings");
			if (files == null)
				throw new ArgumentNullException("files");
			
			_settings = settings;
			_files = files;

		}

		public IProgramSettings Settings
		{
			get { return _settings; }
		}

		public void SendMessage()
		{
			using (var message = CreateMailMessage(_files))
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

	}
}