using System.Net.Mail;

namespace LatestFileReporter
{
	public interface IMailer
	{
		void Send(MailMessage message);
	}
}