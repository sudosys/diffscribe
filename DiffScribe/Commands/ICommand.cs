namespace DiffScribe.Commands;

public interface ICommand
{
    string Name { get; }
    string Description { get; }
    string[] DefinedArguments { get; }
    
    void Execute(string[] args);
}