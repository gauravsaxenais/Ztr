namespace Business.Parser.Models
{
    using System.Collections.Generic;

    public class Message
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsRepeated { get; set; }

        public List<List<Field>> Fields { get; } = new List<List<Field>>();

        public List<Message> Messages { get; } = new List<Message>();
    }
}
