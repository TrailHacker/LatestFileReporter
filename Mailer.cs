using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using LatestFileReporter.Interfaces;

namespace LatestFileReporter
{
	public class Mailer : IMailer
	{
		private readonly SmtpClient _smtpClient;

		public Mailer()
		{
			var appSettings = ConfigurationManager.AppSettings;
			_smtpClient = CreateSmtpClient(appSettings);
		}

		private SmtpClient CreateSmtpClient(NameValueCollection appSettings)
		{
			var smtpUserName = Environment.GetEnvironmentVariable("SmtpUserName", EnvironmentVariableTarget.User);
			if (string.IsNullOrEmpty(smtpUserName))
				throw new ApplicationException("Expecting a User environment variable named 'SmtpUserName'");

			var smtpPassword = Environment.GetEnvironmentVariable("SmtpPassword", EnvironmentVariableTarget.User);
			if (string.IsNullOrEmpty(smtpPassword))
				throw new ApplicationException("Expecting a User environment variable named 'SmtpPassword'");

			SmtpDeliveryMethod deliveryMethod;
			if (!Enum.TryParse(appSettings["smtpDeliveryMethod"] ?? string.Empty, true, out deliveryMethod))
				deliveryMethod = SmtpDeliveryMethod.Network;

			return new SmtpClient
			{
				Port = Convert.ToInt32(appSettings["smtpPort"]),
				Host = appSettings["smtpHost"],
				EnableSsl = Convert.ToBoolean(appSettings["smtpEnableSsl"]),
				UseDefaultCredentials = Convert.ToBoolean(appSettings["smtpUseDefaultCreds"]),
				Credentials = new NetworkCredential(smtpUserName, smtpPassword),
				DeliveryMethod = deliveryMethod
			};
		}

		public void Send(MailMessage message)
		{
			_smtpClient.Send(message);
		}
	}
}