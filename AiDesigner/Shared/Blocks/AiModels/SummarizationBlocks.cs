using NodeBaseApi.Version2;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Type = NodeBaseApi.Version2.Type;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace NodeExacuteApi.Data.Blocks.AiModels
{
    public class Summarize : Block
    {
        public Summarize()
        {
            Id = Guid.NewGuid();
            Name = "Summarize";
            Description = "Summarize text using Bart Large CNN API";
            Inputs = new List<Input>
        {
            new Input { Name = "Text", Type = Type.String, Description = "The text input for the Bart Large CNN summarization API" }
        };
            Outputs = new List<Output>
        {
            new Output { Name = "Summary", Type = Type.String, Description = "The summarized text from Bart Large CNN API" }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class TextToText : Block
    {
        public TextToText()
        {
            Id = Guid.NewGuid();
            Name = "Text to Text";
            Description = "This block processes text input and generates text output using the Google FLAN-T5-XXL API.";
            Inputs = new List<Input>
        {
            new Input { Name = "Text", Type = Type.String, IsList = false, Description = "Text input for processing" }
        };
            Outputs = new List<Output>
        {
            new Output { Name = "Text", Type = Type.String, IsList = false, Description = "Generated text output" }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class FactualConsistency : Block
    {
        public FactualConsistency()
        {
            Id = Guid.NewGuid();
            Name = "Factual Consistency";
            Description = "This block checks the factual consistency between two text inputs using the vectara/hallucination_evaluation_model.";
            Inputs = new List<Input>
        {
            new Input { Name = "Text1", Type = Type.String, IsList = false, Description = "First text input" },
            new Input { Name = "Text2", Type = Type.String, IsList = false, Description = "Second text input" }
        };
            Outputs = new List<Output>
        {
            new Output { Name = "ConsistencyScore", Type = Type.Number, IsList = false, Description = "Factual consistency score" }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }
}
