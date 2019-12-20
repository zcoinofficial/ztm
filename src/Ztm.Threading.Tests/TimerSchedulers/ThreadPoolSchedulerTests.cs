using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using Ztm.Threading.TimerSchedulers;

namespace Ztm.Threading.Tests.TimerSchedulers
{
    public sealed class ThreadPoolSchedulerTests : IDisposable
    {
        readonly Mock<Action<object>> handler;
        readonly ThreadPoolScheduler subject;
        object schedule;

        public ThreadPoolSchedulerTests()
        {
            this.handler = new Mock<Action<object>>();
            this.subject = new ThreadPoolScheduler();
        }

        public void Dispose()
        {
            if (this.schedule != null)
            {
                this.subject.Stop(this.schedule);
            }
        }

        [Theory]
        [InlineData(-1d)]
        [InlineData(4294967295d)]
        public void IsValidDuration_WithInvalidValue_ShouldReturnFalse(double milliseconds)
        {
            this.subject.IsValidDuration(TimeSpan.FromMilliseconds(milliseconds)).Should().BeFalse();
        }

        [Theory]
        [InlineData(0d)]
        [InlineData(4294967294d)]
        public void IsValidDuration_WithValidValue_ShouldReturnTrue(double milliseconds)
        {
            this.subject.IsValidDuration(TimeSpan.FromMilliseconds(milliseconds)).Should().BeTrue();
        }

        [Theory]
        [InlineData(-1d)]
        [InlineData(4294967295d)]
        public void Schedule_WithInvalidDue_ShouldThrow(double milliseconds)
        {
            var due = TimeSpan.FromMilliseconds(milliseconds);

            this.subject.Invoking(s => s.Schedule(due, null, c => {}, null))
                        .Should().ThrowExactly<ArgumentOutOfRangeException>()
                        .And.ParamName.Should().Be("due");
        }

        [Theory]
        [InlineData(-1d)]
        [InlineData(4294967295d)]
        public void Schedule_WithInvalidPeriod_ShouldThrow(double milliseconds)
        {
            var period = TimeSpan.FromMilliseconds(milliseconds);

            this.subject.Invoking(s => s.Schedule(ThreadPoolScheduler.MinDuration, period, c => {}, null))
                        .Should().ThrowExactly<ArgumentOutOfRangeException>()
                        .And.ParamName.Should().Be("period");
        }

        [Fact]
        public void Schedule_WithNullHandler_ShouldThrow()
        {
            this.subject.Invoking(s => s.Schedule(ThreadPoolScheduler.MinDuration, null, null, null))
                        .Should().ThrowExactly<ArgumentNullException>()
                        .And.ParamName.Should().Be("handler");
        }

        [Fact]
        public void Schedule_WithNullPeriod_ShouldOneShot()
        {
            using (var elapsed = new ManualResetEventSlim())
            {
                // Arrange.
                var context = new object();

                this.handler.Setup(f => f(It.IsAny<object>()))
                            .Callback<object>(c => elapsed.Set());

                // Act.
                this.schedule = this.subject.Schedule(TimeSpan.Zero, null, this.handler.Object, context);

                elapsed.Wait(1000);

                // Assert.
                this.handler.Verify(f => f(context), Times.Once());
            }
        }

        [Fact]
        public void Schedule_WithNonNullPeriod_ShouldNotOneShot()
        {
            using (var elapsed = new ManualResetEventSlim())
            {
                // Arrange.
                var context = new object();

                this.handler.Setup(f => f(It.IsAny<object>()))
                            .Callback<object>(c => elapsed.Set());

                // Act.
                this.schedule = this.subject.Schedule(TimeSpan.Zero, TimeSpan.Zero, this.handler.Object, context);

                if (elapsed.Wait(2000))
                {
                    elapsed.Reset();
                    elapsed.Wait(2000); // Wait until timer to fire again.
                }

                this.subject.Stop(this.schedule);

                // Assert.
                this.handler.Verify(f => f(context), Times.AtLeast(2));
            }
        }

        [Fact]
        public void Stop_WithNull_ShouldThrow()
        {
            this.subject.Invoking(s => s.Stop(null))
                        .Should().ThrowExactly<ArgumentNullException>()
                        .And.ParamName.Should().Be("schedule");
        }

        [Fact]
        public void Stop_WithInvalidSchedule_ShouldThrow()
        {
            this.subject.Invoking(s => s.Stop(""))
                        .Should().ThrowExactly<ArgumentException>()
                        .And.ParamName.Should().Be("schedule");
        }

        [Fact]
        public void Stop_WithValidTimer_ShouldDisposeThatTimer()
        {
            // Arrange.
            var timer = (System.Threading.Timer)this.subject.Schedule(
                TimeSpan.FromSeconds(10),
                null,
                this.handler.Object,
                null
            );

            // Act.
            this.subject.Stop(timer);

            // Assert.
            timer.Invoking(t => t.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan))
                 .Should().ThrowExactly<ObjectDisposedException>();

            this.handler.Verify(f => f(It.IsAny<object>()), Times.Never());
        }
    }
}
