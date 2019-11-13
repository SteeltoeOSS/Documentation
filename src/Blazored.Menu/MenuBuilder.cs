using Microsoft.AspNetCore.Components.Routing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Blazored.Menu
{
	public class MenuBuilder
	{
		private List<MenuItem> _menuItems;

		public MenuBuilder()
		{
			_menuItems = new List<MenuItem>();
		}
		public MenuBuilder(List<MenuItem> menuItems)
		{
			_menuItems = menuItems;
		}
		public List<MenuItem> MenuItems { get { return _menuItems; }}

    //public MenuBuilder AddItem(int position, string title, string link, NavLinkMatch match = NavLinkMatch.Prefix, bool isVisible = true, bool isEnabled = true, string onclick = "")
    //{
    //    var menuItem = new MenuItem
    //    {
    //        Position = position,
				//		OnClick = onclick,
				//		Link = link,
				//		Title = title,
				//		Match = match,
    //        IsVisible = isVisible,
    //        IsEnabled = isEnabled
    //    };

    //    _menuItems.Add(menuItem);

    //    return this;
    //}

    //public MenuBuilder AddSubMenu(int position, string title, string link, List<MenuItem> menuItems, bool isVisible = true, bool isEnabled = true)
    //{
    //    var menuItem = new MenuItem();
    //    menuItem.Position = position;
    //    menuItem.Title = title;
    //    menuItem.MenuItems = menuItems;
    //    menuItem.IsVisible = isVisible;
    //    menuItem.IsEnabled = isEnabled;
				//menuItem.Link = link;

    //    _menuItems.Add(menuItem);
    //    return this;
    //}

    internal List<MenuItem> Build(Func<MenuItem, int> orderBy)
    {
        var menuItems = _menuItems.OrderBy(orderBy);
				
        return menuItems.ToList();
		}
  }

  public class MenuItem
  {
		public int Position { get; set; }
		public string Link { get; set; }
		public string Title { get; set; }
		public NavLinkMatch Match { get; set; } = NavLinkMatch.All;
		public List<MenuItem> MenuItems { get; set; }
		public bool IsVisible { get; set; } = true;
		public bool IsEnabled { get; set; } = true;
  }
	
}
