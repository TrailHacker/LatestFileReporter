using System.Net.Mail;

namespace LatestFileReporter.Interfaces
{
	public interface IMessageFactory
	{
		string FromEmailAddress { get; set; }
		string[] ToEmailAddresses { get; set; }
		MailMessage Create(IFileInfo[] outdatedFiles);
	}
}