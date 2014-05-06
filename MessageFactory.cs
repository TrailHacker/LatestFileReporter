using System;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace LatestFileReporter
{
	public class MessageFactory : IMessageFactory
	{
		public string FromEmailAddress { get; set; }
		public string[] ToEmailAddresses { get; set; }

		public MessageFactory()
		{
			var appSettings = ConfigurationManager.AppSettings;
			FromEmailAddress = appSettings["fromEmailAddress"];
			ToEmailAddresses = appSettings["toEmailAddresses"].Split(';');
		}

		public MailMessage Create(IFileInfo[] outdatedFiles)
		{
			var message = new MailMessage(string.Join(";", ToEmailAddresses), FromEmailAddress);
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
	}
}