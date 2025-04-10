namespace DiffScribe.Commands;

public interface ICommand
{
    string Name { get; }
    string Description { get; }
    CommandArgument[] DefinedArguments { get; }
    void Execute(string[] args);
}