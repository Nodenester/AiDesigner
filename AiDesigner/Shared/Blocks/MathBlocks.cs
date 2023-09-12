using NodeBaseApi.Version2;
using Type = NodeBaseApi.Version2.Type;

namespace AiDesigner.Shared.Blocks
{
    public class AddBlock : Block
    {
        public AddBlock()
        {
            Id = Guid.NewGuid();
            Name = "AddBlock";
            Description = "Adds two numbers together.";
            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "First Number", Description = "The first number to add.", Type = Type.Number, IsRequired = true, IsList = false },
            new Input { Id = Guid.NewGuid(), Name = "Second Number", Description = "The second number to add.", Type = Type.Number, IsRequired = true, IsList = false }
        };
            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Sum", Description = "The sum of the two numbers.", Type = Type.Number, IsList = false }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure)
        {
            double firstNumber = Convert.ToDouble(inputs[0]);
            double secondNumber = Convert.ToDouble(inputs[1]);

            double sum = firstNumber + secondNumber;

            return new List<object> { sum };
        }
    }

    public class SubtractBlock : Block
    {
        public SubtractBlock()
        {
            Id = Guid.NewGuid();
            Name = "SubtractBlock";
            Description = "Subtracts the second number from the first number.";
            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "First Number", Description = "The number to subtract from.", Type = Type.Number, IsRequired = true, IsList = false },
            new Input { Id = Guid.NewGuid(), Name = "Second Number", Description = "The number to subtract.", Type = Type.Number, IsRequired = true, IsList = false }
        };
            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Difference", Description = "The difference of the two numbers.", Type = Type.Number, IsList = false }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure)
        {
            double firstNumber = Convert.ToDouble(inputs[0]);
            double secondNumber = Convert.ToDouble(inputs[1]);

            double difference = firstNumber - secondNumber;

            return new List<object> { difference };
        }
    }

    public class MultiplyBlock : Block
    {
        public MultiplyBlock()
        {
            Id = Guid.NewGuid();
            Name = "MultiplyBlock";
            Description = "Multiplies two numbers together.";
            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "First Number", Description = "The first number to multiply.", Type = Type.Number, IsRequired = true, IsList = false },
            new Input { Id = Guid.NewGuid(), Name = "Second Number", Description = "The second number to multiply.", Type = Type.Number, IsRequired = true, IsList = false }
        };
            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Product", Description = "The product of the two numbers.", Type = Type.Number, IsList = false }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure)
        {
            double firstNumber = Convert.ToDouble(inputs[0]);
            double secondNumber = Convert.ToDouble(inputs[1]);

            double product = firstNumber * secondNumber;

            return new List<object> { product };
        }
    }

    public class DivideBlock : Block
    {
        public DivideBlock()
        {
            Id = Guid.NewGuid();
            Name = "DivideBlock";
            Description = "Divides the first number by the second number.";
            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "First Number", Description = "The number to divide.", Type = Type.Number, IsRequired = true, IsList = false },
            new Input { Id = Guid.NewGuid(), Name = "Second Number", Description = "The number to divide by.", Type = Type.Number, IsRequired = true, IsList = false }
        };
            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Quotient", Description = "The quotient of the two numbers.", Type = Type.Number, IsList = false }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure)
        {
            double firstNumber = Convert.ToDouble(inputs[0]);
            double secondNumber = Convert.ToDouble(inputs[1]);

            if (secondNumber == 0)
            {
                throw new DivideByZeroException("Cannot divide by zero.");
            }

            double quotient = firstNumber / secondNumber;

            return new List<object> { quotient };
        }
    }

}
