namespace Bolly.Interfaces
{
    public interface IParse
    {
        public bool Execute(string source, string firstInput, string secondInput, out string value);
    }
}
