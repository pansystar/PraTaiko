using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using static DxLibDLL.DX;

namespace Pansystar
{
    public static class Extensions
	{
        [Conditional("DEBUG")]
        public static void PrintInit()
        {
        }
		public class ExistingException : Exception
		{
			public ExistingException(string message) : base(message)
			{

			}
		}
		public static DialogResult ErrorBox(string message,string caption,MessageBoxButtons button, string path)
		{
			PrintMessage("エラーが発生しました「" + message + "」");
			DialogResult dr = MessageBox.Show(message, caption, button);
			Process.Start("notepad.exe", path);
			PrintMessage("メモ帳を開きました");
            Environment.Exit(0);
			PrintMessage("ソフトを終了しました");
			return dr;
		}
		public static DialogResult ErrorBox(string message, string caption, MessageBoxButtons button, bool exit)
		{
			PrintMessage("エラーが発生しました「" + message + "」");
			DialogResult dr = MessageBox.Show(message, caption, button);
			if (exit)
            {
                Environment.Exit(0);
                PrintMessage("ソフトを終了しました");
			}
			return dr;
		}
		[Conditional("DEBUG")]
		public static void PrintMessage(string str)
		{
            //textbox.AppendText(DateTime.Now.ToString() + " -> " + str + "\n");
		}
		[Conditional("DEBUG")]
		public static void AddTitleForDebug(this StringBuilder sb)
		{
			sb.Append("  「Debug Activation」");
		}
        public static void GetStringData(string str, IEnumerable<TextExtra> te)
        {
            te.SetString(str);
        }
        public static void GetStringsData(IEnumerable<string> strs, IEnumerable<TextExtra> te)
        {
            foreach(var s in strs)
            {
                te.SetString(s);
            }
        }
        public static void GetStreamData(string path, IEnumerable<TextExtra> te)
        {
            using (StreamReader sr = new StreamReader(path, Encoding.GetEncoding("Shift_JIS")))
            {
                while (!sr.EndOfStream)
                {
                    te.SetString(sr.ReadLine());
                }
            }
        }
    }
}
