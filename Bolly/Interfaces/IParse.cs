namespace Bolly.Interfaces
{
    public interface IParse
    {
        public bool IsSuccess { get; set; }
        public string Result { get; set; }

        public void Execute(string source, string firstInput, string secondInput);
    }
}
