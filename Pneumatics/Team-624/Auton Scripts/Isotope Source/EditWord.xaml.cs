
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
	/// Interaction logic for EditWord.xaml
	/// </summary>
	public partial class EditWord : Window
	{
		KeyWords _k;
		readonly OptionsWindow _ow;
		bool b, i;
		public ObservableCollection<Xceed.Wpf.Toolkit.ColorItem> ColorList;
		
		public EditWord(OptionsWindow ow, KeyWords k)
		{
			InitializeComponent();
			
			wordBox.Text=k.Word;
			ColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(k.color);
			
			createColors();
			ColorPicker.AvailableColors = ColorList;
			ColorPicker.StandardColors = ColorList;
			
			if(k.fontstyle == "bold")
			{
				bold.IsChecked=true;
				b = true;
				i = false;
			}
			else if(k.fontstyle == "bolditalic")
			{
				bold.IsChecked=true;
				italic.IsChecked=true;
				b = true;
				i = true;
			}
			else if(k.fontstyle == "italic")
			{
				italic.IsChecked = true;
				i = true;
				b = false;
			}
			else
			{
				bold.IsChecked=false;
				italic.IsChecked=false;
				b = false;
				i = false;
			}
			
			_k = k;
			
			_ow = ow;
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
				
				_k.Word= wordBox.Text;
				_k.color = ColorPicker.SelectedColorText.ToString();
				_k.fontstyle = stylestr;
				_ow.words.Insert(_k.Index, _k);
				_ow.words.RemoveAt(_k.Index+1);
				
				this.Close();
			}
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