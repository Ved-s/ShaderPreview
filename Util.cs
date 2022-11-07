using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShaderPreview
{
    public static class Util
    {
        public static void SelectFileToOpen(string title, Action<string> fileSelected, string? filter = null)
        {
            Thread thread = new(() =>
            {
				System.Windows.Forms.OpenFileDialog ofd = new()
				{
					Title = title,
					Filter = filter
				};

				if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    fileSelected(ofd.FileName);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
        public static void SelectFileToSave(string title, Action<bool, string> fileSelected, string? filter = null)
        {
            Thread thread = new(() =>
            {
                System.Windows.Forms.SaveFileDialog ofd = new()
                {
                    Title = title,
                    Filter = filter
                };
                fileSelected(ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK, ofd.FileName);
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        public static IEnumerable<T> Enumerate<T>(T arg0)
        {
            yield return arg0;
        }

        public static IEnumerable<T> Enumerate<T>(T arg0, T arg1)
        {
            yield return arg0;
            yield return arg1;
        }

        public static IEnumerable<T> Enumerate<T>(T arg0, T arg1, T arg2)
        {
            yield return arg0;
            yield return arg1;
            yield return arg2;
        }

        public static IEnumerable<T> Enumerate<T>(T arg0, T arg1, T arg2, T arg3)
        {
            yield return arg0;
            yield return arg1;
            yield return arg2;
            yield return arg3;
        }
    }
}
