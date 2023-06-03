# SharpMp4Parser

A C# API to read, write and create MP4 container. It is a C# netstandard2.0 port of the Java mp4parser from https://github.com/sannies/mp4parser 
with some additional fixes and improvements. The API was kept mostly the same as the Java implementation so many examples from the original repo
should work. Because it has no native dependencies (this is NOT another FFMPEG/Media Foundation wrapper), it is portable and can be used on
Windows as well as Linux and MacOS.

## What can you do?
- Muxing audio/video into an MP4 file (supports AAC for audio and H264/H265 for video)
- Append recordings that use the same encode settings
- Modify MP4 metadata
- Shorten recordings by omitting frames
- Move the MOOV box from the end of the file to the beginning to make the video streamable from the web
- Parse the VPS/SPS/PPS to get video information such as the FPS or the size

For the API you can refer to the tests as there are many examples for various use cases.

## Differences from the Java version

### Added features
- H265 Streaming
- H265 Muxing
- API to directly pass H264/H265 NALs to the MP4 writer

### Missing features
- TTML support (TBD)

### Known issues
- `FragmentedMp4Writer` can only encode video without audio, video + audio is not supported and produces an invalid file (the same limitation exists in the Java version).
Use `StandardMp4Writer` which supports both.

## Extensibility

### Logging
By default all warnings and errors get written to the `System.Diagnostics.Debug` output, which is stripped out in Release. I did not want to add any heavy dependencies
so the `SharpMp4Parser.Java.LOG` static class exposes multiple actions that can be assigned by the user:
```cs
SharpMp4Parser.Java.LOG.SinkError = (message, exception) => { Debug.WriteLine(message); };
SharpMp4Parser.Java.LOG.SinkWarning = (message, exception) => { Debug.WriteLine(message); };
```

### Temporary files
The parser is using temporary files to offload as much as possible from RAM onto the disk. This could be an issue on mobile platforms where file access is restricted. 
While there is a default implementation in place that is using the `System.IO.FileStream` APIs, it is also possible to provide your own implementation based upon the
`ITemporaryFile` interface:
```cs
public class MyTemporaryFile : ITemporaryFile
{
    private FileStream _stream;

    public MyTemporaryFile(long contentSize)
    {
        _stream = System.IO.File.Create(System.IO.Path.GetRandomFileName(), (int)contentSize, FileOptions.DeleteOnClose);
    }

    public void Write(byte[] bytes, int offset, int count)
    {
        _stream.Write(bytes, offset, count);
        _stream.Flush();
    }

    public void Read(Stream buffer)
    {
        _stream.Seek(0, SeekOrigin.Begin);
        _stream.CopyTo(buffer);
    }

    public void Close()
    {
        _stream.Close();
    }
}
```

Then create a factory by implementing the `ITemporaryFileFactory` interface:
```cs
public class MyTemporaryFileFactory : ITemporaryFileFactory
{
    public ITemporaryFile Create(long contentSize)
    {
        return new MyTemporaryFile(contentSize);
    }
}
```

And finally replace the default factory implementation:
```cs
SharpMp4Parser.Java.TemporaryFileAccess.Factory = new MyTemporaryFileFactory();
```

## Examples

### Record H264 video with AAC audio

Create the video track:
```cs
var h264Track = new H264StreamingTrack();
```

Create the audio track:
```cs
var aacTrack = new AacStreamingTrack(192, 192, 2, 44100, 0x4); // correct AAC parameters should be retrieved from the source, e.g. from the SDP or ADTS header
```

Create the output file where you want to store the MP4 and wrap it with a `ByteStream` object:
```cs
var outputFile = new ByteStream(File.Create("output.mp4"));
```

Create the MP4 writer with all the tracks that should be in the output:
```cs
var writer = new StandardMp4Writer(new List<StreamingTrack>() { h264Track, aacTrack }, output);
```

Pass the NALs from another source (e.g. RTP, raw *.h264 file, etc.):
```cs
List<byte[]> nals = ...; // retrieve the NALs from your source

foreach(byte[] nalBytes in nals)
{
   h264Track.ProcessNal(nalBytes);
}
```

Pass the AAC frames - the frames should not include ADTS header:
```cs
List<byte[]> frames = ...; // retrieve the AAC frames from your source

foreach(byte[] frameBytes in frames)
{
   aacTrack.ProcessFrame(frameBytes);
}
```

To stop recording and save the file, call:
```cs
h264Track.ProcessNalFinalize();
writer.close();
output.close();
```

### Record H265 video
Create the video track:
```cs
var h265Track = new H265StreamingTrack();
```

Create the output file where you want to store the MP4 and wrap it with a `ByteStream` object:
```cs
var outputFile = new ByteStream(File.Create("output.mp4"));
```

Create the MP4 writer with all the tracks that should be in the output:
```cs
var writer = new StandardMp4Writer(new List<StreamingTrack>() { h265Track }, output);
```

Pass the NALs from another source (e.g. RTP, raw *.h265 file, etc.):
```cs
List<byte[]> nals = ...; // retrieve the NALs from your source

foreach(byte[] nalBytes in nals)
{
   h265Track.ProcessNal(nalBytes);
}
```

To stop recording and save the file, call:
```cs
h265Track.ProcessNalFinalize();
writer.close();
output.close();
```

### Move the MOOV box to the beginning of the MP4 file
When recording the MP4 files as described in the previous examples, the MOOV box will be created at the end of the file
when the stream is stopped. This makes it harder for such files to be streamed to the web as the web browser will have
to download the entire file before it can play it. Fortunatelly, it is easy to re-encode the existing file and move the 
MOOV box to the beginning.

```cs
// open the file from the previous recording
var inputFile = new ByteStream(File.Open("output.mp4"));
var inputMovie = MovieCreator.build(inputFile, "inmem");

// create a new Movie where we will transfer all the tracks from the original Movie
var outputMovie = new Movie();

// move all tracks from the input to the new Movie
var tracks = inputMovie.getTracks();
foreach (var track in tracks)
{
   outputMovie.addTrack(track);
}

Container outputContainer = new DefaultMp4Builder().build(outputMovie);
var outputFile = new ByteStream(File.Create("output_muxed.mp4"));
outputContainer.writeContainer(outputFile);
outputFile.close();

inputFile.close();
```

## Thread safety
The parser is NOT thread safe so thread synchronization is the responsibility of the caller. Multiple instances
can be used simultaneously from multiple threads provided each instance is being accessed by the same thread that created it.

## Contribute
Pull requests with fixes are welcome!

## Credits
- This is a port of https://github.com/sannies/mp4parser into C#.
- ByteBuffer implementation with a few small tweaks is from https://github.com/NightOwl888/J2N/.