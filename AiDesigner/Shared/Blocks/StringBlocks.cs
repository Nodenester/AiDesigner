using NodeBaseApi.Version2;
using System.Text.RegularExpressions;
using Type = NodeBaseApi.Version2.Type;

namespace AiDesigner.Shared.Blocks
{
    public class Concatenation : Block
    {
        public Concatenation()
        {
            Id = Guid.NewGuid();
            Name = "Concatenation";
            Description = "A block that concatenates two strings.";
            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "String 1", Description = "The first string.", Type = Type.String, IsRequired = true },
            new Input { Id = Guid.NewGuid(), Name = "String 2", Description = "The second string.", Type = Type.String, IsRequired = true }
        };
            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Result", Description = "The concatenated string.", Type = Type.String }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class Length : Block
    {
        public Length()
        {
            Id = Guid.NewGuid();
            Name = "Length";
            Description = "A block that returns the length of a string.";
            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Input String", Description = "The input string.", Type = Type.String, IsRequired = true }
        };
            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Length", Description = "The length of the input string.", Type = Type.Number }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }

    }

    public class Substring : Block
    {
        public Substring()
        {
            Id = Guid.NewGuid();
            Name = "Substring";
            Description = "A block that returns a substring from a given string.";
            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Input String", Description = "The input string.", Type = Type.String, IsRequired = true },
            new Input { Id = Guid.NewGuid(), Name = "Start Index", Description = "The starting position of the substring.", Type = Type.Number, IsRequired = true },
            new Input { Id = Guid.NewGuid(), Name = "Length", Description = "The length of the substring.", Type = Type.Number, IsRequired = true }
        };
            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Substring", Description = "The extracted substring.", Type = Type.String }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }

    }

    public class Lowercase : Block
    {
        public Lowercase()
        {
            Id = Guid.NewGuid();
            Name = "Lowercase";
            Description = "Converts a string to lowercase.";
            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Input String", Description = "The input string.", Type = Type.String, IsRequired = true }
        };
            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Lowercase String", Description = "The converted lowercase string.", Type = Type.String }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }

    }

    public class Uppercase : Block
    {
        public Uppercase()
        {
            Id = Guid.NewGuid();
            Name = "Uppercase";
            Description = "Converts a string to uppercase.";
            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Input String", Description = "The input string.", Type = Type.String, IsRequired = true }
        };
            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Uppercase String", Description = "The converted uppercase string.", Type = Type.String }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }

    }

    public class Contains : Block
    {
        public Contains()
        {
            Id = Guid.NewGuid();
            Name = "Contains";
            Description = "Checks if a string contains a specified substring.";
            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Input String", Description = "The input string.", Type = Type.String, IsRequired = true },
            new Input { Id = Guid.NewGuid(), Name = "Substring", Description = "The substring to search for.", Type = Type.String, IsRequired = true }
        };
            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Contains", Description = "Whether the string contains the substring.", Type = Type.Boolean }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }

    }

    public class Split : Block
    {
        public Split()
        {
            Id = Guid.NewGuid();
            Name = "Split";
            Description = "A block that splits a string into an array of substrings based on a specified delimiter.";
            Inputs = new List<Input>
    {
        new Input { Id = Guid.NewGuid(), Name = "Input String", Description = "The input string.", Type = Type.String, IsRequired = true },
        new Input { Id = Guid.NewGuid(), Name = "Delimiter", Description = "The delimiter.", Type = Type.String, IsRequired = true }
    };
            Outputs = new List<Output>
    {
        new Output { Id = Guid.NewGuid(), Name = "Substrings", Description = "The array of substrings.", Type = Type.String, IsList = true }
    };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }

    }

