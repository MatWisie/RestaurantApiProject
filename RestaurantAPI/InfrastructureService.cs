using Newtonsoft.Json;
using RestaurantAPI.Models;

namespace RestaurantAPI
{
    public class InfrastructureService
    {
        public void EnsureInfrastructureJsonExists()
        {
            string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "infrastructure.json");

            if (!File.Exists(jsonFilePath))
            {
                InfrastructureModel model = new InfrastructureModel
                {
                    NumberOfColumns = 10,
                    NumberOfRows = 10
                };

                string json = JsonConvert.SerializeObject(model);

                File.WriteAllText(jsonFilePath, json);
            }
        }

        public InfrastructureModel GetInfrastructureModel()
        {
            string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "infrastructure.json");

            if (File.Exists(jsonFilePath))
            {
                string json = File.ReadAllText(jsonFilePath);
                InfrastructureModel model = JsonConvert.DeserializeObject<InfrastructureModel>(json);

                return model;
            }
            else
            {
                throw new FileNotFoundException("infrastructure.json not found.");
            }
        }

        public bool EditInfrastructure(InfrastructureModel model)
        {
            try
            {
                string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "infrastructure.json");

                string json = JsonConvert.SerializeObject(model);

                File.WriteAllText(jsonFilePath, json);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
