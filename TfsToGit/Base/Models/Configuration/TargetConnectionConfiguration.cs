using System;
using System.Collections.Generic;
using System.Text;

namespace TfsToGit.Base.Models.Configuration
{
    public class TargetConnectionConfiguration
    {
        public string AzureDevOpsUrl { get; set; }
        public string PersonalAccessToken { get; set; }
    }
}
