using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace RenpyTranslate
{
	public class Translator
	{
		static HttpClient httpClient;

		static Translator()
		{
			httpClient = new HttpClient();
			httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
		}

		public static List<string> Run(IEnumerable<string> _lines, Action<string> saveFunc)
		{
			var lines = _lines.ToArray();
			var result = new List<string>();

			for (int i = 0; i < lines.Length; i++)
			{
				var line = lines[i].TrimEnd();

				if (line.TrimStart().StartsWith("# ") && line.EndsWith("\""))
				{
					var nextNine = lines[i].TrimEnd();

					if (nextNine.EndsWith("\"\""))
					{
						try
						{
							var a = line.Trim();
							var b = nextNine.Trim();
							a = a.Remove(a.Length - 1);
							b = b.Remove(b.Length - 1);
							if (a.StartsWith("# " + b))
							{
								a = a.Substring(("# " + b).Length);
							}

							Console.WriteLine("ru: " + a);
							var translated = TranslateString(a);

							Console.WriteLine("en: " + translated);
							line = nextNine.Remove(nextNine.Length - 1) + translated + "\"";
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex);
						}
						result.Add(line);
						saveFunc(string.Join("\n", result));
						Console.WriteLine();
						i++;
					}
					else
					{
						result.Add(line);
					}
				}
				else if (line.TrimStart().StartsWith("old") && line.EndsWith("\""))
				{
					var nextLine = lines[i].TrimEnd();

					if (line.TrimStart().StartsWith("new") && nextLine.EndsWith("\"\""))
					{
						try
						{
							var a = line.Trim().Replace("old \"", "").Trim();
							var b = nextLine.Trim();
							a = a.Remove(a.Length - 1);
							b = b.Remove(b.Length - 1);

							Console.WriteLine("ru 2: " + a);
							var translated = TranslateString(a);
							Console.WriteLine("en 2: " + translated);
							line = nextLine.Remove(nextLine.Length - 1) + translated + "\"";
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex);
						}
						result.Add(line);
						saveFunc(string.Join("\n", result));
						Console.WriteLine();
						i++;
					}
					else
					{
						result.Add(line);
					}
				}
				else
				{
					result.Add(line);
				}
			}

			return result;
		}

		public static string TranslateString(string ru)
		{
			ru = ru.Replace("\\\"", "\"");
			var url = "https://translate.googleapis.com/translate_a/single?client=gtx&sl=ru&tl=en&dt=t&q=" + WebUtility.UrlEncode(ru);
			Console.WriteLine("url: " + url);
			var jsonBytes = httpClient.GetAsync(url).Result.Content.ReadAsByteArrayAsync().Result;
			var json = System.Text.Encoding.UTF8.GetString(jsonBytes);
			var english = new List<string>();
			foreach (var y in JsonConvert.DeserializeObject<dynamic>(json)[0])
			{
				english.Add(y[0].ToString());
			}
			var result = string.Join(" ", english);
			result = result.Replace("preyakulyatom", "preejaculation");
			result = result.Replace("\"", "\\\"");

			return result;
		}
	}
}

