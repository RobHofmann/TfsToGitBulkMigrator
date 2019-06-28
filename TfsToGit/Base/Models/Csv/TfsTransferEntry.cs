using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TfsToGit.Base.Models.Csv
{
    internal class TfsTransferEntry
    {
        public string SourceProjectName { get; set; }
        public string SourceSubProjectName { get; set; }
        public string SourceBranchName { get; set; }
        public string TargetProjectName { get; set; }
        public string TargetRepoName { get; set; }
        public bool TransferedSuccessfully { get; set; }
    }
}
