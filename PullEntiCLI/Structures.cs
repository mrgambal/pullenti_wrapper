using Newtonsoft.Json;
using System;
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
        [JsonProperty(PropertyName = "start")]
        public int Start { get; set; }
        [JsonProperty(PropertyName = "end")]
        public int End { get; set; }
        [JsonProperty(PropertyName = "chunk")]
        public string Chunk { get; set; }
    }

    public struct Entity
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "refs")]
        public IList<int> Refs { get; set; }
        [JsonProperty(PropertyName = "occurences")]
        public IList<Occurence> Occurences { get; set; }
        [JsonProperty(PropertyName = "slots")]
        public IList<KeyValuePair<string, string>> Slots { get; set; }
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
    }

    public struct Result
    {
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }
        [JsonProperty(PropertyName = "structure")]
        public IList<Entity> Structure { get; set; }
    }
}
