namespace CommonTestUtilities.Requests;

public class EnumTestHelper
{
    public static int OutOfRangeEnum<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<int>().Max() + 1;
    }
    public static int EnumRange<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<int>().Max();
    }
}