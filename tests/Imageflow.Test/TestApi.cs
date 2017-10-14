﻿using System;
using System.Diagnostics;
using System.Drawing;
using Xunit;
using Imageflow;
using System.Dynamic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Imageflow.Fluent;
 using Xunit.Abstractions;

namespace Imageflow.Test
{
    public class TestApi
    {
        private readonly ITestOutputHelper output;

        public TestApi(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task TestBuildJob()
        {
            var imageBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABAQMAAAAl21bKAAAAA1BMVEX/TQBcNTh/AAAAAXRSTlPM0jRW/QAAAApJREFUeJxjYgAAAAYAAzY3fKgAAAAASUVORK5CYII=");
            using (var b = new FluentBuildJob())
            {
                var r = await b.Decode(imageBytes).FlipHorizontal().Rotate90().Distort(30, 20).ConstrainWithin(5, 5)
                    .EncodeToBytes(new GifEncoder()).FinishAsync();

                Assert.Equal(5, r.First.Width);
                Assert.True(r.First.TryGetBytes().HasValue);
            }
            
        }


        [Fact]
        public async Task TestCustomDownscaling()
        {
            var imageBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABAQMAAAAl21bKAAAAA1BMVEX/TQBcNTh/AAAAAXRSTlPM0jRW/QAAAApJREFUeJxjYgAAAAYAAzY3fKgAAAAASUVORK5CYII=");
            using (var b = new FluentBuildJob())
            {
                var cmd = new DecodeCommands
                {
                    DownscaleHint = new Size(20, 20),
                    DownscalingMode = DecderDownscalingMode.Fastest,
                    DiscardColorProfile = true
                };
                var r = await b.Decode(new BytesSource(imageBytes), 0, cmd)
                    .Distort(30, 20, 50.0f, InterpolationFilter.RobidouxFast, InterpolationFilter.Cubic)
                    .ConstrainWithin(5, 5)
                    .EncodeToBytes(new LibPngEncoder()).FinishAsync();

                Assert.Equal(5, r.First.Width);
                Assert.True(r.First.TryGetBytes().HasValue);
            }

        }
    }
}