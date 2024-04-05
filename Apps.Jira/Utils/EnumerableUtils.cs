namespace Apps.Jira.Utils;

public static class EnumerableUtils
{
    public static Dictionary<string, string> ToDictionary(IEnumerable<string>? first, IEnumerable<string>? second)
    {
        if (first == null && second == null)
        {
            return new Dictionary<string, string>();
        }
        
        if (first == null || second == null)
        {
            throw new ArgumentException("Both collections must be null or not null.");
        }
        
        var firstList = first.ToList();
        var secondList = second.ToList();
        
        if (firstList.Count != secondList.Count)
        {
            throw new ArgumentException("The collections must have the same length.");
        }
        
        return firstList.Zip(secondList, (k, v) => new { k, v })
            .ToDictionary(x => x.k, x => x.v);
    }
}