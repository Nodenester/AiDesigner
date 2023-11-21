using NodeBaseApi.Version2;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Type = NodeBaseApi.Version2.Type;

namespace NodeExacuteApi.Data.Blocks.AiModels
{
    public class Llama27b : Block
    {
        public Llama27b()
        {
            Id = Guid.NewGuid();
            Name = "Llama2 7b";
            Description = "Llama 2-7b API";
            Inputs = new List<Input>
            {
                new Input { Name = "Query", Type = Type.String, Description = "The input query for the Llama 2-7b API", IsRequired = true },
                new Input { Name = "MaxNewTokens", Type = Type.Number, Description = "Maximum new tokens to be generated (Max tokens 4096)", IsRequired = false},
                new Input { Name = "TopP", Type = Type.Number, Description = "Top P value for controlling randomness", IsRequired = false },
                new Input { Name = "Temperature", Type = Type.Number, Description = "Temperature value for controlling creativity", IsRequired = false },
                new Input { Name = "StopWords", Type = Type.String, IsList = true, Description = "List of words where the AI should stop generating text", IsRequired = false },
                new Input { Name = "ReturnFullText", Type = Type.Boolean, Description = "Whether to include the input prompt in the API response", IsRequired = false }
            };
            Outputs = new List<Output>
            {
                new Output { Name = "ApiResponse", Type = Type.String, Description = "The API response from Llama 2-7b" }
            };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class Llama213b : Block
    {
        public Llama213b()
        {
            Id = Guid.NewGuid();
            Name = "Llama2 13b";
            Description = "Llama 2-13b API";
            Inputs = new List<Input>
            {
                new Input { Name = "Query", Type = Type.String, Description = "The input query for the Llama 2-13b API", IsRequired = true },
                new Input { Name = "MaxNewTokens", Type = Type.Number, Description = "Maximum new tokens to be generated (Max tokens 4096)", IsRequired = false},
                new Input { Name = "TopP", Type = Type.Number, Description = "Top P value for controlling randomness", IsRequired = false },
                new Input { Name = "Temperature", Type = Type.Number, Description = "Temperature value for controlling creativity", IsRequired = false },
                new Input { Name = "StopWords", Type = Type.String, IsList = true, Description = "List of words where the AI should stop generating text", IsRequired = false },
                new Input { Name = "ReturnFullText", Type = Type.Boolean, Description = "Whether to include the input prompt in the API response", IsRequired = false }
            };
            Outputs = new List<Output>
            {
                new Output { Name = "ApiResponse", Type = Type.String, Description = "The API response from Llama 2-13b" }
            };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class Llama270b : Block
    {
        public Llama270b()
        {
            Id = Guid.NewGuid();
            Name = "Llama2 70b";
            Description = "Llama 2-70b API";
            Inputs = new List<Input>
            {
                new Input { Name = "Query", Type = Type.String, Description = "The input query for the Llama 2-70b API", IsRequired = true },
                new Input { Name = "MaxNewTokens", Type = Type.Number, Description = "Maximum new tokens to be generated (Max tokens 4096)", IsRequired = false},
                new Input { Name = "TopP", Type = Type.Number, Description = "Top P value for controlling randomness", IsRequired = false },
                new Input { Name = "Temperature", Type = Type.Number, Description = "Temperature value for controlling creativity", IsRequired = false },
                new Input { Name = "StopWords", Type = Type.String, IsList = true, Description = "List of words where the AI should stop generating text", IsRequired = false },
                new Input { Name = "ReturnFullText", Type = Type.Boolean, Description = "Whether to include the input prompt in the API response", IsRequired = false }
            };
            Outputs = new List<Output>
            {
                new Output { Name = "ApiResponse", Type = Type.String, Description = "The API response from Llama 2-70b" }
            };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class CodeLlama34b : Block
    {
        public CodeLlama34b()
        {
            Id = Guid.NewGuid();
            Name = "CodeLlama 34b";
            Description = "CodeLlama 34b API Integration";
            Inputs = new List<Input>
        {
            new Input { Name = "Query", Type = Type.String, Description = "The input query for the CodeLlama 34b API", IsRequired = true }
        };
            Outputs = new List<Output>
        {
            new Output { Name = "ApiResponse", Type = Type.String, Description = "The API response from CodeLlama 34b" }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }
}
