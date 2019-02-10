
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.Win32;

namespace Isotope
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window
	{
		OptionsWindow _oww = new OptionsWindow();
		
		private List<TabItem> tabs;
		private List<String> names;
		private List<String> contents;
		private List<ICSharpCode.AvalonEdit.TextEditor> edits;
		private List<bool> changed;
		
		private List<TabSaver> tabsaver;
		
		System.Windows.Forms.OpenFileDialog ofd;
		System.Windows.Forms.SaveFileDialog sfd;
		System.Windows.Forms.SaveFileDialog efd;
		private static Regex digitsOnly = new Regex(@"[^\d]");
		
		
		//private static Regex stringsym = new Regex(Regex.);
		public static RoutedCommand _new = new RoutedCommand();
		public static RoutedCommand _open = new RoutedCommand();
		public static RoutedCommand _save = new RoutedCommand();
		public static RoutedCommand _all = new RoutedCommand();
		public static RoutedCommand _export = new RoutedCommand();
		public static RoutedCommand _help = new RoutedCommand();
		
		XshdSyntaxDefinition xshd;
		
		private int _index;
		
		public Window1(String caller)
		{
			InitializeComponent();
			
			tabs = new List<TabItem>();
			names = new List<String>();
			contents = new List<String>();
			edits = new List<ICSharpCode.AvalonEdit.TextEditor>();
			changed = new List<bool>();
			tabsaver = new List<TabSaver>();
			
			tabsaver = LoadTabs();
			
			int realTabs = 0;
			if(tabsaver.Count > 0)
			{
				for(int i = 0; i<tabsaver.Count; i++)
				{
					if(tabsaver[i].filename != "New File.txt" && File.Exists(Path.GetFileName(tabsaver[i].filename)))
					{
					names.Add(Path.GetFileName(tabsaver[i].filename));
					load(tabsaver[i].filename);
					addTab(Path.GetFileName(tabsaver[i].filename), false);
					realTabs++;
					}
				}
			}
			
			
			ofd = new System.Windows.Forms.OpenFileDialog();
			setOpenFileDialog();
			
			sfd = new System.Windows.Forms.SaveFileDialog();
			setSaveFileDialog();
			
			efd = new System.Windows.Forms.SaveFileDialog();
			setExportFileDialog();
			
			CommandBinding cbNew = new CommandBinding(_new, NewExecuted, NewCanExecute);
			CommandBinding cbOpen = new CommandBinding(_open, OpenExecuted, OpenCanExecute);
			CommandBinding cbSave = new CommandBinding(_save, SaveExecuted, SaveCanExecute);
			CommandBinding cbAll = new CommandBinding(_all, AllExecuted, AllCanExecute);
			CommandBinding cbHelp = new CommandBinding(_help, HelpExecuted, HelpCanExecute);
			CommandBinding cbExport = new CommandBinding(_export, ExportExecuted, ExportCanExecute);
			this.CommandBindings.Add(cbNew);
			this.CommandBindings.Add(cbOpen);
			this.CommandBindings.Add(cbSave);
			this.CommandBindings.Add(cbAll);
			this.CommandBindings.Add(cbHelp);
			this.CommandBindings.Add(cbExport);
			
			KeyGesture kgNew = new KeyGesture(Key.N, ModifierKeys.Control);
    		InputBinding ibNew = new InputBinding(_new, kgNew);
    		KeyGesture kgOpen = new KeyGesture(Key.O, ModifierKeys.Control);
    		InputBinding ibOpen = new InputBinding(_open, kgOpen);
    		KeyGesture kgSave = new KeyGesture(Key.S, ModifierKeys.Control);
    		InputBinding ibSave = new InputBinding(_save, kgSave);
    		KeyGesture kgAll = new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift);
    		InputBinding ibAll = new InputBinding(_all, kgAll);
    		KeyGesture kgHelp = new KeyGesture(Key.F1);
    		InputBinding ibHelp = new InputBinding(_help, kgHelp);
    		KeyGesture kgExport = new KeyGesture(Key.E, ModifierKeys.Control | ModifierKeys.Shift);
    		InputBinding ibExport = new InputBinding(_export, kgExport);
    		this.InputBindings.Add(ibNew);
    		this.InputBindings.Add(ibOpen);
    		this.InputBindings.Add(ibSave);
    		this.InputBindings.Add(ibAll);
    		this.InputBindings.Add(ibHelp);
			this.InputBindings.Add(ibExport);
			
			if(caller!="")
			{
				names.Add(Path.GetFileName(caller));
				tabsaver.Add(makeSaver(Path.GetFileName(caller)));
				load(caller);
				addTab(Path.GetFileName(caller), false);
			}
			else{
				
				if(realTabs <= 0)
				{
					New();
				}
			}
		}
		
		#region init
		
		private void setOpenFileDialog()
		{
			// Set the file dialog to filter for text and csv files.
    		ofd.Filter = "Text Files (*.txt)|*.txt|" + "CSV Files (*.csv)|*.csv|" + "All files (*.*)|*.*";

   	 		//  Allow the user to select multiple files.
    		ofd.Multiselect = true;

    		ofd.Title = "Open Autonomous";
		}
		
		private void setSaveFileDialog()
		{
			// Set the file dialog to filter for text files.
    		sfd.Filter = "Text Files (*.txt)|*.txt|" + "All files (*.*)|*.*";

    		sfd.Title = "Save Autonomous";
		}
		
		private void setExportFileDialog()
		{
			// Set the file dialog to filter for text files.
    		efd.Filter = "CSV Files (*.csv)|*.csv";

    		efd.Title = "Export Autonomous to CSV";
		}
		
		
		
		#endregion
		
		#region file_controls
		
		private void open_click(object sender, EventArgs e)
		{
			Open();
		}
		
		private void save_click(object sender, EventArgs e)
		{
			Save();
   		}
		
		private void save_as_click(object sender, EventArgs e)
		{
			SaveAs();
   		}
		
		private void save_all_click(object sender, EventArgs e)
		{
			SaveAll();
   		}
		
		private void new_click(object sender, EventArgs e)
		{
			New();
   		}
		
		private void export_click(object sender, EventArgs e)
		{
			Export();
   		}
		
		#endregion
		
		#region low_level
		
		private void Open()
		{
				
    	DialogResult dr = ofd.ShowDialog();
   		if (dr == System.Windows.Forms.DialogResult.OK)
    	{
        // Read the files
        foreach (String file in ofd.FileNames) 
        {
            // Create text.
            try
            {
            	if((Path.GetExtension(file)) == ".txt")
            	{
            		names.Add(file);
            		tabsaver.Add(makeSaver(file));
            		load(file);
            		addTab(Path.GetFileName(file), false);
            	}
            	else if((Path.GetExtension(file)) == ".csv")
            	{
            		Import_CSV(file);
            	}
            	else
            	{	//notify user of non native format
            		System.Windows.Forms.DialogResult dir = System.Windows.Forms.MessageBox.Show("WARNING: A file that you are trying to import is not of native format. The editor may fail if it tries to open it." ,
				                						"Non-Native File Extension Detected", 
				               							System.Windows.Forms.MessageBoxButtons.OKCancel, 
				                						System.Windows.Forms.MessageBoxIcon.Warning);
            		
            		if(dir == System.Windows.Forms.DialogResult.OK) //proceed if accepted
            		{
            			names.Add(file);
            			tabsaver.Add(makeSaver(file));
            			load(file);
            			addTab(Path.GetFileName(file), false);
            		}
            	}
            }
            catch (SecurityException ex)
            {
                // The user lacks appropriate permissions to read files, discover paths, etc.
                System.Windows.Forms.MessageBox.Show("Security error. Please contact your administrator for details.\n\n" +
                    "Error message: " + ex.Message + "\n\n" +
                    "Details (send to Support):\n\n" + ex.StackTrace);
            }
            catch (Exception ex)
            {
                // Could not load the file - probably related to Windows file system permissions.
                System.Windows.Forms.MessageBox.Show("Cannot load file: " + file.Substring(file.LastIndexOf('\\'))
                    + ". You may not have permission to read the file, or " +
                    "it may be corrupt.\n\nReported error: " + ex.Message,
                   "Open File Error",
								      	System.Windows.Forms.MessageBoxButtons.OK,
								      	System.Windows.Forms.MessageBoxIcon.Error);
            } 
        }
    }
		}
		
		private void Save()
		{
			getIndex();
			if(names[_index]=="New File.txt")
			{
				DialogResult dr = sfd.ShowDialog();
   				if (dr == System.Windows.Forms.DialogResult.OK)
   				{	
					try
					{
						edits[_index].Save(sfd.FileName);
						names[_index]=sfd.FileName;
					}
					catch (Exception ex)
					{
						System.Windows.Forms.MessageBox.Show(ex.Message);
					}
					
					tabsaver.Insert(_index, makeSaver(names[_index]));
					tabsaver.RemoveAt(_index+1);
					tab_update(_index,false);
   				}
			}
			else
			{
				try
				{
					edits[_index].Save(names[_index]);
				}
				catch (Exception ex)
				{
					System.Windows.Forms.MessageBox.Show(ex.Message);
				}
			}
   			
		}
		
		private bool saveWithFeedback()
		{
			getIndex();
			if(names[_index]=="New File.txt")
			{
				DialogResult dr = sfd.ShowDialog();
   				if (dr == System.Windows.Forms.DialogResult.OK)
   				{	
					try
					{
						edits[_index].Save(sfd.FileName);
						names[_index]=sfd.FileName;
					}
					catch (Exception ex)
					{
						System.Windows.Forms.MessageBox.Show(ex.Message);
					}
					
					tabsaver.Insert(_index, makeSaver(names[_index]));
					tabsaver.RemoveAt(_index+1);
					tab_update(_index,false);
					
					return true;
   				}
   				
   				return false;
			}
			else
			{
				try
				{
					edits[_index].Save(names[_index]);
				}
				catch (Exception ex)
				{
					System.Windows.Forms.MessageBox.Show(ex.Message);
				}
				
				return true;
			}
   			
		}
		
		private void SaveAs()
		{
			getIndex();
			DialogResult dr = sfd.ShowDialog();
   			if (dr == System.Windows.Forms.DialogResult.OK)
   			{
				try
				{
					edits[_index].Save(sfd.FileName);
					names[_index]=sfd.FileName;
				}
				catch (Exception ex)
				{
					System.Windows.Forms.MessageBox.Show(ex.Message);
				}
				
				tabsaver.Insert(_index, makeSaver(names[_index]));
				tabsaver.RemoveAt(_index+1);
				tab_update(_index, false);
   			}
		}
		
		private void SaveAll()
		{
			for(int i=0;i<names.Count;i++)
			{
				
				if(names[i]=="New File.txt")
				{
					DialogResult dr = sfd.ShowDialog();
   					if (dr == System.Windows.Forms.DialogResult.OK)
   					{
						try
						{
							edits[i].Save(sfd.FileName);
							names[i]=sfd.FileName;
						}
						catch (Exception ex)
						{
							System.Windows.Forms.MessageBox.Show(ex.Message);
						}
						
						tabsaver.Insert(i,makeSaver(names[i]));
						tabsaver.RemoveAt(i+1);
						tab_update(i,false);
   					}
				}
				else
				{
					if(!changed[i])
					{
						try
						{
							edits[i].Save(names[i]);
						}
						catch (Exception ex)
						{
							System.Windows.Forms.MessageBox.Show(ex.Message);
						}
					}
				}
				
				
			}
		}
		
		private void load(string caller)
		{
			contents.Add(caller);
		}
	
		private void New()
		{
			names.Add("New File.txt");
			contents.Add("new file");
			changed.Add(true);
			tabsaver.Add(makeSaver("New File.txt"));
			addTab("New File.txt",true);
		}
		
		public static string Convert_Str(string str)
		{
    		return digitsOnly.Replace(str, "");
		}
		
		private void file_drop(object sender, System.Windows.DragEventArgs e)
		{
			if(e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop, true);
				
				foreach (string file in files)
				{
					dropOpen(file);
				}
			}
		}
		
		private void dropOpen(string file)
		{
			// Create text.
           try
           {
            	if((Path.GetExtension(file)) == ".txt")
            	{
            		names.Add(file);
            		tabsaver.Add(makeSaver(file));
            		load(file);
            		addTab(Path.GetFileName(file), false);
            	}
            	else if((Path.GetExtension(file)) == ".csv")
            	{
            		Import_CSV(file);
            	}
            	else
            	{	//notify user of non native format
            		System.Windows.Forms.DialogResult dr = System.Windows.Forms.MessageBox.Show("WARNING: A file that you are trying to import is not of native format. The editor may fail if it tries to open it." ,
				                						"Non-Native File Extension Detected", 
				               							System.Windows.Forms.MessageBoxButtons.OKCancel, 
				                						System.Windows.Forms.MessageBoxIcon.Warning);
            		
            		if(dr == System.Windows.Forms.DialogResult.OK) //proceed if accepted
            		{
            			names.Add(file);
            			tabsaver.Add(makeSaver(file));
            			load(file);
            			addTab(Path.GetFileName(file), false);
            		}
            	}
           	}
            catch (SecurityException ex)
            {
                // The user lacks appropriate permissions to read files, discover paths, etc.
                System.Windows.Forms.MessageBox.Show("Security error. Please contact your administrator for details.\n\n" +
                    "Error message: " + ex.Message + "\n\n" +
                    "Details (send to Support):\n\n" + ex.StackTrace);
            }
            catch (Exception ex)
            {
            	if(Path.GetExtension(file) == ".txt")
            	{
                // Could not load the file - probably related to Windows file system permissions.
                System.Windows.Forms.MessageBox.Show("Cannot load file: " + file.Substring(file.LastIndexOf('\\'))
                    + ". You may not have permission to read the file, or " +
                    "it may be corrupt.\n\nReported error: " + ex.Message,
                   						"Open File Error",
								      	System.Windows.Forms.MessageBoxButtons.OK,
								      	System.Windows.Forms.MessageBoxIcon.Error);
            	}
            	else if(Path.GetExtension(file) == ".csv")
            	{
            		// Could not load csv. Probably corrupt
                	System.Windows.Forms.MessageBox.Show("CSV Load error: " + ex.Message, 
								      "CSV Import Error",
								      System.Windows.Forms.MessageBoxButtons.OK,
								      System.Windows.Forms.MessageBoxIcon.Error);
            	} 
            } 
			
		}
		
		
		#endregion
		
		#region csv
		
		private void Import_CSV(string file)
		{
			OptionsWindow _ow = new OptionsWindow();
			//getIndex();
			
			char delimiter = ','; //comma delimited csv
			int rows = getBiggestSubID()+1; //rows in command set
			string[][] csv_in = new string[rows+1][]; //jagged array of csv input
			List<string> text = new List<string>(); //list of text that will be put into the textbox
			
			var reader = new StreamReader(File.OpenRead(file));
			int r = 0;
        
			if(rows > 1)
			{
        	while (r<=rows)
        	{
            	var line = reader.ReadLine();
            	var values = line.Split(delimiter);
            	
            	csv_in[r] = new string[values.GetLength(0)];
            	csv_in[r] = values;
            	
            	r++;
        	}/*
        	foreach(var row in csv_in)
        	{
        		foreach(var rowz in row)
        		{
        			text.Add(rowz);
        		}
        	}
        	*/
        	for(int i=0; i<csv_in[0].Length; i++)
			{
				string c = "";
				List<SubCommands> lsc = new List<SubCommands>();
				
				for(int rr=0; rr<=rows; rr++)
				{
					Commands comms;
					
					if(rr==0)
					{
						for(int cc=0; cc<_ow.commands.Count; cc++)
						{
							
							int con = 0;
							
							con = Convert.ToInt32(csv_in[rr][i]);
							
							if(con == _ow.commands[cc].Index)
							{
								comms = _ow.commands[cc];
								for(int k=0;k<_ow.subs.Count;k++)
								{
									if(_ow.subs[k].parentCommand.Commnd == comms.Commnd)
									{
										lsc.Add(_ow.subs[k]);
									}
								}
								c += _ow.commands[cc].Commnd;
								c+= " ";
								break;
							}
							if(con != _ow.commands[cc].Index && cc == _ow.commands.Count-1)
							{
								//csv is corrupt. no such command exists. output bad command #
								System.Windows.Forms.MessageBox.Show("CSV Import Error: .CSV file contains invalid command id",
				                						"File Error", 
				               							System.Windows.Forms.MessageBoxButtons.OK, 
				                						System.Windows.Forms.MessageBoxIcon.Error);
								
								break;
							}
						}
					}
					else
					{
						
						if(lsc.Count > 0)
						{
							for(int cc=0; cc<lsc.Count; cc++)
							{
								if(rr == lsc[cc].Row+1)
								{
									c += lsc[cc].Sub + "=";
									c += csv_in[rr][i];
									c+= " ";
									break;
								}
							}
						}
						
					}
					
				}
			
				
				text.Add(c);
			}
			
			SaveRawTextAndOpen(file, text);
			}
			else
			{
				//no command set loaded
				System.Windows.Forms.MessageBox.Show("Error: No command set loaded for .CSV import",
				                						"Settings Error", 
				               							System.Windows.Forms.MessageBoxButtons.OK, 
				                						System.Windows.Forms.MessageBoxIcon.Error);
			}
		}
		
		private void SaveRawTextAndOpen(string file, List<string> txt)
		{
			string name = Path.ChangeExtension(file, ".txt");
			string contents = string.Join("\n", txt.ToArray());
			
			File.WriteAllText(name, contents);
			
			names.Add(name);
            tabsaver.Add(makeSaver(name));
            load(name);
            addTab(Path.GetFileName(name), false);
		}
		
		private void Export()
		{	
				OptionsWindow _ow = new OptionsWindow();
				getIndex();
			
				List<int> lines = getMeaningfulLines();
				List<int> lins = getMeaninglessLines();//index all lines that do not start with comments
				int totalLines = edits[_index].LineCount; //count total lines to read
				int rows = getBiggestSubID()+1; //count subcommand rows //+1 for command id
				int inc = 0, ink = 0; //iteration variables for meaningful lines array
				string delimiter = ","; //comma delimited csv
				bool safe = true; //stays true if no errors
				
				//List<string> label = new List<string>();
				//List<int> label_ids = new List<int>();
				
				Dictionary<string, int> labels = new Dictionary<string, int>();
				
				//for(int fp=0;fp<lins.Count; fp++)
				//{
				foreach(int l in lins)
				{
					//int mLines = 0;
					
						int start = edits[_index].Document.GetLineByNumber(l).Offset;
						int leng = edits[_index].Document.GetLineByNumber(l).EndOffset;
						//string line = "";
						if(edits[_index].Document.GetCharAt(leng-1) == ':')
						{
							string hold = "";
							for(int mg = start; mg<leng-1; mg++)
							{
								hold+=edits[_index].Document.GetCharAt(mg);
							}
							labels.Add(hold, l);
							//label.Add(hold);
							//label_ids.Add(fp+1);
						}
						ink++;
					
				}
				
				//jagged array for output
				string[][] csv_out = new string[rows+1][]; //+1 for command id //columns
				for(int r=0;r<=rows;r++)
				{
					csv_out[r] = new string[lines.Count]; //create rows of csv
				}
				//for(int i = 0; i < lines.Count; i++) // add commands
				//{
				foreach(int ml in lines)
				{
					Console.WriteLine(ml.ToString());
					   	//get the line offset (the document is a 0 indexed char array)
					   	int off = edits[_index].Document.GetLineByNumber(ml).Offset;
					   	int leng = edits[_index].Document.GetLineByNumber(ml).EndOffset;
					   	string line = "";
					   	
					   	//holds words and numbers for parsing
					   	List<string> lineVals = new List<string>();
					   	string holder = "";
					   	
					   	//create the line
					   	for(int u = off; u < leng; u++)
					   	{
					   		line+=edits[_index].Document.GetCharAt(u);
					   	}
					   	
					   	//find text and numbers and store into list for parsing
					   	for(int ch = 0; ch < line.Length; ch++)
					   	{
					   		if(line[ch] != ' ' && line[ch] != '=') //if valid string
					   		{
					   			holder+=line[ch]; //add to the string
					   		}
					   		if((line[ch] == '=' || line[ch] ==' ') || ch == (line.Length-1)) //when string is no longer valid
					   		{
					   			if(holder!="")
					   			{
					   				lineVals.Add(holder); //if the holder exists then add the word
					   				holder = "";// reset holder
					   			}
					   		}
					   	}
					   	
					   	//find which command is used
					   	for(int ii=0; ii<(_ow.commands.Count); ii++)
						{
							//if(line.StartsWith(_ow.commands[ii].Commnd))
							
							if(lineVals.Count>0)
							{
							if(lineVals[0].Equals(_ow.commands[ii].Commnd))
							{
								//put all associated sub commands into a list
								List<SubCommands> scs = new List<SubCommands>();
								for(int cs = 0; cs < _ow.subs.Count; cs++)
								{	
									if(_ow.subs[cs].parentCommand.Commnd == _ow.commands[ii].Commnd)
									{
										scs.Add(_ow.subs[cs]);
									}
								}
								
								//output command index to csv jagged array
								csv_out[0][inc] = _ow.commands[ii].Index.ToString();
								
								//fill out each row's data for the command
								for(int r = 0; r<rows; r++)
								{
									if(scs.Count > 0)
									{
									for(int h = 0; h < scs.Count; h++)
									{
										//find the subcommand for the current row
										
										if(scs[h].Row == r)
										{
											int fid = 0; //id of number that sub command equals
											
											//find sub commands in parsing list
											for(int id = 0; id<lineVals.Count; id++)
											{	
												if(lineVals[id].Equals(scs[h].Sub))
												{
													fid = id + 1; //set id to the next list item
													break;
												}
											}
											
											if(fid == lineVals.Count) //open equal sign
											{
												System.Windows.Forms.MessageBox.Show("Error: Equal signs must always be followed by numerical values. On line number: " + ml 
								                                     + ", Faulty Text: " + line,
				                						"Syntax Error", 
				               							System.Windows.Forms.MessageBoxButtons.OK, 
				                						System.Windows.Forms.MessageBoxIcon.Error);
													safe = false;
												break;
											}
											
											//if fid points to the command
											if(fid!=0)
											{
												double d = 0;
												bool flag = true;
												try
												{
													foreach(var scd in _ow.oc_alias)
													{
														if(lineVals[fid] == scd.Alias)
														{
															d = Convert.ToDouble(scd.Number);
															flag = false;
															break;
														}
													}
													foreach(KeyValuePair<string,int> l in labels)
													{
														if(lineVals[fid] == l.Key)
														{	
															d = Convert.ToDouble(l.Value);
															flag = false;
															break;
														}
													}
													if(flag)
													{
														d = Convert.ToDouble(lineVals[fid]);
													}
													
													if(d!=0)
													{
														csv_out[r+1][inc] = d.ToString(); //output value to array
													}
													else //d is null or 0
													{
														csv_out[r+1][inc] = "0";
													}
												}
												catch(FormatException)//not a number so open equal sign
												{
													System.Windows.Forms.MessageBox.Show("Error: Equal signs must always be followed by numerical values. On line number: " + ml 
								                                     + ", Faulty Text: " + line,
				                						"Syntax Error", 
				               							System.Windows.Forms.MessageBoxButtons.OK, 
				                						System.Windows.Forms.MessageBoxIcon.Error);
													safe = false;
												}
												
												break;
											}
											else //the sub command for the row was not used; output 0
											{
												csv_out[r+1][inc] = "0";
												break;
											}
											
										}
										if(scs[h].Row != r && h == (scs.Count-1))
										{//there is no sub command for this row, so output 0.
											csv_out[r+1][inc] = "0";
											break;
										}
									}
								}
										else
										{
											//there are no sub commands for this command, so output 0
											csv_out[r+1][inc] = "0";
										}
										
									
								}
								
								break;
							}
							
							//if(!line.StartsWith(_ow.commands[ii].Commnd) && ii == (_ow.commands.Count-1))
							if(!(lineVals[0].Equals(_ow.commands[ii].Commnd)) && ii == (_ow.commands.Count-1))
							{//line does not begin with a command. spaces are filtered out
								
								//show error box with line number explaining that line must begin with command
								System.Windows.Forms.MessageBox.Show("Lines must always begin with commands or a comment mark. Error on line number: " + ml 
								                                     + ", Faulty Text: " + line,
				                						"Syntax Error", 
				               							System.Windows.Forms.MessageBoxButtons.OK, 
				                						System.Windows.Forms.MessageBoxIcon.Error);
								safe = false;
							}
							}
							
						}
					
					   	inc++;
					
				}
				if(safe) // if no errors
				{
					DialogResult dr = efd.ShowDialog();
					if(dr == System.Windows.Forms.DialogResult.OK)
					{
						string file = efd.FileName;
						StringBuilder sb = new StringBuilder();
						int len = csv_out.GetLength(0);
						for(int y = 0; y<len; y++)
						{
							sb.AppendLine(string.Join(delimiter, csv_out[y]));
						}
				
						bool complete = false;
						try
						{
							File.WriteAllText(file, sb.ToString());
							complete = true;
						}
						catch(IOException e)
						{
							System.Windows.Forms.MessageBox.Show("Export Failed! Reason: " + e.Message,
				                			"Export Failed", 
				               				System.Windows.Forms.MessageBoxButtons.OK, 
				                			System.Windows.Forms.MessageBoxIcon.Error);
							complete = false;
						}
						
						if(complete)
						{
						//export done. tell user
						System.Windows.Forms.MessageBox.Show("Export Complete!",
				                			"Export Complete", 
				               				System.Windows.Forms.MessageBoxButtons.OK, 
				                			System.Windows.Forms.MessageBoxIcon.Information);
						}
				
					}
					
					
				}
		
			
		}
		
		private int getBiggestSubID()
		{
			OptionsWindow _ow = new OptionsWindow();
			int id=0;
			
			for(int i=0;i<_ow.subs.Count;i++)
			{
				if(_ow.subs[i].Row > id)
				{
					id=_ow.subs[i].Row;
				}
			}
			
			return id;
		}
		
		private List<int> getMeaningfulLines()
		{
			getIndex();
			int totalLines = edits[_index].LineCount;
			List<int> mLines = new List<int>();
			
			for(int i = 1; i<=totalLines; i++)
			{
				int off = edits[_index].Document.GetLineByNumber(i).Offset;
				int leng = edits[_index].Document.GetLineByNumber(i).EndOffset;
				string line = "";
					   	
				for(int u = off; u < leng; u++)
				{
					line+=edits[_index].Document.GetCharAt(u);
				}
				
				if(!(line.StartsWith("//")) && !(line.StartsWith(";")) && !(line.EndsWith(":")))
				{
					mLines.Add(i);
				}
			}
			
			return mLines;
			
		}
		private List<int> getMeaninglessLines()
		{
			getIndex();
			int totalLines = edits[_index].LineCount;
			List<int> mLines = new List<int>();
			
			for(int i = 1; i<=totalLines; i++)
			{
				int off = edits[_index].Document.GetLineByNumber(i).Offset;
				int leng = edits[_index].Document.GetLineByNumber(i).EndOffset;
				string line = "";
					   	
				for(int u = off; u < leng; u++)
				{
					line+=edits[_index].Document.GetCharAt(u);
				}
				
				if((line.StartsWith("//")) || (line.StartsWith(";")) || (line.EndsWith(":")))
				{
					mLines.Add(i);
				}
			}
			
			return mLines;
			
		}
		#endregion
		
		#region shortcuts
		
		private void NewCanExecute(object sender, CanExecuteRoutedEventArgs e)
  		{
    		e.CanExecute = true;
    		e.Handled = true;
  		}

  		private void NewExecuted(object sender, ExecutedRoutedEventArgs e)
  		{
  			New();
  		}
  		
  		private void OpenCanExecute(object sender, CanExecuteRoutedEventArgs e)
  		{
    		e.CanExecute = true;
    		e.Handled = true;
  		}

  		private void OpenExecuted(object sender, ExecutedRoutedEventArgs e)
  		{
  			Open();
  		}
  		
  		private void SaveCanExecute(object sender, CanExecuteRoutedEventArgs e)
  		{
    		e.CanExecute = true;
    		e.Handled = true;
  		}

  		private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
  		{
  			Save();
  		}
  		
  		private void AllCanExecute(object sender, CanExecuteRoutedEventArgs e)
  		{
    		e.CanExecute = true;
    		e.Handled = true;
  		}

  		private void AllExecuted(object sender, ExecutedRoutedEventArgs e)
  		{
  			SaveAll();
  		}
  		
  		private void ExportCanExecute(object sender, CanExecuteRoutedEventArgs e)
  		{
    		e.CanExecute = true;
    		e.Handled = true;
  		}

  		private void ExportExecuted(object sender, ExecutedRoutedEventArgs e)
  		{
  			Export();
  		}
  		
  		private void HelpCanExecute(object sender, CanExecuteRoutedEventArgs e)
  		{
    		e.CanExecute = true;
    		e.Handled = true;
  		}

  		private void HelpExecuted(object sender, ExecutedRoutedEventArgs e)
  		{
  			Help();
  		}
		#endregion
		
		#region tabs
		void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
    		if (e.Source is System.Windows.Forms.TabControl)
    		{
     		 	_index = tabControl.SelectedIndex;
    		}
		}
		
		private void addTab(string name, bool isnew)
		{
			StackPanel sp = new StackPanel();
			sp.Orientation = System.Windows.Controls.Orientation.Horizontal;
			
			if(!isnew)
			{
				changed.Add(false);
			}
			
			System.Windows.Controls.Button b = exitButton(tabs.Count);
    		
    		TextBlock tb = new TextBlock();
    		tb.Text = name + "  ";
    		//tb.Text = "_" + tabs.Count + "t";
    		
    		sp.Children.Add(tb);
    		sp.Children.Add(b);
    		
			TabItem tab = new TabItem();
    		tab.Name = "_" + tabs.Count + "t";
    		tab.Header = sp;
    		tabs.Add(tab);
    		
    		FontFamily myFont;
    		myFont = new FontFamily("Consolas");
    		ICSharpCode.AvalonEdit.TextEditor editor = new ICSharpCode.AvalonEdit.TextEditor();
    		editor.ShowLineNumbers = true;
    		editor.FontFamily = myFont;
    		editor.FontSize = 14;
    		editor.Name = "_" + (tabs.Count-1) + "e";
    		setColoring(editor);
    		//editor.TextArea.TextChanged += textEditor_TextChanged;
    		edits.Add(editor);
    		
    		Grid grid = new Grid();
        	grid.Children.Add(editor); 
        	
        	
    		tab.Content = grid; 
    		
    		tabControl.Items.Add(tab);
    		
    		loadToEditor((tabs.Count-1),isnew);
		}
		
		private System.Windows.Controls.Button exitButton(int id)
		{
			System.Windows.Controls.Image img = new System.Windows.Controls.Image();
			BitmapImage image = new BitmapImage(new Uri("/Resources/delete.ico", UriKind.Relative));
			img.Source=image;
			
			StackPanel bsp = new StackPanel();
			bsp.Children.Add(img);
			
    		System.Windows.Controls.Button b = new System.Windows.Controls.Button();
    		b.AddHandler(System.Windows.Controls.Button.ClickEvent, new RoutedEventHandler(closeTab));
    		b.Content=bsp;
    		b.Height=16;
    		b.Width=16;
    		b.Padding = new Thickness(0, 0, 0, 0);
    		
    		Thickness thick = new Thickness();
    		thick.Bottom = 0;
    		thick.Top = 0;
    		thick.Left = 0;
    		thick.Right = 0;
    		SolidColorBrush scb = new SolidColorBrush();
    		scb.Color = Color.FromArgb(0,0,0,0);
    		
    		b.Background = scb;
    		b.BorderThickness = thick;
    		b.Name = "_" + id + "b";
    		
    		return b;
		}
		
		private void tab_update(int index, bool edit)
		{
			
    		StackPanel sp = new StackPanel();
			sp.Orientation = System.Windows.Controls.Orientation.Horizontal;
    		
			System.Windows.Controls.Button b = exitButton(index);
    		
    		
			if(edit)
			{
				TextBlock tb = new TextBlock();
				tb.Text = Path.GetFileName(names[index]) + "*  ";
				//tb.Text = tabs[index].Name + "t*  ";
    			sp.Children.Add(tb);
    			
				changed[index]=true;
			}
			else
			{
				TextBlock tb = new TextBlock();
				tb.Text = Path.GetFileName(names[index]) + "  ";
				//tb.Text = tabs[index].Name + "t*  ";
    			sp.Children.Add(tb);
    			
				changed[index]=false;
			}
			
			sp.Children.Add(b);
			tabs[index].Header=sp;
		}
		
		private void resetNames()
		{
			for(int i = 0 ; i < tabs.Count; i++)
			{
				tabs[i].Name = "_" + i + "t";
				tab_update(i, changed[i]);
			}
		}
		
		private void getIndex()
		{
			_index = tabControl.SelectedIndex;
		}
		
		private void closeTab(object sender, RoutedEventArgs e)
		{
			string bName = (sender as System.Windows.Controls.Button).Name.ToString();
			string n = Convert_Str(bName);
			int id = Convert.ToInt32(n);
			//button names need to be updated to handle deletion, or else exceptions occur
		/*	if(tabs.Count>1 && !changed[id])
			{
				tabControl.Items.Remove(tabs[id]);
				tabs.RemoveAt(id);
				names.RemoveAt(id);
				tabsaver.RemoveAt(id);
				contents.RemoveAt(id);
				changed.RemoveAt(id);
				edits.RemoveAt(id);
				resetNames();
				
				if(id == tabControl.SelectedIndex)
				{
				if(id!=0)
				{
					tabControl.SelectedIndex = (id-1);
				}
				else
				{
					tabControl.SelectedIndex = (id);
				}
				}
				
			} */
			//else if(tabs.Count>1 && changed[id])
			if(tabs.Count>1)
			{
				//Do you want to save?
				DialogResult dr = System.Windows.Forms.MessageBox.Show("Do you want to save before closing?", "Save Confirmation", 
				                   System.Windows.Forms.MessageBoxButtons.YesNoCancel, System.Windows.Forms.MessageBoxIcon.Warning);
				if(dr == System.Windows.Forms.DialogResult.Yes)
				{
					bool b = saveWithFeedback();
					if(b)
					{
						tabControl.Items.Remove(tabs[id]);
						tabs.RemoveAt(id);
						names.RemoveAt(id);
						contents.RemoveAt(id);
						changed.RemoveAt(id);
						edits.RemoveAt(id);
						tabsaver.RemoveAt(id);
						
						resetNames();
						
						if(id == tabControl.SelectedIndex)
						{
						if(id!=0)
						{
							tabControl.SelectedIndex = (id-1);
						}
						else
						{
							tabControl.SelectedIndex = (id);
						}
						}
					}
				}
				else if(dr == System.Windows.Forms.DialogResult.No)
				{
					tabControl.Items.Remove(tabs[id]);
					tabs.RemoveAt(id);
					names.RemoveAt(id);
					contents.RemoveAt(id);
					changed.RemoveAt(id);
					edits.RemoveAt(id);
					tabsaver.RemoveAt(id);
					
					resetNames();
					
					if(id == tabControl.SelectedIndex)
					{
					if(id!=0)
					{
						tabControl.SelectedIndex = (id-1);
					}
					else
					{
						tabControl.SelectedIndex = (id);
					}
					}
				}
			}
			//else if(tabs.Count==1 && changed[id])
			else if(tabs.Count==1)
			{
				//Do you want to save and quit?
				DialogResult dr = System.Windows.Forms.MessageBox.Show("Do you want to save before closing?", "Save and Quit Confirmation", 
				                   System.Windows.Forms.MessageBoxButtons.YesNoCancel, System.Windows.Forms.MessageBoxIcon.Warning);
				if(dr == System.Windows.Forms.DialogResult.Yes)
				{
					bool b = saveWithFeedback();
					if(b)
					{
						tabControl.Items.Remove(tabs[id]);
						tabs.RemoveAt(id);
						names.RemoveAt(id);
						contents.RemoveAt(id);
						changed.RemoveAt(id);
						edits.RemoveAt(id);
						tabsaver.RemoveAt(id);
						
						if(id == tabControl.SelectedIndex)
						{
						if(id!=0)
						{
							tabControl.SelectedIndex = (id-1);
						}
						else
						{
							tabControl.SelectedIndex = (id);
						}
						}
						
						quit_confirm();
					}
					
				}
				else if(dr == System.Windows.Forms.DialogResult.No)
				{
					quit_confirm();
				}
			}
		/*	else
			{
				//Do you want to quit?
				if(System.Windows.Forms.MessageBox.Show("Are you sure you want to quit?", "Quit Confirmation", 
				                   System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
				{
					quit_confirm();
				}
				
			}*/
			
		}
		
		#endregion
		
		#region editor
		
		private void textEditor_TextChanged(object sender, TextCompositionEventArgs e)
		{
			var textEditor = sender as TextEditor;
			if (textEditor != null) {
				string str = textEditor.Name.ToString();
				string n = Convert_Str(str);
				int index = Convert.ToInt32(str);
				tab_update(index, true);
			}
		}
		
		private void loadToEditor(int index, bool isnew)
		{
			if(isnew)
			{
				edits[index].Clear();
				tabControl.SelectedIndex = index;
			}
			else
			{
				edits[index].Load(contents[index]);
				tabControl.SelectedIndex = index;
			}
		}
		
		private void options_popup(object sender, EventArgs e)
		{
			OptionsWindow popup = new OptionsWindow();
			popup.ShowDialog();
			reloadOptions();
		}
		
		private void reloadOptions()
		{
			foreach(ICSharpCode.AvalonEdit.TextEditor te in edits)
			{
				setColoring(te);
			}
		}
		#endregion
			
		
		#region exit
		
		private void quit_confirm()
		{	
			if(tabsaver.Count>0)
			{
				save_tabs(tabsaver);
			}
			SaveAll();
			Properties.RealSettings.Default.Save();
			Environment.Exit(0);
		}
		
		private void WindowClosing(object sender, CancelEventArgs e)
		{
			quit_confirm();
		}
		#endregion
		
		#region settings
		private void save_tabs(List<TabSaver> t)
		{
			 using (MemoryStream ms = new MemoryStream())
    		{
        		using (StreamReader sr = new StreamReader(ms))
        		{
          		  	BinaryFormatter bf = new BinaryFormatter();
            		bf.Serialize(ms, t);
            		ms.Position = 0;
           		 	byte[] buffer = new byte[(int)ms.Length];
            		ms.Read(buffer, 0, buffer.Length);
            		Properties.RealSettings.Default.opentabs = Convert.ToBase64String(buffer);
        		}
    		}
		}
		public List<TabSaver> LoadTabs()
		{
    		using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(Properties.RealSettings.Default.opentabs)))
    		{
    			if(ms.Length > 0)
    			{
        			BinaryFormatter bf = new BinaryFormatter();
        			return (List<TabSaver>)bf.Deserialize(ms);
    			}
    			else
    			{
    				return new List<TabSaver>();
    			}
    		}
		}
		
		private TabSaver makeSaver(string n)
		{
			TabSaver ts = new TabSaver { filename = n };
			return ts;
		}
		
		private void updateWordList(int i)
		{
			OptionsWindow _ow = new OptionsWindow();

			XshdKeywords newKeyWords = new XshdKeywords();
			XshdRuleSet mainRuleSet = xshd.Elements.OfType<XshdRuleSet>().Where(o => string.IsNullOrEmpty(o.Name)).First();
				
			newKeyWords.Words.Add(_ow.words[i].Word);
			XshdColor xcol = xshd.Elements.OfType<XshdColor>().Where(xc => string.Equals(xc.Name, (_ow.words[i].color+_ow.words[i].fontstyle), StringComparison.CurrentCultureIgnoreCase)).First();
			newKeyWords.ColorReference = new XshdReference<XshdColor>(null, xcol.Name);
			
    		mainRuleSet.Elements.Add(newKeyWords);
					
		}
		
		private void updateStandardWordList(int i)
		{
			OptionsWindow _ow = new OptionsWindow();

			XshdKeywords newKeyWords = new XshdKeywords();
			XshdSpan newSpan = new XshdSpan();
			XshdSpan otherNewSpan = new XshdSpan();
			XshdSpan thirdNewSpan = new XshdSpan();
			XshdSpan thNewSpan = new XshdSpan();
			XshdRule rule = new XshdRule();
			 
			XshdRuleSet mainRuleSet = xshd.Elements.OfType<XshdRuleSet>().Where(o => string.IsNullOrEmpty(o.Name)).First();
			
			if(i==0)
			{
				for(int ii=0;ii<_ow.commands.Count;ii++)
				{
					newKeyWords.Words.Add(_ow.commands[ii].Commnd);
				}
				
				XshdColor xcol = xshd.Elements.OfType<XshdColor>().Where(xc => string.Equals(xc.Name, (_ow.words[i].color+_ow.words[i].fontstyle), StringComparison.CurrentCultureIgnoreCase)).First();
				newKeyWords.ColorReference = new XshdReference<XshdColor>(null, xcol.Name);
				mainRuleSet.Elements.Add(newKeyWords);
			}
			if(i==1)
			{
				for(int ii=0;ii<_ow.subs.Count;ii++)
				{
					newKeyWords.Words.Add(_ow.subs[ii].Sub);
				}
				
				XshdColor xcol = xshd.Elements.OfType<XshdColor>().Where(xc => string.Equals(xc.Name, (_ow.words[i].color+_ow.words[i].fontstyle), StringComparison.CurrentCultureIgnoreCase)).First();
				newKeyWords.ColorReference = new XshdReference<XshdColor>(null, xcol.Name);
				mainRuleSet.Elements.Add(newKeyWords);
			}
			if(i==2)
			{
				XshdColor xcol = xshd.Elements.OfType<XshdColor>().Where(xc => string.Equals(xc.Name, (_ow.words[i].color+_ow.words[i].fontstyle), StringComparison.CurrentCultureIgnoreCase)).First();
				
				newSpan.SpanColorReference = new XshdReference<XshdColor>(null, xcol.Name);
				newSpan.BeginRegex = @"[//]{2,3}";
				
				otherNewSpan.SpanColorReference = new XshdReference<XshdColor>(null, xcol.Name);
				otherNewSpan.BeginRegex = @";.";
				
				mainRuleSet.Elements.Add(newSpan);
				mainRuleSet.Elements.Add(otherNewSpan);
				
			}
			if(i==3)
			{
				XshdColor xcol = xshd.Elements.OfType<XshdColor>().Where(xc => string.Equals(xc.Name, (_ow.words[i].color+_ow.words[i].fontstyle), StringComparison.CurrentCultureIgnoreCase)).First();
				rule.ColorReference = new XshdReference<XshdColor>(null, xcol.Name);
				rule.Regex = @"\b0[xX][0-9a-fA-F]+|\b(\d+(\.[0-9]+)?|\.[0-9]+)([eE][+-]?[0-9]+)?";
				mainRuleSet.Elements.Add(rule);
			}
			if(i==4)
			{
				XshdColor xcol = xshd.Elements.OfType<XshdColor>().Where(xc => string.Equals(xc.Name, (_ow.words[i].color+_ow.words[i].fontstyle), StringComparison.CurrentCultureIgnoreCase)).First();
				thNewSpan.SpanColorReference = new XshdReference<XshdColor>(null, xcol.Name);
				thNewSpan.BeginRegex = "\"";
				thNewSpan.EndRegex = "\"";
				mainRuleSet.Elements.Add(thNewSpan);
			}
			if(i==5)
			{
				for(int ii=0;ii<_ow.oc_alias.Count;ii++)
				{
					newKeyWords.Words.Add(_ow.oc_alias[ii].Alias);
				}
				
				XshdColor xcol = xshd.Elements.OfType<XshdColor>().Where(xc => string.Equals(xc.Name, (_ow.words[i].color+_ow.words[i].fontstyle), StringComparison.CurrentCultureIgnoreCase)).First();
				newKeyWords.ColorReference = new XshdReference<XshdColor>(null, xcol.Name);
				mainRuleSet.Elements.Add(newKeyWords);
			}
			
					
		}
		
		private void setColoring(ICSharpCode.AvalonEdit.TextEditor textEditor)
		{
			string dir =System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			using (StreamReader s = new StreamReader(dir + @"\Resources\AutonEditColors.xshd"))
        	{
            	using (XmlTextReader reader = new XmlTextReader(s))
            	{
            		xshd = HighlightingLoader.LoadXshd(reader);
            		
            		OptionsWindow _ow = new OptionsWindow();
            		for(int ii=0;ii<6;ii++)
					{
    					updateStandardWordList(ii);
            		}
            		for(int i=6;i<_ow.words.Count;i++)
					{
    					updateWordList(i);
            		}
                	textEditor.SyntaxHighlighting =
                    ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(xshd, HighlightingManager.Instance);
            	}
        	}
		}
		#endregion
		
		#region Help and About
		
		private void Help()
		{
			Help h = new Help();
			h.ShowDialog();
		}
		void AboutPop()
		{
			About _a = new About();
			_a.ShowDialog();
		}
		private void about_click(object sender, EventArgs e)
		{
			AboutPop();
		}
		private void help_click(object sender, EventArgs e)
		{
			Help();
		}
		#endregion
		
		#region other_click_events
		
		
		#endregion
		}
	
	[Serializable()]
	public class TabSaver 
	{
		public string filename {get; set;}
	}
	
	}