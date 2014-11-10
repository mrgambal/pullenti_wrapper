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
            processor = new Processor();
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
        /// title of it and list of parsed entities.
        /// </returns>
        /// <param name="article">Article.</param>
        public Result ProcessArticle(Item article)
        {
            var entities = new List<Entity>();
            AnalysisResult result = processor.Process(new SourceOfAnalysis(article.Content), null, MorphLang.UA);

            foreach (Referent entity in result.Entities)
                entities.Add(CreateEntity(entity));

            return new Result()
            {
                Title = article.Title,
                Text = article.Content,
                Structure = entities
            };
        }

        private Entity CreateEntity(Referent entity)
        {
            return new Entity
            {
                Name = entity.ToString(),
                Slots = (
                    from slot in entity.Slots
                    where !(slot.IsInternal || slot.Value is Referent)
                    select new KeyValuePair<string, string>(slot.TypeName, slot.Value.ToString())
                    ).ToList(),
                Type = entity.InstanceOf.Caption,
                Occurences = GatherOccurences(entity.Occurrence)
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