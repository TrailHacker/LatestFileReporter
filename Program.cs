using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace LatestFileReporter
{
	sealed class Program
	{

		private bool _showHelp;
        private string _destinationsDirectoryPath;
        private string _sourceDirectoryPath;
		private string _searchPattern;
		private string _logFileExtension;
		private string _batchFileDirectoryPath;
		private string _fromEmailAddress;
		private string[] _toEmailAddresses;
		private SmtpClient _smtpClient;
        private bool _runBatch;
		private int _attempts;

		private Program()
		{
			var appSettings = ConfigurationManager.AppSettings;
			_showHelp = false;
			_runBatch = false; // <-- todo: set this to true and add logic
			_attempts = 3;

			_searchPattern = appSettings["searchPattern"];
			_logFileExtension = appSettings["logFileExtension"];
			_destinationsDirectoryPath = appSettings["destinationDirectory"];
			_sourceDirectoryPath = appSettings["sourceDirectory"];
			_batchFileDirectoryPath = appSettings["batchDirectory"];
			_fromEmailAddress = appSettings["fromEmailAddress"];
			_toEmailAddresses = appSettings["toEmailAddresses"].Split(';');
			_smtpClient = CreateSmtpClient(appSettings);
		}

		private void ParseArgs(string[] args)
		{
			if (args.Length == 0)
				return;

			_destinationsDirectoryPath = args[0];

			// do more complex processing here!

		}

		private int Run(string[] args)
		{

			_showHelp = false;
			var result = 0;

			try
			{
				ParseArgs(args);

				var expectedDate = DateTime.Now.Date;
				var directory = new DirectoryInfo(_sourceDirectoryPath);
				var message = new MailMessage(string.Join(";", _toEmailAddresses), _fromEmailAddress);
				for(var index = 0; index < _attempts; index++)
				{
					var files = (from file in directory.GetFileSystemInfos(_searchPattern)
						orderby file.LastWriteTime descending
						where file.LastWriteTime < expectedDate
						select file).ToArray();

					if (!files.Any())
					{
						Console.WriteLine("All files are up to date!");
						message.Subject = "Keep sleeping... All is Good.";
						message.Body = "All files are up to date!";
						break;
					}

					ReportFailure(files, message);

					if (!_runBatch)
						break;
				}

				_smtpClient.Send(message);

			}
			catch(Exception oops)
			{
				Console.WriteLine(oops.StackTrace);
				result = -1;
			}

			return result;

		}

		static int Main(string[] args)
		{
			var program = new Program();
			return program.Run(args);
		}

		private void ReportFailure(FileSystemInfo[] files, MailMessage message)
		{
			var friendlyMessage = string.Format("There are {0} files that were not copied: ", files.Count());
			var body = new StringBuilder();
			Console.WriteLine(friendlyMessage);
			body.Append(friendlyMessage);
			foreach (var file in files)
			{
				Console.WriteLine("\t" + file);
				body.Append("\t" + file.Name);
			}
			message.Subject = "Wake Up!";
			message.Body = body.ToString();
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

	}
}
