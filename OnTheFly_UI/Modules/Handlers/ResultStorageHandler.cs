using Compunet.YoloSharp.Data;
using Emgu.CV;
using Microsoft.Win32;
using OnTheFly_UI.Components;
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
        private static readonly string _basePath = @".\Results";
        public static void SaveTest(RequestObject requestObject) {

            var options = new JsonSerializerOptions { WriteIndented = true };

            var serialized = JsonSerializer.Serialize(requestObject, options);

            Directory.CreateDirectory(_basePath);
            File.WriteAllText($@"{_basePath}\{requestObject.Id}.json", serialized); 

        }


        public static RequestObject? LoadTest(string jsonPath) 
        {

            var json = File.ReadAllText(jsonPath);


            var resultTest = JsonSerializer.Deserialize<RequestObject>(json);

            if(resultTest != null)
            {
                if(File.Exists(resultTest.Source))
                    return resultTest;
                else
                {
                    UIMessageBox.Show($"Source does not exist");
                }
            }

            return null;
        }

        

    }

}