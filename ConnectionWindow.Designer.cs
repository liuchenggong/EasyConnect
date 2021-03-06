namespace EasyConnect
{
    partial class ConnectionWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectionWindow));
            this._toolsMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._newTabMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._newWindowMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._historyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolsMenuSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._settingsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._updatesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolsMenuSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._aboutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._bookmarksMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._bookmarksManagerMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.bookmarkThisSiteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._connectionContainerPanel = new System.Windows.Forms.Panel();
            this._toolbarBorder = new System.Windows.Forms.Panel();
            this._omniBarPanel = new System.Windows.Forms.Panel();
            this._omniBarBorder = new System.Windows.Forms.Panel();
            this.toolbarBackground = new System.Windows.Forms.Panel();
            this._urlPanelContainer = new System.Windows.Forms.Panel();
            this.urlTextBox = new System.Windows.Forms.TextBox();
            this.urlBackground = new System.Windows.Forms.Panel();
            this._iconPictureBox = new System.Windows.Forms.PictureBox();
            this.urlBackgroundRight = new System.Windows.Forms.PictureBox();
            this.urlBackgroundLeft = new System.Windows.Forms.PictureBox();
            this._toolsButton = new System.Windows.Forms.PictureBox();
            this._bookmarksButton = new System.Windows.Forms.PictureBox();
            this._toolsMenu.SuspendLayout();
            this._bookmarksMenu.SuspendLayout();
            this.toolbarBackground.SuspendLayout();
            this.urlBackground.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._iconPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.urlBackgroundRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.urlBackgroundLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._toolsButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._bookmarksButton)).BeginInit();
            this.SuspendLayout();
            // 
            // _toolsMenu
            // 
            this._toolsMenu.BackColor = System.Drawing.Color.White;
            this._toolsMenu.DropShadowEnabled = false;
            this._toolsMenu.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._toolsMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._newTabMenuItem,
            this._newWindowMenuItem,
            this.toolStripSeparator1,
            this._historyToolStripMenuItem,
            this._toolsMenuSeparator1,
            this._settingsMenuItem,
            this.toolStripSeparator2,
            this._updatesMenuItem,
            this._toolsMenuSeparator2,
            this._aboutMenuItem,
            this._exitMenuItem});
            this._toolsMenu.Name = "_toolsMenu";
            this._toolsMenu.ShowImageMargin = false;
            this._toolsMenu.Size = new System.Drawing.Size(173, 182);
            this._toolsMenu.VisibleChanged += new System.EventHandler(this._toolsMenu_VisibleChanged);
            // 
            // _newTabMenuItem
            // 
            this._newTabMenuItem.Name = "_newTabMenuItem";
            this._newTabMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this._newTabMenuItem.Size = new System.Drawing.Size(172, 22);
            this._newTabMenuItem.Text = "New tab";
            this._newTabMenuItem.Click += new System.EventHandler(this._newTabMenuItem_Click);
            // 
            // _newWindowMenuItem
            // 
            this._newWindowMenuItem.Name = "_newWindowMenuItem";
            this._newWindowMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this._newWindowMenuItem.Size = new System.Drawing.Size(172, 22);
            this._newWindowMenuItem.Text = "New window";
            this._newWindowMenuItem.Click += new System.EventHandler(this._newWindowMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(169, 6);
            // 
            // _historyToolStripMenuItem
            // 
            this._historyToolStripMenuItem.Name = "_historyToolStripMenuItem";
            this._historyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
            this._historyToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this._historyToolStripMenuItem.Text = "History";
            this._historyToolStripMenuItem.Click += new System.EventHandler(this._historyToolStripMenuItem_Click);
            // 
            // _toolsMenuSeparator1
            // 
            this._toolsMenuSeparator1.Name = "_toolsMenuSeparator1";
            this._toolsMenuSeparator1.Size = new System.Drawing.Size(169, 6);
            // 
            // _settingsMenuItem
            // 
            this._settingsMenuItem.Name = "_settingsMenuItem";
            this._settingsMenuItem.Size = new System.Drawing.Size(172, 22);
            this._settingsMenuItem.Text = "Settings";
            this._settingsMenuItem.Click += new System.EventHandler(this._settingsMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(169, 6);
            // 
            // _updatesMenuItem
            // 
            this._updatesMenuItem.Name = "_updatesMenuItem";
            this._updatesMenuItem.Size = new System.Drawing.Size(172, 22);
            this._updatesMenuItem.Text = "Check for update";
            this._updatesMenuItem.Visible = false;
            this._updatesMenuItem.Click += new System.EventHandler(this._updatesMenuItem_Click);
            // 
            // _toolsMenuSeparator2
            // 
            this._toolsMenuSeparator2.Name = "_toolsMenuSeparator2";
            this._toolsMenuSeparator2.Size = new System.Drawing.Size(169, 6);
            this._toolsMenuSeparator2.Visible = false;
            // 
            // _aboutMenuItem
            // 
            this._aboutMenuItem.Name = "_aboutMenuItem";
            this._aboutMenuItem.Size = new System.Drawing.Size(172, 22);
            this._aboutMenuItem.Text = "About...";
            this._aboutMenuItem.Click += new System.EventHandler(this._aboutMenuItem_Click);
            // 
            // _exitMenuItem
            // 
            this._exitMenuItem.Name = "_exitMenuItem";
            this._exitMenuItem.Size = new System.Drawing.Size(172, 22);
            this._exitMenuItem.Text = "Exit";
            this._exitMenuItem.Click += new System.EventHandler(this._exitMenuItem_Click);
            // 
            // _bookmarksMenu
            // 
            this._bookmarksMenu.BackColor = System.Drawing.Color.White;
            this._bookmarksMenu.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._bookmarksMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._bookmarksManagerMenuItem2,
            this.bookmarkThisSiteToolStripMenuItem});
            this._bookmarksMenu.Name = "_bookmarksMenu";
            this._bookmarksMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this._bookmarksMenu.Size = new System.Drawing.Size(278, 48);
            this._bookmarksMenu.VisibleChanged += new System.EventHandler(this._bookmarksMenu_VisibleChanged);
            // 
            // _bookmarksManagerMenuItem2
            // 
            this._bookmarksManagerMenuItem2.Name = "_bookmarksManagerMenuItem2";
            this._bookmarksManagerMenuItem2.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.O)));
            this._bookmarksManagerMenuItem2.Size = new System.Drawing.Size(277, 22);
            this._bookmarksManagerMenuItem2.Text = "Bookmarks manager";
            this._bookmarksManagerMenuItem2.Click += new System.EventHandler(this._bookmarksManagerMenuItem2_Click);
            // 
            // bookmarkThisSiteToolStripMenuItem
            // 
            this.bookmarkThisSiteToolStripMenuItem.Name = "bookmarkThisSiteToolStripMenuItem";
            this.bookmarkThisSiteToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.bookmarkThisSiteToolStripMenuItem.Size = new System.Drawing.Size(277, 22);
            this.bookmarkThisSiteToolStripMenuItem.Text = "Bookmark this server";
            this.bookmarkThisSiteToolStripMenuItem.Click += new System.EventHandler(this._bookmarkMenuItem_Click);
            // 
            // _connectionContainerPanel
            // 
            this._connectionContainerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._connectionContainerPanel.BackColor = System.Drawing.Color.White;
            this._connectionContainerPanel.Location = new System.Drawing.Point(0, 40);
            this._connectionContainerPanel.Name = "_connectionContainerPanel";
            this._connectionContainerPanel.Size = new System.Drawing.Size(622, 395);
            this._connectionContainerPanel.TabIndex = 6;
            // 
            // _toolbarBorder
            // 
            this._toolbarBorder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._toolbarBorder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(220)))), ((int)(((byte)(224)))));
            this._toolbarBorder.Location = new System.Drawing.Point(0, 40);
            this._toolbarBorder.Name = "_toolbarBorder";
            this._toolbarBorder.Size = new System.Drawing.Size(622, 1);
            this._toolbarBorder.TabIndex = 9;
            // 
            // _omniBarPanel
            // 
            this._omniBarPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._omniBarPanel.BackColor = System.Drawing.Color.White;
            this._omniBarPanel.ForeColor = System.Drawing.Color.Silver;
            this._omniBarPanel.Location = new System.Drawing.Point(6, 30);
            this._omniBarPanel.Name = "_omniBarPanel";
            this._omniBarPanel.Size = new System.Drawing.Size(546, 72);
            this._omniBarPanel.TabIndex = 7;
            this._omniBarPanel.Visible = false;
            // 
            // _omniBarBorder
            // 
            this._omniBarBorder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._omniBarBorder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(233)))));
            this._omniBarBorder.ForeColor = System.Drawing.Color.Silver;
            this._omniBarBorder.Location = new System.Drawing.Point(5, 29);
            this._omniBarBorder.Name = "_omniBarBorder";
            this._omniBarBorder.Size = new System.Drawing.Size(548, 74);
            this._omniBarBorder.TabIndex = 8;
            this._omniBarBorder.Visible = false;
            // 
            // toolbarBackground
            // 
            this.toolbarBackground.BackColor = System.Drawing.Color.White;
            this.toolbarBackground.Controls.Add(this._urlPanelContainer);
            this.toolbarBackground.Controls.Add(this.urlTextBox);
            this.toolbarBackground.Controls.Add(this.urlBackground);
            this.toolbarBackground.Controls.Add(this._toolsButton);
            this.toolbarBackground.Controls.Add(this._bookmarksButton);
            this.toolbarBackground.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolbarBackground.Location = new System.Drawing.Point(0, 0);
            this.toolbarBackground.Name = "toolbarBackground";
            this.toolbarBackground.Size = new System.Drawing.Size(622, 40);
            this.toolbarBackground.TabIndex = 5;
            this.toolbarBackground.Click += new System.EventHandler(this.toolbarBackground_Click);
            // 
            // _urlPanelContainer
            // 
            this._urlPanelContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._urlPanelContainer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(243)))), ((int)(((byte)(244)))));
            this._urlPanelContainer.Location = new System.Drawing.Point(41, 9);
            this._urlPanelContainer.Name = "_urlPanelContainer";
            this._urlPanelContainer.Size = new System.Drawing.Size(489, 20);
            this._urlPanelContainer.TabIndex = 6;
            // 
            // urlTextBox
            // 
            this.urlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.urlTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(243)))), ((int)(((byte)(244)))));
            this.urlTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.urlTextBox.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.urlTextBox.Location = new System.Drawing.Point(41, 10);
            this.urlTextBox.Margin = new System.Windows.Forms.Padding(9);
            this.urlTextBox.Name = "urlTextBox";
            this.urlTextBox.Size = new System.Drawing.Size(490, 20);
            this.urlTextBox.TabIndex = 7;
            this.urlTextBox.WordWrap = false;
            this.urlTextBox.TextChanged += new System.EventHandler(this.urlTextBox_TextChanged);
            this.urlTextBox.Enter += new System.EventHandler(this.urlTextBox_Enter);
            this.urlTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.urlTextBox_KeyDown);
            this.urlTextBox.Leave += new System.EventHandler(this.urlTextBox_Leave);
            // 
            // urlBackground
            // 
            this.urlBackground.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.urlBackground.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(243)))), ((int)(((byte)(244)))));
            this.urlBackground.Controls.Add(this._iconPictureBox);
            this.urlBackground.Controls.Add(this.urlBackgroundRight);
            this.urlBackground.Controls.Add(this.urlBackgroundLeft);
            this.urlBackground.ForeColor = System.Drawing.Color.Silver;
            this.urlBackground.Location = new System.Drawing.Point(5, 6);
            this.urlBackground.Name = "urlBackground";
            this.urlBackground.Size = new System.Drawing.Size(545, 28);
            this.urlBackground.TabIndex = 8;
            this.urlBackground.Resize += new System.EventHandler(this.urlBackground_Resize);
            // 
            // _iconPictureBox
            // 
            this._iconPictureBox.InitialImage = null;
            this._iconPictureBox.Location = new System.Drawing.Point(11, 6);
            this._iconPictureBox.Name = "_iconPictureBox";
            this._iconPictureBox.Size = new System.Drawing.Size(16, 16);
            this._iconPictureBox.TabIndex = 0;
            this._iconPictureBox.TabStop = false;
            // 
            // urlBackgroundRight
            // 
            this.urlBackgroundRight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.urlBackgroundRight.Image = global::EasyConnect.Properties.Resources.UrlBoxRight;
            this.urlBackgroundRight.Location = new System.Drawing.Point(533, 0);
            this.urlBackgroundRight.Name = "urlBackgroundRight";
            this.urlBackgroundRight.Size = new System.Drawing.Size(12, 28);
            this.urlBackgroundRight.TabIndex = 2;
            this.urlBackgroundRight.TabStop = false;
            // 
            // urlBackgroundLeft
            // 
            this.urlBackgroundLeft.Image = global::EasyConnect.Properties.Resources.UrlBoxLeft;
            this.urlBackgroundLeft.Location = new System.Drawing.Point(0, 0);
            this.urlBackgroundLeft.Name = "urlBackgroundLeft";
            this.urlBackgroundLeft.Size = new System.Drawing.Size(12, 28);
            this.urlBackgroundLeft.TabIndex = 1;
            this.urlBackgroundLeft.TabStop = false;
            // 
            // _toolsButton
            // 
            this._toolsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._toolsButton.BackColor = System.Drawing.Color.Transparent;
            this._toolsButton.Image = global::EasyConnect.Properties.Resources.ToolsActive;
            this._toolsButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._toolsButton.Location = new System.Drawing.Point(589, 6);
            this._toolsButton.Margin = new System.Windows.Forms.Padding(4, 4, 3, 3);
            this._toolsButton.Name = "_toolsButton";
            this._toolsButton.Size = new System.Drawing.Size(27, 27);
            this._toolsButton.TabIndex = 5;
            this._toolsButton.TabStop = false;
            this._toolsButton.Click += new System.EventHandler(this._toolsButton_Click);
            this._toolsButton.MouseEnter += new System.EventHandler(this._toolsButton_MouseEnter);
            this._toolsButton.MouseLeave += new System.EventHandler(this._toolsButton_MouseLeave);
            // 
            // _bookmarksButton
            // 
            this._bookmarksButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._bookmarksButton.BackColor = System.Drawing.Color.Transparent;
            this._bookmarksButton.Image = global::EasyConnect.Properties.Resources.BookmarksActive;
            this._bookmarksButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._bookmarksButton.Location = new System.Drawing.Point(558, 6);
            this._bookmarksButton.Margin = new System.Windows.Forms.Padding(4, 4, 3, 3);
            this._bookmarksButton.Name = "_bookmarksButton";
            this._bookmarksButton.Size = new System.Drawing.Size(27, 27);
            this._bookmarksButton.TabIndex = 4;
            this._bookmarksButton.TabStop = false;
            this._bookmarksButton.Click += new System.EventHandler(this._bookmarksButton_Click);
            this._bookmarksButton.MouseEnter += new System.EventHandler(this._bookmarksButton_MouseEnter);
            this._bookmarksButton.MouseLeave += new System.EventHandler(this._bookmarksButton_MouseLeave);
            // 
            // ConnectionWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(622, 435);
            this.Controls.Add(this._omniBarPanel);
            this.Controls.Add(this._toolbarBorder);
            this.Controls.Add(this._omniBarBorder);
            this.Controls.Add(this.toolbarBackground);
            this.Controls.Add(this._connectionContainerPanel);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ConnectionWindow";
            this.Text = "New Tab";
            this.Shown += new System.EventHandler(this.ConnectionWindow_Shown);
            this._toolsMenu.ResumeLayout(false);
            this._bookmarksMenu.ResumeLayout(false);
            this.toolbarBackground.ResumeLayout(false);
            this.toolbarBackground.PerformLayout();
            this.urlBackground.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._iconPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.urlBackgroundRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.urlBackgroundLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._toolsButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._bookmarksButton)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel toolbarBackground;
        private System.Windows.Forms.PictureBox _toolsButton;
        private System.Windows.Forms.PictureBox _bookmarksButton;
        private System.Windows.Forms.ContextMenuStrip _toolsMenu;
        private System.Windows.Forms.ToolStripMenuItem _newTabMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _newWindowMenuItem;
        private System.Windows.Forms.ToolStripSeparator _toolsMenuSeparator1;
        private System.Windows.Forms.ToolStripMenuItem _settingsMenuItem;
        private System.Windows.Forms.ToolStripSeparator _toolsMenuSeparator2;
        private System.Windows.Forms.ToolStripMenuItem _exitMenuItem;
        private System.Windows.Forms.ContextMenuStrip _bookmarksMenu;
        private System.Windows.Forms.ToolStripMenuItem _bookmarksManagerMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem bookmarkThisSiteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem _historyToolStripMenuItem;
        private System.Windows.Forms.Panel _connectionContainerPanel;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem _updatesMenuItem;
        private System.Windows.Forms.Panel _omniBarPanel;
        private System.Windows.Forms.Panel _omniBarBorder;
		private System.Windows.Forms.ToolStripMenuItem _aboutMenuItem;
        private System.Windows.Forms.Panel _urlPanelContainer;
        private System.Windows.Forms.TextBox urlTextBox;
        private System.Windows.Forms.Panel urlBackground;
        private System.Windows.Forms.PictureBox _iconPictureBox;
        private System.Windows.Forms.PictureBox urlBackgroundRight;
        private System.Windows.Forms.PictureBox urlBackgroundLeft;
        private System.Windows.Forms.Panel _toolbarBorder;
    }
}