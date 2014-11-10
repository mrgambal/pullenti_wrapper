using System.Collections.Generic;

namespace PullEntiCLI
{
    public struct Item
    {
        public string Title { get; set; }
        public string Content { get; set; }
    }

    public struct Occurence
    {
        public int Start { get; set; }
        public int End { get; set; }
        public string Chunk { get; set; }
    }

    public struct Entity
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public IList<Occurence> Occurences { get; set; }
        public IList<KeyValuePair<string, string>> Slots { get; set; }
    }

    public struct Payload
    {
        public int Counter { get; set; }
        public string Line { get; set; }
    }

    public struct Result
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public IList<Entity> Structure { get; set; }
    }
}
