

using gitko.CLI;


string? _args = Console.ReadLine();
if (string.IsNullOrEmpty(_args))
{
    Console.Write("$*@$&(@%^(@$*@)#(_");
    return;
}
string[] argss = _args.Split(' ');

Parser.ParseAndExecute(argss);