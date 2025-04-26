using AutoFixture;

namespace Docplanner.Test.Utilities
{
    public class DateOnlyCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<DateOnly>(composer =>
                composer.FromFactory(() =>
                {
                    var randomDate = fixture.Create<DateTime>();
                    return new DateOnly(randomDate.Year, randomDate.Month, randomDate.Day);
                }));
        }
    }
}