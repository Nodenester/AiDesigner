using NodeBaseApi.Version2;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Type = NodeBaseApi.Version2.Type;
using System.Net.Http.Headers;

namespace NodeExacuteApi.Data.Blocks.AiModels
{
    public class Transcription : Block
    {
        public Transcription()
        {
            Id = Guid.NewGuid();
            Name = "Transcription";
            Description = "This block processes audio data using the Whisper Large V3 API and returns the transcription.";
            Inputs = new List<Input>
        {
            new Input { Name = "AudioData", Type = Type.Audio, IsList = false, Description = "Audio data for processing" }
        };
            Outputs = new List<Output>
        {
            new Output { Name = "Transcription", Type = Type.String, IsList = false, Description = "Transcribed audio text" }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class TextToSpeech : Block
    {
        public TextToSpeech()
        {
            Id = Guid.NewGuid();
            Name = "Text to Speech";
            Description = "This block converts text to speech using the Hugging Face Suno Bark API, supporting multiple languages and voice presets.";
            Inputs = new List<Input>
        {
            new Input { Name = "Text", Type = Type.String, IsList = false, Description = "Text input for speech synthesis" },
            new Input { Name = "VoicePreset", Type = Type.String, IsList = false, Description = "Optional voice preset for speech synthesis" }
        };
            Outputs = new List<Output>
        {
            new Output { Name = "AudioData", Type = Type.Audio, IsList = false, Description = "Generated speech audio data" }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class MusicGeneration : Block
    {
        public MusicGeneration()
        {
            Id = Guid.NewGuid();
            Name = "Music Generation";
            Description = "This block generates music based on textual prompts using the Hugging Face Music Generation API.";
            Inputs = new List<Input>
        {
            new Input { Name = "Prompt", Type = Type.String, IsList = false, Description = "Prompt describing the style or elements of the music" }
        };
            Outputs = new List<Output>
        {
            new Output { Name = "MusicData", Type = Type.Audio, IsList = false, Description = "Generated music audio data" }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class SpeechEnhancement : Block
    {
        public SpeechEnhancement()
        {
            Id = Guid.NewGuid();
            Name = "Speech Enhancement";
            Description = "This block enhances speech quality using the speechbrain/mtl-mimic-voicebank model.";
            Inputs = new List<Input>
        {
            new Input { Name = "AudioData", Type = Type.Audio, IsList = false, Description = "Audio data for enhancement" }
        };
            Outputs = new List<Output>
        {
            new Output { Name = "EnhancedAudio", Type = Type.Audio, IsList = false, Description = "Enhanced audio data" }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }
}
