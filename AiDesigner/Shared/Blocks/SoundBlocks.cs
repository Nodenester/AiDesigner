using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using NodeBaseApi.Version2;
using Type = NodeBaseApi.Version2.Type;

namespace NodeExacuteApi.Data.Blocks
{
    public class AudioLength : Block
    {
        public AudioLength()
        {
            Id = Guid.NewGuid();
            Name = "AudioLength";
            Description = "Gets the length of the audio in seconds.";
            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Audio", Description = "The audio file.", Type = Type.Audio, IsRequired = true }
        };
            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Length", Description = "The length of the audio in seconds.", Type = Type.Number }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }

    }

    public class AudioVolumeChange : Block
    {
        public AudioVolumeChange()
        {
            Id = Guid.NewGuid();
            Name = "AudioVolumeChange";
            Description = "Changes the volume of the audio.";
            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Audio", Description = "The audio file.", Type = Type.Audio, IsRequired = true },
            new Input { Id = Guid.NewGuid(), Name = "Volume", Description = "The desired volume level (0.0 to 1.0).", Type = Type.Number, IsRequired = true }
        };
            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Output Audio", Description = "The audio with the new volume level.", Type = Type.Audio }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }

    }

    public class AudioConversion : Block
    {
        public AudioConversion()
        {
            Id = Guid.NewGuid();
            Name = "AudioConversion";
            Description = "Converts the audio to a specified format.";
            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Audio", Description = "The audio file.", Type = Type.Audio, IsRequired = true },
            new Input { Id = Guid.NewGuid(), Name = "Target Format", Description = "The target audio format (e.g., MP3, WAV).", Type = Type.String, IsRequired = true }
        };
            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Output Audio", Description = "The converted audio file.", Type = Type.Audio }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }

    }

    public class AudioTrimming : Block
    {
        public AudioTrimming()
        {
            Id = Guid.NewGuid();
            Name = "AudioTrimming";
            Description = "Trims the audio to a specified duration.";
            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Audio", Description = "The audio file.", Type = Type.Audio, IsRequired = true },
            new Input { Id = Guid.NewGuid(), Name = "Start Time", Description = "The start time (in seconds).", Type = Type.Number, IsRequired = true },
            new Input { Id = Guid.NewGuid(), Name = "End Time", Description = "The end time (in seconds).", Type = Type.Number, IsRequired = true }
        };
            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Output Audio", Description = "The trimmed audio file.", Type = Type.Audio }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }

    }

    public class AudioAmplification : Block
    {
        public AudioAmplification()
        {
            Id = Guid.NewGuid();
            Name = "AudioAmplification";
            Description = "Amplifies the audio by a specified factor.";
            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Audio", Description = "The audio file.", Type = Type.Audio },
            new Input { Id = Guid.NewGuid(), Name = "Amplification Factor", Description = "The amplification factor.", Type = Type.Number }
        };
            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Output Audio", Description = "The amplified audio file.", Type = Type.Audio }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class AudioOverlay : Block
    {
        public AudioOverlay()
        {
            Id = Guid.NewGuid();
            Name = "Audio Overlay";
            Description = "Overlays two audio files over each other.";
            Inputs = new List<Input>
        {
            new Input { Id = Guid.NewGuid(), Name = "Audio 1", Description = "The audio file.", Type = Type.Audio },
            new Input { Id = Guid.NewGuid(), Name = "Audio 2", Description = "The audio file 2.", Type = Type.Audio },
        };
            Outputs = new List<Output>
        {
            new Output { Id = Guid.NewGuid(), Name = "Output Audio", Description = "The combined audio file.", Type = Type.Audio }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }
}
