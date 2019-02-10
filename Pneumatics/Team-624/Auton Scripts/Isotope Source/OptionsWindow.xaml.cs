
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;

using Newtonsoft.Json;
using Isotope.Properties;

namespace Isotope
{
	/// <summary>
	/// Interaction logic for OptionsWindow.xaml
	/// </summary>
	public partial class OptionsWindow : Window
	{
		System.Windows.Forms.OpenFileDialog json;
		System.Windows.Forms.SaveFileDialog exp;
		
		public ObservableCollection<Commands> commands;
		public ObservableCollection<SubCommands> subs;
		public ObservableCollection<KeyWords> words;
		public ObservableCollection<Aliases> oc_alias;
		
		public bool use_load_json;
		public string load_json;
		
		private List<JSONCommands> json_cmds;
		private List<JSONAliases> json_als;
		
		public OptionsWindow()
		{
			InitializeComponent();
			
			json = new System.Windows.Forms.OpenFileDialog();
			exp = new System.Windows.Forms.SaveFileDialog();
			
			commands = new ObservableCollection<Commands>();
			subs = new ObservableCollection<SubCommands>();
			words = new ObservableCollection<KeyWords>();
			oc_alias = new ObservableCollection<Aliases>();
			
			json_cmds = new List<JSONCommands>();
			
			commands = LoadCommands();
			subs = LoadSubCommands();
			words = LoadKeyWords();
			oc_alias = LoadAliases();
			
			use_load_json = LoadJSONCheckBox();
			if(use_load_json)
			{
				use_json_cb.IsChecked = true;
				load_json = LoadJSONName();
				load_json_file_from_res(load_json);
				display_current_json(load_json);
			}
			else
			{
				load_json = "Sample Text";
			}
			
			commandList.ItemsSource = commands;
			subcommandList.ItemsSource = subs;
			syntaxList.ItemsSource = words;
			aliasList.ItemsSource = oc_alias;
			
			setOpenFileDialog(json);
			setSaveFileDialog(exp);
		}
		
		#region init
		private void setOpenFileDialog(System.Windows.Forms.OpenFileDialog ofd)
		{
   	 		// DO NOT Allow the user to select multiple files.
    		ofd.Multiselect = false;

    		ofd.Title = "Choose JSON CommandSet";
    		ofd.Filter = "JSON Files (*.json)|*.json";
		}
		private void setSaveFileDialog(System.Windows.Forms.SaveFileDialog sfd)
		{
    		sfd.Title = "Save CommandSet";
    		sfd.Filter = "JSON Files (*.json)|*.json";
		}
		#endregion
		
		#region exit_handling
		private void cancel_click(object sender, EventArgs e)
		{
			this.Close();
		}
		private void ok_click(object sender, EventArgs e)
		{
			save_commandlist(commands);
			save_subcommandlist(subs);
			save_wordlist(words);
			save_aliases(oc_alias);
			save_json_loaders(load_json, use_load_json);
			Properties.RealSettings.Default.Save();
			this.Close();
		}
		#endregion
		
		#region tab_editor
		private void add_word(object sender, EventArgs e)
		{
			NewWord nw = new NewWord(this);
			nw.ShowDialog();
			
		}
		
		private void edit_word(object sender, EventArgs e)
		{
			KeyWords k = syntaxList.SelectedItem as KeyWords;
			if (k != null)
			{
				EditWord edw = new EditWord(this, k);
				edw.ShowDialog();
			}
		}
		
		private void delete_word_click(object sender, EventArgs e)
		{
			List<int> ids = new List<int>();
				foreach(KeyWords k in syntaxList.SelectedItems)
				{
					if(!k.Permanent)
					{
						ids.Add(k.Index);
					}
				}
				
				ids.Sort();
				
				for(int i=(ids.Count)-1;i>=0;i--)
				{
					words.RemoveAt(ids[i]);
				}
		}
		#endregion
		
