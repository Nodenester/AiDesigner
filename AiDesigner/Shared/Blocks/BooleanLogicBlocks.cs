using NodeBaseApi.Version2;
using Type = NodeBaseApi.Version2.Type;

namespace AiDesigner.Shared.Blocks
{
    public class AndBlock : Block
    {
        public AndBlock()
        {
            Id = Guid.NewGuid();
            Name = "And Block";
            Description = "A block that performs a logical AND operation";
            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Input 1", Description = "First input", Type = Type.Boolean, IsRequired = true },
            new Input { Id = Guid.NewGuid(), Name = "Input 2", Description = "Second input", Type = Type.Boolean, IsRequired = true }
        };
            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Output", Description = "Output of the AND operation", Type = Type.Boolean }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class OrBlock : Block
    {
        public OrBlock()
        {
            Id = Guid.NewGuid();
            Name = "Or Block";
            Description = "A block that performs a logical OR operation";
            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Input 1", Description = "First input", Type = Type.Boolean, IsRequired = true },
            new Input { Id = Guid.NewGuid(), Name = "Input 2", Description = "Second input", Type = Type.Boolean, IsRequired = true }
        };
            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Output", Description = "Output of the OR operation", Type = Type.Boolean }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class NotBlock : Block
    {
        public NotBlock()
        {
            Id = Guid.NewGuid();
            Name = "Not Block";
            Description = "A block that performs a logical NOT operation";
            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Input", Description = "Input", Type = Type.Boolean, IsRequired = true }
        };
            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Output", Description = "Output of the NOT operation", Type = Type.Boolean }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class XorBlock : Block
    {
        public XorBlock()
        {
            Id = Guid.NewGuid();
            Name = "Xor Block";
            Description = "A block that performs a logical XOR operation";
            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Input 1", Description = "First input", Type = Type.Boolean, IsRequired = true },
            new Input { Id = Guid.NewGuid(), Name = "Input 2", Description = "Second input", Type = Type.Boolean, IsRequired = true }
        };
            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Output", Description = "Output of the XOR operation", Type = Type.Boolean }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class NandBlock : Block
    {
        public NandBlock()
        {
            Id = Guid.NewGuid();
            Name = "Nand Block";
            Description = "A block that performs a logical NAND operation";
            Inputs = new List<Input>
    {
        new Input { Id = Guid.NewGuid(), Name = "Input 1", Description = "First input", Type = Type.Boolean, IsRequired = true },
        new Input { Id = Guid.NewGuid(), Name = "Input 2", Description = "Second input", Type = Type.Boolean, IsRequired = true }
    };
            Outputs = new List<Output>
    {
        new Output { Id = Guid.NewGuid(), Name = "Output", Description = "Output of the NAND operation", Type = Type.Boolean }
    };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class NorBlock : Block
    {
        public NorBlock()
        {
            Id = Guid.NewGuid();
            Name = "Nor Block";
            Description = "A block that performs a logical NOR operation";
            Inputs = new List<Input>
    {
        new Input { Id = Guid.NewGuid(), Name = "Input 1", Description = "First input", Type = Type.Boolean, IsRequired = true },
        new Input { Id = Guid.NewGuid(), Name = "Input 2", Description = "Second input", Type = Type.Boolean, IsRequired = true }
    };
            Outputs = new List<Output>
    {
        new Output { Id = Guid.NewGuid(), Name = "Output", Description = "Output of the NOR operation", Type = Type.Boolean }
    };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class BooleanEquality : Block
    {
        public BooleanEquality()
        {
            Id = Guid.NewGuid();
            Name = "Boolean Equality";
            Description = "A block that checks if two boolean values are equal";
            Inputs = new List<Input>
    {
        new Input { Id = Guid.NewGuid(), Name = "Input 1", Description = "First input", Type = Type.Boolean, IsRequired = true },
        new Input { Id = Guid.NewGuid(), Name = "Input 2", Description = "Second input", Type = Type.Boolean, IsRequired = true }
    };
            Outputs = new List<Output>
    {
        new Output { Id = Guid.NewGuid(), Name = "Output", Description = "Result of the equality check", Type = Type.Boolean }
    };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class BooleanToggle : Block
    {
        public BooleanToggle()
        {
            Id = Guid.NewGuid();
            Name = "Boolean Toggle";
            Description = "A block that toggles a boolean value";
            Inputs = new List<Input>
    {
        new Input { Id = Guid.NewGuid(), Name = "Input", Description = "Input value", Type = Type.Boolean, IsRequired = true }
    };
            Outputs = new List<Output>
    {
        new Output { Id = Guid.NewGuid(), Name = "Output", Description = "Toggled value", Type = Type.Boolean }
    };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class BooleanToString : Block
    {
        public BooleanToString()
        {
            Id = Guid.NewGuid();
            Name = "Boolean ToString";
            Description = "A block that converts a boolean value to a string";
            Inputs = new List<Input>
    {
        new Input { Id = Guid.NewGuid(), Name = "Input", Description = "Input value", Type = Type.Boolean, IsRequired = true }
    };
            Outputs = new List<Output>
    {
        new Output { Id = Guid.NewGuid(), Name = "Output", Description = "String representation", Type = Type.String }
    };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class StringToBoolean : Block
    {
        public StringToBoolean()
        {
            Id = Guid.NewGuid();
            Name = "String ToBoolean";
            Description = "A block that converts a string to a boolean value";
            Inputs = new List<Input>
    {
        new Input { Id = Guid.NewGuid(), Name = "Input", Description = "Input string", Type = Type.String, IsRequired = true }
    };
            Outputs = new List<Output>
    {
        new Output { Id = Guid.NewGuid(), Name = "Output", Description = "Boolean value", Type = Type.Boolean }
    };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class BooleanToInteger : Block
    {
        public BooleanToInteger()
        {
            Id = Guid.NewGuid();
            Name = "Boolean ToInteger";
            Description = "A block that converts a boolean value to an integer";
            Inputs = new List<Input>
            {
        new Input { Id = Guid.NewGuid(), Name = "Input", Description = "Input value", Type = Type.Boolean, IsRequired = true }
    };
            Outputs = new List<Output>
            {
        new Output { Id = Guid.NewGuid(), Name = "Output", Description = "Integer representation", Type = Type.Number }
    };
        }
        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }
}
