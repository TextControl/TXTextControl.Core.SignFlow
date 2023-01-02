using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;
using TXTextControl;
using TXTextControl.DocumentServer.ESign.Models;

namespace tx_viewer_custom_signing.Controllers {
	public class HomeController : Controller {

		public IActionResult Index() {
			List<DocumentFlow> DocumentFlows = new List<DocumentFlow>();

			// read all envelope structures
			foreach (string info in Directory.EnumerateDirectories("App_Data/documentflows/")) {
				DocumentFlow flow = new DocumentFlow(new DirectoryInfo(info).Name);
				DocumentFlows.Add(flow);
			}

			// return view with envelope structures
			return View(DocumentFlows);
		}

		public IActionResult Thanks() {
			return View();
		}

      public IActionResult New() {
         return View();
      }

   }
}