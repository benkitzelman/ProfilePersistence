using System;
using System.Configuration;
using System.Configuration.Provider;
using System.Collections.Specialized;
using System.Security.Permissions;
using System.Web;
using System.Web.Profile;
using System.Web.Hosting;
using System.Globalization;
using System.IO;
using System.Text;

[SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.SerializationFormatter)]
public class TextFileProfileProvider : ProfileProvider
{
	#region Fields

	private const string RelativeProfileFilePath = "~/App_Data/Profile_Data";

	#endregion

	#region Properties

	private string AbsoluteProfileFilePath
	{
		get { return HttpContext.Current.Server.MapPath(RelativeProfileFilePath); }
	}
	private string ProfilePathFormatString
	{
		get { return AbsoluteProfileFilePath + "/{0}_Profile.txt"; }
	}
	private FileInfo[] ProfileFiles
	{
		get
		{
			DirectoryInfo di = new DirectoryInfo(AbsoluteProfileFilePath);
			return di.GetFiles("*_Profile.txt");
		}
	}
	public override string ApplicationName
	{
		get { throw new NotSupportedException(); }
		set { throw new NotSupportedException(); }
	}

	#endregion

	#region Overriden Methods

	public override void Initialize(string name, NameValueCollection config)
	{
		// Verify that config isn't null
		if (config == null)	throw new ArgumentNullException("config");

		// Assign the provider a default name if it doesn't have one
		if (String.IsNullOrEmpty(name))	name = "TextFileProfileProvider";

		// Add a default "description" attribute to config if the
		// attribute doesn't exist or is empty
		if (string.IsNullOrEmpty(config["description"]))
		{
			config.Remove("description");
			config.Add("description", "Text file profile provider");
		}

		// Call the base class's Initialize method
		base.Initialize(name, config);

		// Throw an exception if unrecognized attributes remain
		if (config.Count > 0)
		{
			string attr = config.GetKey(0);
			if (!String.IsNullOrEmpty(attr))	throw new ProviderException("Unrecognized attribute: " + attr);
		}

		// Make sure we can read and write files
		// in the Profile_Data directory
		FileIOPermission permission = new FileIOPermission(FileIOPermissionAccess.AllAccess, AbsoluteProfileFilePath);
		permission.Demand();
	}

	public override SettingsPropertyValueCollection	GetPropertyValues(SettingsContext context, SettingsPropertyCollection properties)
	{
		SettingsPropertyValueCollection settings = new SettingsPropertyValueCollection();

		// Do nothing if there are no properties to retrieve
		if (properties.Count == 0)
			return settings;

		// For properties lacking an explicit SerializeAs setting, set
		// SerializeAs to String for strings and primitives, and XML
		// for everything else
		foreach (SettingsProperty property in properties)
		{
			if (property.SerializeAs == SettingsSerializeAs.ProviderSpecific)
			{
				if (property.PropertyType.IsPrimitive || property.PropertyType == typeof(String))
				{
					property.SerializeAs = SettingsSerializeAs.String;
				}
				else
				{
					property.SerializeAs = SettingsSerializeAs.Xml;
				}
			}
			settings.Add(new SettingsPropertyValue(property));
		}

		// Get the user name or anonymous user ID
		string username = (string) context["UserName"];

		// NOTE: Consider validating the user name here to prevent
		// malicious user names such as "../Foo" from targeting
		// directories other than Profile_Data

		// Load the profile
		if (!String.IsNullOrEmpty(username))
		{
			StreamReader reader = null;
			string[] names;
			string values;
			byte[] buf = null;

			try
			{
				// Open the file containing the profile data
				try
				{
					string path =	string.Format(ProfilePathFormatString,	username.Replace('\\', '_'));
					reader		  = new StreamReader(path);
				}
				catch (IOException)
				{
					// Not an error if file doesn't exist
					return settings;
				}

				// Read names, values, and buf from the file
				names = reader.ReadLine().Split(':');

				values = reader.ReadLine();
				if (!string.IsNullOrEmpty(values))
				{
					UnicodeEncoding encoding = new UnicodeEncoding();
					values = encoding.GetString
							(Convert.FromBase64String(values));
				}

				string temp = reader.ReadLine();
				if (!String.IsNullOrEmpty(temp))
				{
					buf = Convert.FromBase64String(temp);
				}
				else
					buf = new byte[0];
			}
			finally
			{
				if (reader != null)
					reader.Close();
			}

			// Decode names, values, and buf and initialize the
			// SettingsPropertyValueCollection returned to the caller
			DecodeProfileData(names, values, buf, settings);
		}

		return settings;
	}

