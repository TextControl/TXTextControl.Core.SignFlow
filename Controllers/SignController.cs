using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TXTextControl;
using TXTextControl.DocumentServer.ESign.Models;
using TXTextControl.Web.MVC.DocumentViewer.Models;

namespace tx_viewer_custom_signing.Controllers {
	public class SignController : Controller {

      public IActionResult Index(string envelopeId, string signerId) {
         
         DocumentFlow documentFlow = new DocumentFlow(envelopeId);

         Signer signer = documentFlow.Signers.Find(x => x.Id == signerId);

         SignProcess process = new SignProcess() {
            EnvelopeId = documentFlow.EnvelopeId,
            Owner = documentFlow.Owner,
            Signer = signer
         };

         return View(process);
      }

      [HttpPost]
      public IActionResult Sign([FromBody] SignatureData data, string signerId) {
         
         var test = data.SignedDocument.Document;

         DocumentFlow flow = new DocumentFlow(data.UniqueId);
         Signer signer = flow.Signers.Find(x => x.Id == signerId);

         signer.SignatureComplete = true;
         
         flow.Save();

         byte[] imageData = Convert.FromBase64String(data.SignatureImage);
         System.IO.File.WriteAllBytes("App_Data/documentflows/" + flow.EnvelopeId + "/signature_" + signer.Id + ".svg", imageData);

         bool allSigned = flow.Signers.All(y => y.SignatureComplete == true);

         if (allSigned == true) {

            using (TXTextControl.ServerTextControl tx = new TXTextControl.ServerTextControl()) {
               tx.Create();

               // load the signed document
               tx.Load("App_Data/documentflows/" + flow.EnvelopeId + "/template.tx", TXTextControl.StreamType.InternalUnicodeFormat);

               List<TXTextControl.DigitalSignature> digitalSignatures = new List<DigitalSignature>();

               foreach (SignatureField field in tx.SignatureFields) {
                  signer = flow.Signers.Find(x => x.SignatureFieldName == field.Name);
                  field.Image = new SignatureImage("App_Data/documentflows/" + flow.EnvelopeId + "/signature_" + signer.Id + ".svg", 4);
                  field.SignerData = new SignerData(signer.Name, "", "", "", "Text Control Signature Demo");

                  // provide unique field name
                  field.Name = field.Name + "_" + Guid.NewGuid().ToString();

                  digitalSignatures.Add(new DigitalSignature(
                     new System.Security.Cryptography.X509Certificates.X509Certificate2(
                        "App_Data/textcontrolself.pfx", "123"), null, field.Name));
                }

               SaveSettings saveSettings = new SaveSettings() {
                  SignatureFields = digitalSignatures.ToArray()
               };

               tx.Save("App_Data/documentflows/" + flow.EnvelopeId + "/results.pdf", TXTextControl.StreamType.AdobePDF, saveSettings);

               flow.Closed = true;
               flow.Save();
            }
         }

         return Ok();
      }



   }
}