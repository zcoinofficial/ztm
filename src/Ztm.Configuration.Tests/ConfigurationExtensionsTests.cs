using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Ztm.Configuration.Tests
{
    public class ConfigurationExtensionsTests
    {
        readonly IConfiguration config;

        public ConfigurationExtensionsTests()
        {
            var builder = new ConfigurationBuilder();

            builder.AddInMemoryCollection(new Dictionary<string, string>()
            {
                {"Database:Main:ConnectionString", "Host=127.0.0.1;Database=ztm;Username=ztm;Password=1234"},
                {"Zcoin:Rpc:Address", "http://127.0.0.1:8888"},
                {"Zcoin:Rpc:UserName", "root"},
                {"Zcoin:Rpc:Password", "abc"}
            });

            this.config = builder.Build();
        }

        [Fact]
        public void GetDatabaseSection_WithCorrectConfiguration_ShouldSuccess()
        {
            var parsed = this.config.GetDatabaseSection();

            Assert.NotNull(parsed);
            Assert.NotNull(parsed.Main);
            Assert.Equal("Host=127.0.0.1;Database=ztm;Username=ztm;Password=1234", parsed.Main.ConnectionString);
        }

        [Fact]
        public void GetZcoinSection_WithCorrectConfiguration_ShouldSuccess()
        {
            var parsed = this.config.GetZcoinSection();

            Assert.NotNull(parsed);
            Assert.NotNull(parsed.Rpc);
            Assert.Equal(new Uri("http://127.0.0.1:8888"), parsed.Rpc.Address);
            Assert.Equal("root", parsed.Rpc.UserName);
            Assert.Equal("abc", parsed.Rpc.Password);
        }
    }
}