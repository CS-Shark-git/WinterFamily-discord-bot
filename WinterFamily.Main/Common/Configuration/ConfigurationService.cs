using Newtonsoft.Json;
using System.Text;
using WinterFamily.Main.Common.Attributes;

namespace WinterFamily.Main.Common.Configuration;

internal class ConfigurationService<T> where T : IJsonConfiguration
{

    public T Build()
    {
        var service = new AttributeService<T>();
        string fileName = service.GetFileNameAgrument();
        string data = ReadFile(fileName);

        T configModel = JsonConvert.DeserializeObject<T>(data)!;
        return configModel;
    }

    private string ReadFile(string fileName)
    {
        using (StreamReader reader = new StreamReader(fileName, new UTF8Encoding(false)))
        {
            var data = reader.ReadToEnd();
            return data;
        }
    }
}
