using System;
using System.IO;

namespace RenpyTranslate
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			foreach (var file in args)
			{
				if (!File.Exists(file))
				{
					continue;
				}
				if (file.Contains(".original"))
				{
					continue;
				}
				if (file.EndsWith(".rpyc"))
				{
					continue;
				}

				Console.WriteLine("Translate file: " + file);

				var originalFile = file + ".original"; // Path.ChangeExtension(file, ".original" + Path.GetExtension(file));
				if (!File.Exists(originalFile))
				{
					File.Copy(file, originalFile);
				}

				var lines = File.ReadAllLines(originalFile);
				Translator.Run(lines, saveFunc: content => File.WriteAllText(file, content), filename: file);
			}


		}
	}
}
