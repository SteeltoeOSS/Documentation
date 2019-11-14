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

    public List<MenuItem> Build(Func<MenuItem, int> orderBy)
    {
        //var menuItems = _menuItems.OrderBy(orderBy);
        return _menuItems.ToList();
		}
  }

  public class MenuItem
  {
		public int Position { get; set; }
		public string Link { get; set; }
		public string Title { get; set; }
		public NavLinkMatch Match { get; set; } = NavLinkMatch.Prefix;
		public List<MenuItem> MenuItems { get; set; }
		public bool IsVisible { get; set; } = true;
		public bool IsEnabled { get; set; } = true;
		public string MatchLink { get; set; }

	}
	
}
