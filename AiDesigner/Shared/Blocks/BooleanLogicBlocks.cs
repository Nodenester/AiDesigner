using NodeBaseApi.Version2;
using Type = NodeBaseApi.Version2.Type;

namespace AiDesigner.Shared.Blocks
{
    public class AndBlock : Block
    {
        public AndBlock()
        {
            this.Id = Guid.NewGuid();
            this.Name = "AndBlock";
            this.Description = "A block that performs a logical AND operation";
            this.Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Input 1", Description = "First input", Type = Type.Boolean, IsRequired = true },
            new Input { Id = Guid.NewGuid(), Name = "Input 2", Description = "Second input", Type = Type.Boolean, IsRequired = true }
        };
            this.Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Output", Description = "Output of the AND operation", Type = Type.Boolean }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure)
        {
            bool result = (bool)inputs[0] && (bool)inputs[1];
            return new List<object> { result };
        }
    }

    public class OrBlock : Block
    {
        public OrBlock()
        {
            this.Id = Guid.NewGuid();
            this.Name = "OrBlock";
            this.Description = "A block that performs a logical OR operation";
            this.Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Input 1", Description = "First input", Type = Type.Boolean, IsRequired = true },
            new Input { Id = Guid.NewGuid(), Name = "Input 2", Description = "Second input", Type = Type.Boolean, IsRequired = true }
        };
            this.Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Output", Description = "Output of the OR operation", Type = Type.Boolean }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure)
        {
            bool result = (bool)inputs[0] || (bool)inputs[1];
            return new List<object> { result };
        }
    }

    public class NotBlock : Block
    {
        public NotBlock()
        {
            this.Id = Guid.NewGuid();
            this.Name = "NotBlock";
            this.Description = "A block that performs a logical NOT operation";
            this.Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Input", Description = "Input", Type = Type.Boolean, IsRequired = true }
        };
            this.Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Output", Description = "Output of the NOT operation", Type = Type.Boolean }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure)
        {
            bool result = !(bool)inputs[0];
            return new List<object> { result };
        }
    }

    public class XorBlock : Block
    {
        public XorBlock()
        {
            this.Id = Guid.NewGuid();
            this.Name = "XorBlock";
            this.Description = "A block that performs a logical XOR operation";
            this.Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Input 1", Description = "First input", Type = Type.Boolean, IsRequired = true },
            new Input { Id = Guid.NewGuid(), Name = "Input 2", Description = "Second input", Type = Type.Boolean, IsRequired = true }
        };
            this.Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Output", Description = "Output of the XOR operation", Type = Type.Boolean }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure)
        {
            bool result = (bool)inputs[0] ^ (bool)inputs[1];
            return new List<object> { result };
        }
    }
}
