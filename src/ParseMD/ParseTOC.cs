using Markdig;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blazored.Menu;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Web;

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

			foreach (var file in parent.GetFiles()) {
				string html = writeHTMLFile(publish.FullName, file);

				MenuItem childItem = buildMenuItem("/docs", file, componentCnt++, true);
				childItem.MenuItems = null;

				menuItems.Add(childItem);
			}

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
				sw.Write(JsonConvert.SerializeObject(sortMenuItems(menuItems)));
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

				if (elements.Count() > 1) { //only add subelements are there are multiple
					childItem.MenuItems = new List<MenuItem>();
					int hTagCnt = 0;
					foreach (HtmlNode node in elements) {
						MenuItem hMenuItem = buildMenuItem(node, hTagCnt++);

						if(hMenuItem != null)
							childItem.MenuItems.Add(hMenuItem);
					}

					childItem.MenuItems = sortMenuItems(childItem.MenuItems);
				}

				items.Add(childItem);
			}

			return sortMenuItems(items);
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
		private List<MenuItem> sortMenuItems(List<MenuItem> items) {
			items.Sort((x, y) => x.Position.CompareTo(y.Position));
			return items;
		}
		private IEnumerable<HtmlNode> parseHtmlNodes(string html) {
			var doc = new HtmlDocument();
			doc.LoadHtml(html);

			var h1Elements = doc.DocumentNode.Descendants("h2");
			var h2Elements = doc.DocumentNode.Descendants("h3");
			return h1Elements.Union(h2Elements);
		}
		private MenuItem buildMenuItem(DirectoryInfo dir, int cnt) {
			int? pos = parsePositionFromFileName(dir.Name);
			string key = (pos.HasValue ? dir.Name.Replace(pos.Value + "-", "") : dir.Name);
			string title = "key";

			if (componentLookup.ContainsKey(key))
				title = componentLookup[key];

			return new MenuItem() {
				Title = title,
				Link = "/docs/" + HttpUtility.UrlEncode(dir.Name),
				MatchLink = "All",
				Position = (pos.HasValue ? pos.Value : cnt)
			};
		}
		private MenuItem buildMenuItem(string parentDirName, FileInfo file, int cnt, bool asHTML = false) {
			if (file.Name.ToLower().Equals("overview.md"))
				return null;

			int? pos = parsePositionFromFileName(file.Name);

			return new MenuItem() {
				Title = parseTitleFromFileName(file.Name),
				Link = parentDirName+"/"+ HttpUtility.UrlEncode(file.Name.Replace(".md", (asHTML ? ".html" : ""))),
				MatchLink = "All",
				Position = (pos.HasValue ? pos.Value : cnt)
			};
		}
		private MenuItem buildMenuItem(HtmlNode node, int cnt) {
			if (node.InnerText.ToLower().Equals("usage"))
				return null;

			return new MenuItem() {
				Title = node.InnerText,
				Link = "#" + node.Attributes["id"].Value,
				MatchLink = "All",
				Position = cnt
			};
		}
		private string parseTitleFromFileName(string fileName)
		{
			string str = fileName.Replace(".md", "").Replace("-", " ");
			string ret = "";
			int idx = 0;
			foreach (string s in str.Split(" "))
			{
				if (componentLookup.ContainsKey(s)) //don't include the component name in title
					continue;

				int t;
				if (idx == 0 && int.TryParse(s, out t)) //if the item has a position index, don't add to title
					continue;

				string decoded = HttpUtility.UrlDecode(s);
				ret += Char.ToUpperInvariant(decoded[0]) + decoded.Substring(1).ToLower() + " ";
				idx++;
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
			{"connectors","Service Connectors" },
			{"discovery","Service Discovery" },
			{"fileshares","Network File Sharing" },
			{"introduction","Introduction" },
			{"logging","Dynamic Logging" },
			{"management","Cloud Management" },
			{"security","Cloud Security" },
			{"developer-tools","Developer Tools" },
			{"welcome","Welcome" }
		};
	}
}
