using NodeBaseApi.Version2;
using Type = NodeBaseApi.Version2.Type;


namespace AiDesigner.Shared.Blocks
{
    //Loop blocks
    public class IndexLoop : Block
    {
        public IndexLoop()
        {
            Id = Guid.NewGuid();
            Name = "Index Loop";
            Description = "A loop that iterates a specified number of times.";

            Inputs = new List<Input>
        {
            new Input { Name = "Trigger", Type = Type.Trigger, Description = "The trigger input for starting the loop" },
            new Input { Name = "LoopCount", Type = Type.Number, IsRequired = true, Description = "The number of iterations for the loop" }
        };

            Outputs = new List<Output>
        {
            new Output { Name = "Trigger", Type = Type.Trigger, Description = "The trigger output for each iteration of the loop" },
            new Output { Name = "Index", Type = Type.Number, Description = "The current index of the loop" },
            new Output { Name = "Complete", Type = Type.Trigger, Description = "The trigger output when the loop is completed" }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }
    public class ForLoop : Block
    {
        public ForLoop()
        {
            Id = Guid.NewGuid();
            Name = "For Loop";
            Description = "A loop that iterates through each item in a list.";

            Inputs = new List<Input>
        {
            new Input { Name = "Trigger", Type = Type.Trigger, Description = "The trigger input for starting the loop" },
            new Input { Name = "List", Type = Type.Object, IsRequired = true, IsList = true, Description = "The list to loop through" }
        };

            Outputs = new List<Output>
        {
            new Output { Name = "Trigger", Type = Type.Trigger, Description = "The trigger output for each iteration of the loop" },
            new Output { Name = "CurrentItem", Type = Type.Object, Description = "The current item in the list being looped through" },
            new Output { Name = "Complete", Type = Type.Trigger, Description = "The trigger output when the loop is completed" }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }
    public class WhileLoop : Block
    {
        public WhileLoop()
        {
            Id = Guid.NewGuid();
            Name = "While Loop";
            Description = "A loop that continues to iterate while a condition is true.";

            Inputs = new List<Input>
        {
            new Input { Name = "Trigger", Type = Type.Trigger, Description = "The trigger input for starting the loop" },
            new Input { Name = "Condition", Type = Type.Boolean, IsRequired = true, Description = "The condition to be checked each iteration" }
        };

            Outputs = new List<Output>
        {
            new Output { Name = "Trigger", Type = Type.Trigger, Description = "The trigger output for each iteration of the loop" },
            new Output { Name = "Complete", Type = Type.Trigger, Description = "The trigger output when the loop is completed" }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    //If block
    public class IfBlock : Block
    {
        public IfBlock()
        {
            Id = Guid.NewGuid();
            Name = "If Block";
            Description = "A conditional block that executes different outputs based on a boolean condition.";

            Inputs = new List<Input>
        {
            new Input { Name = "Trigger", Type = Type.Trigger, Description = "Triggers the Ifblock" },
            new Input { Name = "Condition", Type = Type.Boolean, IsRequired = true, Description = "The condition to be checked" }
        };

            Outputs = new List<Output>
        {
            new Output { Name = "True", Type = Type.Trigger, Description = "The trigger output when the condition is true" },
            new Output { Name = "False", Type = Type.Trigger, Description = "The trigger output when the condition is false" }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    //Variable handeling
    public class SetVariable : Block
    {
        public SetVariable()
        {
            Id = Guid.NewGuid();
            Name = "Set Variable";
            Description = "Sets the value of a variable";

            Inputs = new List<Input>
        {
            new Input { Name = "Trigger", Type = Type.Trigger, Description = "Triggers Set Variable" },
            new Input {Id = Guid.NewGuid(),Name = "Value",Description = "The value to set",Type = Type.Object,IsRequired = true,IsList = false}
        };

            Outputs = new List<Output>
        {
            new Output
            {Id = Guid.NewGuid(),Name = "Trigger",Description = "Trigger output",Type = Type.Trigger,IsList = false}
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }
    public class GetVariable : Block
    {
        public GetVariable()
        {
            Id = Guid.NewGuid();
            Name = "Get Variable";
            Description = "Gets the value of a variable";

            Inputs = new List<Input>{};

            Outputs = new List<Output>
        {
            new Output
            {
                Id = Guid.NewGuid(),
                Name = "Value",
                Description = "The value of the variable",
                Type = Type.Object,
                IsList = false
            }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    //Custom block
    public class CustomBlock : Block
    {
        public ProgramStructure SubProgramStructure { get; set; }

        public CustomBlock()
        {
            Id = Guid.NewGuid();
            Name = "Custom Block";
            Description = "A block that contains a sub-program";
            Inputs = new List<Input>();
            Outputs = new List<Output>
        {
            new Output
            {
                Id = Guid.NewGuid(),
                Name = "Output",
                Description = "The output of the block",
                Type = Type.String,
                IsList = false
            }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }
}
