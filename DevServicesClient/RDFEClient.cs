//-----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------

namespace DevServicesClient
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using System.Xml;
    using System.Xml.Linq;

    public class RDFEClient
    {
        private const string XMSVersionHeader = "x-ms-version";

        private const string XMSVersionHeaderValue = "2014-12-01";

        private const string ManagementUrl = "https://management.core.windows.net/";

        public string ManagementCertificateThumbprint { get; private set; }

        public string SubscriptionId { get; private set; }

        public string CloudServiceName { get; private set; }

        public RDFEClient(string managementCertificateThumbprint, string subscriptionId, string cloudServiceName)
        {
            this.ManagementCertificateThumbprint = managementCertificateThumbprint;
            this.SubscriptionId = subscriptionId;
            this.CloudServiceName = cloudServiceName;
        }

        public XElement GetCloudService(bool shouldPrint = true)
        {
            using (var handler = new WebRequestHandler())
            {
                handler.ClientCertificates.Add(this.GetCertificate());
                using (var client = new HttpClient(handler))
                {
                    var request = new HttpRequestMessage(
                        HttpMethod.Get,
                        ManagementUrl + this.SubscriptionId +
                        "/cloudservices/" + this.CloudServiceName);
                    request.Headers.Add(XMSVersionHeader, XMSVersionHeaderValue);

                    var response = client.SendAsync(request).Result;

                    if (shouldPrint)
                    {
                        Console.WriteLine("Status Code:" + response.StatusCode);
                    }

                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrWhiteSpace(responseContent))
                    {
                        var parsedResponse = XElement.Parse(responseContent);
                        if (shouldPrint)
                        {
                            Console.WriteLine(parsedResponse);
                        }

                        return parsedResponse;
                    }

                    return null;
                }
            }
        }

        public void GetDevService(string resourceName)
        {
            var cloudService = this.GetCloudService(shouldPrint: false);
            if (cloudService != null)
            {
                var document = new XmlDocument();
                document.LoadXml(cloudService.ToString());

                var ns = new XmlNamespaceManager(document.NameTable);
                ns.AddNamespace("azure", "http://schemas.microsoft.com/windowsazure");

                var resource = document.SelectSingleNode("//azure:Resource[azure:Name='" + resourceName + "']", ns);
                Console.WriteLine(XElement.Parse(resource.OuterXml));
            }
        }

        public void DeleteDevService(string resourceProviderNamespace, string resourceType, string resourceName)
        {
            using (var handler = new WebRequestHandler())
            {
                handler.ClientCertificates.Add(this.GetCertificate());
                using (var client = new HttpClient(handler))
                {
                    var request = new HttpRequestMessage(
                        HttpMethod.Delete,
                        ManagementUrl + this.SubscriptionId +
                        "/cloudservices/" + this.CloudServiceName +
                        "/resources/" + resourceProviderNamespace +
                        "/" + resourceType +
                        "/" + resourceName);
                    request.Headers.Add(XMSVersionHeader, XMSVersionHeaderValue);

                    var response = client.SendAsync(request).Result;
                    Console.WriteLine("Status Code:" + response.StatusCode);

                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrWhiteSpace(responseContent))
                    {
                        var parsedResponse = XElement.Parse(responseContent);
                        Console.WriteLine(parsedResponse);
                    }
                }
            }
        }

        public void UpdateDevService(string resourceProviderNamespace, string resourceType, string resourceName, string pathToXMLDefinition)
        {
            var xmlContent = File.ReadAllLines(pathToXMLDefinition);

            using (var handler = new WebRequestHandler())
            {
                handler.ClientCertificates.Add(this.GetCertificate());
                using (var client = new HttpClient(handler))
                {
                    var request = new HttpRequestMessage(
                        HttpMethod.Put,
                        ManagementUrl + this.SubscriptionId +
                        "/cloudservices/" + this.CloudServiceName +
                        "/resources/" + resourceProviderNamespace +
                        "/" + resourceType +
                        "/" + resourceName);
                    request.Headers.Add(XMSVersionHeader, XMSVersionHeaderValue);

                    var response = client.SendAsync(request).Result;
                    Console.WriteLine("Status Code:" + response.StatusCode);

                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrWhiteSpace(responseContent))
                    {
                        var parsedResponse = XElement.Parse(responseContent);
                        Console.WriteLine(parsedResponse);
                    }
                }
            }
        }

        public void GetSsoToken(string resourceProviderNamespace, string resourceType, string resourceName)
        {
            using (var handler = new WebRequestHandler())
            {
                handler.ClientCertificates.Add(this.GetCertificate());
                using (var client = new HttpClient(handler))
                {
                    var request = new HttpRequestMessage(
                        HttpMethod.Post,
                        ManagementUrl + this.SubscriptionId +
                        "/cloudservices/" + this.CloudServiceName +
                        "/resources/" + resourceProviderNamespace +
                        "/" + resourceType +
                        "/" + resourceName +
                        "/SsoToken");
                    request.Headers.Add(XMSVersionHeader, XMSVersionHeaderValue);

                    var response = client.SendAsync(request).Result;

                    Console.WriteLine("Status Code:" + response.StatusCode);

                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrWhiteSpace(responseContent))
                    {
                        var parsedResponse = XElement.Parse(responseContent);
                        Console.WriteLine(parsedResponse);
                        Console.WriteLine("The URI is URL encoded. Please decode it before use.");
                    }
                }
            }
        }

        private X509Certificate2 GetCertificate()
        {
            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            var certs = store.Certificates.Find(X509FindType.FindByThumbprint, this.ManagementCertificateThumbprint, false);

            if (certs.Count < 1)
            {
                throw new InvalidOperationException("Please install the management certificate in CurrentUser\\My");
            }

            return certs[0];
        }
    }
}
