using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace sorce_engin_keyboard
{
    static class Program
    {
        static NotifyIcon notify = new NotifyIcon();
        static ContextMenuStrip contextMenu = new ContextMenuStrip();
        static string Temp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp");
        static string SoundFolder = Path.Combine(Temp, "sorce_engin");
        static string ZipFile = Path.Combine(SoundFolder, "snd.zip");
        static string ZipFolder = Path.Combine(SoundFolder, "snd");
        static int Selected = 0;
        static bool down = false;
        static Holder holder = new Holder(); //AAAAA IT WON'T SHUT UP ABOUT THE FIELD NEVER BEING USED
        static Utilities.keyboard kbHook = new Utilities.keyboard();
        static Utilities.mouse mHook = new Utilities.mouse();


        private static Dictionary<string, string> Snds = new Dictionary<string, string>();
        private static int sndcnt = 0;
        [DllImport("winmm.dll")]
        private static extern int mciSendString(string command, System.Text.StringBuilder buffer, int bufferSize, IntPtr hwndCallback);

        public static int AddSound(string SoundName, string SndFilePath)
        {
            if (Snds.ContainsKey(SoundName)) return 1;

            if (SoundName.Trim() == "" || !System.IO.File.Exists(SndFilePath)) return 2; 

            mciSendString("open \"" + SndFilePath + "\" type waveaudio alias Snd_" + sndcnt.ToString(), null, 0, IntPtr.Zero);

            Snds.Add(SoundName, "Snd_" + sndcnt.ToString());

            sndcnt++;

            return 0;
        }

        public static bool Play(string SoundName)
        {
            if (!Snds.ContainsKey(SoundName)) { Error("Failed to load sound "+SoundName+", file probably missing from disk/repository"); return false; };
            
            mciSendString("seek " + Snds[SoundName] + " to start", null, 0, IntPtr.Zero);
            int a = mciSendString("play " + Snds[SoundName], null, 0, IntPtr.Zero);

            if (a != 0) return false;

            return true;
        }


        public static void Error(string Text)
        {
            notify.ShowBalloonTip(2000, "Something is creating script errors", Text, ToolTipIcon.Warning);
        }

        static List<string> GetEntriesInDir(string path)
        {
            string[] files = Directory.GetFiles(path); //be good boy and use variables instead of reading all the files over again
            string[] dirs = Directory.GetDirectories(path);
            List<string> combined = new List<string>();
            combined.AddRange(files);
            combined.AddRange(dirs);
            return combined;
        }

        static void RecursiveDeleteFolder(string path)
        {
            foreach (string _path in GetEntriesInDir(path))
            {
                if (Directory.Exists(_path)) //this is a dir
                {
                    RecursiveDeleteFolder(_path);
                    Directory.Delete(_path);
                }
                else //then this must be a file
                {
                    File.Delete(_path);
                }
            }
        }

        static void AddElement(ref ToolStripMenuItem menu, string Caption)
        {
            ToolStripItem elem = menu.DropDownItems.Add(Caption);
            elem.BackColor = System.Drawing.Color.FromArgb(255, 64, 70, 64);
            elem.ForeColor = System.Drawing.Color.White;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        [STAThread]
        static void Main()
        {
            // mmmm why can't i declare this shit somewhere in Program
            // oh well, i won't need it anywhere else

            // ^  fixed ( ͡° ͜ʖ ͡°)
            Utilities.Settings.Load();

            notify.Visible = true;
            notify.Icon = Properties.Resources.sorcse;
            notify.Text = "sorce engin keyboard";
            notify.MouseClick += (sender, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    Play(holder.RandomItem(15));
                }
            };

            contextMenu.BackColor = System.Drawing.Color.FromArgb(255,64,70,64);
            contextMenu.ShowImageMargin = false;
            contextMenu.ForeColor = System.Drawing.Color.White;
            contextMenu.Items.Add("] sound \"\"");
            contextMenu.Items.Add("] settings");
            contextMenu.Items.Add("] about");
            contextMenu.Items.Add("] cleanup");
            contextMenu.Items.Add("] disconnect");

            ToolStripMenuItem menu = contextMenu.Items[0] as ToolStripMenuItem;
            ToolStripMenuItem settings = contextMenu.Items[1] as ToolStripMenuItem;

            // mmmm cursed code ( ͡° ͜ʖ ͡°)
            AddElement(ref menu, "\"body\"");
            AddElement(ref menu, "\"cardboard\"");
            AddElement(ref menu, "\"concrete\"");
            AddElement(ref menu, "\"flesh\"");
            AddElement(ref menu, "\"glass\"");
            AddElement(ref menu, "\"metal\"");
            AddElement(ref menu, "\"plaster\"");
            AddElement(ref menu, "\"plastic\"");
            AddElement(ref menu, "\"rubber\"");
            AddElement(ref menu, "\"surfaces\"");
            AddElement(ref menu, "\"wood\"");
            AddElement(ref menu, "\"cocktail of quiet sounds\"");
            AddElement(ref menu, "\"chaos\"");
            AddElement(ref menu, "\"my EARS\"");
            AddElement(ref menu, "\"you better pray you type at 2 WPM\"");
            AddElement(ref menu, "\"sourcian roulette of auditory doom\"");

            AddElement(ref settings, "ignore_key_hold " + Utilities.Settings.GetKey("ignore_key_hold"));
            AddElement(ref settings, "sound_on_mouse_clicks " + Utilities.Settings.GetKey("sound_on_mouse_clicks"));
            AddElement(ref settings, "sound_on_mouse_scroll " + Utilities.Settings.GetKey("sound_on_mouse_scroll"));

            menu.DropDownItemClicked += (sender, e) =>
            {
                Selected = menu.DropDownItems.IndexOf(e.ClickedItem);
                Debug.WriteLine(e.ClickedItem.Text);
                Debug.WriteLine(Selected);
                menu.Text = "] sounds " + e.ClickedItem.Text;

                Utilities.Settings.SetKey("selected_sounds", Selected);
                Utilities.Settings.Save();
            };

            settings.DropDownItemClicked += (sender, e) =>
            {
                string SelText = e.ClickedItem.Text;

                // char -> int = char index
                // char -> string -> int = actual 0/1 value i need
                // thanks c# very uncool
                int val = Convert.ToInt16(Convert.ToString(SelText[SelText.Length - 1]));
                SelText = SelText.Split(' ')[0];
                e.ClickedItem.Text = SelText + " " + (1 - val);

                //static classes can't return "this" so i can't chain a SetKey with a Save, shame
                Utilities.Settings.SetKey(SelText, 1 - val);
                Utilities.Settings.Save();
            };

            contextMenu.ItemClicked += (sender, e) =>
            {
                Debug.WriteLine(e.ClickedItem.Text);
                switch (e.ClickedItem.Text)
                {
                    case "] sound":
                        Play(holder.RandomItem(15));
                        break;
                    case "] about":
                        notify.ShowBalloonTip(4000, "About \"sorce engin\"", "made by AndrewEathan\nWritten in C#.NET", ToolTipIcon.Info);
                        break;
                    case "] cleanup":
                        notify.ShowBalloonTip(2000, "Cleaned up 388 sounds!", "Disconnected: Server shutting down.", ToolTipIcon.Info);
                        try
                        {
                            RecursiveDeleteFolder(SoundFolder);
                            Directory.Delete(SoundFolder);
                        }
                        catch (Exception ex) { Debug.WriteLine(ex.Message); }
                        System.Threading.Thread.Sleep(2000);
                        Terminate(0);
                        break;
                    case "] disconnect":
                        notify.ShowBalloonTip(2000, "sorce engin", "Disconnected: Server shutting down.", ToolTipIcon.Info);
                        System.Threading.Thread.Sleep(2000);
                        Terminate(0);
                        break;
                }
            };

            contextMenu.Cursor = Cursors.Hand;

            notify.ContextMenuStrip = contextMenu;

            try
            {
                if (!Directory.Exists(ZipFolder))
                {
                    // check if we can write to this folder
                
                        //write zip folder
                        Directory.CreateDirectory(SoundFolder);
                        Directory.CreateDirectory(ZipFolder);
                        File.WriteAllBytes(ZipFile, Properties.Resources.zip);

                        //thankfully .NET has internal zip functionality, would ruin the portability of the program if i added more libraries
                        System.IO.Compression.ZipFile.ExtractToDirectory(ZipFile, ZipFolder);

                        //clean up after ourselves, we don't need the zip afterwards ( ͡° ͜ʖ ͡°)
                        File.Delete(ZipFile);
                    }
                }
            catch (Exception ex)
            {
                Error(ex.Message + "\n" + ex.StackTrace);
            }

            //hook our key listener so that we know when to play funny sound
            kbHook.KeyDown += (sender, e) =>
            {
                if (!down)
                {
                    if (Selected == -1) return;
                    Play(holder.RandomItem(Selected));
                    
                    down = !Convert.ToBoolean(Utilities.Settings.GetKey("ignore_key_hold"));
                    //it's annoying when it spams the sound after you hold the key
                    //quick countermeasure
                }
            };
            kbHook.KeyUp += (sender, e) => down = false;

            void btndown(Utilities.mouse.MSLLHOOKSTRUCT data)
            {
                if (Selected == -1) return;
                if (Utilities.Settings.GetKey("sound_on_mouse_clicks") == 1)
                    Play(holder.RandomItem(Selected));
            }

            void mousewheel(Utilities.mouse.MSLLHOOKSTRUCT data)
            {
                if (Selected == -1) return;
                if (Utilities.Settings.GetKey("sound_on_mouse_scroll") == 1)
                    Play(holder.RandomItem(Selected));
            }

            //thanks for this convenient library rvknth043
            mHook.LeftButtonDown += btndown;
            mHook.MiddleButtonDown += btndown;
            mHook.RightButtonDown += btndown;
            mHook.MouseWheel += mousewheel;
            mHook.Install();

            notify.ShowBalloonTip(2000, "Loading world...", "Loading sounds...", ToolTipIcon.Info);

            //6 months later shortened it
            for (int i = 0; i < 16; i++)
                holder.AddList();

            //oh lordy
            //body
            for (int i = 1; i <= 13; i++)
            {
                holder.AddItem(0, "body/soft (" + Convert.ToString(i) + ")");
            }

            //cardboard
            for (int i = 1; i <= 17; i++)
            {
                holder.AddItem(1, "cardboard/soft (" + Convert.ToString(i) + ")");
            }

            //concrete
            for (int i = 1; i <= 15; i++)
            {
                holder.AddItem(2, "concrete/soft (" + Convert.ToString(i) + ")");
            }

            //flesh
            for (int i = 1; i <= 6; i++)
            {
                holder.AddItem(3, "flesh/soft (" + Convert.ToString(i) + ")");
            }

            //glass
            for (int i = 1; i <= 19; i++)
            {
                holder.AddItem(4, "glass/soft (" + Convert.ToString(i) + ")");
            }

            //metal
            for (int i = 1; i <= 64; i++)
            {
                holder.AddItem(5, "metal/soft (" + Convert.ToString(i) + ")");
            }

            //plaster
            for (int i = 1; i < 23; i++)
            {
                holder.AddItem(6, "plaster/soft (" + Convert.ToString(i) + ")");
            }

            //plastic
            for (int i = 1; i <= 26; i++)
            {
                holder.AddItem(7, "plastic/soft (" + Convert.ToString(i) + ")");
            }

            //rubber
            for (int i = 1; i <= 9; i++)
            {
                holder.AddItem(8, "rubber/soft (" + Convert.ToString(i) + ")");
            }

            //surfaces
            for (int i = 1; i <= 11; i++)
            {
                holder.AddItem(9, "surfaces/soft (" + Convert.ToString(i) + ")");
            }

            //wood
            for (int i = 1; i <= 47; i++)
            {
                holder.AddItem(10, "wood/soft (" + Convert.ToString(i) + ")");
            }

            //cocktail of quiet sounds
            holder.Copy(0, 11);
            holder.Copy(1, 11);
            holder.Copy(2, 11);
            holder.Copy(3, 11);
            holder.Copy(4, 11);
            holder.Copy(5, 11);
            holder.Copy(6, 11);
            holder.Copy(7, 11);
            holder.Copy(8, 11);
            holder.Copy(9, 11);
            holder.Copy(10, 11);

            //chaos
            for (int i = 1; i < 20; i++)
            {
                if (i < 6)
                {
                    holder.AddItem(12, "cardboard/unpleasant (" + Convert.ToString(i) + ")");
                    holder.AddItem(12, "flesh/unpleasant (" + Convert.ToString(i) + ")");
                }

                if (i < 4)
                {
                    holder.AddItem(12, "concrete/unpleasant (" + Convert.ToString(i) + ")");
                }

                holder.AddItem(12, "metal/unpleasant (" + Convert.ToString(i) + ")");

                if (i < 3)
                {
                    holder.AddItem(12, "plaster/unpleasant (" + Convert.ToString(i) + ")");
                }

                if (i < 5)
                {
                    holder.AddItem(12, "plastic/unpleasant (" + Convert.ToString(i) + ")");
                    holder.AddItem(12, "wood/unpleasant (" + Convert.ToString(i) + ")");
                }
            }

            //my ears
            for (int i = 1; i < 19; i++)
            {
                if (i < 4)
                {
                    holder.AddItem(13, "body/ears (" + Convert.ToString(i) + ")");
                    holder.AddItem(13, "cardboard/ears (" + Convert.ToString(i) + ")");
                }

                if (i < 11)
                {
                    holder.AddItem(13, "concrete/ears (" + Convert.ToString(i) + ")");
                }

                if (i < 9)
                {
                    holder.AddItem(13, "flesh/ears (" + Convert.ToString(i) + ")");
                }

                if (i < 5)
                {
                    holder.AddItem(13, "glass/ears (" + Convert.ToString(i) + ")");
                }

                holder.AddItem(13, "metal/ears (" + Convert.ToString(i) + ")");

                if (i < 15)
                {
                    holder.AddItem(13, "wood/ears (" + Convert.ToString(i) + ")");
                }
            }

            //you better pray you're a slow typer
            for (int i = 1; i < 13; i++)
            {
                if (i < 3)
                {
                    holder.AddItem(14, "body/AAA (" + Convert.ToString(i) + ")");
                    holder.AddItem(14, "cardboard/AAA (" + Convert.ToString(i) + ")");
                }

                if (i < 4)
                {
                    holder.AddItem(14, "concrete/AAA (" + Convert.ToString(i) + ")");
                }

                if (i < 2)
                {
                    holder.AddItem(14, "flesh/AAA (" + Convert.ToString(i) + ")");
                    holder.AddItem(14, "plaster/AAA (" + Convert.ToString(i) + ")");
                }

                if (i < 7)
                {
                    holder.AddItem(14, "glass/AAA (" + Convert.ToString(i) + ")");
                    holder.AddItem(14, "wood/AAA (" + Convert.ToString(i) + ")");
                }

                if (i < 6)
                {
                    holder.AddItem(14, "plastic/AAA (" + Convert.ToString(i) + ")");
                }

                holder.AddItem(14, "metal/AAA (" + Convert.ToString(i) + ")");
            }

            //sourcian roulette
            //fuck you past me use a for loop god DAMN
            for (int i = 0; i < 15; i++)
            {
                if (i != 11) holder.Copy(i, 15);
            }

            holder.AddEverySound(15);
            notify.ShowBalloonTip(2000, "Starting Lua...", "Loaded " + Convert.ToString(holder.GetCount(15)) + " sounds!", ToolTipIcon.Info);

            int selectedSound = Utilities.Settings.GetKey("selected_sounds");
            menu.Text = "] sound " + menu.DropDownItems[selectedSound].Text;

            Application.Run();
        }

        static void Terminate(int code)
        {
            // we do a little cleaning ( ͡° ͜ʖ ͡°)
            notify.Visible = false;
            notify.Dispose();
            contextMenu.Dispose();
            Environment.Exit(code);
        }
    }
}
