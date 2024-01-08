using NodeBaseApi.Version2;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Type = NodeBaseApi.Version2.Type;
using System.Net.Http.Headers;
using System.Drawing.Imaging;
using System.Drawing;

namespace NodeExacuteApi.Data.Blocks.AiModels
{
    public class Segmentation : Block
    {
        public Segmentation()
        {
            Id = Guid.NewGuid();
            Name = "Segmentation";
            Description = "Processes an image using the NVIDIA SegFormer API and returns segmentation results.";
            Inputs = new List<Input>
        {
            new Input { Name = "ImageData", Type = Type.Picture, IsList = false, Description = "Image data for segmentation" }
        };
            Outputs = new List<Output>
        {
            new Output { Name = "SegmentationResults", Type = Type.Object, IsList = true, Description = "Segmentation results with labels and masks" }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class ImageClassification : Block
    {
        public ImageClassification()
        {
            Id = Guid.NewGuid();
            Name = "Image Classification";
            Description = "Processes an image using the ViT Base Patch16 API and returns a list of labels with scores.";
            Inputs = new List<Input>
        {
            new Input { Name = "ImageData", Type = Type.Picture, IsList = false, Description = "Image data for analysis" }
        };
            Outputs = new List<Output>
        {
            new Output { Name = "AnalysisResults", Type = Type.Object, IsList = true, Description = "Labels and scores from image analysis" }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class ImageCaptioning : Block
    {
        public ImageCaptioning()
        {
            Id = Guid.NewGuid();
            Name = "Image Captioning";
            Description = "Generates a text description for a given image using the vit-gpt2-image-captioning model.";
            Inputs = new List<Input>
        {
            new Input { Name = "ImageData", Type = Type.Picture, IsList = false, Description = "Image data to be captioned" }
        };
            Outputs = new List<Output>
        {
            new Output { Name = "Caption", Type = Type.String, IsList = false, Description = "Generated image description" }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class ObjectCroping : Block
    {
        public ObjectCroping()
        {
            Id = Guid.NewGuid();
            Name = "Object Croping";
            Description = "Detects objects in an image using DETR-ResNet-50 and returns cropped images with labels.";
            Inputs = new List<Input>
        {
            new Input { Name = "ImageData", Type = Type.Picture, IsList = false, Description = "Image data for object detection" }
        };
            Outputs = new List<Output>
        {
            new Output { Name = "CroppedImagesWithLabels", Type = Type.Object, IsList = true, Description = "List of tuples containing cropped images and labels" }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class TextToImage : Block
    {
        public TextToImage()
        {
            Id = Guid.NewGuid();
            Name = "Text To Image";
            Description = "Generates an image from text using Stable Diffusion v1.5 model. (very fast)";
            Inputs = new List<Input>
        {
            new Input { Name = "TextPrompt", Type = Type.String, IsList = false, Description = "Text prompt for generating the image" }
        };
            Outputs = new List<Output>
        {
            new Output { Name = "GeneratedImage", Type = Type.Picture, IsList = false, Description = "Generated image data" }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class Dalle3 : Block
    {
        public Dalle3()
        {
            Id = Guid.NewGuid();
            Name = "Dalle 3";
            Description = "Generates an image from text using Dalle-3 model. (slow but very good)";
            Inputs = new List<Input>
        {
            new Input { Name = "TextPrompt", Type = Type.String, IsList = false, Description = "Text prompt for generating the image" }
        };
            Outputs = new List<Output>
        {
            new Output { Name = "GeneratedImage", Type = Type.Picture, IsList = false, Description = "Generated image data" }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }

    public class TextReader : Block
    {
        public TextReader()
        {
            Id = Guid.NewGuid();
            Name = "Text Reader";
            Description = "This block reads text from images using the OCR-Donut-CORD model (it is bad so use a llm to predict what it actually means).";
            Inputs = new List<Input>
        {
            new Input { Name = "ImageData", Type = Type.Picture, IsList = false, Description = "Image data for text extraction" }
        };
            Outputs = new List<Output>
        {
            new Output { Name = "ExtractedText", Type = Type.String, IsList = true, Description = "Extracted text from the image" }
        };
        }

        public override List<object> Execute(List<object> inputs, ProgramStructure programStructure) { return null; }
    }
}
