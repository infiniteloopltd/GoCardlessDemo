using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using GoCardless;
using GoCardless.Resources;
using GoCardless.Services;

namespace GoCardlessConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Go Cardless sandbox demo");
            Console.WriteLine("In the UK, use the sort code 200000 and the account number 55779911");
            // Think this requires VS 2017
            GoCardlessClient client = GoCardlessClient.Create(
                // We recommend storing your access token in an
                // configuration setting for security
                ConfigurationManager.AppSettings["GoCardlessAccessToken"],
                // Change me to LIVE when you're ready to go live
                GoCardlessClient.Environment.SANDBOX
            );

            var SessionToken = Guid.NewGuid().ToString();

            var redirectFlowResponse = client.RedirectFlows.CreateAsync(new RedirectFlowCreateRequest()
            {
                Description = "Cider Barrels",
                SessionToken = SessionToken,
                SuccessRedirectUrl = "https://developer.gocardless.com/example-redirect-uri/",
                // Optionally, prefill customer details on the payment page
                PrefilledCustomer = new RedirectFlowCreateRequest.RedirectFlowPrefilledCustomer()
                {
                    GivenName = "Tim",
                    FamilyName = "Rogers",
                    Email = "tim@gocardless.com",
                    AddressLine1 = "338-346 Goswell Road",
                    City = "London",
                    PostalCode = "EC1V 7LQ"
                }
            }).Result;

            var redirectFlow = redirectFlowResponse.RedirectFlow;

            OpenUrl(redirectFlow.RedirectUrl);
            
            Console.WriteLine("Type the redirect_flow_id");
            var redirect_flow_id = Console.ReadLine();

            var redirectFlowResponse2 = client.RedirectFlows
                .CompleteAsync(redirect_flow_id,
                    new RedirectFlowCompleteRequest
                    {
                        SessionToken = SessionToken
                    }
                ).Result;

            Console.WriteLine($"Mandate: {redirectFlowResponse2.RedirectFlow.Links.Mandate}");
            Console.WriteLine($"Customer: {redirectFlowResponse2.RedirectFlow.Links.Customer}");

            OpenUrl(redirectFlowResponse2.RedirectFlow.ConfirmationUrl);

            var mandate = redirectFlowResponse2.RedirectFlow.Links.Mandate;

            var createResponse =  client.Payments.CreateAsync(new PaymentCreateRequest()
            {
                Amount = 1000,
                Currency = PaymentCreateRequest.PaymentCurrency.GBP,
                Links = new PaymentCreateRequest.PaymentLinks()
                {
                    Mandate = mandate,
                },
                Metadata = new Dictionary<string, string>()
                {
                    {"invoice_number", "001"}
                },
                IdempotencyKey = SessionToken
            }).Result;

            Payment payment = createResponse.Payment;

            // Keep hold of this payment ID - we'll use it in a minute
            // It should look like "PM000260X9VKF4"
            Console.WriteLine(payment.Id);


        }

        private static void OpenUrl(string url)
        {
            var proc = new Process
            {
                StartInfo =
                {
                    FileName = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                    Arguments = url
                }
            };
            proc.Start();
        }
    }
}
