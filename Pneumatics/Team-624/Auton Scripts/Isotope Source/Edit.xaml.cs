
using System;
using System.Windows;

namespace Isotope
{
	/// <summary>
	/// Interaction logic for Edit.xaml
	/// </summary>
	public partial class Edit : Window
	{
		Commands _c;
		SubCommands _sc;
		readonly OptionsWindow _ow;
		bool isCommand;
		string oldID, oldName;
		
		public Edit(OptionsWindow ow, Commands c)
		{
			InitializeComponent();
			
			namebox.Text=c.Commnd;
			numbox.Text=c.Index.ToString();
			descbox.Text=c.Description;
			
			oldID=c.Index.ToString();
			oldName=c.Commnd;
			_c = c;
			
			_ow = ow;
			
			isCommand = true;
		}
		
		public Edit(OptionsWindow ow, SubCommands sc)
		{
			InitializeComponent();
			
			namebox.Text=sc.Sub;
			numbox.Text=sc.Row.ToString();
			descbox.Text=sc.sDescription;
			
			_sc = sc;
			
			_ow = ow;
			
			cmd_box.ItemsSource = _ow.commands;
			cmd_box.AllowDrop = true;
			cmd_box.IsReadOnly = true;
			cmd_box.SelectedItem = sc.parentCommand;
			isCommand = false;
		}
		
		
		
		private void save_item(object sender, EventArgs e)
		{
			if(namebox.Text != "" && numbox.Text != "" && descbox.Text != "" )
			{
				if(isCommand)
				{
					bool b;
					if(oldID==numbox.Text.ToString())
					{
						b = true;
					}
					else
					{
						b = _ow.checkID(Convert.ToInt32(numbox.Text));
					}
					bool n = _ow.checkName(namebox.Text);
					if(b)
					{
						_c.Commnd = namebox.Text;
						_c.Index = Convert.ToInt32(numbox.Text);
						_c.Description = descbox.Text;
						_ow.commands.Insert(_c.Ind_, _c);
						_ow.commands.RemoveAt(_c.Ind_+1);
					}
					else if(!b)
					{
						System.Windows.Forms.MessageBox.Show("Error: The ID field must be unique. Please enter another value for index",
					                                     "Error",
					                                     System.Windows.Forms.MessageBoxButtons.OK,
					                                     System.Windows.Forms.MessageBoxIcon.Error);
					}
					else
					{
						System.Windows.Forms.MessageBox.Show("Error: The Name field must be unique. Please enter another value for command name",
					                                     "Error",
					                                     System.Windows.Forms.MessageBoxButtons.OK,
					                                     System.Windows.Forms.MessageBoxIcon.Error);
					}
				}
				else
				{
					if(cmd_box.SelectedIndex != -1)
					{
						_sc.Sub = namebox.Text;
						_sc.Row = Convert.ToInt32(numbox.Text);
						_sc.sDescription = descbox.Text;
						_sc.parentCommand = _ow.commands[cmd_box.SelectedIndex];
						_ow.subs.RemoveAt(_sc.Index);
						_ow.subs.Insert(_sc.Index, _sc);
					}
				}
			
				if(oldName != namebox.Text)
				{
					_ow.refreshSubCommands(oldName, namebox.Text);
				}
				this.Close();
			}
		}
	}
}