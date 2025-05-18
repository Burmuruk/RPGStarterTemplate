namespace Burmuruk.Tesis.Editor
{
    internal class InvalidExeption : System.Exception
    {
        public readonly string Name;

        public InvalidExeption(string name)
        {
            Name = name;
        }
    }

    internal class InvalidNameExeption : InvalidExeption
    {
        public InvalidNameExeption(string name = "Invalid name") : base(name)
        {
        }
    }

    internal class InvalidDataExeption : InvalidExeption
    {
        public InvalidDataExeption(string name = "Invalid information") : base(name)
        {
        }
    }
}
