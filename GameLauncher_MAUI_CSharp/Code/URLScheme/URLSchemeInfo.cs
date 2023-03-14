namespace url_scheme_manager
{
	/// <summary>
	/// Stores information for adding/removing and managing a url-scheme in the device registry.
	/// </summary>
	public struct URLSchemeInfo
	{
		/// <summary>
		/// The friendly-name for identifying the url-protocol of the custom uri-scheme.
		/// </summary>
		public string FriendlyName;

		/// <summary>
		/// The prefix representing the custom uri-scheme. (e.g. notepad://file.txt , wmp://video.mp4 )
		/// </summary>
		public string ProtocolPrefix;

		/// <summary>
		/// The program to be executed or file to be opened when the custom uri-scheme is opened in a browser.
		/// </summary>
		public string ProgramPath;

		/// <summary>
		/// Creates a new instance of the <see cref="URLSchemeInfo"/> type.
		/// </summary>
		/// <param name="FriendlyName">The friendly-name for identifying the url-protocol of the custom uri-scheme.</param>
		/// <param name="ProgramPath">The program to be executed or file to be opened when the custom uri-scheme is opened in a browser.</param>
		public URLSchemeInfo(string FriendlyName, string ProtocolPrefix, string ProgramPath)
		{
			this.FriendlyName = FriendlyName;
			this.ProtocolPrefix = ProtocolPrefix;
			this.ProgramPath = ProgramPath;
		}

		/// <summary>
		/// Auto-generates an instance of the <see cref="URLSchemeInfo"/> type from the given parameters.
		/// </summary>
		/// <param name="Name">The friendly-name for identifying the url-protocol of the custom uri-scheme.</param>
		/// <param name="Prefix">The prefix representing the custom uri-scheme. (e.g. notepad://file.txt , wmp://video.mp4 )</param>
		/// <param name="Program">The program to be executed or file to be opened when the custom uri-scheme is opened in a browser.</param>
		/// <returns>An auto-generated <see cref="URLSchemeInfo"/> object.</returns>
		public static URLSchemeInfo CreateSchemeInfo(string Name, string Prefix, string Program)
		{
			return new URLSchemeInfo(Name, Prefix, Program);
		}
	}
}
