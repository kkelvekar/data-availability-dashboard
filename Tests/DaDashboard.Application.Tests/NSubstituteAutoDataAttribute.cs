using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.Application.Tests
{
    public class NSubstituteAutoDataAttribute : AutoDataAttribute
    {
        public NSubstituteAutoDataAttribute()
            : base(() =>
            {
                var customization = new AutoNSubstituteCustomization();
                var fixture = new Fixture();
                fixture.Customize(customization);
                return fixture;
            })
        {
        }
    }
}
