using Markdig;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Blazored.Menu;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace ParseMD
{
	class ParseTOC
	{
		string _parentDirPath;
		string _publishDirPath;

		public ParseTOC(string parentDirPath, string publishDirPath) {
			if (! Directory.Exists(parentDirPath))
				throw new Exception("Parent path no found");

			_parentDirPath = parentDirPath;
			_publishDirPath = publishDirPath;
		}
		public void Run()
		{
			DirectoryInfo publish = new DirectoryInfo(_publishDirPath);
			DirectoryInfo parent = new DirectoryInfo(_parentDirPath);

			if (Directory.Exists(_publishDirPath)) { publish.Delete(true);  }
			publish.Create();
			List<MenuItem> menuItems = new List<MenuItem>();
			int componentCnt = 0;
			foreach (var componentDir in parent.GetDirectories())
			{
				DirectoryInfo componentPublishDir = publish.CreateSubdirectory(componentDir.Name);
				MenuItem rootItem = new MenuItem()
				{
					Title = componentLookup[componentDir.Name],
					Link = "/docs/" + componentDir.Name,
					MatchLink = "All",
					Position = componentCnt++
				};

				FileInfo[] files = componentDir.GetFiles();
				if (files == null || files.Length < 1){
					menuItems.Add(rootItem);
					continue;
				}

				List<MenuItem> childItems = new List<MenuItem>();
				int fileCnt = 0;
				foreach (var componentFile in files)
				{
					MenuItem childItem = new MenuItem()
					{
						Title = parseTitleFromFileName(componentFile.Name),
						Link = "/docs/" + componentDir.Name + "/" + componentFile.Name.Replace(".md", ""),
						MatchLink = "Prefix",
						Position = fileCnt++
					};

					string fileContent = File.ReadAllText(componentFile.FullName);
					string html = RenderHtmlContent(fileContent);
					string filePath = Path.Combine(componentPublishDir.FullName, componentFile.Name.Replace(".md", ".html"));

					using (StreamWriter sw = File.CreateText(filePath)){
						sw.Write(html);
					}
					
					var doc = new HtmlDocument();
					doc.LoadHtml(html);
					var h1Elements = doc.DocumentNode.Descendants("h2");
					//var h2Elements = doc.DocumentNode.Descendants("h3");
					var elements = h1Elements;//.Union(h2Elements);

					if (elements.Count() < 1)
					{
						childItem.MenuItems = null;
						childItems.Insert(0, childItem);
						continue;
					}
					
					childItem.MenuItems = new List<MenuItem>();
					int hTagCnt = 0;
					foreach (HtmlNode node in elements)
					{
						//Console.WriteLine(s.InnerText);
						childItem.MenuItems.Add(new MenuItem()
						{
							Title = node.InnerText,
							Link = "#" + node.Attributes["id"].Value,
							MatchLink = "Prefix",
							Position = hTagCnt++
						});
					}

					childItems.Add(childItem);
				}

				rootItem.MenuItems = childItems;
				menuItems.Add(rootItem);
			}

			Console.Write(JsonConvert.SerializeObject(menuItems));

			using (StreamWriter sw = File.CreateText(Path.Combine(_publishDirPath, "toc.json")))
			{
				sw.Write(JsonConvert.SerializeObject(menuItems));
			}
		}
		private string parseTitleFromFileName(string fileName)
		{
			string str = fileName.Replace(".md", "").Replace("-", " ");
			string ret = "";
			foreach (string s in str.Split(" "))
			{
				if (componentLookup.ContainsKey(s))
					continue;

				ret += Char.ToUpperInvariant(s[0]) + s.Substring(1).ToLower() + " ";
			}

			return ret.Trim();
		}
		private string parseHeaderValue(string line)
		{
			



			Match m = Regex.Match(line, @"<h1(.*)>\s*(.+?)\s*</h1>");
			
			if (!m.Success)
				return null;
			Console.WriteLine(line);
			Console.WriteLine("[MATCH]:" + m.Value);
			return m.Value;
		}

		private string RenderHtmlContent(string value) => Markdig.Markdown.ToHtml(
			markdown: value,
			pipeline: new MarkdownPipelineBuilder()
												.UseBootstrap()
												.UseAdvancedExtensions()
												.Build()
		);

		private Dictionary<string, string> componentLookup = new Dictionary<string, string>()
		{
			{"circuitbreaker","Circuit Breaker" },
			{"configuration","Application Configuration" },
			{"connectors","Cloud Connectors" },
			{"discovery","Service Discovery" },
			{"fileshares","Network File Sharing" },
			{"introduction","Introduction" },
			{"logging","Dynamic Logging" },
			{"management","Cloud Management" },
			{"security","Security Providers" }
		};
	}
}
