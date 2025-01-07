using Apps.Jira.DataSourceHandlers.CustomFields;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Tests.Jira.Base;

namespace Tests.Jira
{
    [TestClass]
    public class DataSources : TestBase
    {
        [TestMethod]
        public async Task CustomStringFieldHandlerReturnsValues()
        {
            //Arrange
            var handler = new CustomStringFieldDataSourceHandler(InvocationContext);

            //Act
            var response = await handler.GetDataAsync(new DataSourceContext { SearchString = "" }, CancellationToken.None);

            //Assert
            foreach (var item in response)
            {
                Console.WriteLine($"{item.Value}: {item.Key}");
            }

            Assert.IsNotNull(response);

        }     

    }
}
