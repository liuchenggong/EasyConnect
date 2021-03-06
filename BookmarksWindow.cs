using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using EasyConnect.Protocols;
using EasyTabs;
using System.Windows.Forms.VisualStyles;
using EasyConnect.Properties;
using System.Configuration;
using System.Threading.Tasks;
using EasyConnect.Common;
using TheArtOfDev.HtmlRenderer.WinForms;
using Svg;
using System.Globalization;

namespace EasyConnect
{
	/// <summary>
	/// Bookmarks window that displays a split pane with a tree view containing bookmarks folders on the left and a list of bookmarks and immediate child
	/// folders of the currently selected folder on the right.
	/// </summary>
	public partial class BookmarksWindow : Form
	{
		/// <summary>
		/// Main application instance that this window is associated with, which is used to call back into application functionality.
		/// </summary>
		protected MainForm _applicationForm = null;

		/// <summary>
		/// Dictionary containing the indexes in <see cref="_listViewImageList"/> of the icons for each connection protocol.
		/// </summary>
		protected Dictionary<Type, int> _connectionTypeIcons = new Dictionary<Type, int>();

		/// <summary>
		/// When right clicking on a tree node in <see cref="FoldersTreeView"/> or a list item in <see cref="_bookmarksListView"/>, this contains the item that
		/// was clicked on.
		/// </summary>
		protected object _contextMenuItem = null;

		/// <summary>
		/// If the user has copied bookmarks/folders by using Ctrl+C or the context menu, this contains the list of items selected at that time; it can contain
		/// bookmarks, folders, or both.
		/// </summary>
		protected List<object> _copiedItems = new List<object>();

		/// <summary>
		/// If the user has cut bookmarks/folders by using Ctrl+X or the context menu, this contains the list of items selected at that time; it can contain
		/// bookmarks, folders, or both.
		/// </summary>
		protected List<object> _cutItems = new List<object>();

		/// <summary>
		/// Flag that can be set during reorganization of the bookmarks tree view or list view that prevents the items from being automatically sorted and
		/// causing excessive redraws; used during batch insert/removal operations.
		/// </summary>
		protected bool _deferSort = false;

		/// <summary>
		/// Flag set indicating that the user is dragging items from the right-hand list view.
		/// </summary>
		protected bool _draggingFromListView = false;

		/// <summary>
		/// Flag set indicating that the user is dragging items from the left-hand tree view.
		/// </summary>
		protected bool _draggingFromTree = false;

		/// <summary>
		/// Lookup that provides the <see cref="BookmarksFolder"/> instance for each <see cref="TreeNode"/> in <see cref="_bookmarksFoldersTreeView"/>.
		/// </summary>
		protected Dictionary<TreeNode, BookmarksFolder> _folderTreeNodes = new Dictionary<TreeNode, BookmarksFolder>();

		/// <summary>
		/// Lookup that provides the <see cref="IConnection"/> instance for each <see cref="ListViewItem"/> in <see cref="_bookmarksListView"/> that's a
		/// bookmark as opposed to a folder.
		/// </summary>
		protected Dictionary<ListViewItem, IConnection> _listViewConnections = new Dictionary<ListViewItem, IConnection>();

		/// <summary>
		/// If the user is dragging to a location in <see cref="_bookmarksListView"/>, this is the target that they are dropping on.
		/// </summary>
		protected ListViewItem _listViewDropTarget = null;

		/// <summary>
		/// Lookup that provides the <see cref="BookmarksFolder"/> instance for each <see cref="ListViewItem"/> in <see cref="_bookmarksListView"/> that's a
		/// folder as opposed to a bookmark.
		/// </summary>
		protected Dictionary<ListViewItem, BookmarksFolder> _listViewFolders = new Dictionary<ListViewItem, BookmarksFolder>();

		/// <summary>
		/// Flag indicating whether <see cref="_contextMenuItem"/> should be set automatically in <see cref="_folderContextMenu_Opening"/> to the currently
		/// open folder in <see cref="FoldersTreeView"/>.
		/// </summary>
		protected bool _setContextMenuItem = true;

		/// <summary>
		/// Flag set to indicate that we should display the settings for a <see cref="IConnection"/> instance in <see cref="_bookmarksListView"/> when they
		/// finish editing the item's label; this is done when a new bookmark is created as we first ask the user to name the bookmark and, when that's
		/// complete, allow the user to edit the settings for the connection.
		/// </summary>
		protected bool _showSettingsAfterItemLabelEdit = false;

		/// <summary>
		/// If the user is dragging to a location in <see cref="_bookmarksFoldersTreeView"/>, this is the target that they are dropping on.
		/// </summary>
		protected TreeNode _treeViewDropTarget = null;

        protected ListViewItem _itemEditingNotes = null;

        protected bool _listViewNotesDoubleClickStarted = false;

        protected Timer _listViewNotesDoubleClickTimer = new Timer();

        protected TextBox _notesTextBox = null;

        protected HtmlPanel _urlPanel = null;

        protected bool _renamingBookmarkItem = false;

		/// <summary>
		/// Constructor; deserializes the bookmarks folder structure, adds the various folder nodes to <see cref="_bookmarksFoldersTreeView"/>, and gets the
		/// icons for each protocol.
		/// </summary>
		/// <param name="applicationForm">Main application instance.</param>
		public BookmarksWindow(MainForm applicationForm)
		{
			InitializeComponent();

            _treeViewImageList.Images.Add(LoadSvg(Encoding.UTF8.GetString(AppResources.Folder), 24, 24));
            _treeViewImageList.Images.Add(LoadSvg(Encoding.UTF8.GetString(AppResources.FolderOpen), 24, 24));
            _bookmarksFoldersTreeView.SelectedImageIndex = 1;

            _toolsMenu.Renderer = new EasyConnectToolStripRender();

            _listViewNotesDoubleClickTimer.Tick += _listViewNotesDoubleClickTimer_Tick;
            _listViewNotesDoubleClickTimer.Interval = SystemInformation.DoubleClickTime;

			_applicationForm = applicationForm;
			_bookmarksFoldersTreeView.Sorted = true;
			_bookmarksListView.ListViewItemSorter = new BookmarksListViewComparer();

			// Set the handler methods for changing the bookmarks or child folders; these are responsible for updating the tree view and list view UI when
			// items are added or removed from the bookmarks or child folders collections
			Bookmarks.Instance.RootFolder.Bookmarks.CollectionModified += Bookmarks_CollectionModified;
            Bookmarks.Instance.RootFolder.ChildFolders.CollectionModified += ChildFolders_CollectionModified;

			_folderTreeNodes[_bookmarksFoldersTreeView.Nodes[0]] = Bookmarks.Instance.RootFolder;

			// Call Bookmarks_CollectionModified and ChildFolders_CollectionModified recursively through the folder structure to "simulate" bookmarks and
			// folders being added to the collection so that the initial UI state for the tree view can be created
			InitializeTreeView(Bookmarks.Instance.RootFolder);

			_bookmarksFoldersTreeView.Nodes[0].Expand();

            _listViewImageList.Images.Add(ExpandImage(LoadSvg(Encoding.UTF8.GetString(AppResources.Folder), 24, 24), 32, 32));

			foreach (IProtocol protocol in ConnectionFactory.GetProtocols())
			{
				// Get the icon for each protocol type and add an entry for it to the "Add bookmark" menu item
				Icon icon = new Icon(protocol.ProtocolIcon, 24, 24);

				_listViewImageList.Images.Add(ExpandImage(icon.ToBitmap(), 32, 32));
				_connectionTypeIcons[protocol.ConnectionType] = _listViewImageList.Images.Count - 1;

				IProtocol currentProtocol = protocol;
				ToolStripMenuItem protocolMenuItem = new ToolStripMenuItem(
					protocol.ProtocolTitle, null, (sender, args) => _addBookmarkMenuItem_Click(currentProtocol));
				_addBookmarkMenuItem.DropDownItems.Add(protocolMenuItem);
			}

            _bookmarkContextMenu.Renderer = new EasyConnectToolStripRender();
            _folderContextMenu.Renderer = new EasyConnectToolStripRender();

            _iconPictureBox.Image = new Icon(Icon, 16, 16).ToBitmap();

            _urlPanel = new HtmlPanel
            {
                AutoScroll = false,
                Width = _urlPanelContainer.Width,
                Height = _urlPanelContainer.Height,
                Left = 0,
                Top = 0,
                Font = urlTextBox.Font,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
				BackColor = Color.FromArgb(241, 243, 244)
            };

            _urlPanelContainer.Controls.Add(_urlPanel);
            _urlPanel.Text = String.Format(CultureInfo.InvariantCulture,
					@"<span style=""background-color: #F1F3F4; font-family: {2}; font-size: {1}pt; height: {0}px; color: #707172"">easyconnect://<font color=""black"">bookmarks</font></span>",
                    _urlPanel.Height, urlTextBox.Font.SizeInPoints, urlTextBox.Font.FontFamily.GetName(0));

#if APPX
            _updatesMenuItem.Visible = false;
            _toolsMenuSeparator2.Visible = false;
#else
            _updatesMenuItem.Visible = ConfigurationManager.AppSettings["checkForUpdates"] != "false";
            _toolsMenuSeparator2.Visible = ConfigurationManager.AppSettings["checkForUpdates"] != "false";
#endif
        }

