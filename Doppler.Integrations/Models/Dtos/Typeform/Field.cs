namespace Doppler.Integrations.Models.Dtos.Typeform

{
    public class Field
    {
        public string id { get; set; }
        public string type { get; set; }
        public Properties properties { get; set; }

        public string @ref { get; set; }
    }
}