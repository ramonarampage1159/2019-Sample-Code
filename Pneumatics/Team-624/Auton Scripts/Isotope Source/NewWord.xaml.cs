
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace Isotope
{
	/// <summary>
	/// Interaction logic for NewWord.xaml
	/// </summary>
	public partial class NewWord : Window
	{
		
		public ObservableCollection<Xceed.Wpf.Toolkit.ColorItem> ColorList;
		readonly OptionsWindow _ow;
		bool b = false, i = false;
		
		public NewWord(OptionsWindow ow)
		{
			InitializeComponent();
			
			_ow = ow;
			createColors();
			ColorPicker.AvailableColors = ColorList;
			ColorPicker.StandardColors = ColorList;
		}
		
		private void createColors()
		{
			ColorList = new ObservableCollection<Xceed.Wpf.Toolkit.ColorItem>();
			ColorList.Add(new ColorItem(Colors.Black, "Black"));
			ColorList.Add(new ColorItem(Colors.Blue, "Blue"));
			ColorList.Add(new ColorItem(Colors.Brown, "Brown"));
			ColorList.Add(new ColorItem(Colors.DarkGreen, "Dark Green"));
			ColorList.Add(new ColorItem(Colors.DimGray, "Dark Gray"));
			ColorList.Add(new ColorItem(Colors.Gray, "Gray"));
			ColorList.Add(new ColorItem(Colors.Green, "Green"));
			ColorList.Add(new ColorItem(Colors.HotPink, "Pink"));
			ColorList.Add(new ColorItem(Colors.LightBlue, "Light Blue"));
			ColorList.Add(new ColorItem(Colors.LightGray, "Light Gray"));
			ColorList.Add(new ColorItem(Colors.Maroon, "Maroon"));
			ColorList.Add(new ColorItem(Colors.Navy, "Navy Blue"));
			ColorList.Add(new ColorItem(Colors.Orange, "Orange"));
			ColorList.Add(new ColorItem(Colors.Purple, "Purple"));
			ColorList.Add(new ColorItem(Colors.Red, "Red"));
			ColorList.Add(new ColorItem(Colors.Teal, "Teal"));
			ColorList.Add(new ColorItem(Colors.Violet, "Violet"));
			ColorList.Add(new ColorItem(Colors.Yellow, "Yellow"));
			
		}
		
		private void save_word(object sender, EventArgs e)
		{
			if(wordBox.Text != "")
			{
				String stylestr;
				if(b & i)
				{
					stylestr="bolditalic";
				}
				else if(b & !i)
				{
					stylestr="bold";
				}
				else if(!b & i)
				{
					stylestr="italic";
				}
				else
				{
					stylestr="normal";
				}
				_ow.Add_Key_Word(make_key_word(wordBox.Text, ColorPicker.SelectedColorText.ToString(), false, stylestr));
				this.Close();
			}
		}
		
		public KeyWords make_key_word(string name, String c, bool isPerm, String sty)
		{
			int i = _ow.Get_Key_Word_Size();
			KeyWords kw = new KeyWords(){ Word=name, color=c, Permanent=isPerm, Index=i, fontstyle=sty};
			return kw;
		}
		
		private void bold_c(object sender, EventArgs e)
		{
			b=true;
		}
		private void italic_c(object sender, EventArgs e)
		{
			i=true;
		}
		
		private void bold_uc(object sender, EventArgs e)
		{
			b=false;
		}
		private void italic_uc(object sender, EventArgs e)
		{
			i=false;
		}
		
		
	}
}