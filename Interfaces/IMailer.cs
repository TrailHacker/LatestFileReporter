using System.Net.Mail;

namespace LatestFileReporter.Interfaces
{
	public interface IMailer
	{
		void Send(MailMessage message);
	}
}