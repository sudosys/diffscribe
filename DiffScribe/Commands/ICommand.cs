using DiffScribe.Commands.Models;

namespace DiffScribe.Commands;

public interface ICommand
{
    string Name { get; }
    
    string Description { get; }
    
    CommandArgument[] DefinedArguments { get; }
    
    void Execute(Dictionary<string, string?> args);
}