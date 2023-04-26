using AutoPostingVK;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class JsonDataReader
{
    public List<Content> GetData(string jsonFilePath)
    {
        List<Content> result = new List<Content>();

        // Читаем json-файл в строку
        string jsonData = File.ReadAllText(jsonFilePath);

        // Десериализуем json в массив объектов типа Content
        JArray jsonArray = JArray.Parse(jsonData);
        foreach (JObject jsonObject in jsonArray)
        {
            Content? content = JsonConvert.DeserializeObject<Content>(jsonObject.ToString());

            result.Add(content);
        }

        return result;
    }
}