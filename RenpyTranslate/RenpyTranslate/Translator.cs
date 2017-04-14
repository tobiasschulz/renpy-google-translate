using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

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

		public static List<string> Run(IEnumerable<string> _lines, Action<string> saveFunc, string filename)
		{
			var lines = _lines.ToArray();
			var result = new List<string>();

			for (int i = 0; i < lines.Length; i++)
			{
				var line = lines[i].TrimEnd();

				if (line.TrimStart().StartsWith("# ") && line.EndsWith("\""))
				{
					var nextLine = lines[i + 1].TrimEnd();

					if (nextLine.EndsWith(" \"\""))
					{
						result.Add(line);
						try
						{
							var a = line.Trim();

							var b = nextLine.Trim();
							a = a.Remove(a.Length - 1);
							b = b.Remove(b.Length - 1);
							if (a.StartsWith("# " + b))
							{
								a = a.Substring(("# " + b).Length);
							}
							else
							{
								Console.WriteLine("a: " + a);
								Console.WriteLine("b: " + b);
								Environment.Exit(0);
							}

							Console.WriteLine("ru: " + a);
							var translated = TranslateString(a);

							Console.WriteLine("en: " + translated);
							nextLine = nextLine.Replace("\"\"", "\"" + translated + "\"");
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex);
						}
						result.Add(nextLine);
						Console.WriteLine();
						i++;
					}
					else
					{
						result.Add(line);

						nextLine = nextLine.Replace("\\ n", "\\n");
						nextLine = nextLine.Replace("{/ ", "{/");
						if (nextLine.Contains("{"))
						{
							nextLine = regex1.Replace(nextLine, match => match.Captures[0].Value.Replace(" ", "").ToLower());
						}
						const string SEP = "XXXXXXXXXXXXXXXXXXX";
						var correctCapitalization = line
							.Replace("[", SEP + "[")
							.Replace("]", "]" + SEP)
							.Split(new[] { SEP }, StringSplitOptions.None)
							.Select(s => s.StartsWith("[", StringComparison.Ordinal) ? s : null)
							.Where(s => !string.IsNullOrEmpty(s))
							.ToArray();
						nextLine = string.Join("",
											   nextLine
											   .Replace("[", SEP + "[")
											   .Replace("]", "]" + SEP)
											   .Split(new[] { SEP }, StringSplitOptions.None)
											   .Select(s => s.StartsWith("[", StringComparison.Ordinal) ? correctCapitalization.FirstOrDefault(s2 => Regex.Replace(s2.ToLower(), "[^A-Za-z0-9 _]", "") == Regex.Replace(s.ToLower(), "[^A-Za-z0-9 _]", "")) : s)
											  );

						var nextVariables = nextLine
							.Replace("[", SEP + "[")
							.Replace("]", "]" + SEP)
							.Split(new[] { SEP }, StringSplitOptions.None)
							.Select(s => s.StartsWith("[", StringComparison.Ordinal) ? s : null)
							.Where(s => !string.IsNullOrEmpty(s))
							.ToArray();

						if (nextVariables.All(nv => correctCapitalization.Contains(nv)) && correctCapitalization.All(pv => nextVariables.Contains(pv)))
						{
							result.Add(nextLine);
							i++;
						}
						else
						{
							File.AppendAllText("variable-errors.txt", filename + ": " + line + "\n" + filename + ": " + nextLine + "\n" + "\n");
						}
					}
				}
				else if (line.TrimStart().StartsWith("old") && line.EndsWith("\""))
				{
					var nextLine = lines[i + 1].TrimEnd();

					if (line.TrimStart().StartsWith("new") && nextLine.EndsWith(" \"\""))
					{
						result.Add(line);
						try
						{
							var a = line.Trim().Replace("old \"", "").Trim();
							var b = nextLine.Trim();
							a = a.Remove(a.Length - 1);
							b = b.Remove(b.Length - 1);

							Console.WriteLine("ru 2: " + a);
							var translated = TranslateString(a);
							Console.WriteLine("en 2: " + translated);
							nextLine = nextLine.Replace("\"\"", "\"" + translated + "\"");
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex);
						}
						result.Add(nextLine);
						Console.WriteLine();
						i++;
					}
					else
					{

						result.Add(line);

						nextLine = nextLine.Replace("\\ n", "\\n");
						nextLine = nextLine.Replace("{/ ", "{/");
						if (nextLine.Contains("{"))
						{
							nextLine = regex1.Replace(nextLine, match => match.Captures[0].Value.Replace(" ", "").ToLower());
						}
						const string SEP = "XXXXXXXXXXXXXXXXXXX";
						var correctCapitalization = line
							.Replace("[", SEP + "[")
							.Replace("]", "]" + SEP)
							.Split(new[] { SEP }, StringSplitOptions.None)
							.Select(s => s.StartsWith("[", StringComparison.Ordinal) ? s : null)
							.Where(s => !string.IsNullOrEmpty(s))
							.ToArray();
						nextLine = string.Join("",
											   nextLine
											   .Replace("[", SEP + "[")
											   .Replace("]", "]" + SEP)
											   .Split(new[] { SEP }, StringSplitOptions.None)
											   .Select(s => s.StartsWith("[", StringComparison.Ordinal) ? correctCapitalization.FirstOrDefault(s2 => Regex.Replace(s2.ToLower(), "[^A-Za-z0-9]", "") == Regex.Replace(s.ToLower(), "[^A-Za-z0-9]", "")) : s)
											  );

						var nextVariables = nextLine
							.Replace("[", SEP + "[")
							.Replace("]", "]" + SEP)
							.Split(new[] { SEP }, StringSplitOptions.None)
							.Select(s => s.StartsWith("[", StringComparison.Ordinal) ? s : null)
							.Where(s => !string.IsNullOrEmpty(s))
							.ToArray();

						if (nextVariables.All(nv => correctCapitalization.Contains(nv)) && correctCapitalization.All(pv => nextVariables.Contains(pv)))
						{
							result.Add(nextLine);
							i++;
						}
						else
						{
							File.AppendAllText("variable-errors.txt", filename + ": " + line + "\n" + filename + ": " + nextLine + "\n" + "\n");
						}
					}
				}
				else
				{
					result.Add(line);
				}
				saveFunc(string.Join("\n", result));
			}

			return result;
		}

		static Regex regex1 = new Regex(@"{(.+?)}");


		static Dictionary<string, string> preTranslate = new Dictionary<string, string>()
		{
			["\\\""] = "\"",
		};

		static Dictionary<string, string> postTranslate = new Dictionary<string, string>()
		{
			["\\ n"] = "\n",
			["preyakulyatom"] = "preejaculation",
			["{U}"] = "{u}",
			["{/ u}"] = "{/u}",
			["\""] = "\\\"",
		};

		public static string TranslateString(string ru)
		{
			foreach (var kv in preTranslate)
			{
				ru = ru.Replace(kv.Key, kv.Value);
			}

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

			foreach (var kv in postTranslate)
			{
				result = result.Replace(kv.Key, kv.Value);
			}

			return result;
		}
	}
}

