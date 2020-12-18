namespace LocalSettings.Tests
{
    public class ComplexTestClass
    {
        public string Name { get; set; } = "my custom name";
        public int IntegerValue { get; set; } = 42;

        public string[] StringCollection { get; set; } = new[]
        {
            "value 1",
            "Value 2",
        };
    }
}