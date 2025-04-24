namespace Docplanner.Test.Utilities
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TestPriorityAttribute : Attribute
    {
        public int Priority { get; }

        public TestPriorityAttribute(int priority)
        {
            Priority = priority;
        }
    }
}