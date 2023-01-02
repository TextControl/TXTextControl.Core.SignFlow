
using System.Security.Policy;
using System.Text.Json;

namespace TXTextControl.DocumentServer.ESign.Models {
   // DocumentFlow class to store and handle envelope structures
   public class DocumentFlow {

      public DocumentFlow() { }

      public DocumentFlow(string EnvelopeId) {
         string jsonFlow = System.IO.File.ReadAllText(
              "App_Data/documentflows/" + EnvelopeId + "/documentflow.json");

         var df = Newtonsoft.Json.JsonConvert.DeserializeObject<DocumentFlow>(jsonFlow);

         this.Signers = df.Signers;
         this.Owner = df.Owner;
         this.EnvelopeId = df.EnvelopeId;
         this.Closed = df.Closed;
      }

      public void Save() {

         if (Directory.Exists("App_Data/documentflows/" + this.EnvelopeId) == false)
            Directory.CreateDirectory("App_Data/documentflows/" + this.EnvelopeId);

         System.IO.File.WriteAllText(
             "App_Data/documentflows/" + this.EnvelopeId + "/documentflow.json",
             Newtonsoft.Json.JsonConvert.SerializeObject(this));
      }

      public string EnvelopeId { get; set; }
      public List<Signer> Signers { get; set; } = new List<Signer>();
      public string Owner { get; set; }
      public bool Closed { get; set; }
   }

   public class Signer {
      public string Id { get; set; }
      public string Name { get; set; }
      public string Initials { get; set; }
      public string SignatureFieldName { get; set; }
      public bool SignatureComplete { get; set; }
   }

   public class SignProcess : DocumentFlow {
      public Signer Signer { get; set; }
   }
}