    public class Join : Block
    {
        public Join()
        {
            Id = Guid.NewGuid();
            Name = "Join";
            Description = "A block that concatenates an array of strings into a single string with a specified separator.";
            Inputs = new List<Input>
    {
        new Input { Id = Guid.NewGuid(), Name = "Array of Strings", Description = "The array of strings.", Type = Type.String, IsRequired = true, IsList = true },
        new Input { Id = Guid.NewGuid(), Name = "Separator", Description = "The separator.", Type = Type.String, IsRequired = true }
    };
            Outputs = new List<Output>
    {
        new Output { Id = Guid.NewGuid(), Name = "Resultant String", Description = "The concatenated string.", Type = Type.String }
    };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }

    }

    public class Replace : Block
    {
        public Replace()
        {
            Id = Guid.NewGuid();
            Name = "Replace";
            Description = "A block that replaces occurrences of a specified substring with another substring.";
            Inputs = new List<Input>
    {
        new Input { Id = Guid.NewGuid(), Name = "Input String", Description = "The input string.", Type = Type.String, IsRequired = true },
        new Input { Id = Guid.NewGuid(), Name = "Search String", Description = "The substring to replace.", Type = Type.String, IsRequired = true },
        new Input { Id = Guid.NewGuid(), Name = "Replacement String", Description = "The replacement string.", Type = Type.String, IsRequired = true }
    };
            Outputs = new List<Output>
    {
        new Output { Id = Guid.NewGuid(), Name = "Resultant String", Description = "The resultant string.", Type = Type.String }
    };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }

    }

    public class Trim : Block
    {
        public Trim()
        {
            Id = Guid.NewGuid();
            Name = "Trim";
            Description = "A block that removes all leading and trailing white-space characters from the current string.";
            Inputs = new List<Input>
    {
        new Input { Id = Guid.NewGuid(), Name = "Input String", Description = "The input string.", Type = Type.String, IsRequired = true }
    };
            Outputs = new List<Output>
    {
        new Output { Id = Guid.NewGuid(), Name = "Trimmed String", Description = "The trimmed string.", Type = Type.String }
    };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }

    }

    public class IndexOf : Block
    {
        public IndexOf()
        {
            Id = Guid.NewGuid();
            Name = "IndexOf";
            Description = "A block that returns the index of the first occurrence of a specified substring.";
            Inputs = new List<Input>
    {
        new Input { Id = Guid.NewGuid(), Name = "Input String", Description = "The input string.", Type = Type.String, IsRequired = true },
        new Input { Id = Guid.NewGuid(), Name = "Search String", Description = "The substring to search for.", Type = Type.String, IsRequired = true }
    };
            Outputs = new List<Output>
    {
        new Output { Id = Guid.NewGuid(), Name = "Index", Description = "The index of the first occurrence.", Type = Type.Number }
    };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }

    }

