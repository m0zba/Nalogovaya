
using System;
using Nalogovaya;
using Xunit;

namespace Nalogovaya.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void PastDate_ShouldBeInvalid()
        {
            DateTime yesterday = DateTime.Today.AddDays(-1);

            bool result = TaskDateValidator.IsValid(yesterday);

            Assert.False(result);
        }

        [Fact]
        public void Today_ShouldBeValid()
        {
            bool result = TaskDateValidator.IsValid(DateTime.Today);

            Assert.True(result);
        }

        [Fact]
        public void FutureDate_ShouldBeValid()
        {
            DateTime tomorrow = DateTime.Today.AddDays(1);

            bool result = TaskDateValidator.IsValid(tomorrow);

            Assert.True(result);
        }

        [Fact]
        public void NullDate_ShouldBeValid()
        {
            bool result = TaskDateValidator.IsValid(null);

            Assert.True(result);
        }
    }
}