		#region tab_auton
		private void add_command(object sender, EventArgs e)
		{
			NewCommand nc = new NewCommand(this);
			nc.ShowDialog();
			//sortLists();
		}
		private void edit_command(object sender, EventArgs e)
		{
			Commands c = commandList.SelectedItem as Commands;
			if (c != null)
			{
				Edit ed = new Edit(this, c);
				ed.ShowDialog();
			}
			else
			{
				SubCommands sc = subcommandList.SelectedItem as SubCommands;
				if (sc != null)
				{
					Edit ed = new Edit(this, sc);
					ed.ShowDialog();
				}
			}
			
			//sortLists();
			
		}
		private void clearauton()
		{
			commands.Clear();
			subs.Clear();
		}
		
		private void clearaliases()
		{
			oc_alias.Clear();
		}
		
		private void clear_command(object sender, EventArgs e)
		{
			System.Windows.Forms.DialogResult dir = System.Windows.Forms.MessageBox.Show("Are you sure that you want to clear all Commands, Sub Commands and Aliases?" ,
				                						"Delete All Settings?", 
				               							System.Windows.Forms.MessageBoxButtons.YesNo, 
				                						System.Windows.Forms.MessageBoxIcon.Warning);
			if(dir==System.Windows.Forms.DialogResult.Yes)
			{
				clearauton();
				clearaliases();
			}
		}
		
		private void add_json_click(object sender, EventArgs e)
		{
			System.Windows.Forms.DialogResult dir = 
			System.Windows.Forms.MessageBox.Show("When importing a JSON file, all Commands, Sub Commands and Aliases will be overwritten. Continue?" ,
			"Overwrite Settings Action Detected", 
			System.Windows.Forms.MessageBoxButtons.YesNo, 
			System.Windows.Forms.MessageBoxIcon.Warning);
			if(dir==System.Windows.Forms.DialogResult.Yes)
			{
				clearauton();
				clearaliases();
				Open();
			//sortLists();
			}
		}
		
		public void load_json_file_from_res(string filename)
		{
			clearauton();
			clearaliases();
			OpenJSONFileName(filename);
		}
		private void make_json_click(object sender, EventArgs e)
		{
			Save();
		}
		
		private void delete_click(object sender, EventArgs e)
		{
				List<int> ids = new List<int>();
				foreach(Commands c in commandList.SelectedItems)
				{
					ids.Add(c.Ind_);
				}
				
				ids.Sort();
				
				for(int i=(ids.Count)-1;i>=0;i--)
				{
					commands.RemoveAt(ids[i]);
				}
				
				List<int> sids = new List<int>();
				foreach(SubCommands sc in subcommandList.SelectedItems)
				{
					sids.Add(sc.Index);
				}
				
				sids.Sort();
				
				for(int i=(sids.Count)-1;i>=0;i--)
				{
					subs.RemoveAt(sids[i]);
				}
       		
		}
		#endregion
		
