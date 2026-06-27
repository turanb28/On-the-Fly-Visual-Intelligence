using Clipper2Lib;
using Compunet.YoloSharp.Data;
using Compunet.YoloSharp.Memory;
using Compunet.YoloSharp.Metadata;
using Emgu.CV.Cuda;
using Emgu.CV.ML.MlEnum;
using OnTheFly_UI.Modules.DTOs;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Memory;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Documents;

namespace OnTheFly_UI.Modules.Handlers
{
    public class YoloresultJsonConverter : JsonConverter<List<YoloResult>>
    {
        public override List<YoloResult>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {

            
            var tempPackage =  JsonSerializer.Deserialize(ref reader, typeof(YoloResultPackage), options);


            if (tempPackage == null)
                return null;

            YoloResultPackage package = (YoloResultPackage)tempPackage;
            
            switch(package.TaskType)
            {
                case "Detection":
                    return ResultstoDetection(package.Results);
                case "Pose":
                    return ResultstoPose(package.Results);
                case "Segmentation":
                    return ResultstoSegmentation(package.Results);
                case "Classification":
                    return ResultstoClassification(package.Results);
                case "ObbDetection":
                    return ResultstoObbDetection(package.Results);
                default:
                    return null;
            }

        }

        public override void Write(Utf8JsonWriter writer, List<YoloResult> value, JsonSerializerOptions options)
        {


            Type? nullabletype = value.FirstOrDefault()?.GetType().GetGenericArguments().FirstOrDefault();
            
            string type = "None";

            if (nullabletype != null)
                type = nullabletype.ToString().Replace("Compunet.YoloSharp.Data.", string.Empty);
            else
                return;

            var dictList = new List<List<Dictionary<string, object>>>();


            switch (type)
            {
                case "Detection":
                    dictList = DetectiontoResults(value);
                    break;
                case "Pose":
                    dictList = PosetoResults(value);
                    break;
                case "Segmentation":
                    dictList = SegmentationtoResults(value);
                    break;
                case "Classification":
                    dictList = ClassificationtoResult(value);
                    break;
                case "ObbDetection":
                    dictList = ObbDetectiontoResults(value);
                    break;
                default:
                    return;
            }


            var r = new YoloResultPackage() { Results = dictList, TaskType = type };

            JsonSerializer.Serialize(writer, r, options);
        }



        private List<List<Dictionary<string, object>>> DetectiontoResults(List<YoloResult> value)
        {
            var values = value.Cast<YoloResult<Detection>>();

            var list = new List<Detection[]>();

            foreach (var item in values)
                list.Add(item.ToArray());


            List<List<Dictionary<string, object>>> dictList = new List<List<Dictionary<string, object>>>();
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

            return dictList;
        }

        private List<List<Dictionary<string, object>>> PosetoResults(List<YoloResult> value)
        {
            var pose_len = 17;

            var values = value.Cast<YoloResult<Pose>>();

            var list = new List<Pose[]>();


            var keypoints = typeof(Pose).GetField("<keypoints>P", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance); 

            if (keypoints == null)
                return new List<List<Dictionary<string, object>>>();

            foreach (var item in values)
                list.Add(item.ToArray());


            List<List<Dictionary<string, object>>> dictList = new List<List<Dictionary<string, object>>>();

            foreach (var poseArray in list)
            {
                var tempList = new List<Dictionary<string, object>>();
                foreach (var pose in poseArray)
                {
                    var tempDict = new Dictionary<string, object>();
                    var temoKPArray = new Keypoint[pose_len];
                    tempDict["Bounds"] = pose.Bounds;
                    tempDict["Confidence"] = pose.Confidence;
                    tempDict["Name"] = pose.Name;
                    tempDict["Keypoints"] = keypoints.GetValue(pose) ?? new Keypoint[pose_len];

                    tempList.Add(tempDict);
                }
                dictList.Add(tempList);
            }
            return dictList;

        }

