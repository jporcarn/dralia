﻿using AutoMapper;
using Docplanner.Application.Mappings.Profiles;
using Xunit;

namespace Docplanner.InfrastructureTests.Unit.SlotService.Mappings.Profiles.Tests
{
    public class AvailabilityProfileTests
    {
        private readonly IMapper _mapper;

        public AvailabilityProfileTests()
        {
            _mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AvailabilityProfile>();
            }).CreateMapper();
        }

        [Fact()]
        public void AvailabilityProfile_Configuration_IsValid()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}