        protected Image ExpandImage(Image source, int width, int height)
        {
            Image finalImage = new Bitmap(width, height);

            using (Graphics graphics = Graphics.FromImage(finalImage))
            {
                graphics.FillRectangle(new SolidBrush(Color.Transparent), new Rectangle(0, 0, width, height));
                graphics.DrawImage(source, (width - source.Width) / 2, (height - source.Height) / 2);
            }

            return finalImage;
        }

        protected Image LoadSvg(string svgXml, int width, int height)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(svgXml);

            return SvgDocument.Open(xmlDocument).Draw(width, height);
        }

        private void _listViewNotesDoubleClickTimer_Tick(object sender, EventArgs e)
        {
            _listViewNotesDoubleClickTimer.Stop();
            _listViewNotesDoubleClickStarted = false;
        }

		/// <summary>
		/// Main application instance that this window is associated with, which is used to call back into application functionality.
		/// </summary>
		protected MainForm ParentTabs
		{
			get
			{
				return (MainForm) Parent;
			}
		}

		/// <summary>
		/// Lookup that provides the <see cref="BookmarksFolder"/> instance for each <see cref="TreeNode"/> in <see cref="_bookmarksFoldersTreeView"/>.
		/// </summary>
		public Dictionary<TreeNode, BookmarksFolder> TreeNodeFolders
		{
			get
			{
				return _folderTreeNodes;
			}
		}

		/// <summary>
		/// The tree view containing the folder structure for the bookmarks.
		/// </summary>
		public TreeView FoldersTreeView
		{
			get
			{
				return _bookmarksFoldersTreeView;
			}
		}

		/// <summary>
		/// Recursive method to initialize the UI for <see cref="_bookmarksFoldersTreeView"/> by adding each <see cref="BookmarksFolder"/> to it and populate
		/// <see cref="_folderTreeNodes"/>.
		/// </summary>
		/// <param name="currentFolder">Current folder being processed.</param>
		protected void InitializeTreeView(BookmarksFolder currentFolder)
		{
			// Simulate adding all of the bookmarks in the folder to the bookmarks collection
			if (currentFolder.Bookmarks != null && currentFolder.Bookmarks.Count > 0)
			{
				currentFolder.Bookmarks.ForEach(b => b.ParentFolder = currentFolder);
				Bookmarks_CollectionModified(
					currentFolder.Bookmarks, new ListModificationEventArgs(ListModification.RangeAdded, 0, currentFolder.Bookmarks.Count));
			}

			// Simulate adding each child folder to the folders collection
			if (currentFolder.ChildFolders != null && currentFolder.ChildFolders.Count > 0)
			{
				currentFolder.ChildFolders.ForEach(f => f.ParentFolder = currentFolder);
				ChildFolders_CollectionModified(
					currentFolder.ChildFolders, new ListModificationEventArgs(ListModification.RangeAdded, 0, currentFolder.ChildFolders.Count));

				// Call this recursively for each child folder
				foreach (BookmarksFolder childFolder in currentFolder.ChildFolders)
					InitializeTreeView(childFolder);
			}
		}

