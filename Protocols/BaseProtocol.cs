using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EasyConnect.Protocols
{
	/// <summary>
	/// Base class that will be implemented by all connection protocol classes, which wrap the connection type, settings form for the protocol, and connection
	/// form.
	/// </summary>
	/// <typeparam name="TConnection">Type of the connection that will be established by this protocol.</typeparam>
	/// <typeparam name="TSettingsForm">Settings form that should be displayed to capture configuration data for a connection for this protocol.</typeparam>
	/// <typeparam name="TConnectionForm">UI class used to establish and display a connection for this protocol.</typeparam>
	public abstract class BaseProtocol<TConnection, TSettingsForm, TConnectionForm> : IProtocol
		where TConnection : IConnection
		where TSettingsForm : Form, ISettingsForm<TConnection>, new()
		where TConnectionForm : BaseConnectionForm, IConnectionForm<TConnection>, new()
	{
		/// <summary>
		/// Prefix used to identify this protocol in URIs; i.e. rdp or ssh in [protocol]://[hostname].
		/// </summary>
		public abstract string ProtocolPrefix
		{
			get;
		}

		/// <summary>
		/// Display text used to name this protocol.
		/// </summary>
		public abstract string ProtocolTitle
		{
			get;
		}

		/// <summary>
		/// Icon used to identify this protocol in the UI.
		/// </summary>
		public abstract Icon ProtocolIcon
		{
			get;
		}

		/// <summary>
		/// Type of connections established for this protocol.
		/// </summary>
		public Type ConnectionType
		{
			get
			{
				return typeof (TConnection);
			}
		}

		/// <summary>
		/// Gets an settings form used to capture configuration data for a connection for this protocol.
		/// </summary>
		/// <returns>Form used to capture configuration data for a connection for this protocol.</returns>
		public virtual Form GetSettingsForm()
		{
			return new TSettingsForm();
		}

		/// <summary>
		/// Gets an settings form used to capture configuration data for <paramref name="connection"/>.
		/// </summary>
		/// <returns>Form used to capture configuration data for <paramref name="connection"/>.</returns>
		public Form GetSettingsForm(IConnection connection)
		{
			return GetSettingsForm((TConnection) connection);
		}

		/// <summary>
		/// Gets an settings form used to capture defaults to be used in connections for this protocol.  Typically returns a UI identical to 
		/// <see cref="GetSettingsForm()"/> with the exception of the hostname to use for the connection.
		/// </summary>
		/// <returns>Settings form used to capture defaults to be used in connections for this protocol</returns>
		public async Task<Form> GetSettingsFormInDefaultsMode()
		{
			TConnection defaults = (TConnection) await ConnectionFactory.GetDefaults(GetType());

			return GetSettingsFormInDefaultsMode(defaults);
		}

		/// <summary>
		/// Creates the form that will house the UI for establishing and displaying <paramref name="connection"/>.
		/// </summary>
		/// <param name="connection">Connection that we need to create the UI for.</param>
		/// <param name="containerPanel">Panel that this UI will be contained within.</param>
		/// <returns>Form that will house the UI for establishing and displaying <paramref name="connection"/>.</returns>
		public BaseConnectionForm CreateConnectionForm(IConnection connection, Panel containerPanel)
		{
			TConnectionForm connectionForm = new TConnectionForm
				                                 {
					                                 Location = new Point(0, 0),
					                                 Dock = DockStyle.Fill,
					                                 FormBorderStyle = FormBorderStyle.None,
					                                 TopLevel = false,
					                                 Connection = (TConnection) connection,
													 Width = containerPanel.Width,
													 Height = containerPanel.Height
				                                 };

			containerPanel.Controls.Add(connectionForm);
			connectionForm.Show();

			return connectionForm;
		}

		/// <summary>
		/// Gets an settings form used to capture configuration data for <paramref name="connection"/>.
		/// </summary>
		/// <returns>Form used to capture configuration data for <paramref name="connection"/>.</returns>
		public virtual Form GetSettingsForm(TConnection connection)
		{
			return new TSettingsForm
				       {
					       Connection = connection
				       };
		}

		/// <summary>
		/// Gets an settings form used to capture defaults to be used in connections for this protocol.  Typically returns a UI identical to 
		/// <see cref="GetSettingsForm()"/> with the exception of the hostname to use for the connection.
		/// </summary>
		/// <param name="connection">Defaults data that has already been captured or initialized for this protocol.</param>
		/// <returns>Settings form used to capture defaults to be used in connections for this protocol</returns>
		public virtual Form GetSettingsFormInDefaultsMode(TConnection connection)
		{
			return new TSettingsForm
				       {
					       Connection = connection,
					       DefaultsMode = true
				       };
		}
	}
}