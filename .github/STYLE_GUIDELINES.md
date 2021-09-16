# Code style guide

## Quicklinks

* [Code structure](#Code-structure)
* [if statements](#if-statements)
* [Switch statements](#Switch-statements)
* [Methodes](#Methodes)
* [Classes](#Classes)
* [Structs](#Structs)
* [Getters](#Getters)

## Code structure

- No double empty lines!
- Last lines of file always has to be empty for git.
- Pascal case for variables.
- Public function starts with capital case.
- No unnecesary abreviations

## if statements 

```cs

if (provider == null) provider = CultureInfo.CurrentCulture;

if (statement) 
{
    // Code here
}
if (statement) 
{
    // Code here
} else 
{
    // Other code here
}
if (statement) 
{
    // Code here
} else if (statement)
{
    // Other code here
}
```

## Switch statements

```cs

switch (format.ToUpperInvariant())
{
        case "1":

        case "2":
            return 2;
        case "3":
            //Code block
            break;
        default:
            throw new FormatException($"The {format} format string is not supported.");
}
```

## Methodes

```cs
/// <summary>Method does <c>This</c> and is explained here.</summary>
///

private void Method(Data data){
    // Code here
}

/// <summary>Method does <c>This</c> returns <returns>A string</returns> and is explained here.</summary>
///

private String Method(Data data){
    // Code here
    return "A string";
}
```

## Classes

```cs
using System;

namespace RemoteHealthcare
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}
```

## Structs

```cs
public struct Time : IFormattable
        {
            private readonly int hours;
            public int minutes;
            public Time(int hh, int mm)
            {
                this.hours = hh;
                this.minutes = mm;
            }

          
        }

```
## Getters

```cs


public int seconds { get; }

public int Hours
{
    get { return hours; }
}

public int Minutes
{
    get { return minutes; }
}

public int TotalMinutes
{
    get { return 60 * hours + minutes; }
}

       

```
