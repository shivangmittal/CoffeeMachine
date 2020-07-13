using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit.Sdk;

namespace CoffeeMachine.Utility
{
    /// <summary>
    /// This is to read json test files
    /// </summary>
    public class JsonFileDataAttribute : DataAttribute
    {
        private readonly string _jsonFilePath;

        public JsonFileDataAttribute(string jsonFilePath)
        {
            _jsonFilePath = jsonFilePath;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            // Get the absolute path to the JSON file
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), _jsonFilePath);

            if (!File.Exists(path))
            {
                throw new ArgumentException($"Could not find specified file at the given path: {_jsonFilePath}");
            }
            var result = new object[1];
            result[0] = File.ReadAllText(_jsonFilePath);

            // Load the files
            return new[] { result };
        }
    }
}