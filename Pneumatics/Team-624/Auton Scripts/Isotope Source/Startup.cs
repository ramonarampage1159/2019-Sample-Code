
using System;
using System.Linq;

namespace Isotope
{
	/// <summary>
	/// Description of Startup.
	/// </summary>
	public class Startup
	{
		[STAThread]
		public static void Main(string[] args)
        {
            App app = new App();
            if(args.Count()>=1)
			{
				app.Run(new Window1(args[0]));
			}
			else
			{
				app.Run(new Window1(""));
			}
        }
	}
}
