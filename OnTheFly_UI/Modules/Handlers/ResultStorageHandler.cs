using Compunet.YoloSharp.Data;
using Emgu.CV;
using Microsoft.Win32;
using OnTheFly_UI.Modules.DTOs;
using OnTheFly_UI.Modules.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OnTheFly_UI.Modules.Handlers
{
    static class ResultStorageHandler
    {
        private static readonly string _basePath = @"C:\Users\PC\Desktop\Result";
        public static void SaveTest(RequestObject requestObject) {

            var options = new JsonSerializerOptions { WriteIndented = true };

            var b = JsonSerializer.Serialize(requestObject, options);

            Directory.CreateDirectory(_basePath);
            File.WriteAllText($@"{_basePath}\{requestObject.Id}.json", b); 

        }


        public static RequestObject? LoadTest(string jsonPath) 
        {

            var json = File.ReadAllText(jsonPath);


            var resultTest = JsonSerializer.Deserialize<RequestObject>(json);

            return resultTest;

        }

        

    }

}