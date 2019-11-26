using Markdig;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blazored.Menu;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace ParseMD
{
	class ParseTOC
	{
		private readonly string _parentDirPath;
		private readonly string _publishDirPath;

		public ParseTOC(string parentDirPath, string publishDirPath) {
			if (!Directory.Exists(parentDirPath)) {
				Console.Write("Parent path no found");
				Environment.Exit(1);
			}

			if (Directory.Exists(publishDirPath)) { Directory.Delete(publishDirPath,true); }
			Directory.CreateDirectory(publishDirPath);

			_parentDirPath = parentDirPath;
			_publishDirPath = publishDirPath;
		}
		public void Run()
		{
			DirectoryInfo publish = new DirectoryInfo(_publishDirPath);
			DirectoryInfo parent = new DirectoryInfo(_parentDirPath);

			List<MenuItem> menuItems = new List<MenuItem>();
			int componentCnt = 0;
			
			foreach (var componentDir in parent.GetDirectories())
			{
				DirectoryInfo componentPublishDir = publish.CreateSubdirectory(componentDir.Name);

				MenuItem rootItem = buildMenuItem(componentDir, componentCnt++);

				FileInfo[] files = componentDir.GetFiles();
				if (files != null & files.Length > 0){
					rootItem.MenuItems = parseFiles(componentPublishDir, files);
				}

				if (! componentDir.Name.ToLower().Equals("introduction"))
					menuItems.Add(rootItem);
			}

			using (StreamWriter sw = File.CreateText(Path.Combine(_publishDirPath, "toc.json"))){
				sw.Write(JsonConvert.SerializeObject(menuItems));
			}
		}
		private List<MenuItem> parseFiles(DirectoryInfo parentDir, FileInfo[] files) {
			List<MenuItem> items = new List<MenuItem>();
			int fileCnt = 0;

			foreach (var file in files) {
				string html = writeHTMLFile(parentDir.FullName, file);

				MenuItem childItem = buildMenuItem("/docs/" + parentDir.Name, file, fileCnt++);

				if (childItem == null)
					continue;

				var elements = parseHtmlNodes(html);

				if (elements.Count() > 0) {
					childItem.MenuItems = new List<MenuItem>();
					int hTagCnt = 0;
					foreach (HtmlNode node in elements) {
						childItem.MenuItems.Add(buildMenuItem(node, hTagCnt++));
					}
				}

				items.Add(childItem);
			}

			return items;
		}
		private string writeHTMLFile(string destDirName, FileInfo soureFile) {
			string filePath = Path.Combine(destDirName, soureFile.Name.Replace(".md", ".html"));
			string fileContent = File.ReadAllText(soureFile.FullName);
			string html = RenderHtmlContent(fileContent);

			using (StreamWriter sw = File.CreateText(filePath)) {
				sw.Write(html);
			}

			return html;
		}
		private IEnumerable<HtmlNode> parseHtmlNodes(string html) {
			var doc = new HtmlDocument();
			doc.LoadHtml(html);

			var h1Elements = doc.DocumentNode.Descendants("h2");
			var h2Elements = doc.DocumentNode.Descendants("h3");
			return h1Elements.Union(h2Elements);
		}
		private MenuItem buildMenuItem(DirectoryInfo dir, int cnt) {
			return new MenuItem() {
				Title = componentLookup[dir.Name],
				Link = "/docs/" + dir.Name,
				MatchLink = "All",
				Position = cnt
			};
		}
		private MenuItem buildMenuItem(string parentDirName, FileInfo file, int cnt) {
			if (file.Name.ToLower().Equals("overview.md"))
				return null;

			int? pos = parsePositionFromFileName(file.Name);

			return new MenuItem() {
				Title = parseTitleFromFileName(file.Name),
				Link = parentDirName+"/"+file.Name.Replace(".md", ""),
				MatchLink = "Prefix",
				Position = (pos.HasValue ? pos.Value : cnt)
			};
		}
		private MenuItem buildMenuItem(HtmlNode node, int cnt) {
			return new MenuItem() {
				Title = node.InnerText,
				Link = "#" + node.Attributes["id"].Value,
				MatchLink = "Prefix",
				Position = cnt
			};
		}
		private string parseTitleFromFileName(string fileName)
		{
			string str = fileName.Replace(".md", "").Replace("-", " ");
			string ret = "";
			foreach (string s in str.Split(" "))
			{
				if (componentLookup.ContainsKey(s)) //don't include the component name in title
					continue;

				ret += Char.ToUpperInvariant(s[0]) + s.Substring(1).ToLower() + " ";
			}

			return ret.Trim();
		}
		private int? parsePositionFromFileName(string fileName) {
			if (! fileName.Contains("-"))
				return null;

			string[] splt = fileName.Split("-");

			int pos;
			if (int.TryParse(splt[0], out pos))
				return pos;

			return null;
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
			{"circuitbreaker","Circuit Breakers" },
			{"configuration","Application Configuration" },
			{"connectors","Cloud Connectors" },
			{"discovery","Service Discovery" },
			{"fileshares","Network File Sharing" },
			{"introduction","Introduction" },
			{"logging","Dynamic Logging" },
			{"management","Cloud Management" },
			{"security","Cloud Security" }
		};
	}
}
