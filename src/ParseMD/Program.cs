using System;

namespace ParseMD
{
	class Program
	{
		static void Main(string[] args)
		{
			//arg[0] the folder containing documentation markdown
			//arg[1] where to publish the folder holding the parsed HTML and TOC.json
			if(args.Length < 2) {
				Console.WriteLine("2 arguments are expected, the folder containing documentation markdown and where to publish the resulting parsed HTML and TOC.");
				Environment.Exit(1);
			}

			ParseTOC p = new ParseTOC(args[0],args[1]);
			p.Run();

			Environment.Exit(0);
		}
	}
}
