using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

// Create the base and all functions   and write what they should do in the comments
namespace NodeBaseApi.Version2
{
    public abstract class ProgramObject
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string AuthorName { get; set; }
        public bool IsPublic { get; set; }
        public bool SupportsSessions { get; set; }
        public byte[]? Image { get; set; }
        public ProgramStructure ProgramStructure { get; set; }
        public DateTime? LastOpened { get; set; }
    }

    public class CustomProgram : ProgramObject
    {

        public Guid ApiKey { get; set; }
        public CustomProgram()
        {
            // Parameterless constructor
        }
    }

    public class CustomBlockProgram : ProgramObject
    {
        public CustomBlockProgram()
        {
            // Parameterless constructor
        }
    }

    public class ProgramStructure
    {
        public List<ProgramBlock> ProgramBlocks { get; set; } = new List<ProgramBlock>();
        public Dictionary<Guid, Variable> Variables { get; set; } = new Dictionary<Guid, Variable>();
        public List<Output> ProgramStart { get; set; } = new List<Output>(
            new Output[]
            {
                new Output()
                {
                    Id = Guid.NewGuid(),
                    Name = "Start",
                    Type = Type.Trigger,
                    IsList = false,
                    Description = "The start of the program."
                }
            }
            );
        public Tuple<int, int> ProgramStartLocation { get; set; } = new Tuple<int, int>(0, 200);

        public List<Input> ProgramEnd { get; set; } = new List<Input>(
            new Input[]
            {
                new Input()
                {
                    Id = Guid.NewGuid(),
                    Name = "End",
                    Type = Type.Trigger,
                    IsList = false,
                    Description = "The end of the program."
                }
            }
            );
        public Dictionary<Guid, Guid> ProgramEndConnections { get; set; } = new Dictionary<Guid, Guid>();
        public Tuple<int, int> ProgramEndLocation { get; set; } = new Tuple<int, int>(300, 200);
        public float zoom { get; set; } = 1;

        public Tuple<double, double> CameraPos { get; set; } = new Tuple<double, double>(0, 0);
        public Dictionary<Guid, object> InputValues { get; set; } = new Dictionary<Guid, object>();
        public Dictionary<Guid, object> OutputValues { get; set; } = new Dictionary<Guid, object>();
        public Dictionary<Guid, CustomBlockProgram> CustomPrograms { get; set; } = new Dictionary<Guid, CustomBlockProgram>();
        public Dictionary<Guid, object> BlockOutputValues { get; set; } = new Dictionary<Guid, object>();
        public Dictionary<Guid, object> DirectInputValues { get; set; } = new Dictionary<Guid, object>();

        //It Need to contain these functions
        public void AddProgramInput(Output output)
        {
            ProgramStart.Add(output);
        }

        public void AddProgramOutput(Input input)
        {
            ProgramEnd.Add(input);
        }

        public void SetInputValue(int inputIndex, object value)
        {
            if (inputIndex < 0 || inputIndex >= ProgramStart.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(inputIndex), "Invalid input index");
            }

            InputValues[ProgramStart[inputIndex].Id] = value;
        }

        public object GetProgramOutputValue(int outputIndex)
        {
            if (outputIndex < 0 || outputIndex >= ProgramEnd.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(outputIndex), "Invalid output index");
            }

            // Get the ID of the input in ProgramEnd
            Guid inputId = ProgramEnd[outputIndex].Id;

            // Use ProgramEndConnections to get the associated output ID
            if (ProgramEndConnections.TryGetValue(inputId, out Guid outputId))
            {
                return OutputValues[outputId];
            }

            return null;
        }

        public void AddProgramBlock(ProgramBlock block)
        {
            ProgramBlocks.Add(block);
        }

        public void SetDirectInputValue(Guid inputId, object value)
        {
            DirectInputValues[inputId] = value;
        }

        public bool TryConnectProgramBlock(Guid outputId, Guid inputId)
        {
            // Find the blocks that own the output and input.
            ProgramBlock? outputProgramBlock = ProgramBlocks.FirstOrDefault(block => block.Block.Outputs.Any(output => output.Id == outputId));
            ProgramBlock? inputProgramBlock = ProgramBlocks.FirstOrDefault(block => block.Block.Inputs.Any(input => input.Id == inputId));

            Output? programStartOutput = ProgramStart.FirstOrDefault(output => output.Id == outputId);
            Input? programEndInput = ProgramEnd.FirstOrDefault(input => input.Id == inputId);

            if (outputProgramBlock == null && programStartOutput == null)
            {
                throw new NullReferenceException("No matching output found.");
            }

            if (inputProgramBlock == null && programEndInput == null)
            {
                throw new NullReferenceException("No matching input found.");
            }

            Output output;
            Input input;

            if (outputProgramBlock != null)
            {
                output = outputProgramBlock.Block.Outputs.FirstOrDefault(o => o.Id == outputId);
            }
            else
            {
                output = programStartOutput;
            }

            if (inputProgramBlock != null)
            {
                input = inputProgramBlock.Block.Inputs.FirstOrDefault(i => i.Id == inputId);
            }
            else
            {
                input = programEndInput;
            }

            if (output == null || input == null)
            {
                return false;
            }

            // Check that the output and input are compatible.
            if ((output.Type != input.Type || output.IsList != input.IsList) && (output.Type != Type.Object && input.Type != Type.Object))
            {
                return false;
            }

            if (inputProgramBlock != null)
            {
                int index = inputProgramBlock.Block.Inputs.FindIndex(i => i.Id == inputId);
                if (index != -1)
                {
                    if (inputProgramBlock.Inputs == null)
                    {
                        inputProgramBlock.Inputs = new List<Guid>(new Guid[inputProgramBlock.Block.Inputs.Count]);
                    }
                    if (inputProgramBlock.Inputs.Count() -1 >= index )
                    {
                        inputProgramBlock.Inputs[index] = outputId;
                    }
                    else
                    {
                        inputProgramBlock.Inputs.Add(outputId);
                    }
                }
                else
                {
                    throw new Exception($"No input with id '{inputId}' found in the block's inputs.");
                }
                //mayby add  InputValues[inputId] = null;
            }

            if (programEndInput != null)
            {
                // Store the connection
                ProgramEndConnections[inputId] = outputId;

                if (!OutputValues.ContainsKey(outputId))
                {
                    OutputValues[outputId] = null;
                }
            }

            if (programStartOutput != null)
            {
                InputValues[inputId] = null;
            }

            return true;
        }

        public void RemoveDirectInputValue(Guid inputId)
        {
            DirectInputValues.Remove(inputId);
        }

        public void RemoveConnection(Guid inputId)
        {
            bool removed = false;
            foreach (var block in ProgramBlocks)
            {
                if (block.Inputs != null)
                {
                    DirectInputValues.Remove(inputId);
                    InputValues.Remove(inputId);
                    removed = block.Inputs.Remove(inputId);
                }
            }   
            if (!removed)
            {
                var keyToRemove = ProgramEndConnections.FirstOrDefault(pair => pair.Value == inputId).Key;
                if (keyToRemove != Guid.Empty)
                {
                    ProgramEndConnections.Remove(keyToRemove);
                }
            }
        }

        public async Task<bool> RemoveProgramBlock(Guid blockId)
        {
            try
            {
                // Check if ProgramBlocks is null
                if (ProgramBlocks == null)
                {
                    throw new NullReferenceException("ProgramBlocks is null.");
                }

                // Find the block to remove
                ProgramBlock blockToRemove = ProgramBlocks.FirstOrDefault(block => block.Id == blockId);

                // Check if the block exists
                if (blockToRemove == null)
                {
                    throw new KeyNotFoundException($"No block with ID {blockId} exists.");
                }

                // Remove connections to the block's inputs
                if (blockToRemove.Inputs != null)
                {
                    foreach (Guid inputId in blockToRemove.Inputs.ToList())
                    {
                        RemoveConnection(inputId);
                    }
                }

                // Remove connections to the block's outputs
                if (blockToRemove.Block?.Outputs != null) // Ensure block and Outputs are not null
                {
                    foreach (var output in blockToRemove.Block.Outputs)
                    {
                        Guid outputId = output.Id;

                        // Remove from OutputValues
                        OutputValues.Remove(outputId);

                        // Remove from ProgramEndConnections
                        var keysToRemove = ProgramEndConnections
                            .Where(kv => kv.Value == outputId)
                            .Select(kv => kv.Key)
                            .ToList();

                        foreach (var key in keysToRemove)
                        {
                            ProgramEndConnections.Remove(key);
                        }

                        // Remove from other blocks' inputs
                        foreach (var otherBlock in ProgramBlocks)
                        {
                            if (otherBlock.Inputs != null)
                            {
                                for (int i = 0; i < otherBlock.Inputs.Count; i++)
                                {
                                    if (otherBlock.Inputs[i] == outputId)
                                    {
                                        otherBlock.Inputs[i] = Guid.Empty; // or set to some default value
                                    }
                                }
                            }
                        }
                    }
                }

                // Remove the block
                ProgramBlocks.Remove(blockToRemove);

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it accordingly
                Console.WriteLine($"An error occurred while removing the block: {ex.Message}");
                return false;
            }
        }

        public Guid CreateVariable()
        {
            Guid variableId = Guid.NewGuid();
            Variables[variableId] = new Variable { Id = variableId };
            return variableId;
        }

        public void RemoveVariable(Guid variableId)
        {
            Variables.Remove(variableId);
        }

        public bool SetVariableValue(Guid variableId, object value)
        {
            if (!Variables.ContainsKey(variableId))
            {
                return false;
            }

            Variables[variableId].Value = value;
            return true;
        }

        public object GetVariableValue(Guid variableId)
        {
            return Variables.TryGetValue(variableId, out Variable variable) ? variable.Value : null;
        }

        public List<string> CheckErrors()
        {
            List<string> errors = new();

            // Check for required inputs without values
            foreach (var input in ProgramBlocks.SelectMany(block => block.Block.Inputs))
            {
                if (input.IsRequired && !InputValues.ContainsKey(input.Id) && input.Type.ToString() != "Trigger")
                {
                    errors.Add($"Required input '{input.Name}' is missing a value.");
                    //throw new MissingRequiredInputException($"Required input '{input.Name}' is missing a value.");
                }
            }

            // Check for disconnected inputs
            foreach (var input in ProgramBlocks.SelectMany(block => block.Block.Inputs))
            {
                if (!InputValues.ContainsKey(input.Id) && input.Type.ToString() != "Trigger")
                {
                    errors.Add($"Input '{input.Name}' is not connected.");
                    //throw new DisconnectedInputException($"Input '{input.Name}' is not connected.");
                }
            }
            // Additional error checks can be added here...

            return errors;
        }
    }


    //BlockData-------------------------------------------------------
    public abstract class Block
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Input> Inputs { get; set; }
        public List<Output> Outputs { get; set; }

        public abstract List<object> Execute(List<object> inputs, ProgramStructure programStructure);

        public Block()
        {
            // Parameterless constructor
        }
    }

    public class ProgramBlock
    {
        public Guid Id { get; set; }
        public Block Block { get; set; }
        public List<Guid> Inputs { get; set; }
        public List<Guid> Outputs { get; set; }
        public Guid VariableId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
    //Data------------------------------------------------------------
    public class Variable
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Type Type { get; set; }
        public bool IsList { get; set; }
        public bool IsSessionVariable { get; set; }
        public object Value { get; set; }
    }
    public class Session
    {
        public string SessionId { get; set; }
        public string UserId { get; set; }
        public string ProgramId { get; set; }
        public string Variables { get; set; }
        public string SessionName { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime LastEditedTime { get; set; }
    }
    public class Input
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Type Type { get; set; }
        public bool IsRequired { get; set; }
        public bool IsList { get; set; }
    }
    public class Output
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Type Type { get; set; }
        public bool IsList { get; set; }
    }

    public enum Type
    {
        String,
        Picture,
        Number,
        Boolean,
        Audio,
        Trigger,
        Object
    }

    public class Call
    {
        public string ProgramId { get; set; }
        public string ApiUserId { get; set; }
        public bool? IsTest { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Cost { get; set; }

        [JsonIgnore]
        public List<object> Input { get; set; }

        [JsonIgnore]
        public List<object> Output { get; set; }

        public string InputJson
        {
            get => JsonConvert.SerializeObject(Input);
            set => Input = JsonConvert.DeserializeObject<List<object>>(value);
        }

        public string OutputJson
        {
            get => JsonConvert.SerializeObject(Output);
            set => Output = JsonConvert.DeserializeObject<List<object>>(value);
        }

        public Call()
        {
            Input = new List<object>();
            Output = new List<object>();
        }
    }

    //Workshop handeling
    public class WorkshopArticle
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SearchClass { get; set; } = "Type 1";
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string ProgramId { get; set; }
        public string ApiKey { get; set; }
        public byte[] ProgramImage { get; set; }
        public byte[] Image { get; set; }
        public string Type { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public int Downloads { get; set; } = 0;
        public int Rating { get; set; } = 0;
        public string Status { get; set; } = "Pending";
        public bool IsPublic { get; set; } = false;
    }

    public class UserArticle
    {
        public string UserId { get; set; }
        public string ArticleId { get; set; }
        public bool IsCreator { get; set; }
        public int? Rating { get; set; }
        public string Review { get; set; }
        public bool IsFavorite { get; set; } = false;
    }

    //News
    public class NewsArticle
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime PublishDate { get; set; }
        public byte[] ImageData { get; set; }
    }

    //ApiKey
    public class ApiKey
    {
        public string apiKey { get; set; }
        public string UserId { get; set; }
        public DateTime Created { get; set; }
        public string Name { get; set; }
    }

    //Tutoraial stuff
    public class Tutorial
    {
        public int TutorialId { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public byte[] Image { get; set; }
        public bool IsCompleted { get; set; } // User-specific
        public DateTime? CompletionDate { get; set; } // User-specific
    }

    public class UserTutorial
    {
        public string UserId { get; set; }
        public int TutorialId { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CompletionDate { get; set; }
    }

    //chache stuff
    public class ChacheAnalytics
    {
        public List<DataItem> programItems { get; set; }
        public List<DataItem> sourceItems { get; set; }
        public List<Call> last100Calls { get; set; }
        public List<WorkshopArticle> workshopArticles { get; set; }
        public List<NewsArticle> newsArticle { get; set; }
        public List<Tutorial> tutorials { get; set; }
    }

    public class DataItem
    {
        public string ProgramShortcut { get; set; }
        public string Program { get; set; }
        public double Cost { get; set; }
    }



    //.netStuuff
    public class AggregatedData
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int? Day { get; set; } // Optional, based on grouping
        public int? Hour { get; set; } // Optional, based on grouping
        public int CallCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }



    //Error handling------------------------------------------------
    public class InvalidConnectionException : Exception
    {
        public InvalidConnectionException(string message) : base(message) { }
    }

    public class MissingRequiredInputException : Exception
    {
        public MissingRequiredInputException(string message) : base(message) { }
    }

    public class DisconnectedInputException : Exception
    {
        public DisconnectedInputException(string message) : base(message) { }
    }
}

