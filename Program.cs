using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Runtime.InteropServices;
namespace hsearch
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        private static string texteditor;

        static void Main(string[] ar)
        {
            var handle = GetConsoleWindow();

            // Hide
            ShowWindow(handle, SW_HIDE);
            
            //test param 
            if (ar == null || ar.Length == 0) ar = new[] { "dbgen" };
            //var paths = @"e:\aha\path\,e:\aha\text\"

            var paths = ConfigurationManager.AppSettings["paths"].Split(',');
            texteditor = ConfigurationManager.AppSettings["texteditor"];
            var frm = new Form();
            
            var flp = new FlowLayoutPanel();
            frm.AutoScroll = true;
            flp.AutoScroll = true;
            flp.Dock = DockStyle.Fill;
            var mat = new List<string>();
            var text = String.Join(" ",ar.Where(x=>!x.StartsWith("--")));
            var opt = ar.Where(x => x.StartsWith("--")).Select(x => x.ToLower().Replace("--", "")).ToArray();
            var lw = new object();
            frm.Controls.Add(flp);
            frm.Width = 1000;
            
            frm.KeyPress += (a, b) =>
            {
            
                if (b.KeyChar == (char)Keys.Escape) frm.Close();
                var cont = flp.Controls.Find("key-" + b.KeyChar.ToString(), false);
                if (cont.Any()) ((Button)cont.First()).PerformClick();
            };
            //frm.KeyUp+=(a,b)=>frm.Close();
            frm.KeyPreview = true;
            frm.Height = 800;
            string keys = "123456789qwertyuiopasdfghkjlzxcvbnm";
            int kk = 0;
            foreach (var pa in paths)
            {
                Helper.GetFilesRecursive(pa, "*.*").AsParallel().ForAll(fi =>
                {
                    int i = 1;

                    var tx = File.ReadAllLines(fi).Select(x => new
                    {
                        File = fi,
                        LineNo = i++,
                        Text = x
                    })
                        .Where(x => x.Text.HContainsInCI(text)).ToList().Take(10).ToList();

                    if (tx.Any())
                    {
                        var bt = new Button();
                        var inf = new FileInfo(fi);
                        string key;
                        lock (lw)
                        {
                            if (kk < keys.Length)
                                key = keys[kk++].ToString();
                            else
                                key = "";
                            bt.Name = "key-" + key;
                        }
                        bt.Text = key + " - " + inf.Name;
                        bt.Width = 200;
                        var txb = new TextBox();
                        txb.Multiline = true;
                        txb.Width = 600;
                        txb.TextAlign = HorizontalAlignment.Left;
                        txb.ReadOnly = true;
                        txb.Height = tx.Count * 40;
                        
                        txb.Lines = tx.Select(k => k.Text).ToArray();
                        var lb = new Label();
                        lb.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
                        lb.Text = ""+(int) (DateTime.UtcNow - inf.LastWriteTime).TotalDays + " days ago ";
                        bt.Click += (a, b) =>
                        {
                            Process.Start(texteditor, $" \"{inf.FullName}\"");
                            if(Control.ModifierKeys!=Keys.Shift)
                                frm.Close();
                        };
                        lock (lw)
                        {
                            flp.Controls.Add(bt);
                            flp.Controls.Add(lb);
                            flp.Controls.Add(txb);
                            flp.SetFlowBreak(txb, true);
                        }
                    }
                });
            }
            frm.ShowDialog();
        }

  

    }
}