    public class RegularExpressionMatch : Block
    {
        public RegularExpressionMatch()
        {
            Id = Guid.NewGuid();
            Name = "RegularExpressionMatch";
            Description = "A block that checks if a string matches a specified regular expression pattern.";
            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Input String", Description = "The input string.", Type = Type.String, IsRequired = true },
            new Input { Id = Guid.NewGuid(), Name = "Regular Expression", Description = "The regular expression.", Type = Type.String, IsRequired = true }
        };
            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Match Result", Description = "Whether the string matches the pattern.", Type = Type.Boolean },
            new Output { Id = Guid.NewGuid(), Name = "Matched Groups", Description = "The groups matched by the regular expression.", Type = Type.String, IsList = true }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }

    }

    public class Format : Block
    {
        public Format()
        {
            Id = Guid.NewGuid();
            Name = "Format";
            Description = "A block that replaces the format items in a specified string with the string representation of specified objects.";
            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Format String", Description = "The format string.", Type = Type.String, IsRequired = true },
            new Input { Id = Guid.NewGuid(), Name = "Arguments", Description = "The arguments.", Type = Type.String, IsRequired = true, IsList = true }
        };
            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Formatted String", Description = "The formatted string.", Type = Type.String }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }

    }

    public class StringCleaner : Block
    {
        public StringCleaner()
        {
            Id = Guid.NewGuid();
            Name = "StringCleaner";
            Description = "A block that removes substrings between two specified delimiters from a given string.";
            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Input String", Description = "The main string from which substrings will be removed.", Type = Type.String, IsRequired = true },
            new Input { Id = Guid.NewGuid(), Name = "Start Delimiter", Description = "The start delimiter.", Type = Type.String, IsRequired = true },
            new Input { Id = Guid.NewGuid(), Name = "End Delimiter", Description = "The end delimiter.", Type = Type.String, IsRequired = true }
        };
            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Cleaned String", Description = "The string after removal of specified substrings.", Type = Type.String, IsList = false }
        };
        }
        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class IntParser : Block
    {
        public IntParser()
        {
            Id = Guid.NewGuid();
            Name = "IntParser";
            Description = "A block that parses a string into an integer.";
            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Input String", Description = "The string to be parsed into an integer.", Type = Type.String, IsRequired = true }
        };
            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Parsed Integer", Description = "The parsed integer value.", Type = Type.Number, IsList = false }
        };
        }
        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class BoolParser : Block
    {
        public BoolParser()
        {
            Id = Guid.NewGuid();
            Name = "BoolParser";
            Description = "A block that parses a string into a boolean.";
            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Input String", Description = "The string to be parsed into a boolean.", Type = Type.String, IsRequired = true }
        };
            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Parsed Boolean", Description = "The parsed boolean value.", Type = Type.Boolean, IsList = false }
        };
        }
        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    //math logic
    public class Equal : Block
    {
        public Equal()
        {
            Id = Guid.NewGuid();
            Name = "Equal";
            Description = "Checks if two numbers are equal.";

            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Number1", Description = "First number for comparison.", Type = Type.Number, IsRequired = true },
            new Input { Id = Guid.NewGuid(), Name = "Number2", Description = "Second number for comparison.", Type = Type.Number, IsRequired = true }
        };

            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Result", Description = "True if numbers are equal, else false.", Type = Type.Boolean }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class LessThan : Block
    {
        public LessThan()
        {
            Id = Guid.NewGuid();
            Name = "LessThan";
            Description = "Checks if the first number is less than the second number.";

            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Number1", Description = "First number for comparison.", Type = Type.Number, IsRequired = true },
            new Input { Id = Guid.NewGuid(), Name = "Number2", Description = "Second number for comparison.", Type = Type.Number, IsRequired = true }
        };

            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Result", Description = "True if the first number is less than the second, else false.", Type = Type.Boolean }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class MoreThan : Block
    {
        public MoreThan()
        {
            Id = Guid.NewGuid();
            Name = "MoreThan";
            Description = "Checks if the first number is greater than the second number.";

            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Number1", Description = "First number for comparison.", Type = Type.Number, IsRequired = true },
            new Input { Id = Guid.NewGuid(), Name = "Number2", Description = "Second number for comparison.", Type = Type.Number, IsRequired = true }
        };

            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Result", Description = "True if the first number is greater than the second, else false.", Type = Type.Boolean }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class LessThanOrEqual : Block
    {
        public LessThanOrEqual()
        {
            Id = Guid.NewGuid();
            Name = "LessThanOrEqual";
            Description = "Checks if the first number is less than or equal to the second number.";

            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Number1", Description = "First number for comparison.", Type = Type.Number, IsRequired = true },
            new Input { Id = Guid.NewGuid(), Name = "Number2", Description = "Second number for comparison.", Type = Type.Number, IsRequired = true }
        };

            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Result", Description = "True if the first number is less than or equal to the second, else false.", Type = Type.Boolean }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class MoreThanOrEqual : Block
    {
        public MoreThanOrEqual()
        {
            Id = Guid.NewGuid();
            Name = "MoreThanOrEqual";
            Description = "Checks if the first number is greater than or equal to the second number.";

            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Number1", Description = "First number for comparison.", Type = Type.Number, IsRequired = true },
            new Input { Id = Guid.NewGuid(), Name = "Number2", Description = "Second number for comparison.", Type = Type.Number, IsRequired = true }
        };

            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Result", Description = "True if the first number is greater than or equal to the second, else false.", Type = Type.Boolean }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }
}
