/*
 * Created by SharpDevelop.
 * User: team624
 * Date: 7/23/2014
 * Time: 8:09 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Isotope
{
	/// <summary>
	/// Interaction logic for Help.xaml
	/// </summary>
	public partial class Help : Window
	{
		public Help()
		{
			InitializeComponent();
		}
		void button1_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}