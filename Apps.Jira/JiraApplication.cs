using Blackbird.Applications.Sdk.Common;

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
