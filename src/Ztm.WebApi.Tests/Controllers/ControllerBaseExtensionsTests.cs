using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Ztm.WebApi.Controllers;

namespace Ztm.WebApi.Tests.Controllers
{
    public sealed class ControllerBaseExtensionsTests
    {
        readonly static string CallbackUrlKey = "X-Callback-URL";
        readonly static string CallbackIdKey = "X-Callback-ID";

        readonly ControllerBase subject;

        public ControllerBaseExtensionsTests()
        {
            this.subject = new TestControllerBase();
            this.subject.ControllerContext.HttpContext = new DefaultHttpContext();
        }

        [Theory]
        [InlineData("http://zcoin.io/")]
        [InlineData("https://zcoin.io/")]
        public void TryGetCallbackUrl_WithValidUrl_ShouldSuccess(string rawUrl)
        {
            // Arrange.
            this.subject.HttpContext.Request.Headers.Add(CallbackUrlKey, rawUrl);

            // Act.
            var success = this.subject.TryGetCallbackUrl(out var url);

            // Assert.
            Assert.True(success);
            Assert.Equal(new Uri(rawUrl), url);
        }

        [Theory]
        [InlineData("urn:isbn:0451450523")]
        [InlineData("urn:lsid:zoobank.org:pub:CDC8D258-8F57-41DC-B560-247E17D3DC8C")]
        [InlineData("Foo")]
        public void TryGetCallbackUrl_WithInvalidUrl_ShouldReturnFalse(string invalidUrl)
        {
            // Arrange.
            this.subject.HttpContext.Request.Headers.Add(CallbackUrlKey, invalidUrl);

            // Act.
            var success = this.subject.TryGetCallbackUrl(out var url);

            // Assert.
            Assert.False(success);
            Assert.Null(url);
        }

        [Fact]
        public void TryGetCallbackUrl_WithUnsetHeader_ShouldReturnFalse()
        {
            // Act.
            var success = this.subject.TryGetCallbackUrl(out var url);

            // Assert.
            Assert.False(success);
            Assert.Null(url);
        }

        [Fact]
        public void SetCallbackId_WithValidGuid_ShouldSuccess()
        {
            // Arrange.
            var id = Guid.NewGuid();

            // Act.
            this.subject.SetCallbackId(id);

            // Assert.
            Assert.True(
                this.subject.HttpContext.Response.Headers.TryGetValue(CallbackIdKey, out var retreived)
            );
            Assert.Equal(id.ToString(), retreived);
            Assert.Equal(this.subject.HttpContext.Response.StatusCode, (int)HttpStatusCode.Accepted);
        }
    }

    class TestControllerBase : ControllerBase
    {
    }
}