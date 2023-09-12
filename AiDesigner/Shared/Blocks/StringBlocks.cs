using NodeBaseApi.Version2;
using Type = NodeBaseApi.Version2.Type;

namespace AiDesigner.Shared.Blocks
{
    public class StringConcatenationBlock : Block
    {
        public StringConcatenationBlock()
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

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure)
        {
            var result = (string)inputs[0] + (string)inputs[1];
            programStructure.OutputValues[Outputs[0].Id] = result;
            return new List<object> { result };
        }
    }

    public class StringLengthBlock : Block
    {
        public StringLengthBlock()
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

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure)
        {
            var length = ((string)inputs[0]).Length;
            programStructure.OutputValues[Outputs[0].Id] = length;
            return new List<object> { length };
        }
    }

    public class StringSubstringBlock : Block
    {
        public StringSubstringBlock()
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

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure)
        {
            var substring = ((string)inputs[0]).Substring((int)inputs[1], (int)inputs[2]);
            programStructure.OutputValues[Outputs[0].Id] = substring;
            return new List<object> { substring };
        }
    }

    public class ToLowercaseBlock : Block
    {
        public ToLowercaseBlock()
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

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure)
        {
            var result = ((string)inputs[0]).ToLower();
            programStructure.OutputValues[Outputs[0].Id] = result;
            return new List<object> { result };
        }
    }

    public class ToUppercaseBlock : Block
    {
        public ToUppercaseBlock()
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

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure)
        {
            var result = ((string)inputs[0]).ToUpper();
            programStructure.OutputValues[Outputs[0].Id] = result;
            return new List<object> { result };
        }
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

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure)
        {
            var contains = ((string)inputs[0]).Contains((string)inputs[1]);
            programStructure.OutputValues[Outputs[0].Id] = contains;
            return new List<object> { contains };
        }
    }

}
