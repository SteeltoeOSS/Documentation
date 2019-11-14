using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Generic;
using System.Diagnostics;

namespace Blazored.Menu
{
    public class BlazoredSubMenuBase : ComponentBase
    {
			[Inject]
			protected NavigationManager NavigationManager { get; set; }

      [Parameter] public string Header { get; set; }
			[Parameter] public string Link { get; set; }
      [Parameter] public string IconClass { get; set; }
      [Parameter] public RenderFragment ChildContent { get; set; }
      [Parameter] public RenderFragment HeaderTemplate { get; set; }
      [Parameter] public RenderFragment MenuTemplate { get; set; } 
      [Parameter] public string Css { get; set; }
      [Parameter] public bool IsEnabled { get; set; } = true;
      [Parameter] public IEnumerable<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
		[Parameter] public string MatchLink { get; set; }

		protected string Icon { get; set; }
      protected bool IsOpen { get; set; }
		public NavLinkMatch Match { get; set; }
		protected string CssString
      {
          get
          {
              var cssString = "blazored-sub-menu-header";

              cssString += $" {Css}";
              cssString += IsOpen ? " open" : "";

              return cssString.Trim();
          }
      }

      protected void ToggleSubMenu()
      {
          IsOpen = !IsOpen;
          //Icon = IsOpen ? "-" : "+";
      }

      /// <summary>
      /// Handler for the key down events
      /// </summary>
      /// <param name="eventArgs">keyboard event</param>
      protected void KeyDownHandler(KeyboardEventArgs eventArgs)
      {
          if (eventArgs.Key == "Enter" || eventArgs.Key == " " || eventArgs.Key == "Spacebar")
          {
              ToggleSubMenu();
          }
      }

			protected override void OnInitialized()
			{
				string path = NavigationManager.Uri;

				Debug.WriteLine("==" + Header);
				Debug.WriteLine(path);
				Debug.WriteLine(Link);

				if (string.IsNullOrEmpty(Link))
					return;

				if (path.Contains(Link)){
					ToggleSubMenu();
				}

				if (!string.IsNullOrEmpty(MatchLink)) {
					if (MatchLink.Equals("All"))
						Match = NavLinkMatch.All;
					else
						Match = NavLinkMatch.Prefix;
				}
			}
			protected override void OnAfterRender(bool firstRender)
			{
			
			}

	}
}
