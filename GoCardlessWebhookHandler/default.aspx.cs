using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GoCardless;
using GoCardless.Exceptions;
using GoCardless.Resources;

namespace GoCardlessWebhookHandler
{
    public partial class _default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // start ngrok like follows
            // ngrok http --host-header=rewrite localhost:12193
            var requestBody = Request.InputStream;
            var requestJson = new StreamReader(requestBody).ReadToEnd();
            var secret = ConfigurationManager.AppSettings["GoCardlessWebhookSecret"];
            var signature = Request.Headers["Webhook-Signature"] ?? "";

            foreach (Event evt in WebhookParser.Parse(requestJson, secret, signature))
            {
                switch (evt.Action)
                {
                    case "created":
                        System.Diagnostics.Debug.WriteLine("Mandate " + evt.Links.Mandate + " has been created, yay!");
                        break;
                    case "cancelled":
                        System.Diagnostics.Debug.WriteLine("Mandate " + evt.Links.Mandate + " has been cancelled");
                        break;
                }
            }
        }
    }
}