	public override void SetPropertyValues(SettingsContext context,	SettingsPropertyValueCollection properties)
	{
		// Get information about the user who owns the profile
		string username    = (string) context["UserName"];
		bool authenticated = (bool) context["IsAuthenticated"];

		// NOTE: Consider validating the user name here to prevent
		// malicious user names such as "../Foo" from targeting
		// directories other thanProfile_Data

		// Do nothing if there is no user name or no properties
		if (String.IsNullOrEmpty(username) || properties.Count == 0) return;

		// Format the profile data for saving
		string names  = String.Empty;
		string values = String.Empty;
		byte[] buf    = null;

		EncodeProfileData(ref names, ref values, ref buf,	properties, authenticated);

		// Do nothing if no properties need saving
		if (names == String.Empty) return;

		// Save the profile data    
		StreamWriter writer = null;

		try
		{
			string path = string.Format(ProfilePathFormatString, username.Replace('\\', '_'));
			writer			= new StreamWriter(path, false);

			writer.WriteLine(names);

			if (!String.IsNullOrEmpty(values))
			{
				UnicodeEncoding encoding = new UnicodeEncoding();
				writer.WriteLine(Convert.ToBase64String(encoding.GetBytes(values)));
			}
			else
			{
				writer.WriteLine();
			}

			if (buf != null && buf.Length > 0) writer.WriteLine(Convert.ToBase64String(buf));
			else writer.WriteLine();
		}
		finally
		{
			if (writer != null)	writer.Close();
		}
	}

	public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
	{
		int count = 0;
		foreach (FileInfo fi in ProfileFiles)
		{
			if (fi.Exists == true && fi.LastAccessTime < userInactiveSinceDate)
			{
				fi.Delete();
				count++;
			}
		}
		return count;
	}

	public override int DeleteProfiles(string[] usernames)
	{
		int count = 0;
		foreach (string username in usernames)
		{
			FileInfo fi = this.GetProfileFile(username);
			if (fi != null)
			{
				fi.Delete();
				count++;
			}
		}
		return count;
	}

	public override int DeleteProfiles(ProfileInfoCollection profiles)
	{
		int count = 0;
		foreach (ProfileInfo pi in profiles)
		{
			FileInfo fi = this.GetProfileFile(pi.UserName);
			if (fi != null)
			{
				fi.Delete();
				count++;
			}
		}
		return count;
	}

	public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
	{
		int count = 0;
		foreach (FileInfo fi in this.ProfileFiles)
		{
			if (fi.Exists == true && fi.LastAccessTime < userInactiveSinceDate) count++;
		}
		return count;
	}

	public override ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
	{
		ProfileInfoCollection profiles = new ProfileInfoCollection();
		FileInfo fi = this.GetProfileFile(usernameToMatch);
		if (fi != null && fi.Exists == true && fi.LastAccessTime < userInactiveSinceDate)
		{
			ProfileInfo profile = new ProfileInfo(usernameToMatch, false, fi.LastAccessTime, fi.LastWriteTime, GetProfileSize(fi));
			profiles.Add(profile);
		}
		totalRecords = profiles.Count;
		return profiles;
	}

	public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
	{		
		ProfileInfoCollection profiles = new ProfileInfoCollection();
		FileInfo fi = this.GetProfileFile(usernameToMatch);
		if (fi != null && fi.Exists == true)
		{
			ProfileInfo profile = new ProfileInfo(usernameToMatch, false, fi.LastAccessTime, fi.LastWriteTime, GetProfileSize(fi));
			profiles.Add(profile);
		}
		totalRecords = profiles.Count;
		return profiles;
	}

	public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
	{
		ProfileInfoCollection profiles = new ProfileInfoCollection();
		foreach (FileInfo fi in this.ProfileFiles)
		{
			if (fi != null && fi.Exists == true && fi.LastAccessTime < userInactiveSinceDate)
			{
				ProfileInfo profile = new ProfileInfo(this.GetUsernameFromFile(fi.Name), false, fi.LastAccessTime, fi.LastWriteTime, GetProfileSize(fi));
				profiles.Add(profile);
			}
		}
		totalRecords = profiles.Count;
		return profiles;
	}

	public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
	{
		ProfileInfoCollection profiles = new ProfileInfoCollection();
		foreach (FileInfo fi in this.ProfileFiles)
		{
			if (fi != null && fi.Exists == true)
			{
				ProfileInfo profile = new ProfileInfo(this.GetUsernameFromFile(fi.Name), false, fi.LastAccessTime, fi.LastWriteTime, GetProfileSize(fi));
				profiles.Add(profile);
			}
		}
		totalRecords = profiles.Count;
		return profiles;
	}

	#endregion

	#region Private Helpers