        private List<List<Dictionary<string, object>>> SegmentationtoResults(List<YoloResult> value)
        {
            var values = value.Cast<YoloResult<Segmentation>>();

            var list = new List<Segmentation[]>();

            foreach (var item in values)
                list.Add(item.ToArray());

            List<List<Dictionary<string, object>>> dictList = new List<List<Dictionary<string, object>>>();


            var bigWidth =  list.Max(x => x.Max(y => y.Bounds.Width));
            var bigHeight = list.Max(x => x.Max(y => y.Bounds.Height));

            foreach (var segArray in list)
            {
                 FieldInfo? field = typeof(BitmapBuffer).GetField(
                                "_buffer",
                                BindingFlags.NonPublic | BindingFlags.Instance
                            );

                if (field == null)
                    return new List<List<Dictionary<string, object>>>();

                var tempList = new List<Dictionary<string, object>>();
                foreach (var seg in segArray)
                {
                    var tempDict = new Dictionary<string, object?>();
                    tempDict["Bounds"] = seg.Bounds;
                    tempDict["Confidence"] = seg.Confidence;
                    tempDict["Name"] = seg.Name;

                    var buffer = (Memory<float>)field.GetValue(seg.Mask);

                    var bytearray = new byte[buffer.Length * sizeof(float)];

                    Buffer.BlockCopy(buffer.ToArray(), 0, bytearray, 0, bytearray.Length);

                    tempDict["Mask"] = bytearray;

                    tempList.Add(tempDict);

                }
                dictList.Add(tempList);


            }
            return dictList;


        }

        private List<List<Dictionary<string, object>>> ClassificationtoResult(List<YoloResult> value)
        {
            var values = value.Cast<YoloResult<Classification>>();
            var list = new List<Classification[]>();
            foreach (var item in values)
                list.Add(item.ToArray());
            List<List<Dictionary<string, object>>> dictList = new List<List<Dictionary<string, object>>>();
            foreach (var classArray in list)
            {
                var tempList = new List<Dictionary<string, object>>();
                foreach (var classification in classArray)
                {
                    var tempDict = new Dictionary<string, object?>();
                    tempDict["Confidence"] = classification.Confidence;
                    tempDict["Name"] = classification.Name;
                    tempList.Add(tempDict);
                }
                dictList.Add(tempList);
            }
            return dictList;
        }
           
        private List<List<Dictionary<string, object>>> ObbDetectiontoResults(List<YoloResult> value)
        {
            var values = value.Cast<YoloResult<ObbDetection>>();
            var list = new List<ObbDetection[]>();
            foreach (var item in values)
                list.Add(item.ToArray());
            List<List<Dictionary<string, object>>> dictList = new List<List<Dictionary<string, object>>>();
            foreach (var obbArray in list)
            {
                var tempList = new List<Dictionary<string, object>>();
                foreach (var obb in obbArray)
                {
                    var tempDict = new Dictionary<string, object?>();
                    tempDict["Bounds"] = obb.Bounds;
                    tempDict["Confidence"] = obb.Confidence;
                    tempDict["Name"] = obb.Name;
                    tempDict["Angle"] = obb.Angle;
                    tempList.Add(tempDict);
                }
                dictList.Add(tempList);
            }
            return dictList;
        }


        private List<YoloResult> ResultstoPose(List<List<Dictionary<string, object>>> list)
        {
            var pose_len = 17;
            List<List<Pose>> poseList = new List<List<Pose>>();
            foreach (var dictArray in list)
            {
                var tempList = new List<Pose>();
                foreach (var dict in dictArray)
                {
                    var tempkeypoints = ((JsonElement)dict["Keypoints"]).Deserialize<Keypoint[]>();
                    tempList.Add(new Pose(tempkeypoints)
                    {
                        Bounds = ((JsonElement)dict["Bounds"]).Deserialize<SixLabors.ImageSharp.Rectangle>(),
                        Confidence = ((JsonElement)dict["Confidence"]).Deserialize<float>(),
                        Name = ((JsonElement)dict["Name"]).Deserialize<YoloName>(),
                    });
                }
                poseList.Add(tempList);
            }

            List<YoloResult> result = new List<YoloResult>();


            foreach (var item in poseList)
                result.Add(new YoloResult<Pose>(item.ToArray()) { ImageSize = SixLabors.ImageSharp.Size.Empty, Speed = new SpeedResult() });


            return result;
        }

