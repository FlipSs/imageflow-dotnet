﻿using System;
using System.Drawing;
using System.IO;
using Xunit;
using System.Threading.Tasks;
using Imageflow.Bindings;
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
        public async Task TestGetImageInfo()
        {
            var imageBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABAQMAAAAl21bKAAAAA1BMVEX/TQBcNTh/AAAAAXRSTlPM0jRW/QAAAApJREFUeJxjYgAAAAYAAzY3fKgAAAAASUVORK5CYII=");
            using (var c = new JobContext())
            {
                c.AddInputBytes(0, imageBytes);
                var result = c.GetImageInfo(0);
                var d = result.DeserializeDynamic();

                var width = d.data.image_info.image_width.Value;
                Assert.Equal(width, 1);


            }

        }

        [Fact]
        public async Task TestBuildJob()
        {
            var imageBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABAQMAAAAl21bKAAAAA1BMVEX/TQBcNTh/AAAAAXRSTlPM0jRW/QAAAApJREFUeJxjYgAAAAYAAzY3fKgAAAAASUVORK5CYII=");
            using (var b = new FluentBuildJob())
            {
                var r = await b.Decode(imageBytes).FlipHorizontal().Rotate90().Distort(30, 20).ConstrainWithin(5, 5)
                    .EncodeToBytes(new GifEncoder()).Finish().InProcessAsync();

                Assert.Equal(5, r.First.Width);
                Assert.True(r.First.TryGetBytes().HasValue);
            }
            
        }
        [Fact]
        public async Task TestCommandStringJob()
        {
            var imageBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABAQMAAAAl21bKAAAAA1BMVEX/TQBcNTh/AAAAAXRSTlPM0jRW/QAAAApJREFUeJxjYgAAAAYAAzY3fKgAAAAASUVORK5CYII=");
            using (var b = new FluentBuildJob())
            {
                var r = await b.Decode(imageBytes).ResizerCommands("width=3&height=2&mode=stretch&scale=both")
                    .EncodeToBytes(new GifEncoder()).Finish().InProcessAsync();

                Assert.Equal(3, r.First.Width);
                Assert.True(r.First.TryGetBytes().HasValue);
            }

        }
        [Fact]
        public async Task TestFilesystemJobPrep()
        {
            var isUnix = Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX;

            var imageBytes = Convert.FromBase64String(
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABAQMAAAAl21bKAAAAA1BMVEX/TQBcNTh/AAAAAXRSTlPM0jRW/QAAAApJREFUeJxjYgAAAAYAAzY3fKgAAAAASUVORK5CYII=");
            using (var b = new FluentBuildJob())
            {
                string jsonPath;
                using (var job = await b.Decode(imageBytes).FlipHorizontal().Rotate90().Distort(30, 20)
                    .ConstrainWithin(5, 5)
                    .EncodeToBytes(new GifEncoder()).FinishWithTimeout(2000)
                    .WriteJsonJobAndInputs(true))
                {
                    jsonPath = job.JsonPath;

                    if (isUnix)
                    {
                        Assert.True(File.Exists(jsonPath));
                    }
                    else
                    {
                        using (var file = System.IO.MemoryMappedFiles.MemoryMappedFile.OpenExisting(jsonPath))
                        {
                        } // Will throw filenotfoundexception if missing 
                    }
                }

                if (isUnix)
                {
                    Assert.False(File.Exists(jsonPath));
                }
                else
                {

                    Assert.Throws<FileNotFoundException>(delegate ()
                    {

                        using (var file = System.IO.MemoryMappedFiles.MemoryMappedFile.OpenExisting(jsonPath))
                        {
                        }
                    });
                }
            }
        }

        [Fact]
        public async Task TestBuildJobSubprocess()
        {
            string imageflowTool = Environment.GetEnvironmentVariable("IMAGEFLOW_TOOL");

            if (!string.IsNullOrWhiteSpace(imageflowTool))
            {
                var imageBytes = Convert.FromBase64String(
                    "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABAQMAAAAl21bKAAAAA1BMVEX/TQBcNTh/AAAAAXRSTlPM0jRW/QAAAApJREFUeJxjYgAAAAYAAzY3fKgAAAAASUVORK5CYII=");
                using (var b = new FluentBuildJob())
                {
                    var r = await b.Decode(imageBytes).FlipHorizontal().Rotate90().Distort(30, 20).ConstrainWithin(5, 5)
                        .EncodeToBytes(new GifEncoder()).FinishWithTimeout(2000)
                        .InSubprocessAsync(imageflowTool);
                           
                    // ExecutableLocator.FindExecutable("imageflow_tool", new [] {"/home/n/Documents/imazen/imageflow/target/release/"})

                    Assert.Equal(5, r.First.Width);
                    Assert.True(r.First.TryGetBytes().HasValue);
                }
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
                    DownscalingMode = DecoderDownscalingMode.Fastest,
                    DiscardColorProfile = true
                };
                var r = await b.Decode(new BytesSource(imageBytes), 0, cmd)
                    .Distort(30, 20, new ResampleHints().Sharpen(50.0f, SharpenWhen.Always).ResampleFilter(InterpolationFilter.RobidouxFast, InterpolationFilter.Cubic))
                    .ConstrainWithin(5, 5)
                    .EncodeToBytes(new LibPngEncoder()).Finish().InProcessAsync();

                Assert.Equal(5, r.First.Width);
                Assert.True(r.First.TryGetBytes().HasValue);
            }

        }
    }
}