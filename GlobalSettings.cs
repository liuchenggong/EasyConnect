using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using EasyConnect.Protocols;
using System.Threading.Tasks;
#if APPX
using Windows.Storage;
#endif

namespace EasyConnect
{
	/// <summary>
	/// Global settings for the application, currently just includes the flag indicating whether the toolbar should be automatically hidden in 
	/// <see cref="ConnectionWindow"/> instances.
	/// </summary>
	[Serializable]
	[XmlRoot(ElementName = "Options")]
	public class GlobalSettings
	{
        private static readonly string SettingsFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EasyConnect", "Options.xml");

        private GlobalSettings()
        {
        }

		/// <summary>
		/// Flag indicating whether the toolbar in <see cref="ConnectionWindow"/> instances should be hidden when the user is focused on the connection content
		/// area.
		/// </summary>
		public bool AutoHideToolbar
		{
			get;
			set;
		}

		/// <summary>
		/// Type of encryption that should be used to protect passwords and other sensitive data in settings files.
		/// </summary>
		public EncryptionType? EncryptionType
		{
			get;
			set;
		}

		public bool EnableAeroPeek
		{
			get;
			set;
		}

        public static GlobalSettings Instance
        {
            get;
            private set;
        }

        [XmlIgnore]
        public bool FirstLaunch
        {
            get;
            private set;
        }

		/// <summary>
		/// Deserializes an instance of this class from an XML file on disk.
		/// </summary>
		/// <returns></returns>
		public static async Task Init()
		{
		    Instance = new GlobalSettings();

#if APPX
            IStorageFile optionsFile = (IStorageFile) await ApplicationData.Current.LocalFolder.TryGetItemAsync("Options.xml");
		    string optionsFileText = null;

		    // If there isn't a file in the Windows Store app data directory, try the desktop app directory
		    if (optionsFile == null)
		    {
		        try
		        {
		            if (File.Exists(SettingsFileName))
		            {
		                optionsFileText = File.ReadAllText(SettingsFileName);
		            }
		        }

#pragma warning disable RECS0022
                catch (Exception)
#pragma warning restore RECS0022
                {
		        }
		    }

		    else
		    {
		        optionsFileText = await FileIO.ReadTextAsync(optionsFile);
		    }

		    if (String.IsNullOrEmpty(optionsFileText))
		    {
		        Instance.FirstLaunch = true;
		        return;
		    }

            using (StringReader optionsFileTextReader = new StringReader(optionsFileText))
            using (XmlReader optionsXmlReader = new XmlTextReader(optionsFileTextReader))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(GlobalSettings));
                Instance = (GlobalSettings)serializer.Deserialize(optionsXmlReader);
            }
#else
            // If the settings file doesn't exist yet (first time the application is being run), just create a new instance of the class
			if (!File.Exists(SettingsFileName))
            {
				Instance = new GlobalSettings();
                Instance.FirstLaunch = true;

                return;
            }

            using (FileStream fileStream = new FileStream(SettingsFileName, FileMode.Open, FileAccess.Read))
			using (XmlReader reader = new XmlTextReader(fileStream))
			{
				XmlSerializer serializer = new XmlSerializer(typeof (GlobalSettings));
				Instance = (GlobalSettings) serializer.Deserialize(reader);
			}
#endif
        }

        /// <summary>
        /// Serializes the option data to disk in an XML file.
        /// </summary>
        public async Task Save()
		{
			XmlSerializer serializer = new XmlSerializer(GetType());

#if APPX
            IStorageFile optionsFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("Options.xml", CreationCollisionOption.ReplaceExisting);

            StringWriter optionsFileText = new StringWriter();
            serializer.Serialize(optionsFileText, this);

            await FileIO.WriteTextAsync(optionsFile, optionsFileText.ToString());
#else
            using (FileStream fileStream = new FileStream(SettingsFileName, FileMode.Create, FileAccess.Write))
				serializer.Serialize(fileStream, this);
#endif
        }
	}
}