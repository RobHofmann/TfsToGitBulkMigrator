using System;
using System.Collections.Generic;
using System.Text;

namespace TfsToGit.Base.Models.Configuration
{
    public class SourceConnectionConfiguration
    {
        public string Type { get; set; }
        public string AzureDevOpsUrl { get; set; }
        public string AzureDevOpsCollectionName { get; set; }
    }
}
