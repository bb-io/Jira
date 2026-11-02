using System.Diagnostics;
using Apps.Jira;
using Apps.Jira.Actions;
using Apps.Jira.Models.Requests;
using Apps.Jira.Models.Responses;
using RestSharp;
using Tests.Jira.Base;

namespace Tests.Jira;

[TestClass]
public class UserTests : TestBase
{
    private const string NonExistingEmail = "non-existing-user-4f1c2b@example.com";

    [TestMethod]
    public async Task FindUserByEmail_LastUserInDirectory_ReturnsUser()
    {
        var allUsers = await GetAllUsers();
        var targetUser = allUsers.Last(IsActiveAtlassianUserWithEmail);
        var actions = new UserActions(InvocationContext);

        var (foundUser, elapsedMilliseconds, requestCount) =
            await Measure(() => actions.FindUserByEmail(new UserEmailRequest { Email = targetUser.EmailAddress.ToUpperInvariant() }));

        Console.WriteLine($"Users in directory: {allUsers.Count}");
        Console.WriteLine($"Find user by email (existing user): {elapsedMilliseconds} ms, {requestCount} HTTP requests");
        Assert.IsNotNull(foundUser);
        Assert.AreEqual(targetUser.AccountId, foundUser.AccountId);
    }

    [TestMethod]
    public async Task FindUserByEmail_NonExistingEmail_ReturnsNull()
    {
        var actions = new UserActions(InvocationContext);

        var (foundUser, elapsedMilliseconds, requestCount) =
            await Measure(() => actions.FindUserByEmail(new UserEmailRequest { Email = NonExistingEmail }));

        Console.WriteLine($"Find user by email (non-existing user): {elapsedMilliseconds} ms, {requestCount} HTTP requests");
        Assert.IsNull(foundUser);
    }

    private static async Task<(T Result, long ElapsedMilliseconds, int RequestCount)> Measure<T>(Func<Task<T>> action)
    {
        using var requestCounter = new HttpRequestCounter();
        var stopwatch = Stopwatch.StartNew();
        var result = await action();
        stopwatch.Stop();
        return (result, stopwatch.ElapsedMilliseconds, requestCounter.Count);
    }

    private async Task<List<UserDto>> GetAllUsers()
    {
        var client = new JiraClient(Creds);
        var allUsers = new List<UserDto>();
        const int pageSize = 1000;

        while (true)
        {
            var request = new JiraRequest($"/users/search?maxResults={pageSize}&startAt={allUsers.Count}", Method.Get);
            var page = await client.ExecuteWithHandling<List<UserDto>>(request);

            if (page == null || page.Count == 0)
                return allUsers;

            allUsers.AddRange(page);
        }
    }

    private static bool IsActiveAtlassianUserWithEmail(UserDto user) =>
        user.Active
        && string.Equals(user.AccountType, "atlassian", StringComparison.OrdinalIgnoreCase)
        && !string.IsNullOrWhiteSpace(user.EmailAddress);
}
