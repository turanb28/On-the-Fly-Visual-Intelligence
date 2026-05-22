using Compunet.YoloSharp.Data;
using Compunet.YoloSharp.Metadata;
using Emgu.CV.ML.MlEnum;
using OnTheFly_UI.Modules.DTOs;
using SixLabors.Fonts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OnTheFly_UI.Modules.Handlers
{
    public class YoloresultJsonConverter : JsonConverter<List<YoloResult>>
    {
        public override List<YoloResult>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {

            
            var tempPackage =  JsonSerializer.Deserialize(ref reader, typeof(YoloResultPackage), options);

            YoloResultPackage package = (YoloResultPackage)tempPackage;

            List<List<Detection>> detections = new List<List<Detection>>();

            foreach (var detectionArray in package.Results)
            {
                var tempList = new List<Detection>();

                foreach (var detection in detectionArray)
                {
                    tempList.Add(
                    new Detection()
                    {
                        Bounds = ((JsonElement)detection["Bounds"]).Deserialize<SixLabors.ImageSharp.Rectangle>(),
                        Confidence = ((JsonElement)detection["Confidence"]).Deserialize<float>(),
                        Name = ((JsonElement)detection["Name"]).Deserialize<YoloName>()
                    });
                }
                detections.Add(tempList);
            }


            List<YoloResult> result = new List<YoloResult>();


            foreach (var item in detections)
                result.Add(new YoloResult<Detection>(item.ToArray()) { ImageSize = SixLabors.ImageSharp.Size.Empty, Speed = new SpeedResult() } );

            return result;

        }

        public override void Write(Utf8JsonWriter writer, List<YoloResult> value, JsonSerializerOptions options)
        {
            Type? type = value.FirstOrDefault()?.GetType().GetGenericArguments().FirstOrDefault();

            if (type == null)
                throw new InvalidOperationException("Unable to determine the type of YoloResult.");


            var a = value.Cast<YoloResult<Detection>>();

            var list = new List<Detection[]>();

            foreach (var item in a)
                list.Add(item.ToArray());



            var dictList = new List<List<Dictionary<string, object>>>();

            foreach (var detectionArray in list)
            {
                var tempList = new List<Dictionary<string, object>>();
                foreach (var detection in detectionArray)
                {
                    var tempDict = new Dictionary<string, object?>();
                    tempDict["Bounds"] = detection.Bounds;
                    tempDict["Confidence"] = detection.Confidence;
                    tempDict["Name"] = detection.Name;
                    tempList.Add(tempDict);
                }
                dictList.Add(tempList);
            }

            var r = new YoloResultPackage() { Results = dictList, TaskType = type.ToString() };

            JsonSerializer.Serialize(writer, r, options);
        }



        public class YoloResultPackage
        {
            public List<List<Dictionary<string, object>>> Results { get; set; } = new List<List<Dictionary<string, object>>>();
            public string TaskType { get; set; } = string.Empty;
        }


    }
}