		/// <summary>
		/// Processor for the use of shortcut keys, which are currently Ctrl+C for copying bookmarks and folders, Ctrl+X for cutting them, Ctrl+V for
		/// pasting them into a destination folder, and Delete for deleting bookmarks and folders.
		/// </summary>
		/// <param name="msg">Message received by the window's callback method.</param>
		/// <param name="keyData">Keys that the user has pressed.</param>
		/// <returns>True if we processed the command, base.<see cref="ProcessCmdKey"/> otherwise.</returns>
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == (Keys.Control | Keys.C))
			{
				// If the user has the tree view focused currently, then copy the currently selected folder in the tree
				if (_bookmarksFoldersTreeView.Focused && _bookmarksFoldersTreeView.SelectedNode != null && !_renamingBookmarkItem)
				{
					_contextMenuItem = _folderTreeNodes[_bookmarksFoldersTreeView.SelectedNode];
					_copyFolderMenuItem_Click(null, null);
					return true;
				}

					// Otherwise, copy all of the selected items in the list view
				else if (_bookmarksListView.Focused && _bookmarksListView.SelectedItems.Count > 0)
				{
					copyToolStripMenuItem_Click(null, null);
					return true;
				}
			}

			else if (keyData == (Keys.Control | Keys.V))
			{
				// Paste whatever is in the list of cut or copied objects into the currently selected folder in the tree
				if (_bookmarksFoldersTreeView.SelectedNode != null && !_renamingBookmarkItem)
				{
					_contextMenuItem = _folderTreeNodes[_bookmarksFoldersTreeView.SelectedNode];
					_pasteFolderMenuItem_Click(null, null);
					return true;
				}
			}

			else if (keyData == (Keys.Control | Keys.X))
			{
				// If the user has the tree view focused currently, then cut the currently selected folder in the tree
				if (_bookmarksFoldersTreeView.Focused && _bookmarksFoldersTreeView.SelectedNode != null && !_renamingBookmarkItem)
				{
					_contextMenuItem = _folderTreeNodes[_bookmarksFoldersTreeView.SelectedNode];
					_cutFolderMenuItem_Click(null, null);
					return true;
				}

					// Otherwise, cut all of the selected items in the list view
				else if (_bookmarksListView.Focused && _bookmarksListView.SelectedItems.Count > 0)
				{
					_cutBookmarkMenuItem_Click(null, null);
					return true;
				}
			}

			if (keyData == Keys.Delete)
			{
				// If the user has the tree view focused currently, then delete the currently selected folder in the tree
				if (_bookmarksFoldersTreeView.Focused && _bookmarksFoldersTreeView.SelectedNode != null)
				{
					_contextMenuItem = _folderTreeNodes[_bookmarksFoldersTreeView.SelectedNode];
					_deleteFolderMenuItem_Click(null, null);
					return true;
				}

					// Otherwise, delete all of the selected items in the list view
				else if (_bookmarksListView.Focused && _bookmarksListView.SelectedItems.Count > 0)
				{
					deleteToolStripMenuItem1_Click(null, null);
					return true;
				}
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		/// <summary>
		/// When a child folder is added to/removed from <see cref="BookmarksFolder.ChildFolders"/>, we add that folder to the tree view, update 
		/// <see cref="IConnection.ParentFolder"/> for each connection in the folder, and add the folder to <see cref="_folderTreeNodes"/> if necessary.
		/// </summary>
		/// <param name="sender"><see cref="BookmarksFolder.ChildFolders"/> instance that's being modified.</param>
		/// <param name="e">Range specifier indicating which items were added to/removed from the collection.</param>
		private void ChildFolders_CollectionModified(object sender, ListModificationEventArgs e)
		{
			ListWithEvents<BookmarksFolder> childFolders = sender as ListWithEvents<BookmarksFolder>;
			bool sortListView = false;

			// If items are being added to the list of child folders
			if (e.Modification == ListModification.ItemModified || e.Modification == ListModification.ItemAdded || e.Modification == ListModification.RangeAdded)
			{
				for (int i = e.StartIndex; i < e.StartIndex + e.Count; i++)
				{
					// Get the tree nodes in the tree view for the parent (which will always exist) and the child (which will exist if the folder is not
					// newly-created and is instead being moved from another folder)
					BookmarksFolder currentFolder = childFolders[i];
					TreeNode parentTreeNode = _folderTreeNodes.Single(kvp => kvp.Value == currentFolder.ParentFolder).Key;
					TreeNode folderTreeNode = _folderTreeNodes.SingleOrDefault(kvp => kvp.Value == currentFolder).Key;

					// If we don't have a tree node for this child (it's newly created)
					if (folderTreeNode == null)
					{
						// Create a new TreeNode for this child and add it to the parent
						folderTreeNode = new TreeNode(currentFolder.Name);
						folderTreeNode.ForeColor = _bookmarksFoldersTreeView.ForeColor;
						parentTreeNode.Nodes.Add(folderTreeNode);

						// Add this tree node to the lookup
						_folderTreeNodes[folderTreeNode] = currentFolder;

						// Assign the child folders/bookmarks collection modification event handlers
						currentFolder.ChildFolders.CollectionModified += ChildFolders_CollectionModified;
						currentFolder.Bookmarks.CollectionModified += Bookmarks_CollectionModified;

						// Call this method recursively for the child folders under this child folder; this is necessary when we're first setting up the UI
						// from InitializeTreeView to ensure that all the child folders are added to the tree view and all the list modification event handlers
						// are assigned properly
						ChildFolders_CollectionModified(
							currentFolder.ChildFolders, new ListModificationEventArgs(ListModification.RangeAdded, 0, currentFolder.ChildFolders.Count));
					}

						// If it's just being moved from another location, simply update its parent
					else
					{
						folderTreeNode.Parent.Nodes.Remove(folderTreeNode);
						parentTreeNode.Nodes.Add(folderTreeNode);
					}

					// If this node in the tree view is currently focused, add the child folder to the list view and set the flag indicating that we should
					// sort the list view
					if (_bookmarksFoldersTreeView.SelectedNode == parentTreeNode)
					{
						ListViewItem newItem = new ListViewItem(currentFolder.Name, 0);
                        newItem.SubItems.Add("");
                        newItem.SubItems.Add(currentFolder.Notes);

						_bookmarksListView.Items.Add(newItem);
						_listViewFolders[newItem] = currentFolder;

						sortListView = true;
					}
				}
			}

				// Otherwise, items are being deleted from the list of child folders
			else
			{
				KeyValuePair<TreeNode, BookmarksFolder> containerFolder = _folderTreeNodes.SingleOrDefault(kvp => kvp.Value.ChildFolders == childFolders);

				if (containerFolder.Key != null)
				{
					// Responding to delete requests is a little confusing since the items have already been removed from the collection, so we look at each
					// child tree node in the parent tree view folder and see which ones correspond to folders that are no longer in the BookmarksFolder's
					// ChildFolders collection
					for (int i = 0; i < containerFolder.Key.Nodes.Count; i++)
					{
						TreeNode treeNode = containerFolder.Key.Nodes[i];

						// We can't find this folder corresponding to this child tree node in the parent's ChildFolders collection so we know it was deleted
						if (!containerFolder.Value.ChildFolders.Contains(_folderTreeNodes[treeNode]))
						{
							KeyValuePair<ListViewItem, BookmarksFolder> listViewItem =
								_listViewFolders.SingleOrDefault(kvp => kvp.Value == _folderTreeNodes[treeNode]);

							// If the folder being removed from the collection is in the list view, remove it from there and set the sort flag
							if (listViewItem.Key != null)
							{
								_bookmarksListView.Items.Remove(listViewItem.Key);
								_listViewFolders.Remove(listViewItem.Key);

								sortListView = true;
							}

							// Remove this node and all of its children from the tree view
							RemoveAllFolders(treeNode);
							containerFolder.Key.Nodes.Remove(treeNode);

							i--;
						}
					}
				}
			}

			if (IsHandleCreated && !_deferSort)
			{
				// Sort the tree view and, if necessary, the list view
				_bookmarksFoldersTreeView.BeginInvoke(new Action(SortTreeView));

				if (sortListView)
					_bookmarksListView.BeginInvoke(new Action(_bookmarksListView.Sort));
			}

            SetBookmarksListViewHeight();
		}

		/// <summary>
		/// When a bookmark is added to/removed from <see cref="BookmarksFolder.Bookmarks"/>, we update <see cref="_bookmarksListView"/> if the parent folder
		/// is currently selected.
		/// </summary>
		/// <param name="sender"><see cref="BookmarksFolder.Bookmarks"/> collection that's being modified.</param>
		/// <param name="e">Range specifier indicating which items were added to/removed from the collection..</param>
		private void Bookmarks_CollectionModified(object sender, ListModificationEventArgs e)
		{
			ListWithEvents<IConnection> bookmarks = sender as ListWithEvents<IConnection>;
			bool sortListView = false;

			// Bookmarks are being added to the collection
			if (e.Modification == ListModification.ItemModified || e.Modification == ListModification.ItemAdded || e.Modification == ListModification.RangeAdded)
			{
				for (int i = e.StartIndex; i < e.StartIndex + e.Count; i++)
				{
					IConnection currentBookmark = bookmarks[i];
					TreeNode parentTreeNode = _folderTreeNodes.Single(kvp => kvp.Value == currentBookmark.ParentFolder).Key;

					// If the parent folder is currently selected in the tree view, update the list view to add the items and set the flag to sort the list
					// view
					if (_bookmarksFoldersTreeView.SelectedNode == parentTreeNode)
					{
						ListViewItem newItem = new ListViewItem(currentBookmark.DisplayName, _connectionTypeIcons[currentBookmark.GetType()]);
						newItem.SubItems.Add(currentBookmark.Host);
                        newItem.SubItems.Add(currentBookmark.Notes);

						_listViewConnections[newItem] = currentBookmark;
						_bookmarksListView.Items.Add(newItem);

						sortListView = true;
					}
				}
			}

				// Otherwise, bookmarks are being removed from the collection
			else
			{
				KeyValuePair<TreeNode, BookmarksFolder> containerFolder = _folderTreeNodes.SingleOrDefault(kvp => kvp.Value.Bookmarks == bookmarks);

				if (containerFolder.Key != null && containerFolder.Key == _bookmarksFoldersTreeView.SelectedNode)
				{
					// Responding to delete requests is a little confusing since the items have already been removed from the collection, so we look at each
					// list view item in the list view and see which ones correspond to bookmarks that are no longer in the BookmarksFolder's Bookmarks 
					// collection
					for (int i = 0; i < _bookmarksListView.Items.Count; i++)
					{
						ListViewItem bookmark = _bookmarksListView.Items[i];

						if (bookmark.ImageIndex == 0)
							continue;

						// If the list view doesn't contain this bookmark, it's been deleted, so remove it from the list view and set the sort flag
						if (!containerFolder.Value.Bookmarks.Contains(_listViewConnections[bookmark]))
						{
							_listViewConnections.Remove(bookmark);
							_bookmarksListView.Items.Remove(bookmark);
							sortListView = true;

							i--;
						}
					}
				}
			}

			// Sort the list view if necessary
			if (IsHandleCreated && sortListView && !_deferSort)
				_bookmarksListView.BeginInvoke(new Action(_bookmarksListView.Sort));

            SetBookmarksListViewHeight();
		}

		/// <summary>
		/// Imports bookmarks previously saved via a call to <see cref="Bookmarks.Export"/> and overwrites any existing bookmarks data.
		/// </summary>
		/// <param name="path">Path of the file that we're loading from.</param>
		public async Task Import(string path)
		{
			//ISSUE: Display shows old and new Bookmark items
			//ISSUE: Dialog shows truncated suggested file name
			if (await Bookmarks.Instance.Import(path))
            { 
				// Set the handler methods for changing the bookmarks or child folders; these are responsible for updating the tree view and list view 
				// UI when items are added or removed from the bookmarks or child folders collections
				Bookmarks.Instance.RootFolder.Bookmarks.CollectionModified += Bookmarks_CollectionModified;
                Bookmarks.Instance.RootFolder.ChildFolders.CollectionModified += ChildFolders_CollectionModified;

				_folderTreeNodes[_bookmarksFoldersTreeView.Nodes[0]] = Bookmarks.Instance.RootFolder;

				// Call Bookmarks_CollectionModified and ChildFolders_CollectionModified recursively through the folder structure to "simulate" 
				// bookmarks and folders being added to the collection so that the initial UI state for the tree view can be created
				InitializeTreeView(Bookmarks.Instance.RootFolder);

				_bookmarksFoldersTreeView.Nodes[0].Expand();
			}
		}

		/// <summary>
		/// Handler method that is called after a node in <see cref="_bookmarksFoldersTreeView"/> is selected.  It initializes the display of 
		/// <see cref="_bookmarksListView"/> with the bookmarks and immediate child folders in the selected tree view node.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_bookmarksFoldersTreeView"/> in this case.</param>
		/// <param name="e">Selection arguments associated with the event.</param>
		private void _bookmarksTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			// Clear out the list view
			_bookmarksListView.Items.Clear();
			_listViewConnections.Clear();
			_listViewFolders.Clear();

			if (!_folderTreeNodes.ContainsKey(e.Node))
				return;

			BookmarksFolder folder = _folderTreeNodes[e.Node];

			// Add each child folder to the list view
			foreach (BookmarksFolder childFolder in folder.ChildFolders)
			{
				ListViewItem item = new ListViewItem(
					new string[]
						{
							childFolder.Name,
                            "",
                            childFolder.Notes
						}, 0);

				_listViewFolders[item] = childFolder;
				_bookmarksListView.Items.Add(item);
			}

			// Add each bookmark to the list view
			foreach (IConnection bookmark in folder.Bookmarks)
			{
				ListViewItem item = new ListViewItem(bookmark.DisplayName, _connectionTypeIcons[bookmark.GetType()]);
				item.SubItems.Add(bookmark.Host);
                item.SubItems.Add(bookmark.Notes);

				_listViewConnections[item] = bookmark;
				_bookmarksListView.Items.Add(item);
			}

            SetBookmarksListViewHeight();
		}

        protected void SetBookmarksListViewHeight()
        {
            //_bookmarksListView.Height = 27 + (_bookmarksListView.Items.Count * 33);
        }

		/// <summary>
		/// Handler method that is called when the user double-clicks on an item in <see cref="_bookmarksListView"/>.  Opens the folder in the list view if
		/// the clicked item was a folder, otherwise opens the bookmark connection.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_bookmarksListView"/> in this case.</param>
		/// <param name="e">Arguments associated with the click event.</param>
		private async void _bookmarksListView_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (_bookmarksListView.SelectedItems.Count > 0)
			{
				// If the double-clicked item was a bookmark, open its connection in a new tab
				if (_listViewConnections.ContainsKey(_bookmarksListView.SelectedItems[0]))
					await _applicationForm.Connect(_listViewConnections[_bookmarksListView.SelectedItems[0]], true);

					// Otherwise, open the folder
				else
				{
					BookmarksFolder folder = _listViewFolders[_bookmarksListView.SelectedItems[0]];
					TreeNode node = _folderTreeNodes.First(p => p.Value == folder).Key;

					_bookmarksFoldersTreeView.SelectedNode = node;
				}
			}
		}

		/// <summary>
		/// Handler method that is called when a user clicks in <see cref="_bookmarksListView"/>.  If it's a right click and the user has actually clicked on
		/// an item, we display the context menu.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_bookmarksListView"/> in this case.</param>
		/// <param name="e">Arguments associated with the click event.</param>
		private void _bookmarksListView_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				ListViewHitTestInfo hitTestInfo = _bookmarksListView.HitTest(e.Location);

				if (hitTestInfo.Item != null)
				{
					if (_listViewConnections.ContainsKey(hitTestInfo.Item))
					{
						_contextMenuItem = _listViewConnections[hitTestInfo.Item];

						_bookmarksListView.ContextMenuStrip = null;
						_bookmarkContextMenu.Show(Cursor.Position);
					}

					else
					{
						_contextMenuItem = _listViewFolders[hitTestInfo.Item];
						_setContextMenuItem = false;
						_folderContextMenu.Show(Cursor.Position);
					}
				}
			}
		}

		/// <summary>
		/// Handler method that's called when the user clicks the "Open bookmark in new tab" menu item.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private async void _openBookmarkNewTabMenuItem_Click(object sender, EventArgs e)
		{
			await OpenSelectedBookmarks();
		}

		/// <summary>
		/// Handler method that's called when the user clicks the "Edit" menu item in the context menu that appears when the user right-clicks on a bookmark
		/// in the list view.  Opens the settings window for the connection's protocol type.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _editBookmarkMenuItem_Click(object sender, EventArgs e)
		{
			ListViewItem selectedItem = _bookmarksListView.SelectedItems[0];
			TitleBarTab settingsTab = new TitleBarTab(ParentTabs)
				                         {
					                         Content = new SettingsWindow(ParentTabs)
						                                   {
							                                   SettingsForms = new List<Form>
								                                                  {
									                                                  ConnectionFactory.CreateSettingsForm(_listViewConnections[selectedItem])
								                                                  },
							                                   Text = "Settings for " + _listViewConnections[selectedItem].DisplayName
						                                   }
				                         };

			// When the settings form is closed, update the second column in the list view with the updated host for the connection
			settingsTab.Content.FormClosed += (o, args) => selectedItem.SubItems[1].Text = _listViewConnections[selectedItem].Host;

			ParentTabs.Tabs.Add(settingsTab);
			ParentTabs.ResizeTabContents(settingsTab);
			ParentTabs.SelectedTab = settingsTab;
		}

		/// <summary>
		/// Handler method that's called when the user clicks the "Delete" menu item in the context menu that appears when the user right-clicks on an item
		/// in the list view.  Removes the items from the list view, the lookups collection, and their parent folders and then re-sorts the list view and tree
		/// view.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			// Defer the sorting until after we're finished to eliminate unnecessary redraws
			_deferSort = true;

			foreach (ListViewItem selectedItem in _bookmarksListView.SelectedItems)
			{
				if (_listViewConnections.ContainsKey(selectedItem))
				{
					_folderTreeNodes[_bookmarksFoldersTreeView.SelectedNode].Bookmarks.Remove(_listViewConnections[selectedItem]);
					_listViewConnections.Remove(selectedItem);
				}

				else
				{
					_folderTreeNodes[_bookmarksFoldersTreeView.SelectedNode].ChildFolders.Remove(_listViewFolders[selectedItem]);
					_listViewFolders.Remove(selectedItem);
				}

				_bookmarksListView.Items.Remove(selectedItem);
			}

			// Sort the tree view and the list view and then save the bookmarks
			_deferSort = false;

			_bookmarksFoldersTreeView.BeginInvoke(new Action(SortTreeView));
			_bookmarksListView.BeginInvoke(new Action(_bookmarksListView.Sort));

			await Bookmarks.Instance.Save();

            SetBookmarksListViewHeight();
		}

		/// <summary>
		/// Handler method that's called when the user clicks the "Open all bookmarks" menu item in the context menu that appears when the user right-clicks on
		/// a node in the tree view.  Opens all descendant bookmarks in the folder and its children in separate tabs.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private async void _folderOpenAllMenuItem_Click(object sender, EventArgs e)
		{
			await OpenAllBookmarks(_contextMenuItem as BookmarksFolder);
		}

		/// <summary>
		/// Recursive method that opens all the descendant bookmarks in <paramref name="folder"/> in separate tabs.
		/// </summary>
		/// <param name="folder">Current folder for which we are opening bookmarks.</param>
		private async Task OpenAllBookmarks(BookmarksFolder folder)
		{
			foreach (IConnection connection in folder.Bookmarks)
				await _applicationForm.Connect(connection);

			foreach (BookmarksFolder childFolder in folder.ChildFolders)
				await OpenAllBookmarks(childFolder);
		}

		/// <summary>
		/// Opens all bookmarks currently selected in <see cref="_bookmarksListView"/>.
		/// </summary>
		private async Task OpenSelectedBookmarks()
		{
			foreach (ListViewItem item in _bookmarksListView.SelectedItems)
				await _applicationForm.Connect(_listViewConnections[item]);
		}

		/// <summary>
		/// Handler method that's called when the user clicks in <see cref="_bookmarksFoldersTreeView"/>.  If it's a right click and the user actually clicks
		/// on a node, we open the context menu for that folder.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_bookmarksFoldersTreeView"/> in this case.</param>
		/// <param name="e">Arguments associated with the mouse click event.</param>
		private void _bookmarksTreeView_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				TreeViewHitTestInfo hitTestInfo = _bookmarksFoldersTreeView.HitTest(e.Location);

				if (hitTestInfo.Node != null)
				{
					_bookmarksFoldersTreeView.SelectedNode = hitTestInfo.Node;
					_contextMenuItem = _folderTreeNodes[hitTestInfo.Node];
					_setContextMenuItem = false;

					_folderContextMenu.Show(Cursor.Position);
				}
			}
		}

		/// <summary>
		/// Handler method that's called when the user clicks a menu item under the "Add bookmark..." menu item.  We create a new item in 
		/// <see cref="_bookmarksListView"/> with the proper icon for the protocol and open its label for editing.
		/// </summary>
		/// <param name="type">Protocol that the bookmark is being created for.</param>
		private async void _addBookmarkMenuItem_Click(IProtocol type)
		{
			// Create a new connection instance by cloning the protocol's default
			IConnection connection = (IConnection) (await ConnectionFactory.GetDefaults(type.GetType())).Clone();

			connection.Name = "New Connection";

			// Add the bookmark to the current folder
			if (_folderTreeNodes[FoldersTreeView.SelectedNode] != (_contextMenuItem as BookmarksFolder))
			{
				FoldersTreeView.SelectedNode = _folderTreeNodes.Single(n => n.Value == (_contextMenuItem as BookmarksFolder)).Key;
				FoldersTreeView.SelectedNode.Expand();
			}

			_deferSort = true;
			(_contextMenuItem as BookmarksFolder).Bookmarks.Add(connection);
			_deferSort = false;

			// Set the flag so that once the user is finished renaming the connection, we open the settings for it
			_showSettingsAfterItemLabelEdit = true;

			ListViewItem newListItem = _listViewConnections.First(pair => pair.Value == connection).Key;
			_bookmarksListView.SelectedIndices.Clear();

			SortListView();
            await Bookmarks.Instance.Save();

			// Start the edit process for the new list item's name
			newListItem.Selected = true;
			newListItem.BeginEdit();
		}

		/// <summary>
		/// Handler method that's called when the user clicks the "Add folder..." menu item in the context menu that appears when the user right-clicks in the
		/// tree view.  Adds a new node to the tree view and the parent <see cref="BookmarksFolder"/> instance.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event</param>
		private async void _addFolderMenuItem_Click(object sender, EventArgs e)
		{
			// Create a new BookmarksFolder, add it to the tree view, and add it to the parent folder instance
			BookmarksFolder newFolder = new BookmarksFolder
				                            {
					                            Name = "New folder"
				                            };

			_deferSort = true;
			(_contextMenuItem as BookmarksFolder).ChildFolders.Add(newFolder);
			_deferSort = false;

			TreeNode newNode = _folderTreeNodes.SingleOrDefault(kvp => kvp.Value == newFolder).Key;

			_bookmarksFoldersTreeView.SelectedNode.Expand();
			_bookmarksFoldersTreeView.SelectedNode = newNode;

			SortTreeView();
            await Bookmarks.Instance.Save();
			newNode.BeginEdit();
        }

		/// <summary>
		/// Handler method that's called when the user finishes renaming a folder in the tree view.  We set the <see cref="BookmarksFolder.Name"/> property
		/// of the corresponding <see cref="BookmarksFolder"/> to the new value.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private async void _bookmarksTreeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
		{
            _renamingBookmarkItem = false;

			if (e.CancelEdit || String.IsNullOrEmpty(e.Label))
				return;

			_folderTreeNodes[_bookmarksFoldersTreeView.SelectedNode].Name = e.Label;
            await Bookmarks.Instance.Save();

			_bookmarksFoldersTreeView.BeginInvoke(new Action(SortTreeView));
		}

		/// <summary>
		/// Handler method that's called when the user clicks the "Rename..." menu item in the context menu that appears when the user right-clicks on an node
		/// in the tree view.  Starts the label edit process on the tree node.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _renameFolderMenuItem_Click(object sender, EventArgs e)
		{
            if (_bookmarksFoldersTreeView.Focused)
            {
                _bookmarksFoldersTreeView.SelectedNode.BeginEdit();
            }

            else
            {
                _listViewFolders.Single(kvp => kvp.Value == _contextMenuItem as BookmarksFolder).Key.BeginEdit();
            }
		}

		/// <summary>
		/// Handler method that's called when the user clicks the "Delete" menu item in the context menu that appears when the user right-clicks on an node
		/// in the tree view.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private async void _deleteFolderMenuItem_Click(object sender, EventArgs e)
		{
			_deferSort = true;

			BookmarksFolder folder = (BookmarksFolder) _contextMenuItem;
			folder.ParentFolder.ChildFolders.Remove(folder);

			_bookmarksListView.Items.Clear();
			_bookmarksFoldersTreeView.BeginInvoke(new Action(_bookmarksFoldersTreeView.Sort));

			_deferSort = false;

            await Bookmarks.Instance.Save();

            SetBookmarksListViewHeight();
		}

		/// <summary>
		/// Recursive method that's called to remove a folder and all its given descendants from the <see cref="_folderTreeNodes"/> lookup.  Called from 
		/// <see cref="ChildFolders_CollectionModified"/>.
		/// </summary>
		/// <param name="currentNode">Tree node for the folder that we are to remove.</param>
		private void RemoveAllFolders(TreeNode currentNode)
		{
			foreach (TreeNode childNode in currentNode.Nodes)
				RemoveAllFolders(childNode);

			_folderTreeNodes.Remove(currentNode);
		}

		/// <summary>
		/// Handler method that's called after the user finishes renaming an item in <see cref="_bookmarksListView"/>.  Sets the <see cref="IConnection.Name"/>
		/// or <see cref="BookmarksFolder.Name"/> property and, if the user is setting the name for a newly-created bookmark, opens the option window for that
		/// bookmark.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_bookmarksListView"/> in this case.</param>
		/// <param name="e">Item being edited and its new label.</param>
		private async void _bookmarksListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
		{
            _renamingBookmarkItem = false;

			if (e.CancelEdit || String.IsNullOrEmpty(e.Label))
				return;

			if (_listViewConnections.ContainsKey(_bookmarksListView.Items[e.Item]))
			{
				IConnection connection = _listViewConnections[_bookmarksListView.Items[e.Item]];

				connection.Name = e.Label;

				// If this is a new connection (no Host property set) and the item label doesn't contain spaces, we default the host name for the connection to
				// the label name
				if (String.IsNullOrEmpty(connection.Host) && !e.Label.Contains(" "))
				{
					connection.Host = e.Label;
					_bookmarksListView.Items[e.Item].SubItems[1].Text = e.Label;
				}
			}

			else
				_listViewFolders[_bookmarksListView.Items[e.Item]].Name = e.Label;

            await Bookmarks.Instance.Save();

			if (_showSettingsAfterItemLabelEdit)
			{
				// Open the settings window for the new bookmark if its name was set for the first time
				ListViewItem selectedItem = _bookmarksListView.SelectedItems[0];
				TitleBarTab settingsTab = new TitleBarTab(ParentTabs)
					                         {
						                         Content = new SettingsWindow(ParentTabs)
							                                   {
								                                   SettingsForms = new List<Form>
									                                                  {
										                                                  ConnectionFactory.CreateSettingsForm(_listViewConnections[selectedItem])
									                                                  }
							                                   }
					                         };

				// When the settings window is closed, update the value in the host column to what was supplied by the user
				settingsTab.Content.FormClosed += (o, args) => selectedItem.SubItems[1].Text = _listViewConnections[selectedItem].Host;

				ParentTabs.Tabs.Add(settingsTab);
				ParentTabs.ResizeTabContents(settingsTab);
				ParentTabs.SelectedTab = settingsTab;

				_showSettingsAfterItemLabelEdit = false;
			}

			_bookmarksListView.BeginInvoke(new Action(_bookmarksListView.Sort));
		}

		/// <summary>
		/// Handler method that's called when the user clicks the "Rename..." menu item in the context menu that appears when the user right-clicks on an item
		/// in <see cref="_bookmarksListView"/>; calls <see cref="ListViewItem.BeginEdit"/> on the selected item.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void renameToolStripMenuItem_Click(object sender, EventArgs e)
		{
			_bookmarksListView.SelectedItems[0].BeginEdit();
        }

		/// <summary>
		/// Handler method that's called when the "Open bookmark in new window..." menu item in the context menu that appears when the user right-clicks on a
		/// bookmark item in <see cref="_bookmarksListView"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _openBookmarkNewWindowMenuItem_Click(object sender, EventArgs e)
		{
			MainForm mainForm = new MainForm(
				new List<IConnection>
					{
						_listViewConnections[_bookmarksListView.SelectedItems[0]]
					});

			ParentTabs.ApplicationContext.OpenWindow(mainForm);
			mainForm.Show();
		}

		/// <summary>
		/// Handler method that's called when the "Open all in new window..." menu item in the context menu that appears when the user right-clicks on a
		/// folder in <see cref="_bookmarksFoldersTreeView"/>.  Batches up the <see cref="IConnection.Guid"/>s of all of the bookmarks in the selected folder
		/// and its descendants and creates a new <see cref="MainForm"/> instance with them.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _folderOpenAllNewWindowMenuItem_Click(object sender, EventArgs e)
		{
			// Look recursively into this folder and its descendants to get all of the bookmarks
			List<IConnection> bookmarks = new List<IConnection>();
			FindAllBookmarks(_contextMenuItem as BookmarksFolder, bookmarks);

			if (bookmarks.Count > 0)
			{
				MainForm mainForm = new MainForm(bookmarks);
				ParentTabs.ApplicationContext.OpenWindow(mainForm);

				mainForm.Show();
			}
		}

		/// <summary>
		/// Recursive method that searches <paramref name="bookmarksFolder"/> and its descendants for all bookmarks.  Called from 
		/// <see cref="_folderOpenAllNewWindowMenuItem_Click"/>.
		/// </summary>
		/// <param name="bookmarks">List of bookmarks that have been assembled so far.</param>
		/// <param name="bookmarksFolder">Current folder that we're searching.</param>
		private void FindAllBookmarks(BookmarksFolder bookmarksFolder, List<IConnection> bookmarks)
		{
			bookmarks.AddRange(bookmarksFolder.Bookmarks);

			foreach (BookmarksFolder childFolder in bookmarksFolder.ChildFolders)
				FindAllBookmarks(childFolder, bookmarks);
		}

		/// <summary>
		/// Handler method that's called when the "Copy\" menu item in the context menu that appears when the user right-clicks on an item in 
		/// <see cref="_bookmarksListView"/>.  Clears out all items in <see cref="_copiedItems"/> and <see cref="_cutItems"/> and adds all selected
		/// <see cref="BookmarksFolder"/> and <see cref="IConnection"/> instances to <see cref="_copiedItems"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void copyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			_copiedItems.Clear();
			_cutItems.Clear();

			foreach (ListViewItem item in _bookmarksListView.SelectedItems)
			{
				_copiedItems.Add(
					_listViewConnections.ContainsKey(item)
						? _listViewConnections[item]
						: (object) _listViewFolders[item]);
			}
		}

		/// <summary>
		/// Handler method that's called when the "Paste" menu item in the context menu that appears when the user right-clicks on a folder in 
		/// <see cref="_bookmarksFoldersTreeView"/>.  Calls <see cref="PasteItems"/> to add the items to the selected folder.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private async void _pasteFolderMenuItem_Click(object sender, EventArgs e)
		{
			await PasteItems(_contextMenuItem as BookmarksFolder);
		}

		/// <summary>
		/// Adds the <see cref="IConnection"/> and <see cref="BookmarksFolder"/> items in <see cref="_copiedItems"/> or <see cref="_cutItems"/> to 
		/// <paramref name="targetFolder"/>.  Called from <see cref="_pasteFolderMenuItem_Click"/>.
		/// </summary>
		/// <param name="targetFolder">Target folder that we're pasting items into.</param>
		private async Task PasteItems(BookmarksFolder targetFolder)
		{
            List<object> source = _cutItems.Union(_copiedItems).ToList();

            _deferSort = true;

			// Make sure that the source items aren't from folder that we're trying to paste into
			if ((source[0] is BookmarksFolder && ((BookmarksFolder) source[0]).ParentFolder == targetFolder) ||
			    (source[0] is IConnection && ((IConnection) source[0]).ParentFolder == targetFolder))
			{
				MessageBox.Show(this, "You cannot paste items into their existing parent folders.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			List<object> clonedItems = (from item in source
			                            select ((ICloneable) item).Clone()).ToList();

			// Add the items to the target folder
			foreach (object clonedItem in clonedItems)
			{
				IConnection item = clonedItem as IConnection;

				if (item != null)
					targetFolder.Bookmarks.Add(item);

				else
					targetFolder.MergeFolder((BookmarksFolder) clonedItem);
			}

			// If we're pasting cut items, remove those items from their previous parent folders
			if (_cutItems.Count > 0)
			{
				foreach (object cutItem in _cutItems)
				{
					IConnection connection = cutItem as IConnection;

					if (connection != null)
					{
						connection.ParentFolder.Bookmarks.Remove(connection);

						if (_listViewConnections.ContainsValue(connection))
							_bookmarksListView.Items.Remove(_listViewConnections.First(kvp => kvp.Value == connection).Key);
					}

					else
					{
						BookmarksFolder folder = cutItem as BookmarksFolder;

						folder.ParentFolder.ChildFolders.Remove(folder);

						if (_listViewFolders.ContainsValue(folder))
							_bookmarksListView.Items.Remove(_listViewFolders.First(kvp => kvp.Value == folder).Key);
					}
				}

				_cutItems.Clear();
			}

			_bookmarksFoldersTreeView.BeginInvoke(new Action(SortTreeView));
			_bookmarksListView.BeginInvoke(new Action(_bookmarksListView.Sort));

			_deferSort = false;

            await Bookmarks.Instance.Save();

            SetBookmarksListViewHeight();
		}

		/// <summary>
		/// Handler method that's called when the "Copy" menu item in the context menu that appears when the user right-clicks on a folder in 
		/// <see cref="_bookmarksFoldersTreeView"/>.  Clears out <see cref="_copiedItems"/> and <see cref="_cutItems"/> and adds the corresponding 
		/// <see cref="BookmarksFolder"/> instance to <see cref="_copiedItems"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _copyFolderMenuItem_Click(object sender, EventArgs e)
		{
			_copiedItems.Clear();
			_cutItems.Clear();
			_copiedItems.Add(_contextMenuItem as BookmarksFolder);
		}

		/// <summary>
		/// Sorts <see cref="_bookmarksFoldersTreeView"/> by calling <see cref="TreeView.Sort"/>.
		/// </summary>
		private void SortTreeView()
		{
			TreeNode currentlySelectedNode = _bookmarksFoldersTreeView.SelectedNode;

			_bookmarksFoldersTreeView.BeginUpdate();
			_bookmarksFoldersTreeView.Sort();
			_bookmarksFoldersTreeView.SelectedNode = currentlySelectedNode;
			_bookmarksFoldersTreeView.EndUpdate();
		}

		/// <summary>
		/// Sorts <see cref="_bookmarksListView"/> by calling <see cref="ListView.Sort"/>.
		/// </summary>
		private void SortListView()
		{
			_bookmarksListView.BeginUpdate();
			_bookmarksListView.Sort();
			_bookmarksListView.EndUpdate();
		}

		/// <summary>
		/// Handler method that's called when the "Cut" menu item in the context menu that appears when the user right-clicks on an item in
		/// <see cref="_bookmarksListView"/>.  Clears out <see cref="_copiedItems"/> and <see cref="_cutItems"/> and adds the selected items to 
		/// <see cref="_cutItems"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _cutBookmarkMenuItem_Click(object sender, EventArgs e)
		{
			_copiedItems.Clear();
			_cutItems.Clear();

			foreach (ListViewItem item in _bookmarksListView.SelectedItems)
			{
				_cutItems.Add(
					_listViewConnections.ContainsKey(item)
						? _listViewConnections[item]
						: (object) _listViewFolders[item]);

				item.ForeColor = Color.Gray;
			}
		}

		/// <summary>
		/// Handler method that's called when the "Cut" menu item in the context menu that appears when the user right-clicks on a folder in 
		/// <see cref="_bookmarksFoldersTreeView"/>.  Clears out <see cref="_copiedItems"/> and <see cref="_cutItems"/> and adds the corresponding 
		/// <see cref="BookmarksFolder"/> instance to <see cref="_cutItems"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _cutFolderMenuItem_Click(object sender, EventArgs e)
		{
			_copiedItems.Clear();
			_cutItems.Clear();

			_cutItems.Add(_contextMenuItem as BookmarksFolder);

			_folderTreeNodes.Single(kvp => kvp.Value == _contextMenuItem as BookmarksFolder).Key.ForeColor = Color.Gray;

			if (_listViewFolders.FirstOrDefault(kvp => kvp.Value == _contextMenuItem as BookmarksFolder).Value != null)
				_listViewFolders.Single(kvp => kvp.Value == _contextMenuItem as BookmarksFolder).Key.ForeColor = Color.Gray;
		}

		/// <summary>
		/// Handler method that's called when the "Export" menu item in the context menu that appears when the user right-clicks on a folder in 
		/// <see cref="_bookmarksFoldersTreeView"/>.  Opens a <see cref="_bookmarkExportDialog"/> and then calls <see cref="Bookmarks.Export"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _exportBookMarkMenuitem_Click(object sender, EventArgs e)
		{
			_bookmarkExportDialog.ShowDialog();
			Bookmarks.Instance.Export(_bookmarkExportDialog.FileName);
		}

		/// <summary>
		/// Handler method that's called when the "Import" menu item in the context menu that appears when the user right-clicks on a folder in 
		/// <see cref="_bookmarksFoldersTreeView"/>.  Opens a <see cref="_bookmarkImportDialog"/> and then calls <see cref="Import"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private async void _importBookmarkMenuItem_Click(object sender, EventArgs e)
		{
			_bookmarkImportDialog.ShowDialog();
			await Import(_bookmarkImportDialog.FileName);
		}

		/// <summary>
		/// Handler method that's called when user starts dragging an item in <see cref="_bookmarksListView"/>; calls <see cref="Control.DoDragDrop"/> to begin
		/// the drag/drop operation.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _bookmarksListView_ItemDrag(object sender, ItemDragEventArgs e)
		{
			_listViewDropTarget = null;
			_treeViewDropTarget = null;
			_draggingFromListView = true;
			_draggingFromTree = false;
			_bookmarksListView.DoDragDrop(_bookmarksListView.SelectedItems, DragDropEffects.Move);
		}

		/// <summary>
		/// Handler method that's called when a drag operation enters <see cref="_bookmarksFoldersTreeView"/> or <see cref="_bookmarksFoldersTreeView"/>.  
		/// Checks to see if the items being dragged are a <see cref="ListView.SelectedListViewItemCollection"/> or a <see cref="TreeNode"/> and, if so, sets 
		/// the drop effect to <see cref="DragDropEffects.Move"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Data on items being dragged.</param>
		private void _bookmarks_DragEnter(object sender, DragEventArgs e)
		{
			int length = e.Data.GetFormats().Length - 1;

			for (int i = 0; i <= length; i++)
			{
				if (e.Data.GetFormats()[i].Equals("System.Windows.Forms.ListView+SelectedListViewItemCollection") ||
				    e.Data.GetFormats()[i].Equals(typeof (TreeNode).FullName))
					e.Effect = DragDropEffects.Move;
			}
		}

		/// <summary>
		/// Handler method that's called when a the mouse is moved during a drag operation over <see cref="_bookmarksListView"/>.  Gets the item being dragged
		/// over and, if it's anything other than a <see cref="BookmarksFolder"/>, the drop effect is set to <see cref="DragDropEffects.None"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Data on items being dragged.</param>
		private void _bookmarksListView_DragOver(object sender, DragEventArgs e)
		{
			Point clientPoint = _bookmarksListView.PointToClient(new Point(e.X, e.Y));
			ListViewItem targetItem = _bookmarksListView.GetItemAt(clientPoint.X, clientPoint.Y);

			if (_treeViewDropTarget != null)
			{
				_treeViewDropTarget.BackColor = _bookmarksFoldersTreeView.BackColor;
				_treeViewDropTarget.ForeColor = _bookmarksFoldersTreeView.ForeColor;
			}

			_treeViewDropTarget = null;

			// Reset the background and foreground colors on the previous drop target if we've moved onto another one
			if (_listViewDropTarget != null && _listViewDropTarget != targetItem)
			{
				_listViewDropTarget.BackColor = _bookmarksListView.BackColor;
				_listViewDropTarget.ForeColor = _bookmarksListView.ForeColor;

				e.Effect = DragDropEffects.None;
			}

			// If the user has actually moved over an item in the list view and that item is a folder, highlight that list view item and set the drop effect
			// to Move
			if (targetItem != null && targetItem.ImageIndex == 0 && _listViewDropTarget != targetItem)
			{
				BookmarksFolder targetFolder = _listViewFolders[targetItem];
				BookmarksFolder dragTreeFolder = _draggingFromTree
					                             ? _folderTreeNodes[_bookmarksFoldersTreeView.SelectedNode]
					                             : null;

				if (dragTreeFolder == null || (!IsDescendantOf(targetFolder, dragTreeFolder) && dragTreeFolder.ParentFolder != targetFolder))
				{
					if (targetItem.Selected)
					{
						_listViewDropTarget = null;
						e.Effect = DragDropEffects.None;
					}

					else
					{
						targetItem.BackColor = SystemColors.Highlight;
						targetItem.ForeColor = SystemColors.HighlightText;

						_listViewDropTarget = targetItem;
						e.Effect = DragDropEffects.Move;
					}
				}

				else
				{
					e.Effect = DragDropEffects.None;
				}
			}

			// Otherwise, if the user is over an item that's not a folder, set the drop effect to None
			if (targetItem == null || targetItem.ImageIndex != 0)
			{
				_listViewDropTarget = null;
				e.Effect = DragDropEffects.None;
			}
		}

		private bool IsDescendantOf(BookmarksFolder checkFolder, BookmarksFolder potentialParent)
		{
			while (checkFolder != null)
			{
				if (checkFolder == potentialParent)
					return true;

				checkFolder = checkFolder.ParentFolder;
			}

			return false;
		}

		/// <summary>
		/// Handler method that's called when a drop operation occurs on <see cref="_bookmarksFoldersTreeView"/> or <see cref="_bookmarksFoldersTreeView"/>.  
		/// Cuts the selected items and then pastes them into the destination folder using <see cref="PasteItems"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Data on items being dragged.</param>
		private async void _bookmarks_DragDrop(object sender, DragEventArgs e)
		{
			// Call the list view cut menu item handler if we were dragging from the list view
			if (_draggingFromListView)
				_cutBookmarkMenuItem_Click(null, null);

				// Otherwise, cut the folder node being dragged
			else
			{
				_copiedItems.Clear();
				_cutItems.Clear();

				_cutItems.Add(_folderTreeNodes[(TreeNode) e.Data.GetData(typeof (TreeNode))]);
			}

			// Call PasteItems as appropriate to paste the dragged items into the destination folder
			if (_listViewDropTarget != null)
			{
				await PasteItems(_listViewFolders[_listViewDropTarget]);

				_listViewDropTarget.BackColor = _bookmarksListView.BackColor;
				_listViewDropTarget.ForeColor = _bookmarksListView.ForeColor;
			}

			else if (_treeViewDropTarget != null)
			{
				await PasteItems(_folderTreeNodes[_treeViewDropTarget]);

				_treeViewDropTarget.BackColor = _bookmarksFoldersTreeView.BackColor;
				_treeViewDropTarget.ForeColor = _bookmarksFoldersTreeView.ForeColor;
			}
		}

		/// <summary>
		/// Handler method that's called when user starts dragging an item in <see cref="_bookmarksFoldersTreeView"/>; calls <see cref="Control.DoDragDrop"/> 
		/// to begin the drag/drop operation.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _bookmarksFoldersTreeView_ItemDrag(object sender, ItemDragEventArgs e)
		{
			_listViewDropTarget = null;
			_treeViewDropTarget = null;
			_draggingFromListView = false;
			_draggingFromTree = true;

			_bookmarksFoldersTreeView.SelectedNode = e.Item as TreeNode;
			_bookmarksFoldersTreeView.DoDragDrop(e.Item, DragDropEffects.Move);
		}

		/// <summary>
		/// Handler method that's called when a the mouse is moved during a drag operation over <see cref="_bookmarksFoldersTreeView"/>.  If no tree node is 
		/// being dragged over, the drop effect is set to <see cref="DragDropEffects.None"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Data on items being dragged.</param>
		private void _bookmarksFoldersTreeView_DragOver(object sender, DragEventArgs e)
		{
			Point clientPoint = _bookmarksFoldersTreeView.PointToClient(new Point(e.X, e.Y));
			TreeNode targetItem = _bookmarksFoldersTreeView.GetNodeAt(clientPoint.X, clientPoint.Y);

			// Reset the background and foreground colors on the previous drop target if we've moved onto another one
			if (_listViewDropTarget != null)
			{
				_listViewDropTarget.BackColor = _bookmarksListView.BackColor;
				_listViewDropTarget.ForeColor = _bookmarksListView.ForeColor;
			}

			_listViewDropTarget = null;

			if (_treeViewDropTarget != null && _treeViewDropTarget != targetItem)
			{
				_treeViewDropTarget.BackColor = _bookmarksFoldersTreeView.BackColor;
				_treeViewDropTarget.ForeColor = _bookmarksFoldersTreeView.ForeColor;

				e.Effect = DragDropEffects.None;
			}

			// If the user has actually moved over an tree node, highlight that node and set the drop effect to Move
			if (targetItem != null && _treeViewDropTarget != targetItem)
			{
				BookmarksFolder targetFolder = _folderTreeNodes[targetItem];
				BookmarksFolder dragFolder = _draggingFromTree
					                             ? _folderTreeNodes[_bookmarksFoldersTreeView.SelectedNode]
					                             : _listViewFolders[_bookmarksListView.SelectedItems[0]];

				if (dragFolder == null || (!IsDescendantOf(targetFolder, dragFolder) && dragFolder.ParentFolder != targetFolder))
				{
					if (targetItem == _bookmarksFoldersTreeView.SelectedNode)
					{
						_treeViewDropTarget = null;
						e.Effect = DragDropEffects.None;
					}

					else
					{
						targetItem.BackColor = SystemColors.Highlight;
						targetItem.ForeColor = SystemColors.HighlightText;

						_treeViewDropTarget = targetItem;
						e.Effect = DragDropEffects.Move;
					}
				}

				else
				{
					e.Effect = DragDropEffects.None;
				}
			}

			// Otherwise, set the drop effect to None
			if (targetItem == null)
			{
				_treeViewDropTarget = null;
				e.Effect = DragDropEffects.None;
			}
		}

		/// <summary>
		/// Handler method that's called when the "Clear username and password" menu item in the context menu that appears when the user right-clicks on a 
		/// folder  in <see cref="_bookmarksFoldersTreeView"/>.  Clears the <see cref="BookmarksFolder.Username"/> and <see cref="BookmarksFolder.Password"/> 
		/// properties in the selected <see cref="BookmarksFolder"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private async void _clearUsernamePasswordToolStripMenuItem_Click(object sender, EventArgs e)
		{
			BookmarksFolder selectedFolder = _contextMenuItem as BookmarksFolder;

			selectedFolder.Username = null;
			selectedFolder.Password = null;

            await Bookmarks.Instance.Save();
		}

		/// <summary>
		/// Handler method that's called when the "Set username and password..." menu item in the context menu that appears when the user right-clicks on a 
		/// folder  in <see cref="_bookmarksFoldersTreeView"/>.  Opens a <see cref="UsernamePasswordWindow"/> and sets the 
		/// <see cref="BookmarksFolder.Username"/> and <see cref="BookmarksFolder.Password"/> properties in the selected <see cref="BookmarksFolder"/> to the
		/// entered values.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private async void _setUsernamePasswordMenuItem_Click(object sender, EventArgs e)
		{
			BookmarksFolder selectedFolder = _contextMenuItem as BookmarksFolder;
			UsernamePasswordWindow usernamePasswordWindow = new UsernamePasswordWindow
				                                                {
					                                                Username = selectedFolder.Username
				                                                };

			if (selectedFolder.Password != null)
				usernamePasswordWindow.Password = selectedFolder.Password;

			if (usernamePasswordWindow.ShowDialog() == DialogResult.OK)
			{
				selectedFolder.Username = usernamePasswordWindow.Username;
				selectedFolder.Password = usernamePasswordWindow.Password;

                await Bookmarks.Instance.Save();
			}
		}

		/// <summary>
		/// Handler method that's called when <see cref="_folderContextMenu"/> is opening.  Sets <see cref="_contextMenuItem"/> to the currently opened folder
		/// in <see cref="FoldersTreeView"/> (if necessary) and sets the status of <see cref="_pasteFolderMenuItem"/> appropriately depending on if any items
		/// have been copied or cut.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_folderContextMenu"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _folderContextMenu_Opening(object sender, CancelEventArgs e)
		{
			if (_setContextMenuItem)
				_contextMenuItem = _folderTreeNodes[FoldersTreeView.SelectedNode];

			_pasteFolderMenuItem.Enabled = _copiedItems.Count > 0 || _cutItems.Count > 0;
		}

		/// <summary>
		/// Handler method that's called when <see cref="_bookmarkContextMenu"/> is closed.  Resets <see cref="ListView.ContextMenuStrip"/> of
		/// <see cref="_bookmarksListView"/> to <see cref="_folderContextMenu"/> so that right-clicking in empty space in the list view will display the folder
		/// context menu.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_bookmarkContextMenu"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _bookmarkContextMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
		{
			_bookmarksListView.ContextMenuStrip = _folderContextMenu;
		}

		/// <summary>
		/// Handler method that's called when <see cref="_folderContextMenu"/> is closed.  Sets <see cref="_setContextMenuItem"/> to true so that if the user
		/// next right-clicks on empty space in the list view, <see cref="_contextMenuItem"/> will be correctly set to the currently open folder in 
		/// <see cref="FoldersTreeView"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_folderContextMenu"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _folderContextMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
		{
			_setContextMenuItem = true;
		}

		/// <summary>
		/// Custom comparer class for <see cref="BookmarksWindow._bookmarksListView"/> used when calling <see cref="ListView.Sort"/>; makes sure that
		/// <see cref="BookmarksFolder"/> items are listed before <see cref="IConnection"/> items.
		/// </summary>
		protected class BookmarksListViewComparer : IComparer
		{
			/// <summary>
			/// Compares to list items.  <see cref="BookmarksFolder"/> items are always listed before <see cref="IConnection"/> items, otherwise they are
			/// sorted alphabetically.
			/// </summary>
			/// <param name="x">Left item to compare.</param>
			/// <param name="y">Right item to compare.</param>
			/// <returns>-1 if <paramref name="x"/> should be displayed first, 0 if <paramref name="x"/> and <paramref name="y"/> are equivalent, and 1 if
			/// <paramref name="y"/> should be displayed first.</returns>
			public int Compare(object x, object y)
			{
				ListViewItem item1 = x as ListViewItem;
				ListViewItem item2 = y as ListViewItem;

				if (item1.ImageIndex != item2.ImageIndex && item1.ImageIndex == 0)
					return -1;

				return String.CompareOrdinal(item1.Text, item2.Text);
			}
		}

        private void _bookmarksListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void _bookmarksListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
        }

        private void _bookmarksListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (e.ColumnIndex != 2 || _itemEditingNotes != e.Item)
            {
                e.DrawDefault = true;
                return;
            }

            if (_notesTextBox != null)
            {
                return;
            }

            _notesTextBox = new TextBox();

            _notesTextBox.Location = new Point(e.Bounds.Left + _bookmarksListView.Location.X + 3, e.Bounds.Top + _bookmarksListView.Location.Y + 7);
            _notesTextBox.Size = new Size(e.Bounds.Width - 6, e.Bounds.Height + 2);
			_notesTextBox.Font = new Font("Segoe UI", (float)9.75);
            _notesTextBox.Text = _itemEditingNotes.SubItems[2].Text;
            _notesTextBox.LostFocus += NotesTextBox_LostFocus;
            _notesTextBox.KeyDown += NotesTextBox_KeyDown;
            
            _notesTextBox.SelectAll();

            _splitContainer.Panel2.Controls.Add(_notesTextBox);
            _notesTextBox.Focus();
            _notesTextBox.BringToFront();

            _bookmarksListView.Scrollable = false;
        }

        private void NotesTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                _bookmarksListView.Focus();
            }
        }

        private void NotesTextBox_LostFocus(object sender, EventArgs e)
        {
            SuspendLayout();

            if (_listViewConnections.ContainsKey(_itemEditingNotes))
            {
                _listViewConnections[_itemEditingNotes].Notes = _notesTextBox.Text;
            }

            else if (_listViewFolders.ContainsKey(_itemEditingNotes))
            {
                _listViewFolders[_itemEditingNotes].Notes = _notesTextBox.Text;
            }

            _itemEditingNotes.SubItems[2].Text = _notesTextBox.Text;
            _itemEditingNotes = null;

            _notesTextBox.LostFocus -= NotesTextBox_LostFocus;
            _notesTextBox.KeyDown -= NotesTextBox_KeyDown;
            _splitContainer.Panel2.Controls.Remove(_notesTextBox);
            _notesTextBox = null;

            _bookmarksListView.Scrollable = true;
            _bookmarksListView.Invalidate();

            ResumeLayout();
        }

        private void _bookmarksListView_MouseDown(object sender, MouseEventArgs e)
        {
            ListViewItem targetItem;
            ListViewItem.ListViewSubItem targetSubItem;

            GetListViewItemAt(e.X, e.Y, out targetItem, out targetSubItem);

            if (targetSubItem == null || targetItem.SubItems.Count < 3 || targetItem.SubItems[2] != targetSubItem)
            {
                return;
            }

            if (!_listViewNotesDoubleClickStarted)
            {
                _listViewNotesDoubleClickStarted = true;
                _listViewNotesDoubleClickTimer.Start();
            }

            else
            {
                _itemEditingNotes = targetItem;
                _bookmarksListView.Invalidate();
            }
        }

        private void GetListViewItemAt(int x, int y, out ListViewItem item, out ListViewItem.ListViewSubItem subItem)
        {
            subItem = null;
            item = _bookmarksListView.GetItemAt(0, y);

            if (item == null)
                return;

            subItem = item.GetSubItemAt(x, y);
        }

        /// <summary>
		/// Handler method that's called when the user's cursor goes over <see cref="_toolsButton"/>.  Sets the button's background to the standard
		/// "hover" image.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_toolsButton"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _toolsButton_MouseEnter(object sender, EventArgs e)
        {
            _toolsButton.BackgroundImage = Resources.ButtonHoverBackground;
        }

        /// <summary>
        /// Handler method that's called when the user's cursor leaves <see cref="_toolsButton"/>.  Sets the button's background to nothing.
        /// </summary>
        /// <param name="sender">Object from which this event originated, <see cref="_toolsButton"/> in this case.</param>
        /// <param name="e">Arguments associated with this event.</param>
        private void _toolsButton_MouseLeave(object sender, EventArgs e)
        {
            if (!_toolsMenu.Visible)
                _toolsButton.BackgroundImage = null;
        }

        /// <summary>
        /// Handler method that's called when the user clicks the "Exit" menu item in the tools menu.  Exits the entire application.
        /// </summary>
        /// <param name="sender">Object from which this event originated.</param>
        /// <param name="e">Arguments associated with this event.</param>
        private void _exitMenuItem_Click(object sender, EventArgs e)
        {
            ((Form)Parent).Close();
        }

        /// <summary>
        /// Handler method that's called when the user clicks the "Tools" icon in the toolbar.  Opens up <see cref="_toolsMenu"/>.
        /// </summary>
        /// <param name="sender">Object from which this event originated.</param>
        /// <param name="e">Arguments associated with this event.</param>
        private void _toolsButton_Click(object sender, EventArgs e)
        {
            _toolsButton.BackgroundImage = Resources.ButtonPressedBackground;
            _toolsMenu.DefaultDropDownDirection = ToolStripDropDownDirection.Left;
            _toolsMenu.Show(_toolsButton, -1 * _toolsMenu.Width + _toolsButton.Width, _toolsButton.Height);
        }

        private void _aboutMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog(ParentTabs);
        }

        /// <summary>
		/// Handler method that's called when the user clicks on the "Check for updates" menu item under the tools menu.  Starts the update check process by
		/// calling <see cref="MainForm.CheckForUpdate"/> on <see cref="ParentTabs"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _updatesMenuItem_Click(object sender, EventArgs e)
        {
            ParentTabs.CheckForUpdate();
        }

        /// <summary>
		/// Handler method that's called when the user clicks the "History" menu item under the tools menu.  Creates the history tab if one doesn't exist 
		/// already and then switches to it.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _historyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ParentTabs.OpenHistory();
        }

        /// <summary>
		/// Handler method that's called when the user clicks the <see cref="_newWindowMenuItem"/> in the tools menu.  Creates a new <see cref="MainForm"/>
		/// instance and opens it.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_newWindowMenuItem"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _newWindowMenuItem_Click(object sender, EventArgs e)
        {
            MainForm newWindow = new MainForm(new List<Guid>());
            ParentTabs.ApplicationContext.OpenWindow(newWindow);

            newWindow.Show();
        }

        /// <summary>
		/// Handler method that's called when the user clicks the "New tab" menu item under the tools menu.  Creates a new tab and then switches to it.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _newTabMenuItem_Click(object sender, EventArgs e)
        {
            ParentTabs.AddNewTab();
        }

        /// <summary>
		/// Handler method that's called when the user clicks the "Settings" menu item under the tools menu.  Creates the settings tab if one doesn't exist 
		/// already and then switches to it.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private async void _settingsMenuItem_Click(object sender, EventArgs e)
        {
            await ParentTabs.OpenSettings();
        }

        private void _toolsMenu_VisibleChanged(object sender, EventArgs e)
        {
            if (!_toolsMenu.Visible)
                _toolsButton.BackgroundImage = null;
        }

        private void _folderContextMenu_Opened(object sender, EventArgs e)
        {
            _deleteFolderMenuItem.Enabled = !_bookmarksFoldersTreeView.Focused || _bookmarksFoldersTreeView.SelectedNode.Parent != null;
            _renameFolderMenuItem.Enabled = !_bookmarksFoldersTreeView.Focused || _bookmarksFoldersTreeView.SelectedNode.Parent != null;
            _cutFolderMenuItem.Enabled = !_bookmarksFoldersTreeView.Focused || _bookmarksFoldersTreeView.SelectedNode.Parent != null;
            _copyFolderMenuItem.Enabled = !_bookmarksFoldersTreeView.Focused || _bookmarksFoldersTreeView.SelectedNode.Parent != null;
        }

        private void urlBackground_Resize(object sender, EventArgs e)
        {
            _urlPanel.AutoScroll = false;
        }

        private void _bookmarksListView_BeforeLabelEdit(object sender, LabelEditEventArgs e)
        {
            _renamingBookmarkItem = true;
        }

        private void _bookmarksFoldersTreeView_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            _renamingBookmarkItem = true;
        }

        private void _bookmarksFoldersTreeView_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (e.Node == null) 
                return;

            bool selected = (e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected;
            Font font = e.Node.NodeFont ?? e.Node.TreeView.Font;

            e.Graphics.FillRectangle(new SolidBrush(_bookmarksFoldersTreeView.BackColor), e.Bounds);
            TextRenderer.DrawText(e.Graphics, e.Node.Text, font, new Rectangle(e.Bounds.X, e.Bounds.Y + 3, e.Bounds.Width, e.Bounds.Height), selected ? Color.FromArgb(51, 103, 214) : e.Node.ForeColor, TextFormatFlags.GlyphOverhangPadding);

			if (e.Node.Parent != null && (e.Node.IsExpanded || (e.Node.Nodes != null && e.Node.Nodes.Count > 0)))
			{
				e.Graphics.FillRectangle(new SolidBrush(_bookmarksFoldersTreeView.BackColor), new RectangleF(e.Bounds.X - 50, e.Bounds.Y, 20, 20));

				if (e.Node.IsExpanded)
                {
					e.Graphics.DrawImage(Resources.TreeViewNodeExpanded, new Point(e.Bounds.X - 50, e.Bounds.Y + 10));
                }

				else
                {
					e.Graphics.DrawImage(Resources.TreeViewNodeExpandable, new Point(e.Bounds.X - 47, e.Bounds.Y + 7));
				}
			}
        }

        private void _toolsButton_MouseDown(object sender, MouseEventArgs e)
        {
			_toolsButton.BackgroundImage = Resources.ButtonPressedBackground;
		}
    }
}