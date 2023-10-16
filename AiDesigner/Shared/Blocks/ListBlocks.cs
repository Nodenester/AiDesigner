using System;
using System.Collections.Generic;
using System.IO;
using NodeBaseApi.Version2;
using Type = NodeBaseApi.Version2.Type;

namespace NodeExacuteApi.Data.Blocks
{
    public class AddItem : Block
    {
        public AddItem()
        {
            Id = Guid.NewGuid();
            Name = "Add Item";
            Description = "Adds an item to a list.";
            Inputs = new List<Input>
            {
                new Input { Id = Guid.NewGuid(), Name = "List", Description = "The list to add to.", Type = Type.Object, IsRequired = true, IsList = true },
                new Input { Id = Guid.NewGuid(), Name = "Item", Description = "The item to add.", Type = Type.Object, IsRequired = true }
            };
            Outputs = new List<Output>
            {
                new Output { Id = Guid.NewGuid(), Name = "List", Description = "The list with the item added.", Type = Type.Object, IsList = true }
            };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }

    }

    public class RemoveItem : Block
    {
        public RemoveItem()
        {
            Id = Guid.NewGuid();
            Name = "Remove Item";
            Description = "Removes an item from a list by index.";
            Inputs = new List<Input>
            {
                new Input { Id = Guid.NewGuid(), Name = "List", Description = "The list to remove from.", Type = Type.Object, IsRequired = true, IsList = true },
                new Input { Id = Guid.NewGuid(), Name = "Index", Description = "The index of the item to remove.", Type = Type.Number, IsRequired = true }
            };
            Outputs = new List<Output>
            {
                new Output { Id = Guid.NewGuid(), Name = "List", Description = "The list with the item removed.", Type = Type.Object, IsList = true }
            };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }

    }
    public class GetItem : Block
    {
        public GetItem()
        {
            Id = Guid.NewGuid();
            Name = "Get Item";
            Description = "Gets an item from a list by index.";
            Inputs = new List<Input>
            {
                new Input { Id = Guid.NewGuid(), Name = "List", Description = "The list to get from.", Type = Type.Object, IsRequired = true, IsList = true },
                new Input { Id = Guid.NewGuid(), Name = "Index", Description = "The index of the item to get.", Type = Type.Number, IsRequired = true }
            };
            Outputs = new List<Output>
            {
                new Output { Id = Guid.NewGuid(), Name = "Item", Description = "The item from the list.", Type = Type.Object }
            };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }

    }

    public class SetItem : Block
    {
        public SetItem()
        {
            Id = Guid.NewGuid();
            Name = "Set Item";
            Description = "Sets an item in a list by index.";
            Inputs = new List<Input>
            {
                new Input { Id = Guid.NewGuid(), Name = "List", Description = "The list to set in.", Type = Type.Object, IsRequired = true, IsList = true },
                new Input { Id = Guid.NewGuid(), Name = "Index", Description = "The index of the item to set.", Type = Type.Number, IsRequired = true },
                new Input { Id = Guid.NewGuid(), Name = "Item", Description = "The item to set.", Type = Type.Object, IsRequired = true }
            };
            Outputs = new List<Output>
            {
                new Output { Id = Guid.NewGuid(), Name = "List", Description = "The list with the item set.", Type = Type.Object, IsList = true }
            };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }

    }

    public class FindItem : Block
    {
        public FindItem()
        {
            Id = Guid.NewGuid();
            Name = "Find Item";
            Description = "Finds the index of an item in a list.";
            Inputs = new List<Input>
            {
                new Input { Id = Guid.NewGuid(), Name = "List", Description = "The list to search in.", Type = Type.Object, IsRequired = true, IsList = true },
                new Input { Id = Guid.NewGuid(), Name = "Item", Description = "The item to find.", Type = Type.Object, IsRequired = true }
            };
            Outputs = new List<Output>
            {
                new Output { Id = Guid.NewGuid(), Name = "Index", Description = "The index of the item in the list.", Type = Type.Number }
            };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }

    }
}
