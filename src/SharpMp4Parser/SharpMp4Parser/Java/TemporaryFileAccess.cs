using System.IO;

namespace SharpMp4Parser.Java
{
    public interface ITemporaryFileFactory
    {
        ITemporaryFile Create(long contentSize);
    }

    public class TemporaryFileFactory : ITemporaryFileFactory
    {
        public ITemporaryFile Create(long contentSize)
        {
            return new TemporaryFile(contentSize);
        }
    }

    public interface ITemporaryFile
    {
        void Close();

        void Read(Stream buffer);

        void Write(byte[] bytes, int offset, int count);
    }

    public class TemporaryFile : ITemporaryFile
    {
        private FileStream _stream;

        public TemporaryFile(long contentSize)
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

    public static class TemporaryFileAccess
    {
        public static ITemporaryFileFactory Factory { get; set; } = new TemporaryFileFactory();
    }
}
