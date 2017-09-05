
namespace Doppler.Integrations.Models.Dtos
{
    public class ItemFieldDto
    {
        public string Name { get; set; }
   
        public string Type { get; set; }
        
        public bool Predefined { get; set; }

        public bool Private { get; set; }

        public bool Readonly { get; set; }

        public string Sample { get; set; }
    }
}
