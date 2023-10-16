using System;
using System.Collections.Generic;
using System.IO;
using NodeBaseApi.Version2;
using Type = NodeBaseApi.Version2.Type;

namespace NodeExacuteApi.Data.Blocks
{
    public class GreaterThanBlock : Block
    {
        public GreaterThanBlock()
        {
            Id = Guid.NewGuid();
            Name = "GreaterThanBlock";
            Description = "Compares two numbers to see if the first is greater than the second.";

            Inputs = new List<Input>
    {
        new Input { Id = Guid.NewGuid(), Name = "First Number", Description = "The first number.", Type = Type.Number, IsRequired = true },
        new Input { Id = Guid.NewGuid(), Name = "Second Number", Description = "The second number.", Type = Type.Number, IsRequired = true }
    };

            Outputs = new List<Output>
    {
        new Output { Id = Guid.NewGuid(), Name = "Result", Description = "True if First Number > Second Number, false otherwise.", Type = Type.Boolean }
    };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }

    }

    public class LessThanBlock : Block
    {
        public LessThanBlock()
        {
            Id = Guid.NewGuid();
            Name = "LessThanBlock";
            Description = "Compares two numbers to see if the first is less than the second.";

            Inputs = new List<Input>
    {
        new Input { Id = Guid.NewGuid(), Name = "First Number", Description = "The first number.", Type = Type.Number, IsRequired = true },
        new Input { Id = Guid.NewGuid(), Name = "Second Number", Description = "The second number.", Type = Type.Number, IsRequired = true }
    };

            Outputs = new List<Output>
    {
        new Output { Id = Guid.NewGuid(), Name = "Result", Description = "True if First Number < Second Number, false otherwise.", Type = Type.Boolean }
    };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }

    }

    public class EqualToBlock : Block
    {
        public EqualToBlock()
        {
            Id = Guid.NewGuid();
            Name = "EqualToBlock";
            Description = "Compares two numbers for equality.";

            Inputs = new List<Input>
    {
        new Input { Id = Guid.NewGuid(), Name = "First Number", Description = "The first number.", Type = Type.Number, IsRequired = true },
        new Input { Id = Guid.NewGuid(), Name = "Second Number", Description = "The second number.", Type = Type.Number, IsRequired = true }
    };

            Outputs = new List<Output>
    {
        new Output { Id = Guid.NewGuid(), Name = "Result", Description = "True if First Number == Second Number, false otherwise.", Type = Type.Boolean }
    };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }

    }

    public class NotEqualToBlock : Block
    {
        public NotEqualToBlock()
        {
            Id = Guid.NewGuid();
            Name = "NotEqualToBlock";
            Description = "Compares two numbers for inequality.";

            Inputs = new List<Input>
    {
        new Input { Id = Guid.NewGuid(), Name = "First Number", Description = "The first number.", Type = Type.Number, IsRequired = true },
        new Input { Id = Guid.NewGuid(), Name = "Second Number", Description = "The second number.", Type = Type.Number, IsRequired = true }
    };

            Outputs = new List<Output>
    {
        new Output { Id = Guid.NewGuid(), Name = "Result", Description = "True if First Number != Second Number, false otherwise.", Type = Type.Boolean }
    };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }

    }

    public class GreaterThanOrEqualToBlock : Block
    {
        public GreaterThanOrEqualToBlock()
        {
            Id = Guid.NewGuid();
            Name = "GreaterThanOrEqualToBlock";
            Description = "Compares two numbers to see if the first is greater than or equal to the second.";

            Inputs = new List<Input>
    {
        new Input { Id = Guid.NewGuid(), Name = "First Number", Description = "The first number.", Type = Type.Number, IsRequired = true },
        new Input { Id = Guid.NewGuid(), Name = "Second Number", Description = "The second number.", Type = Type.Number, IsRequired = true }
    };

            Outputs = new List<Output>
    {
        new Output { Id = Guid.NewGuid(), Name = "Result", Description = "True if First Number >= Second Number, false otherwise.", Type = Type.Boolean }
    };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }

    }

    public class LessThanOrEqualToBlock : Block
    {
        public LessThanOrEqualToBlock()
        {
            Id = Guid.NewGuid();
            Name = "LessThanOrEqualToBlock";
            Description = "Compares two numbers to see if the first is less than or equal to the second.";

            Inputs = new List<Input>
    {
        new Input { Id = Guid.NewGuid(), Name = "First Number", Description = "The first number.", Type = Type.Number, IsRequired = true },
        new Input { Id = Guid.NewGuid(), Name = "Second Number", Description = "The second number.", Type = Type.Number, IsRequired = true }
    };

            Outputs = new List<Output>
    {
        new Output { Id = Guid.NewGuid(), Name = "Result", Description = "True if First Number <= Second Number, false otherwise.", Type = Type.Boolean }
    };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }

    }
}