		#region files
		private void Save()
		{
			DialogResult dr = exp.ShowDialog();
			if (dr == System.Windows.Forms.DialogResult.OK)
    		{
				try
				{
					string file = exp.FileName;
					
					List<JSONCommands> jscmd = new List<JSONCommands>();
					
					foreach(Commands c in commands)
					{
						List<JSONSubCommands> jsubs = new List<JSONSubCommands>();
						foreach(SubCommands sc in subs)
						{
							if(sc.parentCommand == c)
							{
								jsubs.Add(makejsub(sc));
							}
						}
						
						jscmd.Add(makejcmd(c, jsubs));
					}
					
					string json = JsonConvert.SerializeObject(jscmd.ToArray());
					
					File.WriteAllText(file, json);
					
				}
				catch (Exception ex)
				{
					System.Windows.Forms.MessageBox.Show(ex.Message);
				}
				
			}
		}
		private void Open()
		{
			DialogResult dr = json.ShowDialog();
			if (dr == System.Windows.Forms.DialogResult.OK)
    		{
				try
            	{
					//load json
            		string file = json.FileName;
            		
            		if(file != null)
            		{
            			JSONCommands jsoncmds = new JSONCommands();
            			
            			
            			// deserialize JSON directly from a file
						using (StreamReader srfile = File.OpenText(@file))
						{
    						JsonSerializer serializer = new JsonSerializer();
   							json_cmds = (List<JSONCommands>)serializer.Deserialize(srfile, typeof(List<JSONCommands>));
						}
						
						//JsonSerializer se = new JsonSerializer();
    					//json_cmds = JsonConvert.DeserializeObject<List<JSONCommands>>(File.ReadAllText(file));
            			
            			for(int i = 0; i<(json_cmds.Count); i++)
            			{
            				int ii = Get_Command_Size();
            				if(checkID(json_cmds[i].id))
            				{
            					commands.Add(make_command(json_cmds[i].name,json_cmds[i].id,json_cmds[i].desc, ii));
            				}
            				
            				for(int v = 0; v<(json_cmds[i].subcs.Count); v++)
            				{
            					int iii = Get_Sub_Command_Size();
            					List<JSONSubCommands> jsubs  = json_cmds[i].subcs;
            					
            					bool isExist = false;
            					for(int iv = 0; iv <subs.Count; iv++)
            					{
            						if(subs.Contains(make_sub_command(jsubs[v].name,jsubs[v].id,jsubs[v].desc, iv, i)))
            						{
            							isExist = true;
            						}
            					}
            					
            					//int id = checkSubs(jsubs[v].name);
            					
            					if(!isExist)
            					{
            						subs.Add(make_sub_command(jsubs[v].name,jsubs[v].id,jsubs[v].desc, iii, i));
            					}
            				} 
            			}
            			// deserialize JSON directly from a file
						using (StreamReader srfile = File.OpenText(@file))
						{
    						JsonSerializer serializ = new JsonSerializer();
   							json_als = (List<JSONAliases>)serializ.Deserialize(srfile, typeof(List<JSONAliases>));
							foreach (var ali in json_als) {
								if (!string.IsNullOrEmpty(ali.name))
									{
   									oc_alias.Add(make_aliases(ali.name,ali.num));
   								}
							}
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
            	    System.Windows.Forms.MessageBox.Show("Cannot load file: " + json.FileName.Substring(json.FileName.LastIndexOf('\\'))
            	        + ". You may not have permission to read the file, or " +
            	        "it may be corrupt.\n\nReported error: " + ex.Message);
            	} 
			}
		}
		
		void OpenJSONFileName(string file)
		{
			try
            	{
            		if(file != null)
            		{
            			JSONCommands jsoncmds = new JSONCommands();
            			
            			
            			// deserialize JSON directly from a file
						using (StreamReader srfile = File.OpenText(@file))
						{
    						JsonSerializer serializer = new JsonSerializer();
   							json_cmds = (List<JSONCommands>)serializer.Deserialize(srfile, typeof(List<JSONCommands>));
						}
						
						//JsonSerializer se = new JsonSerializer();
    					//json_cmds = JsonConvert.DeserializeObject<List<JSONCommands>>(File.ReadAllText(file));
            			
            			for(int i = 0; i<(json_cmds.Count); i++)
            			{
            				int ii = Get_Command_Size();
            				if(checkID(json_cmds[i].id))
            				{
            					commands.Add(make_command(json_cmds[i].name,json_cmds[i].id,json_cmds[i].desc, ii));
            				}
            				
            				for(int v = 0; v<(json_cmds[i].subcs.Count); v++)
            				{
            					int iii = Get_Sub_Command_Size();
            					List<JSONSubCommands> jsubs  = json_cmds[i].subcs;
            					
            					bool isExist = false;
            					for(int iv = 0; iv <subs.Count; iv++)
            					{
            						if(subs.Contains(make_sub_command(jsubs[v].name,jsubs[v].id,jsubs[v].desc, iv, i)))
            						{
            							isExist = true;
            						}
            					}
            					
            					//int id = checkSubs(jsubs[v].name);
            					
            					if(!isExist)
            					{
            						subs.Add(make_sub_command(jsubs[v].name,jsubs[v].id,jsubs[v].desc, iii, i));
            					}
            				} 
            			}
            			// deserialize JSON directly from a file
						using (StreamReader srfile = File.OpenText(@file))
						{
    						JsonSerializer serializ = new JsonSerializer();
   							json_als = (List<JSONAliases>)serializ.Deserialize(srfile, typeof(List<JSONAliases>));
							foreach (var ali in json_als) {
								if (!string.IsNullOrEmpty(ali.name))
									{
   									oc_alias.Add(make_aliases(ali.name,ali.num));
   								}
							}
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
            	    System.Windows.Forms.MessageBox.Show("Cannot load file: " + json.FileName
            	        + ". You may not have permission to read the file, it may not be selected yet, or " +
            	        "it may be corrupt.\n\nReported error: " + ex.Message);
            	}
		}
		
		private void save_commandlist(ObservableCollection<Commands> c)
		{
			 using (MemoryStream ms = new MemoryStream())
    		{
        		using (StreamReader sr = new StreamReader(ms))
        		{
          		  	BinaryFormatter bf = new BinaryFormatter();
            		bf.Serialize(ms, c);
            		ms.Position = 0;
           		 	byte[] buffer = new byte[(int)ms.Length];
            		ms.Read(buffer, 0, buffer.Length);
            		Properties.RealSettings.Default.commands = Convert.ToBase64String(buffer);
        		}
    		}
		}
		public ObservableCollection<Commands> LoadCommands()
		{
    		using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(Properties.RealSettings.Default.commands)))
    		{
    			if(ms.Length > 0)
    			{
        			BinaryFormatter bf = new BinaryFormatter();
        			return (ObservableCollection<Commands>)bf.Deserialize(ms);
    			}
    			else
    			{
    				return new ObservableCollection<Commands>();
    			}
    		}
		}
		
		private void save_subcommandlist(ObservableCollection<SubCommands> sc)
		{
			 using (MemoryStream ms = new MemoryStream())
    		{
        		using (StreamReader sr = new StreamReader(ms))
        		{
          		  	BinaryFormatter bf = new BinaryFormatter();
            		bf.Serialize(ms, sc);
            		ms.Position = 0;
           		 	byte[] buffer = new byte[(int)ms.Length];
            		ms.Read(buffer, 0, buffer.Length);
            		Properties.RealSettings.Default.subcommands = Convert.ToBase64String(buffer);
        		}
    		}
		}
		public ObservableCollection<SubCommands> LoadSubCommands()
		{
    		using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(Properties.RealSettings.Default.subcommands)))
    		{
    			if(ms.Length > 0)
    			{
        		BinaryFormatter bf = new BinaryFormatter();
        		return (ObservableCollection<SubCommands>)bf.Deserialize(ms);
    			}
    			else
    			{
    				return new ObservableCollection<SubCommands>();
    			}
    		}
		}
		
