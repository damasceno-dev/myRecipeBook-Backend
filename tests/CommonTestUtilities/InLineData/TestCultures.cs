using System.Collections;
using MyRecipeBook.Application.Services;

namespace CommonTestUtilities.InLineData;

public static class TestCulturesProvider
{
    public static readonly string[] Cultures = ["fr", "pt-BR", "pt-PT", "en"];
}
public class TestCultures: IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        foreach (var culture in TestCulturesProvider.Cultures)
        {
            yield return new object[] { culture };
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class TestPasswordLengthsAndCultures : IEnumerable<object[]>
{
    private static readonly int MinimalPasswordLength = SharedValidators.MinimumPasswordLength;

    public IEnumerator<object[]> GetEnumerator()
    {
        for (int i = 1; i < MinimalPasswordLength; i++)
        {
            foreach (var culture in TestCulturesProvider.Cultures)
            {
                yield return new object[] { i, culture };
            }
        }
            
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

