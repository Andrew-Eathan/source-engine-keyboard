using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace sorce_engin_keyboard
{
    class Holder
    {
        List<List<string>> Container = new List<List<string>>();

        public void AddList() => Container.Add(new List<string>());

        public void AddItem(int index, string str) => Container[index].Add(str);

        public string RandomItem(int index) => Container[index][(int)Math.Floor(new Random(DateTime.Now.Millisecond * DateTime.Now.Second).NextDouble() * Container[index].Count())];

        public void Copy(int copyfrom, int copyto) => Container[copyto].AddRange(Container[copyfrom]);

        public int GetCount(int index) => Container[index].Count();

        unsafe public void AddEverySound(int index)
        {
            foreach(string j in Container[index])
            {
                Application.DoEvents();
                int retr = Program.AddSound(j, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Temp\sorce_engin\snd\" + j.Replace("/", "\\") + ".wav"));

                //oh god why
                if (retr != 0)
                { 
                    Debug.WriteLine("Failed to load sound " + j + ".wav, file probably missing from disk/repository");
                    Debug.WriteLine(retr);
                    Program.Error("Failed to load sound " + j + ".wav, file probably missing from disk/repository");
                }    
            }
        }
    }
}