		private void save_wordlist(ObservableCollection<KeyWords> k)
		{
			 using (MemoryStream ms = new MemoryStream())
    		{
        		using (StreamReader sr = new StreamReader(ms))
        		{
          		  	BinaryFormatter bf = new BinaryFormatter();
            		bf.Serialize(ms, k);
            		ms.Position = 0;
           		 	byte[] buffer = new byte[(int)ms.Length];
            		ms.Read(buffer, 0, buffer.Length);
            		Properties.RealSettings.Default.keywords = Convert.ToBase64String(buffer);
        		}
    		}
		}
		public ObservableCollection<KeyWords> LoadKeyWords()
		{
    		using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(Properties.RealSettings.Default.keywords)))
    		{
    			if(ms.Length > 0)
    			{
        		BinaryFormatter bf = new BinaryFormatter();
        		return (ObservableCollection<KeyWords>)bf.Deserialize(ms);
    			}
    			else
    			{
    				NewWord _nw = new NewWord(this);
    				ObservableCollection<KeyWords> ockw = new ObservableCollection<KeyWords>();
    				ockw.Add(make_key_word("Commands", "DarkGreen", true, "bold",0));
    				ockw.Add(make_key_word("Sub-Commands", "Red", true, "normal",1));
    				ockw.Add(make_key_word("Comments", "Gray", true, "normal",2));
    				ockw.Add(make_key_word("Numbers", "Navy", true, "normal",3));
    				ockw.Add(make_key_word("Strings", "Blue", true, "normal",4)); 
					ockw.Add(make_key_word("Aliases", "Teal", true, "italic",5)); 		
    				return ockw;
    			}
    		}
		}
		
		private void save_aliases(ObservableCollection<Aliases> al)
		{
			 using (MemoryStream ms = new MemoryStream())
    		{
        		using (StreamReader sr = new StreamReader(ms))
        		{
          		  	BinaryFormatter bf = new BinaryFormatter();
            		bf.Serialize(ms, al);
            		ms.Position = 0;
           		 	byte[] buffer = new byte[(int)ms.Length];
            		ms.Read(buffer, 0, buffer.Length);
            		Properties.RealSettings.Default.aliaseslist = Convert.ToBase64String(buffer);
        		}
    		}
		}
		public bool LoadJSONCheckBox()
		{
			bool b = RealSettings.Default.use_json_load;
			return b;
		}
		
		public string LoadJSONName()
		{
			using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(Properties.RealSettings.Default.json_load)))
    		{
    			if(ms.Length > 0)
    			{
        		BinaryFormatter bf = new BinaryFormatter(); 
        		return (string)bf.Deserialize(ms);
    			}
    			else
    			{
    				return "";
    			}
    		}
		}
		
		
		private void save_json_loaders(string file, bool use)
		{
			 using (MemoryStream ms = new MemoryStream())
    		{
        		using (StreamReader sr = new StreamReader(ms))
        		{
          		  	BinaryFormatter bf = new BinaryFormatter();
            		bf.Serialize(ms, file);
            		ms.Position = 0;
           		 	byte[] buffer = new byte[(int)ms.Length];
            		ms.Read(buffer, 0, buffer.Length);
            		Properties.RealSettings.Default.json_load = Convert.ToBase64String(buffer);
            		
            		
            		Properties.RealSettings.Default.use_json_load = use;
        		}
    		}
		}
		public ObservableCollection<Aliases> LoadAliases()
		{
    		using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(Properties.RealSettings.Default.aliaseslist)))
    		{
    			if(ms.Length > 0)
    			{
        		BinaryFormatter bf = new BinaryFormatter();
        		return (ObservableCollection<Aliases>)bf.Deserialize(ms);
    			}
    			else
    			{
    				return new ObservableCollection<Aliases>();
    			}
    		}
		}
		
		private void file_drop(object sender, System.Windows.DragEventArgs e)
		{
			System.Windows.Forms.DialogResult dir = 
			System.Windows.Forms.MessageBox.Show("When importing a JSON file, all commands and Sub-commands will be overwritten. Continue?" ,
			"Non-Native File Extension Detected", 
			System.Windows.Forms.MessageBoxButtons.YesNo, 
			System.Windows.Forms.MessageBoxIcon.Warning);
			if(dir==System.Windows.Forms.DialogResult.Yes)
			{
				clearauton();
			
			if(e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop, true);
				
				foreach (string file in files)
				{
					dropOpen(file);
				}
			}
			}
		}
		
		private void dropOpen(string file)
		{
			// Create text.
            try
            {
            	if((Path.GetExtension(file)) == ".json")
            	{
            		if(file != null)
            		{
            			JSONCommands jsoncmds = new JSONCommands();
            			JSONAliases jsonals = new JSONAliases();
            			
            			// deserialize JSON directly from a file
						using (StreamReader srfile = File.OpenText(@file))
						{
    						JsonSerializer serializer = new JsonSerializer();
   							json_cmds = (List<JSONCommands>)serializer.Deserialize(srfile, typeof(List<JSONCommands>));
						}
						
						//JsonSerializer se = new JsonSerializer();
    					//json_cmds = JsonConvert.DeserializeObject<List<JSONCommands>>(File.ReadAllText(file));
            			
            			for(int i = 0; i<(json_cmds.Count); i++)
            			{
            				int ii = Get_Command_Size();
            				if(checkID(json_cmds[i].id))
            				{
            					commands.Add(make_command(json_cmds[i].name,json_cmds[i].id,json_cmds[i].desc, ii));
            				}
            				
            				for(int v = 0; v<(json_cmds[i].subcs.Count); v++)
            				{
            					int iii = Get_Sub_Command_Size();
            					List<JSONSubCommands> jsubs  = json_cmds[i].subcs;
            					
            					bool isExist = false;
            					for(int iv = 0; iv <subs.Count; iv++)
            					{
            						if(subs.Contains(make_sub_command(jsubs[v].name,jsubs[v].id,jsubs[v].desc, iv, i)))
            						{
            							isExist = true;
            						}
            					}
            					
            					//int id = checkSubs(jsubs[v].name);
            					
            					if(!isExist)
            					{
            						subs.Add(make_sub_command(jsubs[v].name,jsubs[v].id,jsubs[v].desc, iii, i));
            					}
            				} 
            			}
            			
            			// deserialize JSON directly from a file
						using (StreamReader srfile = File.OpenText(@file))
						{
    						JsonSerializer serializ = new JsonSerializer();
   							json_als = (List<JSONAliases>)serializ.Deserialize(srfile, typeof(List<JSONAliases>));
							foreach (var ali in json_als) {
								if (!string.IsNullOrEmpty(ali.name))
									{
   									oc_alias.Add(make_aliases(ali.name,ali.num));
   								}
							}
						}
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
                    "it may be corrupt.\n\nReported error: " + ex.Message);
            } 
			
		}
		#endregion
		
		#region create_types
		public void Add_Command(Commands c)
		{
			commands.Add(c);
		}
		public void Add_Sub_Command(SubCommands sc)
		{
			subs.Add(sc);
		}
		public void Add_Key_Word(KeyWords k)
		{
			words.Add(k);
		}
		
		
		public int Get_Command_Size()
		{
			return commands.Count();
		}
		public int Get_Sub_Command_Size()
		{
			return subs.Count();
		}
		public int Get_Key_Word_Size()
		{
			return words.Count();
		}
		
		public bool checkID(int id)
		{
			for(int i=0; i<commands.Count();i++)
			{
				if(id==commands[i].Index)
				{
					return false;
				}
			}
			
			return true;
		}
		
		public bool checkName(string n)
		{
			for(int i=0; i<commands.Count();i++)
			{
				if(n==commands[i].Commnd)
				{
					return false;
				}
			}
			
			return true;
		}
		
		public int checkSubs(string n)
		{
			for(int i=0; i<subs.Count();i++)
			{
				if(n==subs[i].Sub)
				{
					return i;
				}
			}
			
			return subs.Count;
		}
		
		public void refreshSubCommands(string past, string current)
		{
			foreach(SubCommands scc in subs)
			{
				if (scc.parentCommand.Commnd == past)
				{
					string scc_name=scc.Sub;
					int scc_row = scc.Row;
					string descript = scc.sDescription;
					int idex = scc. Index;
					foreach(Commands cc in commands)
					{
						if(cc.Commnd == current)
						{
							int scc_parent = cc.Index;
							
							subs.RemoveAt(scc.Index);
							SubCommands subc = make_sub_command(scc_name,scc_row,descript,idex,scc_parent);
							subs.Insert(scc.Index, subc);
						}
					}
				}
			}
		}
		
		
		public KeyWords make_key_word(string name, String c, bool isPerm, String sty, int i)
		{
			KeyWords kw = new KeyWords(){ Word=name, color=c, Permanent=isPerm, Index=i, fontstyle=sty};
			return kw;
		}
		
		private SubCommands make_sub_command(string comm, int id, string desc, int i, int cmd_id)
		{
			Commands c = commands[cmd_id];
			SubCommands sc = new SubCommands(){ Sub=comm, Row=id, sDescription=desc, Index=i, parentCommand=c};
			return sc;
		}
		
		private Commands make_command(string comm, int id, string desc, int i)
		{
			Commands c = new Commands(){ Commnd=comm, Index=id, Description=desc, Ind_=i };
			return c;
		}
		
		private Aliases make_aliases(string name, int num)
		{
			Aliases a = new Aliases(){ Alias=name, Number=num};
			return a;
		}
		
		private JSONSubCommands makejsub(SubCommands sc)
		{
			JSONSubCommands jsc = new JSONSubCommands(){ id = sc.Row, name = sc.Sub, desc = sc.sDescription };
			return jsc;
		}
		private JSONCommands makejcmd(Commands c, List<JSONSubCommands> jsub)
		{
			JSONCommands jc = new JSONCommands(){ id = c.Index, name = c.Commnd, desc = c.Description, subcs = jsub };
			return jc;
		}
		
		private void sortLists()
		{
			List<Commands> c_list = commands.ToList();
            c_list.OrderBy(c => c.Index);
           	ObservableCollection<Commands> occ = new ObservableCollection<Commands>();
            foreach(Commands cs in c_list)
            {
            	occ.Add(cs);
            }
            commands = occ;
            
            List<SubCommands> sc_list = subs.ToList();
            sc_list.OrderBy(s => s.Row);
           	ObservableCollection<SubCommands> ocsc = new ObservableCollection<SubCommands>();
            foreach(SubCommands sc in sc_list)
            {
            	ocsc.Add(sc);
            }
            subs = ocsc;
		}
		void loadjsonfile_Click(object sender, RoutedEventArgs e)
		{
			DialogResult dr = json.ShowDialog();
			if (dr == System.Windows.Forms.DialogResult.OK)
    		{
				load_json = json.FileName;
			}
			display_current_json(load_json);
		}	
		void display_current_json(string name)
		{
			file_display.Content = name;
		}
		private void JSONCheckBoxChanged(object sender, RoutedEventArgs e)
		{
			use_load_json = (bool)use_json_cb.IsChecked;
			if(use_load_json)
			{
				load_json_file_from_res(load_json);
				display_current_json(load_json);
			}
		}
		#endregion
		
	}
	
	[Serializable()]
	public class Commands{
		
		public string Commnd { get; set; }
		public int Index { get; set; }
		public string Description { get; set; }
		public int Ind_ { get; set; }
	}
	
	[Serializable()]
	public class KeyWords{
		public string Word { get; set; }
		public string color { get; set; }
		public bool Permanent { get; set; }
		public string fontstyle { get; set; }
		public int Index { get; set; }
		
	}
	
	[Serializable()]
	public class SubCommands{
		public string Sub { get; set; }
		public int Row { get; set; }
		public string sDescription { get; set; }
		public int Index { get; set; }
		public Commands parentCommand { get; set; }
	}
	
	[Serializable()]
	public class Aliases{
		public string Alias { get; set; }
		public int Number { get; set; }
	}
	
	public class JSONAliases{
		
		[JsonProperty(PropertyName = "alias")]
		public string name { get; set; }
		
		[JsonProperty(PropertyName = "represents")]
		public int num { get; set; }
		
	}

	public class JSONSubCommands{
		
		[JsonProperty(PropertyName = "name")]
		public string name { get; set; }
		
		[JsonProperty(PropertyName = "id")]
		public int id { get; set; }
		
		[JsonProperty(PropertyName = "desc")]
		public string desc { get; set; }
	}
	
	public class JSONCommands
	{
		[JsonProperty(PropertyName = "name")]
    	public string name { get; set; }
    	
    	[JsonProperty(PropertyName = "id")]
    	public int id { get; set; }
    	
    	[JsonProperty(PropertyName = "desc")]
    	public string desc { get; set; }
    	
    	[JsonProperty(PropertyName = "subs")]
    	public List<JSONSubCommands> subcs { get; set; }
    	
    	//[JsonProperty(PropertyName = "subs")]
    	//public List<string> substr { get; set; }
    	
    	public JSONCommands()
    	{
    		subcs=new List<JSONSubCommands>();
    	}
	}
}