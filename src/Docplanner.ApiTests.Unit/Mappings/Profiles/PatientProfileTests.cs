using AutoFixture;
using AutoMapper;
using Docplanner.Api.Mappings.Profiles;
using Docplanner.Api.Models;
using Docplanner.Domain.Models;
using FluentAssertions;
using Xunit;

namespace Docplanner.ApiTests.Unit.Mappings.Profiles
{
    public class PatientProfileTests
    {
        private readonly Fixture _fixture;
        private readonly IMapper _mapper;

        public PatientProfileTests()
        {
            // Initialize AutoMapper with the PatientProfile
            var config = new MapperConfiguration(cfg => cfg.AddProfile<PatientProfile>());
            _mapper = config.CreateMapper();

            // Initialize AutoFixture
            _fixture = new Fixture();
        }

        [Fact]
        public void PatientProfile_ConfigurationIsValid()
        {
            // Assert
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Fact]
        public void PatientProfile_ShouldMapPatientRequestToPatientProfile()
        {
            // Arrange
            var patientRequest = _fixture.Create<PatientRequest>();

            // Act
            var result = _mapper.Map<Patient>(patientRequest);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(patientRequest, options => options.ExcludingMissingMembers());
        }
    }
}