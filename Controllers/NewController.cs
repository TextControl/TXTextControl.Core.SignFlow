using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;
using System.Text.Json;
using TXTextControl;
using TXTextControl.DocumentServer.ESign.Models;

namespace tx_viewer_custom_signing.Controllers {
   public class NewController : Controller {

      public IActionResult Index() {
         return View();
      }

      public IActionResult Add(DocumentFlow flow, string id) {
         flow.EnvelopeId = id;
         flow.Save();

         return RedirectToAction("Index", "Home");
      }

      public IActionResult Remove(string id) {

         if (Directory.Exists("App_Data/documentflows/" + id) == true) {
            Directory.Delete("App_Data/documentflows/" + id, true);
         }

         return RedirectToAction("Index", "Home");
      }

      public IActionResult Download(string id) {

         if (Directory.Exists("App_Data/documentflows/" + id) == true) {
            byte[] results = System.IO.File.ReadAllBytes("App_Data/documentflows/" + id + "/results.pdf");
            return File(results, "application/pdf");
         }

         return null;
      }

      public IActionResult Create(IFormFile document) {

         if (document == null) {
            return BadRequest("No document uploaded or document too large.");
         }

         byte[] fileBytes;

         using (MemoryStream stream = new MemoryStream()) {
            document.CopyTo(stream);
            fileBytes = stream.ToArray();
         }

         DocumentFlow flow = new DocumentFlow();

         using (TXTextControl.ServerTextControl tx = new TXTextControl.ServerTextControl()) {
            tx.Create();

            try {

               tx.Load(fileBytes, TXTextControl.BinaryStreamType.InternalUnicodeFormat);

               if (tx.SignatureFields.Count == 0)
                  return BadRequest("There are no SignatureFields in the uploaded document.");

               flow.EnvelopeId = Guid.NewGuid().ToString();

               foreach (SignatureField field in tx.SignatureFields) {

                  if (field.Name == "")
                     field.Name = Guid.NewGuid().ToString();

                  if (flow.Signers.Find(i => i.SignatureFieldName == field.Name) == null) {
                     flow.Signers.Add(new Signer() {
                        SignatureFieldName = field.Name,
                        Id = Guid.NewGuid().ToString()
                  });
                  }

               }

               System.IO.Directory.CreateDirectory("App_Data/documentflows/" + flow.EnvelopeId);
               tx.Save("App_Data/documentflows/" + flow.EnvelopeId + "/template.tx", StreamType.InternalUnicodeFormat);

            }
            catch {
               return BadRequest("Document type not valid. It must be a document in the internal Text Control format.");
            }
         }

         return View(flow);

      }
   }
}