        private List<YoloResult> ResultstoDetection(List<List<Dictionary<string, object>>> list)
        {
            List<Detection[]> detectionList = new List<Detection[]>();
            foreach (var dictArray in list)
            {
                var tempList = new List<Detection>();
                foreach (var dict in dictArray)
                {
                    tempList.Add(
                    new Detection()
                    {
                        Bounds = ((JsonElement)dict["Bounds"]).Deserialize<SixLabors.ImageSharp.Rectangle>(),
                        Confidence = ((JsonElement)dict["Confidence"]).Deserialize<float>(),
                        Name = ((JsonElement)dict["Name"]).Deserialize<YoloName>()
                    });
                }
                detectionList.Add(tempList.ToArray());
            }

            List<YoloResult> result = new List<YoloResult>();


            foreach (var item in detectionList)
                result.Add(new YoloResult<Detection>(item.ToArray()) { ImageSize = SixLabors.ImageSharp.Size.Empty, Speed = new SpeedResult() });

            return result;
        }

        private List<YoloResult> ResultstoSegmentation(List<List<Dictionary<string, object>>> list)
        {
            List<Segmentation[]> segList = new List<Segmentation[]>();
            foreach (var dictArray in list)
            {
                var tempList = new List<Segmentation>();
                foreach (var dict in dictArray)
                {
                    var bound = ((JsonElement)dict["Bounds"]).Deserialize<SixLabors.ImageSharp.Rectangle>();


                    dict["Mask"] = ((JsonElement)dict["Mask"]).Deserialize<byte[]>();

                    var bytearray = (byte[])dict["Mask"];

                    var floatarrray = new float[bytearray.Length / sizeof(float)];

                    Buffer.BlockCopy(bytearray.ToArray(), 0, floatarrray, 0, bytearray.Length);



                    tempList.Add(

                    new Segmentation()
                    {
                        Bounds = bound,
                        Confidence = ((JsonElement)dict["Confidence"]).Deserialize<float>(),
                        Name = ((JsonElement)dict["Name"]).Deserialize<YoloName>(),
                        Mask = new BitmapBuffer(new Memory<float>(floatarrray), bound.Width, bound.Height) //new BitmapBuffer(((JsonElement)dict["Mask"]).Deserialize<Memory<float>>(), bound.Width, bound.Height)
                    });
                }
                segList.Add(tempList.ToArray());
            }
            List<YoloResult> result = new List<YoloResult>();
            
            foreach (var item in segList)
                result.Add(new YoloResult<Segmentation>(item.ToArray()) { ImageSize = SixLabors.ImageSharp.Size.Empty, Speed = new SpeedResult() });

            return result;
        }

        private List<YoloResult> ResultstoClassification(List<List<Dictionary<string, object>>> list)
        {
            List<Classification[]> classList = new List<Classification[]>();
            foreach (var dictArray in list)
            {
                var tempList = new List<Classification>();
                foreach (var dict in dictArray)
                {
                    tempList.Add(
                    new Classification()
                    {
                        Confidence = ((JsonElement)dict["Confidence"]).Deserialize<float>(),
                        Name = ((JsonElement)dict["Name"]).Deserialize<YoloName>()
                    });
                }
                classList.Add(tempList.ToArray());
            }
            List<YoloResult> result = new List<YoloResult>();

            foreach (var item in classList)
                result.Add(new YoloResult<Classification>(item.ToArray()) { ImageSize = SixLabors.ImageSharp.Size.Empty, Speed = new SpeedResult() });

            return result;
        }
    
        private List<YoloResult> ResultstoObbDetection(List<List<Dictionary<string, object>>> list)
        {
            List<ObbDetection[]> obbList = new List<ObbDetection[]>();
            foreach (var dictArray in list)
            {
                var tempList = new List<ObbDetection>();
                foreach (var dict in dictArray)
                {
                    tempList.Add(
                    new ObbDetection()
                    {
                        Bounds = ((JsonElement)dict["Bounds"]).Deserialize<SixLabors.ImageSharp.Rectangle>(),
                        Confidence = ((JsonElement)dict["Confidence"]).Deserialize<float>(),
                        Name = ((JsonElement)dict["Name"]).Deserialize<YoloName>(),
                        Angle = ((JsonElement)dict["Angle"]).Deserialize<float>()
                    });
                }
                obbList.Add(tempList.ToArray());
            }
            List<YoloResult> result = new List<YoloResult>();
            foreach (var item in obbList)
                result.Add(new YoloResult<ObbDetection>(item.ToArray()) { ImageSize = SixLabors.ImageSharp.Size.Empty, Speed = new SpeedResult() });
            return result;
        }






    }





    public class YoloResultPackage
    {
        public List<List<Dictionary<string, object>>> Results { get; set; } = new List<List<Dictionary<string, object>>>();
        public string TaskType { get; set; } = string.Empty;

    }


}
