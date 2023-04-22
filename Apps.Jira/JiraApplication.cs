using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Jira
{
    internal class JiraApplication : IApplication
    {
        public string Name
        {
            get => "Jira";
            set { }
        }

        public T GetInstance<T>()
        {
            throw new NotImplementedException();
        }
    }
}
