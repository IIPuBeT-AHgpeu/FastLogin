using System.IO;
using System.Text.Json;

namespace FastLoginSettings
{
    public class ModelData
    {
        public bool Autorun { get; set; }
        public string FirstString { get; set; }
        public string SecondString { get; set; }
        public string ThirdString { get; set; }
        public bool FirstStart { get; set; }

        public ModelData()
        {
            Autorun = false;
            FirstStart = true;
            FirstString = "";
            SecondString = "";
            ThirdString = "";
        }
    }

    public class Model
    {
        public ModelData data = new ModelData();

        public Model(string path)
        {
            FileInfo fileInfo = new FileInfo(path + "\\FastLoginSettings.json");
            if(!fileInfo.Exists)
            {
                using (fileInfo.Create()) { }
                saveData(path);
            }
        }

        public void loadData(string path)
        {
            using (FileStream fstream = File.OpenRead(path + "\\FastLoginSettings.json"))
            {
                byte[] array = new byte[fstream.Length];
                //чтение файла
                fstream.Read(array, 0, array.Length);

                string textFromFile = System.Text.Encoding.Default.GetString(array);

                data = JsonSerializer.Deserialize<ModelData>(textFromFile);
            }

        }

        public void saveData(string path)
        {
            string json = JsonSerializer.Serialize(data);

            using ( FileStream fstream = new FileStream(path + "\\FastLoginSettings.json", FileMode.Create))
            {
                byte[] array = System.Text.Encoding.Default.GetBytes(json);

                fstream.Write(array, 0, array.Length);
            }
        }

        public void setData(bool autorun, string firstString, string secondString, string thirdString, bool isFirstStart = false)
        {
            data.Autorun = autorun;
            data.FirstStart = isFirstStart;
            data.FirstString = firstString;
            data.SecondString = secondString;
            data.ThirdString = thirdString;
        }

    }
}
