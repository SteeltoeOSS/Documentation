using System;

namespace ParseMD
{
	class Program
	{
		static void Main(string[] args)
		{
			ParseTOC p = new ParseTOC(@"C:\Users\ddieruf\source\Steeltoe\Documentation\docs", @"C:\Users\ddieruf\source\Steeltoe\MainSite\src\Client\wwwroot\site-data\docs");
			p.Run();
		}
	}
}
