using System;

namespace Memory
{
    public class Image
    {
        public int Id { get; set; }
        public string FilePath { get; set; }

        public Image() { } // Required for deserialization

        public Image(int id, string filePath)
        {
            Id = id;
            FilePath = filePath;
        }

        public override string ToString()
        {
            return $"Image ID: {Id}, Path: {FilePath}";
        }
    }
}
