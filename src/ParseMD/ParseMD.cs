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
using YamlDotNet.Serialization;

namespace ParseMD
{
	class ParseMD
	{
		private readonly string _parentDirPath;
		private readonly string _publishDirPath;
		private readonly string _tocYamlPath;

		public ParseMD(string parentDirPath, string publishDirPath) {
			if (!Directory.Exists(parentDirPath)) {
				Console.Write("Parent path no found");
				Environment.Exit(1);
			}

			_tocYamlPath = Path.Combine(parentDirPath, "nav-menu.yaml");
			if (!File.Exists(_tocYamlPath)) {
				Console.Write("nav-menu.yaml no found in folder " + parentDirPath);
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
			FileInfo toc = new FileInfo(_tocYamlPath);

			Console.WriteLine("Parsing markdown and creating file/folder structure");
			//Write the directories and convert md to html
			foreach (var componentDir in parent.GetDirectories())
			{
				DirectoryInfo componentPublishDir = publish.CreateSubdirectory(componentDir.Name);

				FileInfo[] files = componentDir.GetFiles();
				foreach (var file in files) {
					WriteHTMLFile(componentPublishDir.FullName, file);
				}
			}

			Console.WriteLine("Parsing yaml and writing toc");
			//Write the table of contents
			TextReader tr = File.OpenText(toc.FullName);
			var deserializer = new Deserializer();
			item[] pageItems = deserializer.Deserialize<item[]>(tr);

			var menuItems = ConvertPageItemsToMenuItems(pageItems);

			Console.WriteLine("Testing toc");
			//test the new file/folder structure with the proposed menu structure
			//TestMenuItems(_publishDirPath, menuItems);

			Console.WriteLine("Writing toc.json");
			using (StreamWriter sw = File.CreateText(Path.Combine(_publishDirPath, "toc.json"))) {
				sw.Write(JsonConvert.SerializeObject(menuItems));
			}

			return;
		}
		private void TestMenuItems(string parentFolderPath, List<MenuItem> menuItems) {
			foreach (var items in menuItems) {
				if (items.Link.IndexOf("#") > -1)
					continue;
				
				string stringRootItmPath = parentFolderPath + items.Link.Replace("/docs", "") + ".html";

				if (!File.Exists(stringRootItmPath)) {
					Console.Error.WriteLine("Menu item path '" + stringRootItmPath + "' not found");
					Environment.Exit(1);
				}

				if (items.MenuItems != null && items.MenuItems.Count() > 0) {
					TestMenuItems(parentFolderPath, items.MenuItems);
				}
			}

			return;
		}
		private void WriteHTMLFile(string destDirName, FileInfo sourceFile) {
			string filePath = Path.Combine(destDirName, sourceFile.Name.Replace(".md", ".html"));
			string fileContent = File.ReadAllText(sourceFile.FullName);
			string html = RenderHtmlContent(fileContent);

			using (StreamWriter sw = File.CreateText(filePath)) {
				sw.Write(html);
			}

			return;
		}
		private string RenderHtmlContent(string value) => Markdig.Markdown.ToHtml(
			markdown: value,
			pipeline: new MarkdownPipelineBuilder()
												.UseBootstrap()
												.UseAdvancedExtensions()
												.Build()
		);
		private List<MenuItem> ConvertPageItemsToMenuItems(item[] pageItems) {
			List<MenuItem> menuItems = new List<MenuItem>();

			foreach (item pageItm in pageItems) {
				MenuItem menuItem = buildMenuItem(pageItm);

				if (pageItm.items != null && pageItm.items.Count() > 0) {
					menuItem.MenuItems = ConvertPageItemsToMenuItems(pageItm.items);
				}

				menuItems.Add(menuItem);
			}

			return menuItems;
		}
		private MenuItem buildMenuItem(item pageItm) {
			return new MenuItem() {
				Title = pageItm.name,
				Link = pageItm.page?.Replace(".md", ".html") ?? "#" + pageItm.page_ref,
				MatchLink = "All"
			};
		}
		private class item {
			public string name { get; set; }
			public string page { get; set; }
			public string page_ref { get; set; }
			public item[] items { get; set; }
		}
	}
}