	private string GetUsernameFromFile(string p_fileName)
	{
		return System.Text.RegularExpressions.Regex.Replace(p_fileName, "_Profile.txt", string.Empty);
	}

	private void DecodeProfileData(string[] names, string values, byte[] buf, SettingsPropertyValueCollection properties)
	{
		if (names == null || values == null || buf == null || properties == null) return;

		for (int i = 0; i < names.Length; i += 4)
		{
			// Read the next property name from "names" and retrieve
			// the corresponding SettingsPropertyValue from
			// "properties"
			string name = names[i];
			SettingsPropertyValue pp = properties[name];

			if (pp == null) continue;

			// Get the length and index of the persisted property value
			int pos = Int32.Parse(names[i + 2], CultureInfo.InvariantCulture);
			int len = Int32.Parse(names[i + 3], CultureInfo.InvariantCulture);

			// If the length is -1 and the property is a reference
			// type, then the property value is null
			if (len == -1 && !pp.Property.PropertyType.IsValueType)
			{
				pp.PropertyValue = null;
				pp.IsDirty = false;
				pp.Deserialized = true;
			}

		// If the property value was peristed as a string,
			// restore it from "values"
			else if (names[i + 1] == "S" && pos >= 0 && len > 0 && values.Length >= pos + len)
				pp.SerializedValue = values.Substring(pos, len);

		// If the property value was peristed as a byte array,
			// restore it from "buf"
			else if (names[i + 1] == "B" && pos >= 0 && len > 0 && buf.Length >= pos + len)
			{
				byte[] buf2 = new byte[len];
				Buffer.BlockCopy(buf, pos, buf2, 0, len);
				pp.SerializedValue = buf2;
			}
		}
	}

	private void EncodeProfileData(ref string allNames, ref string allValues, ref byte[] buf, SettingsPropertyValueCollection properties, bool userIsAuthenticated)
	{
		StringBuilder names = new StringBuilder();
		StringBuilder values = new StringBuilder();
		MemoryStream stream = new MemoryStream();

		try
		{
			foreach (SettingsPropertyValue pp in properties)
			{
				// Ignore this property if the user is anonymous and
				// the property's AllowAnonymous property is false
				if (!userIsAuthenticated && !(bool) pp.Property.Attributes["AllowAnonymous"]) continue;

				// Ignore this property if it's not dirty and is
				// currently assigned its default value
				if (!pp.IsDirty && pp.UsingDefaultValue) continue;

				int len = 0, pos = 0;
				string propValue = null;

				// If Deserialized is true and PropertyValue is null,
				// then the property's current value is null (which
				// we'll represent by setting len to -1)
				if (pp.Deserialized && pp.PropertyValue == null)
				{
					len = -1;
				}

				// Otherwise get the property value from
				// SerializedValue
				else
				{
					object sVal = pp.SerializedValue;

					// If SerializedValue is null, then the property's
					// current value is null
					if (sVal == null)
					{
						len = -1;
					}

					// If sVal is a string, then encode it as a string
					else if (sVal is string)
					{
						propValue = (string) sVal;
						len = propValue.Length;
						pos = values.Length;
					}

				// If sVal is binary, then encode it as a byte
					// array
					else
					{
						byte[] b2 = (byte[]) sVal;
						pos = (int) stream.Position;
						stream.Write(b2, 0, b2.Length);
						stream.Position = pos + b2.Length;
						len = b2.Length;
					}
				}

				// Add a string conforming to the following format
				// to "names:"
				//                
				// "name:B|S:pos:len"
				//    ^   ^   ^   ^
				//    |   |   |   |
				//    |   |   |   +--- Length of data
				//    |   |   +------- Offset of data
				//    |   +----------- Location (B="buf", S="values")
				//    +--------------- Property name

				names.Append(pp.Name + ":" + ((propValue != null) ? "S" : "B") + ":" + pos.ToString(CultureInfo.InvariantCulture) + ":" + len.ToString(CultureInfo.InvariantCulture) + ":");

				// If the propery value is encoded as a string, add the
				// string to "values"
				if (propValue != null) values.Append(propValue);
			}

			// Copy the binary property values written to the
			// stream to "buf"
			buf = stream.ToArray();
		}
		finally
		{
			if (stream != null) stream.Close();
		}

		allNames = names.ToString();
		allValues = values.ToString();
	}

	private FileInfo GetProfileFile(string p_username)
	{
		System.IO.FileInfo fi = new FileInfo(string.Format(ProfilePathFormatString, p_username.Replace('\\', '_')));
		if (fi.Exists == false) return null;
		return fi;

	}

	private int GetProfileSize(FileInfo fi)
	{
		return (int) fi.Length;
	}

	#endregion
}
