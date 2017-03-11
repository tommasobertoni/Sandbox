#ArgsParser

Single class utility that provides a set of basic features that can be used to easly parse the argument given to a console application.
<br>
###A small example:
```csharp
static void Main(string[] args)
{
    int count = 5;
    var argsParser = new ArgsParser()
    {
        new ArgsParser.Parameter()
        {
            Commands = new string[] { "-count" },
            Description = $"(int) Defines how many times the text has to be printed (default {count}).",
            HasValue = true,
            Action = (val) =>
            {
                int.TryParse(val, out count);
            }
        }
    };

    // Parse(args) returns true if the help command was found.
    bool helpCommandWasFound = argsParser.Parse(args);
    
    if (helpCommandWasFound)
    {
        // Do things...
    }
    else
    {
        // Do other things...
    }
}
```
So if I start the application with `-count 8` it will recognize the *-count* command and will invoke the associated *Action*, which will change the value of the `count` variable.
<br><br>
If I start the application with `-?` or `-help` instead, it will print:
```
-------------------------------------
Help

  -count
    (int) Defines how many times the text has to be printed (default 5).
-------------------------------------
```
If you want to know more, check out the [Demo](ArgsParser/Demo/Program.cs).
<br><br>
####Tech
C#6, VS2015, .NET 4.0
