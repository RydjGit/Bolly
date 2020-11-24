namespace Bolly.Interfaces
{
    public interface IParse
    {
        public (bool success, string result) Execute(string source, string firstInput, string secondInput);
    }
}
