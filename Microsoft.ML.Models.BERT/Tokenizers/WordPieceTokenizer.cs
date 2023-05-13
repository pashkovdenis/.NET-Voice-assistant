using Microsoft.ML.Models.BERT.Extensions;

namespace Microsoft.ML.Models.BERT.Tokenizers
{
    public class WordPieceTokenizer
    {
        public class DefaultTokens
        {
            public const string Padding = "";
            public const string Unknown = "[UNK]";
            public const string Classification = "[CLS]";
            public const string Separation = "[SEP]";
            public const string Mask = "[MASK]";
        }

        private readonly List<string> _vocabulary;

        public WordPieceTokenizer(List<string> vocabulary)
        {
            _vocabulary = vocabulary;
        }

        public List<(string Token, int VocabularyIndex)> Tokenize(params string[] texts)
        {
            // [CLS] Words of sentence [SEP] Words of next sentence [SEP]
            IEnumerable<string> tokens = new string[] { DefaultTokens.Classification };

            foreach (var text in texts)
            {
                var txt = text.Trim('\n');
                tokens = tokens.Concat(TokenizeSentence(txt));
                tokens = tokens.Concat(new string[] { DefaultTokens.Separation });
            }

            return tokens
                .SelectMany(TokenizeSubwords)
                .ToList();
        }

        /**
         * Some words in the vocabulary are too big and will be broken up in to subwords
         * Example "Embeddings"
         * [‘em’, ‘##bed’, ‘##ding’, ‘##s’]
         * https://mccormickml.com/2019/05/14/BERT-word-embeddings-tutorial/
         * https://developpaper.com/bert-visual-learning-of-the-strongest-nlp-model/
         * https://medium.com/@_init_/why-bert-has-3-embedding-layers-and-their-implementation-details-9c261108e28a
         */
        private IEnumerable<(string Token, int VocabularyIndex)> TokenizeSubwords(string word)
        {
            if (_vocabulary.Contains(word))
            {
                return new (string, int)[] { (word, _vocabulary.IndexOf(word)) };
            }

            var tokens = new List<(string, int)>();
            var remaining = word;
            int counter = 0; 

            while (!string.IsNullOrEmpty(remaining) && remaining.Length > 2 && counter < 258)
            {
                var prefix = _vocabulary.Where(remaining.StartsWith)
                    .OrderByDescending(o => o.Count())
                    .FirstOrDefault();

                if (prefix == null)
                {
                    tokens.Add((DefaultTokens.Unknown, _vocabulary.IndexOf(DefaultTokens.Unknown)));

                    return tokens;
                }

                remaining = remaining.Replace(prefix, "##");

                tokens.Add((prefix, _vocabulary.IndexOf(prefix)));
                counter++;
            }
             

            if (!string.IsNullOrWhiteSpace(word) && !tokens.Any())
            {
                tokens.Add((DefaultTokens.Unknown, _vocabulary.IndexOf(DefaultTokens.Unknown)));
            }

            return tokens;
        }

        private IEnumerable<string> TokenizeSentence(string text)
        {
            // remove spaces and split the , . : ; etc..
            return text.Split(new string[] { " ", "   ", "\r\n" }, StringSplitOptions.None)
                .SelectMany(o => o.SplitAndKeep(".,;:\\/?!#$%()=+-*\"'–_`<>&^@{}[]|~'".ToArray()))
                .Select(o => o.ToLower());
        }
    }
}
