
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace Isotope
{
	/// <summary>
	/// Interaction logic for NewCommand.xaml
	/// </summary>
	public partial class NewCommand : Window
	{
		readonly OptionsWindow _ow;
		public NewCommand(OptionsWindow ow)
		{
			InitializeComponent();
			_ow = ow;
			
			cmd_box.ItemsSource = _ow.commands;
			cmd_box.DisplayMemberPath = "Commnd";
			cmd_box.AllowDrop = true;
			cmd_box.IsReadOnly = true;
			
		}
		
		void save_command(object sender, RoutedEventArgs e)
		{
			if(namebox.Text != "" && numbox.Text != "" && descbox.Text != "")
			{
				bool b = _ow.checkID(Convert.ToInt32(numbox.Text));
				bool n = _ow.checkName(namebox.Text);
				if(b && n)
				{
					int i = _ow.Get_Command_Size();
					_ow.Add_Command(make_command(namebox.Text, Convert.ToInt32(numbox.Text), descbox.Text,i));
					this.Close();
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
		}
		
		void save_sub_command(object sender, RoutedEventArgs e)
		{
			if(namebox.Text != "" && numbox.Text != "" && descbox.Text != "" && cmd_box.SelectedIndex != -1)
			{
			int i = _ow.Get_Sub_Command_Size();
			_ow.Add_Sub_Command(make_sub_command(namebox.Text, Convert.ToInt32(numbox.Text), descbox.Text, i, cmd_box.SelectedIndex));
			this.Close();
			}
		}
		
		private Commands make_command(string comm, int id, string desc, int i)
		{
			Commands c = new Commands(){ Commnd=comm, Index=id, Description=desc, Ind_=i };
			return c;
		}
		private SubCommands make_sub_command(string comm, int id, string desc, int i, int cmd_id)
		{
			Commands c = _ow.commands[cmd_id];
			SubCommands sc = new SubCommands(){ Sub=comm, Row=id, sDescription=desc, Index=i, parentCommand=c};
			return sc;
		}
	}
}