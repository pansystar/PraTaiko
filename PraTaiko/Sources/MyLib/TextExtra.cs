using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pansystar
{
    public static class TextExtraExtensions
    {
        public static bool SetString(this IEnumerable<TextExtra> arrays, string text)
        {
            var te = from a in arrays where a.regex.IsMatch(text) select a;
            
            foreach (var t in te)
            {
                t.set(t.regex.Replace(text, ""));
                return true;
            }
            return false;
        }
        public static TextExtra Get(this IEnumerable<TextExtra> arrays, string text)
        {
            return arrays.Where(a => a.regex.IsMatch(text)).First();
        }
    }
    public class TextExtra
    {
        public string str { get; private set; }
        public Regex regex { get; private set; }
        public Action<string> set { get; private set; }
        public TextExtra(string pattern, Action<string> action)
        {
            str = pattern;
            regex = new Regex("^" + pattern, RegexOptions.Compiled);
            set = action;
        }
    }
}