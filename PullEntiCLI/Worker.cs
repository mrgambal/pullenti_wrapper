using System;
using System.Collections.Generic;
using System.Linq;
using EP;
using EP.Text;
using Newtonsoft.Json;

namespace PullEntiCLI
{
    /// <summary>
    /// Worker. Implements text processing using Pullenti SDK.
    /// </summary>
    public class Worker : IDisposable
    {
        private readonly Processor processor = null;
        private bool disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullEntiCLI.Worker"/> class.
        /// </summary>
        public Worker()
        {
            processor = new Processor("FACT");
        }

        /// <summary>
        /// Gathers occurences of entities in processed text.
        /// </summary>
        /// <returns>List of occurences.</returns>
        /// <param name="occurence">Occurence.</param>
        private IList<Occurence> GatherOccurences(IEnumerable<TextAnnotation> occurence)
        {
            return (
                from oc in occurence
                select new Occurence
                {
                    Start = oc.BeginChar,
                    End = oc.EndChar,
                    Chunk = oc.ToString()
                }).ToList();
        }

        /// <summary>
        /// Processes the article.
        /// </summary>
        /// <returns>Combined data for the article. Contains the article, 
        /// title of it and graph of parsed entities in JSON.
        /// </returns>
        /// <param name="article">Article.</param>
        public IDictionary<String, String> ProcessArticle(Item article)
        {
            var entities = new List<Entity>();
            Entity e;
            AnalysisResult result = processor.Process(new SourceOfAnalysis(article.Content), null, MorphLang.UA);

            foreach (Referent entity in result.Entities)
            {
                if (!(entity is FactReferent))
                    continue;

                e = new Entity
                {
                    Name = entity.ToString(),
                    Refs = new List<String>(),
                    Slots = (
                        from slot in entity.Slots
                        where !slot.IsInternal && !(slot.Value is Referent)
                        select slot.ToString()).ToList(),
                    Type = entity.InstanceOf.Caption,
                    Occurences = GatherOccurences(entity.Occurrence)
                };
                entities.Add(e);
            }

            return new Dictionary<String, String> {
				{ "title", article.Title },
				{ "text", article.Content },
				{ "structure", JsonConvert.SerializeObject(entities) }
			};
        }

        #region IDispposable

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        /// <param name="disposing">Flag</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
                processor.Dispose();

            disposed = true;
        }

        #endregion
    }
}

