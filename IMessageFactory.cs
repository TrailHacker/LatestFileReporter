using System.IO;
using System.Net.Mail;

namespace LatestFileReporter
{
	public interface IMessageFactory
	{
		string FromEmailAddress { get; set; }
		string[] ToEmailAddresses { get; set; }
		MailMessage Create(IFileInfo[] outdatedFiles);
	}
}