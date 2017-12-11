//-----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------

namespace DevServicesClient
{
    using System;
    using System.IO;

    public class Program
    {
        public static void Main(string[] args)
        {
            if (args == null)
            {
                PrintHelp();
                return;
            }

            if (args.Length < 4)
            {
                PrintHelp();
                throw new ArgumentException("Invalid arguments.");
            }

            var managementCertificateThumbprint = args[0];
            var operation = args[1];
            var subscriptionId = args[2];
            var cloudServiceName = args[3];

            if (string.IsNullOrWhiteSpace(managementCertificateThumbprint))
            {
                throw new ArgumentNullException("managementCertificateThumbprint");
            }

            if (string.IsNullOrEmpty(operation))
            {
                throw new ArgumentNullException("Please specify operation.");
            }

            if (string.IsNullOrWhiteSpace(subscriptionId))
            {
                throw new ArgumentNullException("subscriptionId");
            }

            if (string.IsNullOrWhiteSpace(cloudServiceName))
            {
                throw new ArgumentNullException("cloudServiceName");
            }

            var rdfeClient = new RDFEClient(
                managementCertificateThumbprint: args[0],
                subscriptionId: args[2],
                cloudServiceName: args[3]);

            if (operation.Equals("GetCloudService", StringComparison.OrdinalIgnoreCase))
            {
                rdfeClient.GetCloudService();
                return;
            }

            var resourceProviderNamespace = args[4];
            var resourceType = args[5];
            var resourceName = args[6];

            if (string.IsNullOrWhiteSpace(resourceProviderNamespace))
            {
                throw new ArgumentNullException("resourceProviderNamespace");
            }

            if (string.IsNullOrWhiteSpace(resourceType))
            {
                throw new ArgumentNullException("resourceType");
            }

            if (string.IsNullOrWhiteSpace(resourceName))
            {
                throw new ArgumentNullException("resourceName");
            }

            if (operation.Equals("GetResource", StringComparison.OrdinalIgnoreCase))
            {
                rdfeClient.GetDevService(resourceName);
                return;
            }

            if (operation.Equals("Update", StringComparison.OrdinalIgnoreCase))
            {
                var pathToXMLDefinition = args[8];
                if (string.IsNullOrWhiteSpace(pathToXMLDefinition))
                {
                    throw new ArgumentNullException("pathToXMLDefinition");
                }

                if (!File.Exists(pathToXMLDefinition))
                {
                    throw new ArgumentException("Unable to read file. It is likely that the path is incorrect.");
                }

                rdfeClient.UpdateDevService(resourceProviderNamespace, resourceType, resourceName, pathToXMLDefinition);
                return;
            }

            if (operation.Equals("Delete", StringComparison.OrdinalIgnoreCase))
            {
                rdfeClient.DeleteDevService(resourceProviderNamespace, resourceType, resourceName);
                return;
            }

            if (operation.Equals("GetSsoToken", StringComparison.OrdinalIgnoreCase))
            {
                rdfeClient.GetSsoToken(resourceProviderNamespace, resourceType, resourceName);
                return;
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine("================ HELP ==================");
            Console.WriteLine("Argument 1: Management certificate thumbprint.");
            Console.WriteLine("Argument 2: Operation. Supported values: GetCloudService, GetResource, Update, Delete, GetSsoToken");
            Console.WriteLine("Argument 3: Subscription ID.");
            Console.WriteLine("Argument 4: Cloud service name.");
            Console.WriteLine("Argument 5: Resource provider namespace.");
            Console.WriteLine("Argument 6: Resource type.");
            Console.WriteLine("Argument 7: Resource name.");
            Console.WriteLine("Argument 8: Path to resource file to update the resource definition.");
        }
    }
}
