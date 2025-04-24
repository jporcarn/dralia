using Xunit.Abstractions;
using Xunit.Sdk;

namespace Docplanner.Test.Utilities
{
    public class PriorityOrderer : ITestCaseOrderer
    {
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
            where TTestCase : ITestCase
        {
            // Order test cases by the Priority attribute
            return testCases.OrderBy(testCase =>
            {
                var priorityAttribute = testCase.TestMethod.Method
                    .GetCustomAttributes(typeof(TestPriorityAttribute))
                    .FirstOrDefault();

                return priorityAttribute == null
                    ? int.MaxValue // Default priority if not specified
                    : priorityAttribute.GetNamedArgument<int>("Priority");
            });
        }
    }

}
