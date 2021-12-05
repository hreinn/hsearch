using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hsearch
{
    public static class Helper
    {
		public static bool HContainsInCI(this string val, params string[] vals)
		{
			if (val == null)
			{
				return false;
			}
			return vals.Any((string p) => val.ToLower().Contains(p.ToLower()));
		}
		public static IEnumerable<string> GetFilesRecursive(string root, string searchPattern, bool continueOnUnauthorizedException = false)
		{
			foreach (string item in GetFilesInFolder(root, searchPattern, continueOnUnauthorizedException))
			{
				yield return item;
			}
			IEnumerable<string> directories = GetDirectoriesInFolder(root, continueOnUnauthorizedException);
			foreach (string folder in directories)
			{
				IEnumerable<string> ls = new List<string>().AsEnumerable();
				try
				{
					ls = GetFilesRecursive(folder, searchPattern, continueOnUnauthorizedException);
				}
				catch (UnauthorizedAccessException unau)
				{
					//$"{folder}:{unau}".HToConsole();
					if (!continueOnUnauthorizedException)
					{
						throw unau;
					}
				}
				foreach (string item2 in ls)
				{
					yield return item2;
				}
			}
		}
		private static IEnumerable<string> GetFilesInFolder(string dir, string searchpattern, bool continueOnUnauthorizedException)
		{
			try
			{
				return Directory.GetFiles(dir, searchpattern, SearchOption.TopDirectoryOnly);
			}
			catch (UnauthorizedAccessException ex)
			{
				
				if (!continueOnUnauthorizedException)
				{
					throw ex;
				}
			}
			return new string[0];
		}
		private static IEnumerable<string> GetDirectoriesInFolder(string dir, bool continueOnUnauthorizedException)
		{
			try
			{
				return Directory.GetDirectories(dir);
			}
			catch (UnauthorizedAccessException ex)
			{
				//$"{dir}:{ex}".HToConsole();
				if (!continueOnUnauthorizedException)
				{
					throw ex;
				}
			}
			return new string[0];
		}
	}
}
